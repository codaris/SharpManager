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

        /// <summary>The list of Data available waiters</summary>
        private readonly List<TaskCompletionSource<bool>> waiters = new();

        /// <summary>The synchronization object for this class</summary>
        private readonly object syncRoot = new();

        /// <summary>The data available flag</summary>
        private bool? dataAvailable;

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
            throw new DataException($"Serial port error: {e.EventType}");
        }

        /// <summary>
        /// Handles the DataReceived event of the SerialPort control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SerialDataReceivedEventArgs"/> instance containing the event data.</param>
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Temporary list of waiters copied from inside the lock below
            List<TaskCompletionSource<bool>> localWaiters;

            lock (syncRoot)
            {
                dataAvailable = true;
                localWaiters = new List<TaskCompletionSource<bool>>(waiters);
                waiters.Clear();
            }

            // Complete waiters *outside* the lock to keep the critical section small.
            foreach (var waiter in localWaiters) waiter.TrySetResult(true);
        }

        /// <summary>
        /// Gets a value indicating whether data is available.
        /// </summary>
        public bool DataAvailable => dataAvailable ??= serialPort.BytesToRead > 0;

        /// <summary>
        /// Waits for data available.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public Task WaitForDataAvailable(CancellationToken cancellationToken = default)
        {
            // If data is available, return immediately
            if (DataAvailable) return Task.CompletedTask;

            // One TCS per waiter so each caller gets its own cancellation.
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            // We need the registration variable *outside* so we can dispose it later.
            CancellationTokenRegistration ctr = default;

            lock (syncRoot)
            {
                // Data might have become available while we were allocating the TCS.
                if (DataAvailable) return Task.CompletedTask;

                waiters.Add(tcs);

                // Set up cancellation *after* we’re sure the waiter is in the list.
                if (cancellationToken.CanBeCanceled)
                {
                    ctr = cancellationToken.Register(() =>
                    {
                        bool removed;
                        lock (syncRoot)
                        {
                            removed = waiters.Remove(tcs);
                        }
                        // Only cancel if we really owned the waiter.
                        if (removed) tcs.TrySetCanceled(cancellationToken);
                    });
                }
            }

            // Dispose the registration once the task completes in *any* way.
            if (ctr.Token.CanBeCanceled)
            {
                tcs.Task.ContinueWith(
                    _ => ctr.Dispose(),
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
            }

            return tcs.Task;
        }

        /// <summary>
        /// Reads the byte asynchronously.
        /// </summary>
        /// <returns></returns>
        public async Task<byte> ReadByteAsync(CancellationToken cancellationToken)
        {
            // Wait for byte to be available
            await WaitForDataAvailable(cancellationToken).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            lock (syncRoot)
            {
                var result = (byte)serialPort.ReadByte();
                if (serialPort.BytesToRead == 0) dataAvailable = false;
                return result;
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
