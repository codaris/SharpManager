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
    public enum ErrorCode
    {
        Unknown = 0,
        BadPacketNum = 1,
        BadChecksum = 2,
        Overflow = 3,
        InvalidData = 4,
        Cancelled = 5,
        Unexpected = 6
    }

    public enum PacketType
    {
        Ping = 1,
        StartTape = 2,
        TapeHeaderBlock = 3,
        TapeDataBlock = 4,
        EndType = 5,
        StartTapeStream = 6
    }

    public class Connection : IDisposable
    {
        /// <summary>The serial port</summary>
        SerialPort? serialPort = null;

        SerialPortByteStream? serialPortByteStream = null;

        /// <summary>Cancel the current operation</summary>
        CancellationTokenSource? cancellationTokenSource = null;

        private bool inCommand = false;

        private const int PacketSize = 8;

        private const int HeaderSize = 10;

        /// <summary>
        /// Initializes a new instance of the <see cref="Connection"/> class.
        /// </summary>
        public Connection()
        {
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
            serialPortByteStream = new SerialPortByteStream(serialPort);

            // Begin the main loop
            _ = Task.Run(async () => { await Mainloop(); });
        }

        /// <summary>
        /// Disconnects this instance.
        /// </summary>
        public void Disconnect()
        {
            serialPortByteStream?.Dispose();
            serialPortByteStream = null;
            serialPort?.Close();
            serialPort?.Dispose();
            serialPort = null;
        }

        private async Task Mainloop()
        {
            while (serialPortByteStream != null)
            {
                // If data is available and not processing a command, check incoming packet
                if (!inCommand && serialPortByteStream.DataAvailable)
                {
                    var data = serialPortByteStream.ReadByte();
                    // If sync then ack
                    if (data == Ascii.SYN) serialPortByteStream.WriteByte(Ascii.SYN);
                    //if (data == Ascii.SOH) await ReadPacket(serialPortByteStream);
                    serialPortByteStream.WriteByte(Ascii.NAK);
                    serialPortByteStream.WriteByte((byte)ErrorCode.Unexpected);
                }
                await Task.Yield();
            }
        }

        public async Task Test(Action<string> logger)
        {
            if (serialPortByteStream == null) throw new InvalidOperationException("Cannot test if not connected");

            using var _ = StartCommand();

            logger("Clearing stream...\r\n");

            // Empty the read buffer
            while (serialPortByteStream.DataAvailable) await serialPortByteStream.ReadByteAsync().ConfigureAwait(false);

            logger("Synchronizing...\r\n");

            // Try synchronizing
            if (!await Synchronize(serialPortByteStream).ConfigureAwait(false))
            {
                logger("Synchronize failed.\r\n");
            }

            logger("Ping...\r\n");
            serialPortByteStream.WriteByte(Ascii.SOH);
            serialPortByteStream.WriteByte((byte)PacketType.Ping);
            var response = await serialPortByteStream.TryReadByteAsync(2500);       // Wait for response
            if (response == Ascii.ACK)
            {
                logger("Success.\r\n");
            }
            else if (response == Ascii.NAK)
            {
                response = await serialPortByteStream.TryReadByteAsync(2000);
                //var errorCode = ErrorCode.Unknown;
                //if (response.HasValue) errorCode = (ErrorCode)response.Value;
                logger($"Fail: {response}\r\n");
            }
            else
            {
                logger($"No response.\r\n");
            }
        }

        public async Task<bool> Ping()
        {
            if (serialPortByteStream == null) throw new InvalidOperationException("Cannot send file if not connected");

            try
            {
                // Start processing a command
                inCommand = true;

                // Empty the read buffer
                while (serialPortByteStream.DataAvailable) await serialPortByteStream.ReadByteAsync().ConfigureAwait(false);

                // Send Syn character and wait for ack
                // if (!await Synchronize(serialPortByteStream).ConfigureAwait(false)) throw new Exception("Synchronization failed");

                SendPacket(serialPortByteStream, PacketType.Ping);

                var response = await serialPortByteStream.TryReadByteAsync(2500);       // Wait for response
                if (response == Ascii.ACK) return true;
                if (response == Ascii.NAK)
                {
                    response = await serialPortByteStream.TryReadByteAsync(2000);
                    var errorCode = ErrorCode.Unknown;
                    if (response.HasValue) errorCode = (ErrorCode)response.Value;
                    throw new Exception($"Error {response.Value}");
                }
                return false;
            }
            finally
            {
                inCommand = false;
            }
        }

        public async Task SendFile(Stream fileStream, Action<string> logger)
        {
            if (serialPortByteStream == null) throw new InvalidOperationException("Cannot send file if not connected");

            using var _ = StartCommand();

            // Empty the read buffer
            logger("Clearing stream...\r\n");
            while (serialPortByteStream.DataAvailable) await serialPortByteStream.ReadByteAsync().ConfigureAwait(false);

            // Send Syn character and wait for syn
            logger("Synchronizing...\r\n");
            if (!await Synchronize(serialPortByteStream)) throw new Exception("Unable to start file transfer");

            var packet = new byte[8];
            logger($"Start tape\r\n");
            SendPacket(serialPortByteStream, PacketType.StartTape);
            await ReadResponse(serialPortByteStream);

            int headerBytes = HeaderSize;

            while (true)
            {
                int packetSize = fileStream.Read(packet, 0, (headerBytes > 0) ? Math.Min(headerBytes, packet.Length) : packet.Length);
                if (packetSize == 0) break;
                logger($"Sendng packet {packetSize} {((headerBytes > 0) ? PacketType.TapeHeaderBlock : PacketType.TapeDataBlock)} bytes...\r\n");
                SendPacket(serialPortByteStream, (headerBytes > 0) ? PacketType.TapeHeaderBlock : PacketType.TapeDataBlock, packet, (byte)packetSize);
                headerBytes -= packetSize;
                await ReadResponse(serialPortByteStream);
            }

            logger($"End tape\r\n");
            SendPacket(serialPortByteStream, PacketType.EndType);
            await ReadResponse(serialPortByteStream);

            /*
            try
            {
                // Start processing a command
                inCommand = true;

                // Empty the read buffer
                while (serialPortByteStream.DataAvailable) await serialPortByteStream.ReadByteAsync().ConfigureAwait(false);

                // Send Syn character and wait for ack
                // if (!await Synchronize(serialPortByteStream)) throw new Exception("Unable to start file transfer");

                var packet = new byte[256];

                while (true)
                {
                    int packetSize = fileStream.Read(packet, 0, packet.Length);
                    if (packetSize == 0) break;
                    SendPacket(serialPortByteStream, PacketType.SendFile, packet, (byte)packetSize);

                    var response = await serialPortByteStream.TryReadByteAsync(2500);       // Wait for response

                    if (response == Ascii.ACK)
                    {
                        continue;
                    }

                    if (response == Ascii.NAK)
                    {
                        response = await serialPortByteStream.TryReadByteAsync(2000);
                        var errorCode = ErrorCode.Unknown;
                        if (response.HasValue) errorCode = (ErrorCode)response.Value;
                        throw new Exception($"File transfer error {response.Value}");
                    }
                }
            }
            catch
            {
                // Ensure cancelled
                for (int i = 0; i < 5; i++) serialPortByteStream.WriteByte(Ascii.CAN);
                throw;
            }
            finally
            {
                inCommand = false;
            }
            */
        }

        public async Task SendFileStream(Stream fileStream, Action<string> logger)
        {
            if (serialPortByteStream == null) throw new InvalidOperationException("Cannot send file if not connected");

            using var _ = StartCommand();

            // Empty the read buffer
            logger("Clearing stream...\r\n");
            while (serialPortByteStream.DataAvailable) await serialPortByteStream.ReadByteAsync().ConfigureAwait(false);

            // Send Syn character and wait for syn
            logger("Synchronizing...\r\n");
            if (!await Synchronize(serialPortByteStream)) throw new Exception("Unable to start file transfer");

            var packet = new byte[64];
            logger($"Start new tape...  Length: {fileStream.Length}\r\n");

            serialPortByteStream.WriteByte(Ascii.SOH);    // Start of packet 
            serialPortByteStream.WriteByte((byte)PacketType.StartTapeStream);
            serialPortByteStream.WriteWord((ushort)fileStream.Length);
            serialPortByteStream.WriteByte(HeaderSize);
            await ReadResponse(serialPortByteStream);

            while (true)
            {
                int packetSize = fileStream.Read(packet, 0, packet.Length);
                if (packetSize == 0) break;
                logger($"Sendng packet {packetSize} bytes...\r\n");
                for (int i = 0; i < packetSize; i++) serialPortByteStream.WriteByte(packet[i]);
                await ReadResponse(serialPortByteStream);
            }

            logger($"Done.\r\n");
        }


        private void SendPacket(IWriteByteStream stream, PacketType type)
        {
            stream.WriteByte(Ascii.SOH);    // Start of packet 
            stream.WriteByte((byte)type);
        }

        private void SendPacket(IWriteByteStream stream, PacketType type, byte[] data)
        {
            SendPacket(stream, type, data, (byte)data.Length);
        }

        private void SendPacket(IWriteByteStream stream, PacketType type, byte[] data, byte length)
        {
            stream.WriteByte(Ascii.SOH);    // Start of packet 
            stream.WriteByte((byte)type);
            stream.WriteByte(length);
            for (int i = 0; i < length; i++) stream.WriteByte(data[i]);
        }

        private async Task ReadResponse(IReadByteStream stream)
        {
            var response = await stream.TryReadByteAsync(2500);    // Wait for response
            if (response == Ascii.ACK) return;
            response = await stream.TryReadByteAsync(2000);         // Wait for error code
            var errorCode = ErrorCode.Unknown;
            if (response.HasValue) errorCode = (ErrorCode)response.Value;
            throw new Exception($"Transmission Error {errorCode}");
        }

        /*
        public async Task SendPacket(IWriteByteStream baseStream, byte type, byte num, byte size, byte[] data)
        {
            var stream = new ChecksumWriteByteStream(baseStream);
            baseStream.WriteByte(Ascii.SOH);    // Start of packet 
            stream.WriteByte(type);
            stream.WriteByte(num);
            stream.WriteByte(size);
            if (size > 0)
            {
                baseStream.WriteByte(Ascii.STX);
                for (int i = 0; i < size; i++)
                {
                    switch (data[i])
                    {
                        case Ascii.DLE:
                        case Ascii.SYN:
                        case Ascii.CAN:
                        case Ascii.ETX:
                            baseStream.WriteByte(Ascii.DLE);
                            break;
                    }
                    stream.WriteByte(data[i]);
                }
                baseStream.WriteByte(Ascii.ETX);
            }
            stream.WriteChecksum();
        }
        */

        /// <summary>
        /// Synchronizes the specified byte stream.
        /// </summary>
        /// <param name="byteStream">The byte stream.</param>
        /// <returns></returns>
        private static async Task<bool> Synchronize(IByteStream byteStream)
        {
            int tryCount = 0;
            while (true)
            {
                byteStream.WriteByte(Ascii.SYN);
                var startAck = await byteStream.TryReadByteAsync(1000).ConfigureAwait(false);       // Wait one second for response
                if (startAck == Ascii.SYN) break;
                if (startAck == Ascii.NAK)
                {
                    // Ignore error code
                    await byteStream.TryReadByteAsync(1000).ConfigureAwait(false);
                    continue;
                }
                tryCount++;
                if (tryCount > 5) return false;
            }
            return true;
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
            inCommand = true;
            return new ScopeGuard(() => inCommand = false);
        }
    }
}
