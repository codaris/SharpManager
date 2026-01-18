using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SharpManager
{
    public class SerialPortByteStream : IByteStream, IDisposable
    {
        /// <summary>The serial port to wrap</summary>
        private readonly SerialPort serialPort;

        /// <summary>The send byte array</summary>
        private readonly byte[] oneByte = new byte[1];

        /// <summary>The synchronization object for this class</summary>
        private readonly object syncRoot = new();

        // Receive channel
        private readonly Channel<byte> rx;

        // Terminal error
        private Exception? terminalError;

        /// <summary>The disposed value</summary>
        private bool disposed;

        private Action<byte>? byteLog = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerialPortByteStream"/> class.
        /// </summary>
        /// <param name="serialPort">The serial port.</param>
        public SerialPortByteStream(SerialPort serialPort, Action<byte>? byteLog = null)
        {
            this.serialPort = serialPort;
            this.byteLog = byteLog;

            rx = Channel.CreateUnbounded<byte>(new UnboundedChannelOptions
            {
                SingleWriter = true,
                SingleReader = false,
                AllowSynchronousContinuations = false
            });

            serialPort.DataReceived += SerialPort_DataReceived;
            serialPort.ErrorReceived += SerialPort_ErrorReceived;
        }

        /// <summary>
        /// Waits for data available.
        /// </summary>
        /// <param name="ct">The cancellation token.</param>
        public async Task WaitForDataAvailable(CancellationToken ct = default)
        {
            ThrowIfFaultedOrDisposed();

            try
            {
                // Wait for data to be available and return if so
                if (await rx.Reader.WaitToReadAsync(ct).ConfigureAwait(false)) return;

                // Completed (possibly faulted) channel: propagate
                await rx.Reader.Completion.ConfigureAwait(false);
            }
            catch (ChannelClosedException)
            {
                // Normal completion/disconnect path.
                // Prefer cancellation semantics if caller provided a token.
                if (ct.IsCancellationRequested) throw new OperationCanceledException(ct);
                // Otherwise surface a consistent "disconnected" exception.
                throw new ObjectDisposedException(nameof(SerialPortByteStream), "Serial stream closed.");
            }
        }

        /// <summary>
        /// Reads the byte asynchronously.
        /// </summary>
        /// <param name="ct">The cancellation token</param>
        /// <returns></returns>
        public async Task<byte> ReadByteAsync(CancellationToken ct = default)
        {
            ThrowIfFaultedOrDisposed();

            try
            {
                return await rx.Reader.ReadAsync(ct).ConfigureAwait(false);
            }
            catch (ChannelClosedException)
            {
                // Normal completion/disconnect path.
                // Prefer cancellation semantics if caller provided a token.
                if (ct.IsCancellationRequested) throw new OperationCanceledException(ct);
                // Otherwise surface a consistent "disconnected" exception.
                throw new ObjectDisposedException(nameof(SerialPortByteStream), "Serial stream closed.");
            }
        }

        /// <summary>
        /// Clears the receive buffer.
        /// </summary>
        public void ClearReceiveBuffer()
        {
            ThrowIfFaultedOrDisposed();
            while (rx.Reader.TryRead(out _));
        }

        /// <summary>
        /// Writes the byte.
        /// </summary>
        /// <param name="value">The value.</param>
        public void WriteByte(byte value)
        {
            ThrowIfFaultedOrDisposed();

            lock (syncRoot)
            {
                oneByte[0] = value;
                serialPort.Write(oneByte, 0, 1);
            }
        }

        /// <summary>
        /// Handles the ErrorReceived event of the SerialPort control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SerialErrorReceivedEventArgs"/> instance containing the event data.</param>
        private void SerialPort_ErrorReceived(object? sender, SerialErrorReceivedEventArgs e)
            => Fault(new DataException($"Serial port error: {e.EventType}"));

        /// <summary>
        /// Handles the DataReceived event of the SerialPort control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SerialDataReceivedEventArgs"/> instance containing the event data.</param>
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Only continue if data exists
            if (e.EventType != SerialData.Chars) return;
            if (disposed) return;

            try
            {
                lock (syncRoot)
                {
                    int count = serialPort.BytesToRead;
                    if (count <= 0) return;

                    var buffer = new byte[count];
                    int read = serialPort.Read(buffer, 0, count);
                    if (read <= 0) return;

                    for (int i = 0; i < read; i++)
                    {
                        byteLog?.Invoke(buffer[i]);
                        if (!rx.Writer.TryWrite(buffer[i])) return; // completed/faulted
                    }
                }
            }
            catch (Exception ex)
            {
                Fault(ex);
            }
        }

        /// <summary>
        /// Faults with the specified exception.
        /// </summary>
        /// <param name="ex">The exception.</param>
        private void Fault(Exception ex)
        {
            terminalError = ex;
            rx.Writer.TryComplete(ex);
        }

        /// <summary>
        /// Throws if faulted or disposed.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">SerialPortByteStream</exception>
        private void ThrowIfFaultedOrDisposed()
        {
            if (disposed) throw new ObjectDisposedException(nameof(SerialPortByteStream));
            if (terminalError is not null) throw terminalError;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            serialPort.DataReceived -= SerialPort_DataReceived;
            serialPort.ErrorReceived -= SerialPort_ErrorReceived;

            rx.Writer.TryComplete();
        }
    }
}
