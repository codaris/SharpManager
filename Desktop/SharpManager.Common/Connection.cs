using System;
using System.Collections.Generic;
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

    public class Connection : IDisposable
    {
        /// <summary>The serial port</summary>
        SerialPort? serialPort = null;

        SerialPortByteStream? serialPortByteStream = null;

        /// <summary>Cancel the current operation</summary>
        CancellationTokenSource? cancellationTokenSource = null;

        private bool inCommand = false;

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
        public async Task Connect(string portName)
        {
            cancellationTokenSource = new();
            serialPort = new SerialPort(portName, 115200);
            serialPort.DtrEnable = false;
            serialPort.Open();
            serialPortByteStream = new SerialPortByteStream(serialPort);

            // Begin the main loop
            await Mainloop();
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
                    if (data == Ascii.SYN) serialPortByteStream.WriteByte(Ascii.ACK);
                    if (data == Ascii.SOH) await ReadPacket(serialPortByteStream);
                    serialPortByteStream.WriteByte(Ascii.NAK);
                    serialPortByteStream.WriteByte((byte)ErrorCode.Unexpected);
                }
                await Task.Yield();
            }
        }

        private async Task ReadPacket(IReadByteStream stream)
        {
            var packetType = await stream.ReadByteAsync(1000);
            var packetTypeInv = await stream.ReadByteAsync(1000);
            var packetNum = await stream.ReadByteAsync(1000);
            var packetNumInv = await stream.ReadByteAsync(1000);
            var packetSize = await stream.ReadByteAsync(1000);
            var packetSizeInv = await stream.ReadByteAsync(1000);
        }

        public async Task SendFile(Stream dataStream, IByteStream byteStream)
        {
            // Empty the read buffer
            while (byteStream.DataAvailable) await byteStream.ReadByteAsync();

            // Send Syn character and wait for ack
            if (!await Synchronize(byteStream)) throw new Exception("Unable to start file transfer");


        }

        public async Task SendPacket(IWriteByteStream baseStream, byte type, byte num, byte size, byte[] data)
        {
            var stream = new ChecksumWriteByteStream(baseStream);
            baseStream.WriteByte(Ascii.SOH);    // Start of packet (not in checksum)
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
                var startAck = await byteStream.TryReadByteAsync(1000);       // Wait one second for response
                if (startAck == Ascii.SYN) break;
                if (startAck == Ascii.NAK)
                {
                    // Ignore error code
                    await byteStream.TryReadByteAsync(1000);
                    continue;
                }
                tryCount++;
                if (tryCount > 5) return false;
            }
            return true;
        }

        /// <summary>
        /// Handles the ErrorReceived event of the SerialPort control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SerialErrorReceivedEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.Exception">Serial port error: {e.EventType}</exception>
        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            throw new Exception($"Serial port error: {e.EventType}");
        }

        /// <summary>
        /// Handles the DataReceived event of the SerialPort control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SerialDataReceivedEventArgs"/> instance containing the event data.</param>
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // throw new NotImplementedException();
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
    }
}
