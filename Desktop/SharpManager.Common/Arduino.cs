using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.IO.Ports;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SharpManager
{
    /// <summary>
    /// The error codes
    /// </summary>
    public enum ErrorCode
    {
        Ok = 0,
        Timeout = 1,
        Cancelled = 2,
        Unexpected = 3,
        Overflow = 4,
        SyncError = 5,
        End = 0xFF
    }

    /// <summary>
    /// The packet types
    /// </summary>
    public enum Command
    {
        Init = 1,
        Ping = 2,
        DeviceSelect = 3,
        Print = 4,
        Data = 5,
        LoadTape = 6,
        SaveTape = 7,
        Disk = 8
    }

    public class Arduino : NotifyObject, IDisposable
    {
        /// <summary>The serial port</summary>
        private SerialPort? serialPort = null;

        /// <summary>The serial stream</summary>
        private SerialPortByteStream? serialStream = null;

        /// <summary>Cancel the current operation</summary>
        private CancellationTokenSource? cancellationTokenSource = null;

        /// <summary>Cancel the main loop</summary>
        private CancellationTokenSource? mainLoopCts = null;

        /// <summary>The message log</summary>
        private readonly IDebugTarget messageTarget;

        /// <summary>Async suspend gate for tasks</summary>
        private readonly AsyncSuspendGate mainLoopGate = new();

        /// <summary>The arduino buffer size</summary>
        private const int BufferSize = 64;

        /// <summary>The file header size</summary>
        private const int HeaderSize = 10;

        /// <summary>The high version value</summary>
        private const int VersionHigh = 1;

        /// <summary>The low version value</summary>
        private const int VersionLow = 3;

        /// <summary>The default read timeout</summary>
        private const int ReadTimeout = 5000;

        /// <summary>Lock object for disconnect state</summary>
        private readonly object disconnectSync = new();
        /// <summary>Disconnecting flag</summary>
        private bool disconnectingOrDisconnected = true;

        private enum FileFormat
        {
            Basic = 0x70,
            BasicPassword = 0x71,
            ExtBasic = 0x72,
            ExtBasicPassword = 0x73,
            Data = 0x74,
            Binary = 0x76
        }

        /// <summary>
        /// Gets a value indicating whether this instance is connected.
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance can cancel.
        /// </summary>
        public bool CanCancel => cancellationTokenSource != null;

        /// <summary>
        /// Gets or sets the disk directory.
        /// </summary>
        public string? DiskDirectory
        {
            get => diskDrive.DiskDirectory;
            set => diskDrive.DiskDirectory = value; 
        }

        /// <summary>The disk drive instance</summary>
        private readonly CE140F diskDrive;

        /// <summary>
        /// Initializes a new instance of the <see cref="Arduino" /> class.
        /// </summary>
        /// <param name="messageTarget">The message log.</param>
        /// <param name="diskDirectory">The disk directory.</param>
        public Arduino(IDebugTarget messageTarget, string? diskDirectory)
        {
            this.messageTarget = messageTarget;
            this.diskDrive = new CE140F(messageTarget);
            this.diskDrive.DiskDirectory = diskDirectory;
        }

        /// <summary>
        /// Connects the specified port name.
        /// </summary>
        /// <param name="portName">Name of the port.</param>
        /// <returns></returns>
        public async Task Connect(string portName)
        {
            lock (disconnectSync)
            {
                disconnectingOrDisconnected = false;
            }

            mainLoopCts = new();
            serialPort = new SerialPort(portName, 115200);
            serialPort.DtrEnable = false;
            serialPort.ReadTimeout = 20;
            serialPort.Open();
            serialStream = new SerialPortByteStream(serialPort);
            IsConnected = true;
            OnPropertyChanged(nameof(IsConnected));
            messageTarget.WriteLine($"Connected to {portName}.");

            // Empty the read buffer
            try
            {
                await Initialize();
            } 
            catch
            {
                Disconnect();
                throw;
            }

            // Begin the main loop
            _ = Task.Run(() => MainloopSupervisor(mainLoopCts.Token))
                .ContinueWith(t =>
                    {
                        var ex = t.Exception;
                    }, TaskContinuationOptions.OnlyOnFaulted)
                .ContinueWith(t =>
                    {
                        try
                        {
                            Disconnect();
                        }
                        catch { }
                    });           
        }

        /// <summary>
        /// Disconnects this instance.
        /// </summary>
        public void Disconnect()
        {
            // Fast exit for concurrent callers
            lock (disconnectSync)
            {
                if (disconnectingOrDisconnected) return;
                disconnectingOrDisconnected = true;
            }

            mainLoopCts?.Cancel();
            cancellationTokenSource?.Cancel();
            
            try
            {
                // Try to send cancellation byte
                serialStream?.WriteByte(Ascii.CAN);
            }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
            
            diskDrive.Reset();
            serialStream?.Dispose();
            serialStream = null;
            serialPort?.Close();
            serialPort?.Dispose();
            serialPort = null;

            IsConnected = false;
            OnPropertyChanged(nameof(IsConnected));
            messageTarget.WriteLine("Disconnected.");
        }

        /// <summary>
        /// Supervises the main loop
        /// </summary>
        /// <param name="ct"></param>
        /// <param name="exceptionHandler"></param>
        /// <returns></returns>
        private async Task MainloopSupervisor(CancellationToken ct)
        {
            int failures = 0;

            while (!ct.IsCancellationRequested && serialStream != null)
            {
                try
                {
                    await Mainloop(ct).ConfigureAwait(false);
                    failures = 0; // if Mainloop returns normally, reset
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    // expected shutdown
                    return;
                }
                catch (ObjectDisposedException)
                {
                    // likely disconnect/dispose
                    return;
                }
                catch (Exception ex)
                {
                    failures++;
                    messageTarget.ShowException(ex);

                    // Backoff to avoid a tight restart loop
                    var delayMs = failures switch
                    {
                        <= 3 => 100,
                        <= 10 => 500,
                        _ => 1000
                    };

                    try
                    {
                        await Task.Delay(delayMs, ct).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) when (ct.IsCancellationRequested)
                    {
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// The main loop of procesing incoming packets
        /// </summary>
        private async Task Mainloop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && serialStream != null)
            {
                // Wait for data to be available
                await serialStream.WaitForDataAvailable(ct);

                if (mainLoopGate.IsSuspended)
                {
                    await mainLoopGate.WaitUntilResumedAsync(ct);
                    continue;
                }

                // If data is available and not processing a command, check incoming packet
                var data = await serialStream.ReadByteAsync(ct).ConfigureAwait(false);
                // If sync then syn back
                if (data == Ascii.SYN) serialStream.WriteByte(Ascii.SYN);
                // Processing incoming packet
                if (data == Ascii.SOH) await ProcessIncomingCommand(ct).ConfigureAwait(false); ;
                serialStream.WriteByte(Ascii.NAK);
                serialStream.WriteByte((byte)ErrorCode.Unexpected);
            }
        }

        /// <summary>
        /// Cancels the current operation
        /// </summary>
        public void Cancel()
        {
            serialStream?.WriteByte(Ascii.CAN);
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = null;
            OnPropertyChanged(nameof(CanCancel));
        }

        /// <summary>
        /// Initializes the Arduino 
        /// </summary>
        private async Task Initialize()
        {
            if (serialStream == null) throw new ArduinoException("Arduino is not connected");

            // Suspend the main loop
            using var _ = mainLoopGate.Suspend();

            // Empty the read buffer
            serialStream.ClearReceiveBuffer();

            // Try synchronizing
            if (!await Synchronize().ConfigureAwait(false)) throw new Exception("Initialize failed.");

            serialStream.WriteByte(Ascii.SOH);
            serialStream.WriteByte((byte)Command.Init);
            
            // Read the header
            await serialStream.ExpectByteAsync(Ascii.SOH, 2500).ConfigureAwait(false);
            int versionHigh = await serialStream.ReadByteAsync(1000).ConfigureAwait(false);
            int versionLow = await serialStream.ReadByteAsync(1000).ConfigureAwait(false);
            if (versionHigh != VersionHigh || versionLow != VersionLow)
            {
                throw new DataException($"Unexpected Arduino version (Expected {VersionHigh}.{VersionLow} but received {versionHigh}.{versionLow}");
            }
            int bufferSize = await serialStream.ReadByteAsync(1000).ConfigureAwait(false);
            if (bufferSize < 16) throw new DataException($"Received buffer size of '{bufferSize}' is too small.");

            // Read the text stream from the Arduino
            await serialStream.ExpectByteAsync(Ascii.STX, 1000).ConfigureAwait(false);
            while (true)
            {
                byte value = await serialStream.ReadByteAsync(1000).ConfigureAwait(false);
                if (value == Ascii.ETX) break;
                messageTarget.Write(char.ConvertFromUtf32(value));
            }
        }

        /// <summary>
        /// Pings the arduino
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Cannot test if not connected</exception>
        public async Task Ping()
        {
            if (serialStream == null) throw new ArduinoException("Arduino is not connected");

            using var _ = mainLoopGate.Suspend();

            // Empty the read buffer
            messageTarget.DebugWriteLine("Clearing stream...");
            serialStream.ClearReceiveBuffer();

            // Try synchronizing
            messageTarget.DebugWriteLine("Synchronizing...");
            if (!await Synchronize().ConfigureAwait(false)) messageTarget.WriteLine("Synchronization failed.");

            messageTarget.Write("Pinging... ");
            serialStream.WriteByte(Ascii.SOH);
            serialStream.WriteByte((byte)Command.Ping);
            var response = await serialStream.TryReadByteAsync(2500).ConfigureAwait(false); ;       // Wait for response
            if (response == Ascii.ACK)
            {
                messageTarget.WriteLine("Success.");
            }
            else if (response == Ascii.NAK)
            {
                response = await serialStream.TryReadByteAsync(2000).ConfigureAwait(false); ;
                var errorCode = ErrorCode.Timeout;
                if (response.HasValue) errorCode = (ErrorCode)response.Value;
                messageTarget.WriteLine($"Ping Failure.  Error {errorCode}");
            }
            else
            {
                messageTarget.WriteLine($"No response.");
            }
        }

        /// <summary>
        /// Sends the tape file.
        /// </summary>
        /// <param name="fileStream">The file stream.</param>
        /// <exception cref="System.InvalidOperationException">Cannot send file if not connected</exception>
        /// <exception cref="System.Exception">Unable to start file transfer</exception>
        public async Task SendTapeFile(Stream fileStream)
        {
            if (serialStream == null) throw new ArduinoException("Arduino is not connected");

            // TODO improve file parsing
            using var memoryStream = new MemoryStream();
            fileStream.CopyTo(memoryStream);
            var data = ProcessTapeFile(memoryStream.ToArray());

            using var _ = mainLoopGate.Suspend();

            // Empty the read buffer
            messageTarget.DebugWriteLine("Clearing stream... ");
            serialStream.ClearReceiveBuffer();

            // Send Syn character and wait for syn
            messageTarget.DebugWriteLine("Synchronizing...");
            if (!await Synchronize().ConfigureAwait(false)) throw new ArduinoException("Unable to start file transfer");

            messageTarget.WriteLine($"Sending type file; Length: {fileStream.Length}");

            serialStream.WriteByte(Ascii.SOH);    // Start of packet 
            serialStream.WriteByte((byte)Command.LoadTape);
            serialStream.WriteWord((ushort)fileStream.Length);
            serialStream.WriteByte(HeaderSize);
            await ReadResponse().ConfigureAwait(false);
            await SendBuffer(data).ConfigureAwait(false);
            messageTarget.WriteLine($"Done.");
        }

        /// <summary>
        /// Reads the tape file.
        /// </summary>
        /// <returns></returns>
        public async Task<byte[]> ReadTapeFile()
        {
            if (serialStream == null) throw new ArduinoException("Arduino is not connected");
            using var _ = mainLoopGate.Suspend();

            // Empty the read buffer
            messageTarget.DebugWriteLine("Clearing stream... ");
            serialStream.ClearReceiveBuffer();

            // Send Syn character and wait for syn
            messageTarget.DebugWriteLine("Synchronizing...");
            if (!await Synchronize().ConfigureAwait(false)) throw new ArduinoException("Unable to start file transfer");

            messageTarget.WriteLine($"Waiting for CSAVE on pocket computer...");
            serialStream.WriteByte(Ascii.SOH);    // Start of packet 
            serialStream.WriteByte((byte)Command.SaveTape);
            await ReadResponse().ConfigureAwait(false);

            try
            {
                cancellationTokenSource = new();
                OnPropertyChanged(nameof(CanCancel));
                return ProcessTapeFile(await ReadFrame(cancellationTokenSource.Token).ConfigureAwait(false));
            }
            finally
            {
                cancellationTokenSource = null;
                OnPropertyChanged(nameof(CanCancel));
            }
        }

        /// <summary>
        /// Reads and parses the response.
        /// </summary>
        /// <param name="timeout">ACK Timeout value</param>
        /// <exception cref="System.InvalidOperationException">Not Connected</exception>
        /// <exception cref="System.Exception">Transmission Error {errorCode}</exception>
        private async Task ReadResponse(int timeout = ReadTimeout, CancellationToken ct = default)
        {
            if (serialStream == null) throw new ArduinoException("Arduino is not connected");
            var response = await serialStream.ReadByteAsync(timeout, ct).ConfigureAwait(false);    // Wait for response
            if (response == Ascii.ACK) return;
            if (response == Ascii.NAK) throw new ArduinoException(await serialStream.ReadByteAsync(1000, ct).ConfigureAwait(false));
            throw new ArduinoException($"Unexpected response received 0x{response:X2}");
        }

        /// <summary>
        /// Synchronizes the serial connection
        /// </summary>
        /// <param name="byteStream">The byte stream.</param>
        /// <returns></returns>
        private async Task<bool> Synchronize()
        {
            if (serialStream == null) throw new ArduinoException("Arduino is not connected");

            int tryCount = 0;
            while (true)
            {
                serialStream.WriteByte(Ascii.SYN);
                var response = await serialStream.TryReadByteAsync(1000).ConfigureAwait(false);       // Wait one second for response
                if (response == Ascii.SYN) break;
                if (response == Ascii.NAK)
                {
                    // Ignore error code
                    await serialStream.TryReadByteAsync(1000).ConfigureAwait(false);
                    continue;
                }
                tryCount++;
                if (tryCount > 10) return false;
            }
            return true;
        }

        /// <summary>
        /// Processes the incoming packet.
        /// </summary>
        private async Task ProcessIncomingCommand(CancellationToken ct)
        {
            if (serialStream == null) throw new ArduinoException("Arduino is not connected");
            var command = await serialStream.TryReadByteAsync(1000, ct).ConfigureAwait(false);
            if (!command.HasValue) return;
            switch ((Command)command.Value)
            {
                case Command.Ping:
                    serialStream.WriteByte(Ascii.ACK);
                    break;
                case Command.DeviceSelect:
                    var device = await serialStream.TryReadByteAsync(1000, ct).ConfigureAwait(false);
                    if (!device.HasValue) return;
                    messageTarget.DebugWriteLine($"Device Select: 0x{device.Value:X}");
                    break;
                case Command.Print:
                    var character = await serialStream.TryReadByteAsync(1000, ct).ConfigureAwait(false);
                    if (!character.HasValue) return;
                    if (character.Value == 13) messageTarget.WriteLine();
                    else messageTarget.Write(((char)character.Value).ToString());
                    break;
                case Command.Data:
                    var value = await serialStream.TryReadByteAsync(1000, ct).ConfigureAwait(false);
                    if (!value.HasValue) return;
                    messageTarget.WriteLine($"Data: {value:X2}");
                    break;
                case Command.Disk:
                    messageTarget.DebugWriteLine("Reading Disk Command:");
                    var response = diskDrive.ProcessCommand(await ReadDiskCommand(ct));
                    await SendDiskResponse(response, ct);
                    break;
                default:
                    return;
            }
        }

        /// <summary>
        /// Reads the disk command.
        /// </summary>
        /// <returns></returns>
        private async Task<byte[]> ReadDiskCommand(CancellationToken ct)
        {
            return await ReadFrame(ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends the disk packet.
        /// </summary>
        /// <param name="response">The data.</param>
        /// <exception cref="SharpManager.ArduinoException">Arduino is not connected</exception>
        private async Task SendDiskResponse(DiskResponse response, CancellationToken ct)
        {
            if (serialStream == null) throw new ArduinoException("Arduino is not connected");
            serialStream.WriteByte(Ascii.SOH);
            serialStream.WriteByte((byte)Command.Disk);
            serialStream.WriteByte(response.Capture ? 0xFF : 0);
            serialStream.WriteWord(response.Data.Length);
            await ReadResponse(ct: ct).ConfigureAwait(false);     // Wait for acknowledge
            await SendBuffer(response.Data, ct: ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Reads a data frame.
        /// </summary>
        /// <returns>Byte array of data</returns>
        private async Task<byte[]> ReadFrame(CancellationToken ct = default)
        {
            if (serialStream == null) throw new ArduinoException("Arduino is not connected");

            // Wait for start value
            var startValue = await serialStream.ReadByteAsync(ct).ConfigureAwait(false);
            if (startValue == Ascii.NAK)
            {
                throw new ArduinoException(await serialStream.ReadByteAsync(2000, ct).ConfigureAwait(false));
            }
            else if (startValue != Ascii.STX)
            {
                throw new ArduinoException($"Expecting transmisson start (STX) received 0x{startValue:X} instead.");
            }

            // The data result
            var result = new List<byte>();

            while (true)
            {
                var data = await serialStream.ReadByteAsync(1000, ct).ConfigureAwait(false);
                switch (data)
                {
                    case Ascii.DLE:
                        data = await serialStream.ReadByteAsync(1000, ct).ConfigureAwait(false);
                        break;
                    case Ascii.NAK:
                        throw new ArduinoException(await serialStream.ReadByteAsync(1000, ct).ConfigureAwait(false));
                    case Ascii.CAN:
                        throw new ArduinoException(ErrorCode.Cancelled);
                    case Ascii.ETX:
                        if (result.Count % 40 != 0) messageTarget.WriteLine();
                        messageTarget.Dump(result);
                        return result.ToArray();
                }
                result.Add(data);
                messageTarget.Write(".");
                // messageTarget.Write(" " + data.ToString("X2"));
                if (result.Count % 80 == 0) messageTarget.WriteLine();
            }
        }

        /// <summary>
        /// Sends the buffer by breaking into BufferSize sized groups
        /// </summary>
        /// <param name="data">The data.</param>
        /// <exception cref="SharpManager.ArduinoException">Arduino is not connected</exception>
        private async Task SendBuffer(byte[] data, int timeout = ReadTimeout, CancellationToken ct = default)
        {
            if (serialStream == null) throw new ArduinoException("Arduino is not connected");
            int offset = 0;
            while (true)
            {
                int size = Math.Min(BufferSize, data.Length - offset);
                if (size == 0) break;
                messageTarget.DebugWriteLine($"Sending {size} bytes:");
                messageTarget.Dump(new ArraySegment<byte>(data, offset, size));
                for (int i = 0; i < size; i++) serialStream.WriteByte(data[offset++]);
                await ReadResponse(timeout, ct).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Processes the tape file by inverting nibbles as needed
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private byte[] ProcessTapeFile(byte[] data)
        {
            if (data.Length < 8) return data;
            // Get the file format of the file
            var fileFormat = (FileFormat)data[0];
            // Swap the nibbles of bytes 1 through 7
            for (int i = 1; i <= 7; i++) data[i] = data[i].SwapNibbles();
            // If password also swap the password bytes
            if (fileFormat == FileFormat.BasicPassword || fileFormat == FileFormat.ExtBasicPassword)
            {
                if (data.Length < 18) return data;
                // Swap the nibbles of bytes 10 through 17
                for (int i = 10; i <= 17; i++) data[i] = data[i].SwapNibbles();
            }
            return data;
        }

        /// <summary>
        /// The disposed value
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Disconnect();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposed = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
