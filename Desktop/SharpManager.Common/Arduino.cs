using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.IO.Ports;
using System.Linq;
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
        Unknown = 0,
        Timeout = 1,
        InvalidData = 2,
        Cancelled = 3,
        Unexpected = 4
    }

    /// <summary>
    /// The packet types
    /// </summary>
    public enum Command
    {
        Ping = 1,
        DeviceSelect = 2,
        LoadTape = 3,
        Print = 4,
        SaveTape = 5
    }

    public class Arduino : NotifyObject, IDisposable
    {
        /// <summary>The serial port</summary>
        private SerialPort? serialPort = null;

        /// <summary>The serial stream</summary>
        private SerialPortByteStream? serialStream = null;

        /// <summary>Cancel the current operation</summary>
        CancellationTokenSource? cancellationTokenSource = null;

        /// <summary>The message log</summary>
        private IMessageLog messageLog;

        /// <summary>Whether or not current processing a command</summary>
        private int commandCount = 0;

        /// <summary>The arduino buffer size</summary>
        private const int BufferSize = 64;

        /// <summary>The file header size</summary>
        private const int HeaderSize = 10;

        /// <summary>
        /// Gets a value indicating whether this instance is connected.
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Arduino" /> class.
        /// </summary>
        /// <param name="messageLog">The message log.</param>
        public Arduino(IMessageLog messageLog)
        {
            this.messageLog = messageLog;
        }

        /// <summary>
        /// Connects the specified port name.
        /// </summary>
        /// <param name="portName">Name of the port.</param>
        /// <returns></returns>
        public void Connect(string portName)
        {
            cancellationTokenSource = new();
            serialPort = new SerialPort(portName, 115200);
            serialPort.DtrEnable = false;
            serialPort.Open();
            serialStream = new SerialPortByteStream(serialPort);
            IsConnected = true;
            OnPropertyChanged(nameof(IsConnected));

            // Begin the main loop
            _ = Task.Run(async () => { await Mainloop(); });
        }

        /// <summary>
        /// Disconnects this instance.
        /// </summary>
        public void Disconnect()
        {
            serialStream?.Dispose();
            serialStream = null;
            serialPort?.Close();
            serialPort?.Dispose();
            serialPort = null;

            IsConnected = false;
            OnPropertyChanged(nameof(IsConnected));
        }

        /// <summary>
        /// The main loop of procesing incoming packets
        /// </summary>
        private async Task Mainloop()
        {
            while (serialStream != null)
            {
                // If data is available and not processing a command, check incoming packet
                if (commandCount == 0 && serialStream.DataAvailable)
                {
                    var data = serialStream.ReadByte();
                    // If sync then ack
                    if (data == Ascii.SYN) serialStream.WriteByte(Ascii.SYN);
                    if (data == Ascii.SOH) await ProcessPacket();
                    serialStream.WriteByte(Ascii.NAK);
                    serialStream.WriteByte((byte)ErrorCode.Unexpected);
                }
                await Task.Yield();
            }
        }

        /// <summary>
        /// Pings the arduino
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Cannot test if not connected</exception>
        public async Task Ping()
        {
            if (serialStream == null) throw new InvalidOperationException("Cannot test if not connected");

            using var _ = StartCommand();

            messageLog.WriteLine("Clearing stream...");

            // Empty the read buffer
            while (serialStream.DataAvailable) await serialStream.ReadByteAsync().ConfigureAwait(false);

            messageLog.WriteLine("Synchronizing...");

            // Try synchronizing
            if (!await Synchronize().ConfigureAwait(false))
            {
                messageLog.WriteLine("Synchronize failed.");
            }

            messageLog.WriteLine("Ping...");
            serialStream.WriteByte(Ascii.SOH);
            serialStream.WriteByte((byte)Command.Ping);
            var response = await serialStream.TryReadByteAsync(2500);       // Wait for response
            if (response == Ascii.ACK)
            {
                messageLog.WriteLine("Success.\r\n");
            }
            else if (response == Ascii.NAK)
            {
                response = await serialStream.TryReadByteAsync(2000);
                var errorCode = ErrorCode.Unknown;
                if (response.HasValue) errorCode = (ErrorCode)response.Value;
                messageLog.WriteLine($"Fail: {errorCode}");
            }
            else
            {
                messageLog.WriteLine($"No response.\r\n");
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
            if (serialStream == null) throw new InvalidOperationException("Cannot send file if not connected");

            using var _ = StartCommand();

            // Empty the read buffer
            messageLog.WriteLine("Clearing stream...");
            while (serialStream.DataAvailable) await serialStream.ReadByteAsync().ConfigureAwait(false);

            // Send Syn character and wait for syn
            messageLog.WriteLine("Synchronizing...");
            if (!await Synchronize()) throw new Exception("Unable to start file transfer");

            var packet = new byte[64];
            messageLog.WriteLine($"Start new tape...  Length: {fileStream.Length}");

            serialStream.WriteByte(Ascii.SOH);    // Start of packet 
            serialStream.WriteByte((byte)Command.LoadTape);
            serialStream.WriteWord((ushort)fileStream.Length);
            serialStream.WriteByte(HeaderSize);
            await ReadResponse();

            while (true)
            {
                int packetSize = fileStream.Read(packet, 0, packet.Length);
                if (packetSize == 0) break;
                messageLog.WriteLine($"Sendng {packetSize} bytes...");
                for (int i = 0; i < packetSize; i++) serialStream.WriteByte(packet[i]);
                await ReadResponse();
            }

            messageLog.WriteLine($"Done.");
        }


        public async Task<byte[]> ReadTapeFile()
        {
            if (serialStream == null) throw new InvalidOperationException("Cannot send file if not connected");
            using var _ = StartCommand();

            // Empty the read buffer
            messageLog.WriteLine("Clearing stream...");
            while (serialStream.DataAvailable) await serialStream.ReadByteAsync().ConfigureAwait(false);

            // Send Syn character and wait for syn
            messageLog.WriteLine("Synchronizing...");
            if (!await Synchronize()) throw new Exception("Unable to start file transfer");

            messageLog.WriteLine($"Waiting for CSAVE...");

            serialStream.WriteByte(Ascii.SOH);    // Start of packet 
            serialStream.WriteByte((byte)Command.SaveTape);
            await ReadResponse();



        }

        /// <summary>
        /// Reads and parses the response.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Not Connected</exception>
        /// <exception cref="System.Exception">Transmission Error {errorCode}</exception>
        private async Task ReadResponse()
        {
            if (serialStream == null) throw new InvalidOperationException("Not Connected");
            var response = await serialStream.TryReadByteAsync(2500);    // Wait for response
            if (response == Ascii.ACK) return;
            response = await serialStream.TryReadByteAsync(2000);         // Wait for error code
            var errorCode = ErrorCode.Unknown;
            if (response.HasValue) errorCode = (ErrorCode)response.Value;
            throw new Exception($"Transmission Error {errorCode}");
        }


        /// <summary>
        /// Synchronizes the serial connection
        /// </summary>
        /// <param name="byteStream">The byte stream.</param>
        /// <returns></returns>
        private async Task<bool> Synchronize()
        {
            if (serialStream == null) throw new InvalidOperationException("Not Connected");

            int tryCount = 0;
            while (true)
            {
                serialStream.WriteByte(Ascii.SYN);
                var startAck = await serialStream.TryReadByteAsync(1000).ConfigureAwait(false);       // Wait one second for response
                if (startAck == Ascii.SYN) break;
                if (startAck == Ascii.NAK)
                {
                    // Ignore error code
                    await serialStream.TryReadByteAsync(1000).ConfigureAwait(false);
                    continue;
                }
                tryCount++;
                if (tryCount > 5) return false;
            }
            return true;
        }

        /// <summary>
        /// Processes the incoming packet.
        /// </summary>
        private async Task ProcessPacket()
        {
            if (serialStream == null) throw new InvalidOperationException("Not Connected");
            var command = await serialStream.TryReadByteAsync(1000).ConfigureAwait(false);
            if (!command.HasValue) return;
            switch ((Command)command.Value)
            {
                case Command.Ping:
                    serialStream.WriteByte(Ascii.ACK);
                    break;
                case Command.DeviceSelect:
                    var device = await serialStream.TryReadByteAsync(1000).ConfigureAwait(false);
                    if (!device.HasValue) return;
                    messageLog.WriteLine($"Device Select: 0x{device.Value:X}");
                    break;
                case Command.Print:
                    var character = await serialStream.TryReadByteAsync(1000).ConfigureAwait(false);
                    if (!character.HasValue) return;
                    if (character.Value == 13) messageLog.WriteLine();
                    else messageLog.Write(((char)character.Value).ToString());
                    break;
                default:
                    return;
            }
        }


        /// <summary>
        /// The disposed value
        /// </summary>
        private bool disposedValue;

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Disconnect();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
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

        /// <summary>
        /// Starts the command.
        /// </summary>
        internal ScopeGuard StartCommand()
        {
            commandCount++;
            return new ScopeGuard(() => { if (commandCount > 0) commandCount--; });
        }
    }
}
