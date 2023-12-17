using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace SharpManager
{
    /// <summary>
    /// Class representing the floppy disk drive
    /// </summary>
    public class CE140F
    {
        private enum CaptureMode
        {
            None,
            BinarySave,
            TextSave,
            Print
        }

        /// <summary>The maximum number of open files</summary>
        private const int MaxFileHandles = 6;

        public DirectoryInfo? DriveDirectory { get; set; }

        private List<string> files = new();

        private int fileIndex = 0;

        private readonly IMessageLog messageLog;

        /// <summary>The currently open file</summary>
        private FileStream? currentFile = null;

        /// <summary>The error frame</summary>
        private readonly byte[] ErrorFrame = new byte[] { 0xFF, 0 };

        /// <summary>The current file size</summary>
        private int currentFileSize = 0;

        /// <summary>The file handles</summary>
        private FileStream?[] fileHandles = new FileStream[MaxFileHandles];

        private CaptureMode captureMode = CaptureMode.None;

        /// <summary>
        /// Initializes a new instance of the <see cref="CE140F"/> class.
        /// </summary>
        /// <param name="messageLog">The message log.</param>
        public CE140F(IMessageLog messageLog)
        {
            this.messageLog = messageLog;
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            files.Clear();
            fileIndex = 0;
            currentFile?.Dispose();
            currentFile = null;
            currentFileSize = 0;
            for (int i = 0; i < MaxFileHandles; i++)
            {
                fileHandles[i]?.Dispose();
                fileHandles[i] = null;
            }
        }

        /// <summary>
        /// Processes the command.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The response from the command</returns>
        public DiskResponse ProcessCommand(byte[] data)
        {
            return new DiskResponse(ProcessCommandInternal(data), captureMode != CaptureMode.None);
        }

        /// <summary>
        /// Processes the command main implementation
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The response from the command</returns>
        private byte[] ProcessCommandInternal(byte[] data)
        {
            // Save the current mode and reset to none berfore calling the mode commands
            if (captureMode != CaptureMode.None)
            {
                var currentMode = captureMode;
                captureMode = CaptureMode.None;
                if (currentMode == CaptureMode.TextSave) return CommandSaveWriteLine(data);
                if (currentMode == CaptureMode.BinarySave) return CommandSaveWriteBinary(data);
                if (currentMode == CaptureMode.Print) return CommandPrintWrite(data);
            }

            messageLog.Write($"[Command #{data[0]:X2}] ");

            switch (data[0])
            {
                case 0x05: return CommandFilesInit();
                case 0x06: return CommandFilesItem(false);
                case 0x07: return CommandFilesItem(true);
                case 0x0E: return CommandLoadOpen(data);
                case 0x17: return CommandLoadReadByte();
                case 0x12: return CommandLoadReadLine();
                case 0x0F: return CommandLoadBinary();
                case 0x1D: return CommandDiskFree(data);
                case 0x10: return CommandSaveOpen(data);
                case 0x11: return CommandSaveBinary(data);
                case 0x16: return CommandSaveText();
                case 0x03: return CommandOpen(data);
                case 0x04: return CommandClose(data);
                case 0x0A: return CommandKill(data);
                case 0x13: return CommandInput(data); // string         INPUT #x, X$
                case 0x14: return CommandInput(data); // number         INPUT #x, X
                case 0x15: return CommandPrint(data);
                // case 0x1F: return CommandInput(data); // ?? TODO
                case 0x20: return CommandInput(data); // number array

                /*
                //case 0x08: process_INIT(0x08);break;
                //case 0x09: process_INIT(0x09);break;
                //    case 0x0B: process_NAME(0x0B);break;
                //    case 0x0C: process_SET(0x0C);break;
                //    case 0x0D: process_COPY(0x0D);break;
                //    case 0x1A: process_EOF(0x1A);break;
                //    case 0x1C: process_LOC(0x1C);break;
                */
                default:
                    messageLog.WriteLine($"Unknown");
                    return new byte[] { 0xFF, 0 };
            }
        }

        /// <summary>
        /// Commands the disk free.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private byte[] CommandDiskFree(byte[] data)
        {
            int driveNumber = data[1];
            int diskFree = 65000;
            messageLog.WriteLine($"DSKF({driveNumber})");
            var result = new List<byte>();
            result.AddSize(diskFree);
            return result.ToFrame();
        }

        /// <summary>
        /// The FILES command which parses the directory and returns the total number of files
        /// </summary>
        /// <returns>Number of files</returns>
        private byte[] CommandFilesInit()
        {
            messageLog.WriteLine($"FILES");
            fileIndex = 0;
            files.Clear();
            if (DriveDirectory != null) foreach (var file in DriveDirectory.EnumerateFiles())
            {
                files.Add(file.Name);
            }
            return CreateFrame((byte)files.Count);
        }

        /// <summary>
        /// Returns the file name at the current index and then increment or decrements the index
        /// </summary>
        /// <param name="previous">if set to <c>true</c> then previous.</param>
        /// <returns>File name</returns>
        private byte[] CommandFilesItem(bool previous)
        {
            messageLog.WriteLine($"FILES " + (previous ? "Up" : "Down"));

            // If no files, return error to computer
            if (files.Count == 0) return ErrorFrame;

            // Add file name to the result
            var result = new List<byte>();
            result.AddString(FormatFileName(files[fileIndex]));

            // Update the index
            fileIndex += previous ? -1 : 1;
            if (fileIndex < 0) fileIndex = 0;
            if (fileIndex > files.Count - 1) fileIndex = files.Count - 1;

            return result.ToFrame();
        }

        /// <summary>
        /// Opens a file
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private byte[] CommandLoadOpen(byte[] data)
        {
            string fileName = Encoding.ASCII.GetString(data, 3, 12).Replace(" ", "");
            messageLog.WriteLine($"LOAD \"{fileName}\"");

            var result = new List<byte>();
            result.AddString(" ");

            if (DriveDirectory == null)
            {
                messageLog.WriteLine("No directory selected for drive.");
                result.AddSize(0);
                return result.ToFrame();
            }

            // Get filename
            string filePath = Path.Combine(DriveDirectory.FullName, fileName);
            messageLog.WriteLine($"  {filePath}");
            currentFile = File.OpenRead(filePath);
            result.AddSize((int)currentFile.Length);
            return result.ToFrame();
        }

        /// <summary>
        /// Commands the load read byte.
        /// </summary>
        /// <returns></returns>
        private byte[] CommandLoadReadByte()
        {
            var result = new List<byte>();
            var value = currentFile?.ReadByte() ?? -1;
            if (value == -1) return ErrorFrame;
            result.Add((byte)value);
            return result.ToFrame();
        }

        /// <summary>
        /// Commands the load read line.
        /// </summary>
        /// <returns></returns>
        private byte[] CommandLoadReadLine() 
        {
            messageLog.WriteLine("Load file line");
            var result = new List<byte>();
            while (true)
            {
                var value = currentFile?.ReadByte() ?? -1;
                if (value == 0x0A) continue;            // Ignore line-feeds
                if (value == -1)
                {
                    result.Add(0x1A);           // Send EOF
                    currentFile?.Dispose();     // Close file
                    currentFile = null;
                    break;                  // End
                }
                result.Add((byte)value);    // Send byte
                if (value == 0x0D) break;   // If CR then end line
            }
            result.AddFrame();              // Frame the line
            result.Add(0);                  // Add an additional zero byte
            return result.ToArray();
        }

        /// <summary>
        /// Commands the load binary.
        /// </summary>
        /// <returns></returns>
        private byte[] CommandLoadBinary()
        {
            messageLog.WriteLine("Load file data");
            var result = new List<byte>();
            result.Add(0);                      // Start of frame
            var buffer = new byte[256];
            while (true)
            {
                int bytesRead = currentFile?.Read(buffer, 0, buffer.Length) ?? 0;
                if (bytesRead == 0) break;
                result.AddBlock(new ArraySegment<byte>(buffer, 0, bytesRead));
            }
            result.Add(0);                      // Additional zero byte
            currentFile?.Dispose();             // Close file
            currentFile = null;
            return result.ToArray();
        }

        /// <summary>
        /// The save command
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private byte[] CommandSaveOpen(byte[] data)
        {
            string fileName = Encoding.ASCII.GetString(data, 3, 12).Replace(" ", "");
            messageLog.WriteLine($"SAVE \"{fileName}\"");

            if (DriveDirectory == null)
            {
                messageLog.WriteLine("No directory selected for drive.");
                return CreateResult(false);
            }

            // Get filename
            string filePath = Path.Combine(DriveDirectory.FullName, fileName);
            messageLog.WriteLine($"  {filePath}");
            currentFile = File.OpenWrite(filePath);
            return CreateResult(true);
        }

        /// <summary>
        /// Commands the save binary.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private byte[] CommandSaveBinary(byte[] data)
        {
            currentFileSize = data[2] + (data[3] << 8) + (data[4] << 16);
            messageLog.WriteLine($"Save binary file (size {currentFileSize:n0})");
            captureMode = CaptureMode.BinarySave;
            return CreateResult(true);
        }

        /// <summary>
        /// Commands the save text.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private byte[] CommandSaveText()
        {
            messageLog.Write($"Save text file");
            captureMode = CaptureMode.TextSave;
            return CreateResult(true);
        }

        /// <summary>
        /// Save file text line
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private byte[] CommandSaveWriteLine(byte[] data)
        {
            // File not open
            if (currentFile == null) return CreateResult(false);

            // End of file
            if (data[0] == 0x1A)
            {
                messageLog.WriteLine();
                messageLog.WriteLine("Done.");
                currentFile.Dispose();
                currentFile = null;
                return CreateResult(true);
            }

            messageLog.Write(".");

            // Last byte is checksum so ignore
            for (int i = 0; i < data.Length - 1; i++)
            {
                currentFile.WriteByte(data[i]);
            }

            return CreateResult(true);
        }

        /// <summary>
        /// Save file binary
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private byte[] CommandSaveWriteBinary(byte[] data)
        {
            // File not open
            if (currentFile == null) return CreateResult(false);

            messageLog.Write(".");

            // Write the data -- last byte is checksum
            for (int i = 0; i < data.Length - 1; i++)
            {
                currentFile.WriteByte(data[i]);
            }

            // If the file is the correct size, close the file
            if (currentFile.Length == currentFileSize)
            {
                messageLog.WriteLine();
                messageLog.WriteLine("Done.");
                currentFile.Dispose();
                currentFile = null;
            }
            else
            {
                // Otherwise receive the next block
                captureMode = CaptureMode.BinarySave;
            }

            return CreateResult(true);
        }

        /// <summary>
        /// Close a file or all files
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private byte[] CommandClose(byte[] data)
        {
            int fileNumber = data[1];
            if (fileNumber == 0xFF)
            {
                messageLog.WriteLine($"CLOSE ALL");
                for (int i = 0; i < MaxFileHandles; i++)
                {
                    fileHandles[i]?.Dispose();
                    fileHandles[i] = null;
                }
            }
            else
            {
                messageLog.WriteLine($"CLOSE #{fileNumber:X2}");
                int fileIndex = fileNumber - 2;        // Convert to index
                fileHandles[fileIndex]?.Dispose();
                fileHandles[fileIndex] = null;
            }

            return CreateResult(true);
        }

        /// <summary>
        /// Open a file
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private byte[] CommandOpen(byte[] data)
        {
            if (DriveDirectory == null)
            {
                messageLog.WriteLine("No directory selected for drive.");
                return CreateResult(false);
            }

            // Get filename
            string fileName = Encoding.ASCII.GetString(data, 3, 12).Replace(" ", "");
            int fileMode = data[15];            // 1: input, 2: output, 3: append
            int fileNumber = data[16];          // file#
            int fileIndex = fileNumber - 2;     // file index

            messageLog.WriteLine($"OPEN \"{fileName}\" FOR '{fileMode}' AS #{fileNumber}");
            if (fileIndex < 0 || fileIndex > MaxFileHandles)
            {
                messageLog.WriteLine($"Invalid file #{fileNumber}");
                return CreateResult(false);
            }

            if (fileHandles[fileIndex] != null)
            {
                fileHandles[fileIndex]?.Dispose();
                fileHandles[fileIndex] = null;
            }

            fileName = Path.Combine(DriveDirectory.FullName, fileName);

            try
            {
                if (fileMode == 1) fileHandles[fileIndex] = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                else if (fileMode == 2) fileHandles[fileIndex] = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                else if (fileMode == 3) fileHandles[fileIndex] = new FileStream(fileName, FileMode.Append, FileAccess.Write);
                else
                {
                    messageLog.WriteLine($"Invalid file mode {fileMode}");
                    return CreateResult(false);
                }
            }
            catch (Exception ex)
            {
                messageLog.WriteLine($"Error {ex.Message}");
                return CreateResult(false);
            }

            return CreateResult(true);
        }

        /// <summary>
        /// Print to an open file
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private byte[] CommandPrint(byte[] data)
        {
            int fileNumber = data[1];
            int fileIndex = fileNumber - 2;
            messageLog.WriteLine($"PRINT #{fileNumber}");
            if (fileIndex < 0 || fileIndex > MaxFileHandles)
            {
                messageLog.WriteLine($"Invalid file #{fileNumber}");
                return CreateResult(false);
            }
            if (fileHandles[fileIndex] == null)
            {
                messageLog.WriteLine($"File #{fileNumber} not open");
                return CreateResult(false);
            }
            if (!fileHandles[fileIndex]?.CanWrite ?? false)
            {
                messageLog.WriteLine($"File #{fileNumber} not writable");
                return CreateResult(false);
            }

            captureMode = CaptureMode.Print;
            currentFile = fileHandles[fileIndex];
            return CreateResult(true);
        }

        /// <summary>
        /// Print line to file
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private byte[] CommandPrintWrite(byte[] data)
        {
            // No current file
            if (currentFile == null) return CreateResult(false);

            // skip empty message (CRLF only)
            if (data[0] == 0x0D && data[1] == 0x0A) return CreateResult(true);

            // Write data
            for (int i = 0; i < data.Length - 2; i++)  // Ignore 0 and checksum
            {
                currentFile.WriteByte(data[i]);
            }

            // If no line terminater then add
            if (data[^3] != 0x0A)
            {
                messageLog.WriteLine("  Appending CRLF");
                currentFile.WriteByte(0x0D);
                currentFile.WriteByte(0x0A);
            }

            return CreateResult(true);
        }

        /// <summary>
        /// Read line from file
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private byte[] CommandInput(byte[] data)
        {
            int fileNumber = data[1];
            int fileIndex = fileNumber - 2;
            messageLog.WriteLine($"INPUT #{fileNumber}");
            if (fileIndex < 0 || fileIndex > MaxFileHandles)
            {
                messageLog.WriteLine($"Invalid file #{fileNumber}");
                return CreateResult(false);
            }
            if (fileHandles[fileIndex] == null)
            {
                messageLog.WriteLine($"File #{fileNumber} not open");
                return CreateResult(false);
            }
            if (!fileHandles[fileIndex]?.CanRead ?? false)
            {
                messageLog.WriteLine($"File #{fileNumber} not readable");
                return CreateResult(false);
            }

            // Read line from the file
            var result = new List<byte>();
            while (true)
            {
                var value = fileHandles[fileIndex]?.ReadByte() ?? -1;
                if (value == -1) break;
                result.Add((byte)value);    // Send byte
                if (value == 0x0A) break;   // If LF then end line
            }
            result.Add(0);                  // Add an additional zero byte
            result.AddFrame();              // Frame the line
            result.Add(0);                  // Add an additional zero byte
            return result.ToArray();
        }

        /// <summary>
        /// Kill command deletes a file
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private byte[] CommandKill(byte[] data)
        {
            string fileName = Encoding.ASCII.GetString(data, 3, 12).Replace(" ", "");
            messageLog.WriteLine($"KILL \"{fileName}\"");
            // TODO implement kill
            return CreateResult(false);
        }

        /// <summary>
        /// Formats the name of the file for the Sharp Pocket PC
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        private static string FormatFileName(string fileName)
        {
            const string drivePrefix = "X:";
            const int maxNameLength = 8;
            const int maxExtensionLength = 3;

            string name = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName).TrimStart('.'); // Remove the dot

            // Ensure the name and extension are within the limits
            name = name.Length > maxNameLength ? name[..maxNameLength] : name.PadRight(maxNameLength, ' ');
            extension = extension.Length > maxExtensionLength ? extension[..maxExtensionLength] : extension.PadRight(maxExtensionLength, ' ');
            return $"{drivePrefix}{name}.{extension} ";
        }

        /// <summary>
        /// Creates the frame.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private static byte[] CreateFrame(params byte[] data)
        {
            var result = new List<byte>();
            result.AddRange(data);
            return result.ToFrame();
        }

        /// <summary>
        /// Creates the array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private static byte[] CreateArray(params byte[] data)
        {
            return data;
        }

        /// <summary>
        /// Creates the result.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        /// <returns></returns>
        private static byte[] CreateResult(bool success)
        {
            return CreateArray(success ? (byte)0 : (byte)0xFF);
        }
    }
}
