using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace SharpManager
{
    /// <summary>
    /// Class representing the floppy disk drive
    /// </summary>
    public class CE140F
    {
        public DirectoryInfo? DriveDirectory { get; set; }

        private List<string> files = new List<string>();

        private int fileIndex = 0;

        private readonly IMessageLog messageLog;

        /// <summary>
        /// Initializes a new instance of the <see cref="CE140F"/> class.
        /// </summary>
        /// <param name="messageLog">The message log.</param>
        public CE140F(IMessageLog messageLog)
        {
            this.messageLog = messageLog;
            //files.Add("Test1.bas");
            //files.Add("Test2.bas");
            //files.Add("Test3.txt");
        }

        public byte[] ProcessCommand(byte[] data)
        {
            List<byte>? result = null;
            switch (data[0])
            {
                case 0x05: result = CommandFilesInit(); break;
                case 0x06: result = CommandFilesItem(false); break;
                case 0x07: result = CommandFilesItem(true); break;
                case 0x0E: result = CommandLoadOpen(data); break;
                /*
                case 0x03: process_OPEN(); break;
                case 0x04: process_CLOSE(); break;
                //case 0x08: process_INIT(0x08);break;
                //case 0x09: process_INIT(0x09);break;
                case 0x0A: process_KILL(); break;
                //    case 0x0B: process_NAME(0x0B);break;
                //    case 0x0C: process_SET(0x0C);break;
                //    case 0x0D: process_COPY(0x0D);break;
                case 0x0E: process_LOAD(0x0E); break;
                case 0x0F: process_LOAD(0x0F); break;
                case 0x10: process_SAVE(0x10); break;
                case 0x11: process_SAVE(0x11); break;
                case 0x16: process_SAVE(0x16); break;    // SAVE ASCII
                case 0xFE: process_SAVE(0xfe); break;    // next SAVE ascii cmd
                case 0xFF: process_SAVE(0xff); break;    // next SAVE cmd
                case 0x12: process_LOAD(0x12); break;
                case 0x13: process_INPUT(0x13); break; // INPUT #x, X$
                case 0x14: process_INPUT(0x14); break; // INPUT #x, X
                case 0x15: process_PRINT(0x15); break;
                case 0xFD: process_PRINT(0xfd); break; // next PRINT cmd
                case 0x17: process_LOAD(0x17); break;
                //    case 0x1A: process_EOF(0x1A);break;
                //    case 0x1C: process_LOC(0x1C);break;
                */
                case 0x1D: result = CommandDiskFree(data); break;
                //    case 0x1F: process_INPUT(0x1f);break;
                // case 0x20: process_INPUT(0x20); break;
                default:
                    messageLog.WriteLine($"Unknown command {data[0]:X2}");
                    break;
            }

            // If no result then return error
            if (result == null) return new byte[] { 0xFF, 0 };

            // First byte is always zero                
            result.Insert(0, 0);

            // Add checksum
            int checksum = 0;
            foreach (var value in result) checksum = (checksum + value) & 0xFF;
            result.Add((byte)checksum);

            // Return result
            return result.ToArray();
        }

        /// <summary>
        /// Commands the disk free.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private List<byte> CommandDiskFree(byte[] data)
        {
            int driveNumber = data[1];
            int diskFree = 65000;
            messageLog.WriteLine($"DSKF command ({driveNumber})");
            var result = new List<byte>();
            result.AddSize(diskFree);
            return result;
        }

        /// <summary>
        /// The FILES command which parses the directory and returns the total number of files
        /// </summary>
        /// <returns>Number of files</returns>
        private List<byte> CommandFilesInit()
        {
            messageLog.WriteLine($"FILES command");
            fileIndex = 0;
            files.Clear();
            if (DriveDirectory != null) foreach (var file in DriveDirectory.EnumerateFiles())
            {
                files.Add(file.Name);
            }
            return new List<byte>() { (byte)files.Count };
        }

        /// <summary>
        /// Returns the file name at the current index and then increment or decrements the index
        /// </summary>
        /// <param name="previous">if set to <c>true</c> then previous.</param>
        /// <returns>File name</returns>
        private List<byte>? CommandFilesItem(bool previous)
        {
            // If no files, return error to computer
            if (files.Count == 0) return null;

            // Add file name to the result
            var result = new List<byte>();
            result.AddString(FormatFileName(files[fileIndex]));

            // Update the index
            fileIndex += previous ? -1 : 1;
            if (fileIndex < 0) fileIndex = 0;
            if (fileIndex > files.Count - 1) fileIndex = files.Count - 1;

            return result;
        }

        /// <summary>
        /// Opens a file
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private List<byte> CommandLoadOpen(byte[] data)
        {
            // Get filename
            string fileName = Encoding.ASCII.GetString(data, 3, 12).Replace(" ", "");
            messageLog.WriteLine($"LOAD command '{fileName}'");

            var result = new List<byte>();
            result.AddString(" ");
            result.AddSize(0);
            return result;
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
    }
}
