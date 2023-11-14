using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpManager
{
    public class SerialPortByteStream : IByteStream, IDisposable
    {
        /// <summary>The serial port to wrap</summary>
        private readonly SerialPort serialPort;

        /// <summary>The send byte array</summary>
        private readonly byte[] sendByteArray = new byte[1];

        /// <summary>The read task completion source</summary>
        private TaskCompletionSource<bool>? readTaskCompletionSource = null;

        /// <summary>The disposed value</summary>
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerialPortByteStream"/> class.
        /// </summary>
        /// <param name="serialPort">The serial port.</param>
        public SerialPortByteStream(SerialPort serialPort)
        {
            this.serialPort = serialPort;
            serialPort.DataReceived += SerialPort_DataReceived;
            serialPort.ErrorReceived += SerialPort_ErrorReceived;
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
            lock (serialPort)
            {
                readTaskCompletionSource?.TrySetResult(true);
            }
        }

        /// <summary>
        /// Gets a value indicating whether data available.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [data available]; otherwise, <c>false</c>.
        /// </value>
        public bool DataAvailable => serialPort.BytesToRead > 0;

        /// <summary>
        /// Reads the byte asynchronously.
        /// </summary>
        /// <returns></returns>
        public async Task<byte> ReadByteAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                lock (serialPort)
                {
                    if (serialPort.BytesToRead > 0) return (byte)serialPort.ReadByte();
                    readTaskCompletionSource = new TaskCompletionSource<bool>();
                }
                await readTaskCompletionSource.Task;
            }
        }

        /// <summary>
        /// Writes the byte.
        /// </summary>
        /// <param name="value">The value.</param>
        public void WriteByte(byte value)
        {
            sendByteArray[0] = value;
            serialPort.Write(sendByteArray, 0, 1);
        }

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
                    serialPort.DataReceived -= SerialPort_DataReceived;
                    serialPort.ErrorReceived -= SerialPort_ErrorReceived;
                }
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
