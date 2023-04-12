using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SharpManager
{
    internal static class SerialExtensions
    {
        /// <summary>
        /// Reads the asynchronous.
        /// </summary>
        /// <param name="serialPort">The serial port.</param>
        /// <param name="buffer">The memory.</param>
        /// <returns></returns>
        public static async Task<int> ReadAsync(this SerialPort serialPort, Memory<byte> buffer)
        {
            return await serialPort.BaseStream.ReadAsync(buffer);
        }

        /// <summary>
        /// Reads the asynchronous.
        /// </summary>
        /// <param name="serialPort">The serial port.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.OperationCanceledException">ReadSerialAsync operation Cancelled.</exception>
        public static async Task<int> ReadAsync(this SerialPort serialPort, Memory<byte> buffer, CancellationToken cancellationToken)
        {
            // Handle concellation
            using CancellationTokenRegistration tokenRegistration = cancellationToken.Register(param =>
            {
                ((SerialPort?)param)?.DiscardInBuffer();
            }, serialPort);


            try
            {
                return await serialPort.BaseStream.ReadAsync(buffer, cancellationToken);
            }
            catch (System.IO.IOException Ex)
            {
                throw new OperationCanceledException("ReadSerialAsync operation Cancelled.", Ex);
            }
        }


        /// <summary>
        /// Reads the asynchronous.
        /// </summary>
        /// <param name="serialPort">The serial port.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<int> ReadAsync(this SerialPort serialPort, Memory<byte> buffer, TimeSpan timeout, CancellationToken cancellationToken)
        {
            using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var task = serialPort.ReadAsync(buffer, linkedTokenSource.Token);

            if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
            {
                return await task;
            }
            else
            {
                linkedTokenSource.Cancel();
                return 0;
            }
        }

        /// <summary>
        /// Reads the byte asynchronous.
        /// </summary>
        /// <param name="serialPort">The serial port.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<int> ReadByteAsync(this SerialPort serialPort, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var buffer = new byte[1];
            var count = await serialPort.ReadAsync(buffer.AsMemory(), timeout, cancellationToken);
            if (count == 0) return -1;
            return buffer[0];
        }

        /// <summary>
        /// Writes the byte.
        /// </summary>
        /// <param name="serial">The serial.</param>
        /// <param name="value">The value.</param>
        public static void WriteByte(this SerialPort serial, byte value)
        {
            var buffer = new byte[1] { value };
            serial.Write(buffer, 0, 1);
        }

        /// <summary>
        /// Writes the byte asynchronous.
        /// </summary>
        /// <param name="serial">The serial.</param>
        /// <param name="value">The value.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static Task WriteByteAsync(this SerialPort serial, byte value, CancellationToken cancellationToken)
        {
            var buffer = new byte[1] { value };
            return WriteAsync(serial, buffer.AsMemory(), cancellationToken);
        }

        /// <summary>
        /// Writes the asynchronous.
        /// </summary>
        /// <param name="serial">The serial.</param>
        /// <param name="buffer">The buffer.</param>
        public static async Task WriteAsync(this SerialPort serial, Memory<byte> buffer)
        {
            await serial.BaseStream.WriteAsync(buffer);
        }

        /// <summary>
        /// Writes the serial asynchronous.
        /// </summary>
        /// <param name="serial">The serial.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="System.OperationCanceledException">WriteSerialAsync operation Cancelled.</exception>
        public static async Task WriteAsync(this SerialPort serial, Memory<byte> buffer, CancellationToken cancellationToken)
        {
            using CancellationTokenRegistration tokenRegistration = cancellationToken.Register(param =>
            {
                ((SerialPort?)param)?.DiscardOutBuffer();
            }, serial);

            try
            {
                await serial.BaseStream.WriteAsync(buffer, cancellationToken);
            }
            catch (System.IO.IOException Ex)
            {
                throw new OperationCanceledException("WriteSerialAsync operation Cancelled.", Ex);
            }
        }
    }
}
