using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SharpManager
{
    public static class ByteStreamExtensions
    {
        /// <summary>
        /// Writes the key char
        /// </summary>
        /// <param name="keyChar">The key character.</param>
        public static void WriteKey(this IWriteByteStream stream, char keyChar)
        {
            stream.WriteByte(Convert.ToByte(keyChar));
        }

        /// <summary>
        /// Writes the byte.
        /// </summary>
        /// <param name="value">The value.</param>
        public static void WriteByte(this IWriteByteStream stream, int value)
        {
            stream.WriteByte((byte)(value & 0xFF));
        }

        /// <summary>
        /// Writes the word.
        /// </summary>
        /// <param name="value">The value.</param>
        public static void WriteWord(this IWriteByteStream stream, int value)
        {
            stream.WriteByte(value);
            stream.WriteByte(value >> 8);
        }

        /// <summary>
        /// Reads the word.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static async Task<ushort> ReadWordAsync(this IReadByteStream stream)
        {
            int result = await stream.ReadByteAsync().ConfigureAwait(false);
            result += await stream.ReadByteAsync().ConfigureAwait(false) << 8;
            return (ushort)result;
        }

        /// <summary>
        /// Reads the byte.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public static byte ReadByte(this IReadByteStream stream) => stream.ReadByteAsync().Result;

        /// <summary>
        /// Reads the byte.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="millisecondsTimeout">The milliseconds timeout.</param>
        /// <returns></returns>
        /// <exception cref="TimeoutException">Read operation timed out before completing</exception>
        public static byte ReadByte(this IReadByteStream stream, int millisecondsTimeout)
        {
            var task = stream.ReadByteAsync();
            if (!task.Wait(millisecondsTimeout))
            {
                throw new TimeoutException("Read operation timed out before completing");
            }
            return task.Result;
        }

        /// <summary>
        /// Tries to read the byte.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="millisecondsTimeout">The milliseconds timeout.</param>
        /// <returns></returns>
        public static byte? TryReadByte(this IReadByteStream stream, int millisecondsTimeout)
        {
            var task = stream.ReadByteAsync();
            if (!task.Wait(millisecondsTimeout)) return null;
            return task.Result;
        }

        /// <summary>
        /// Reads the byte asynchronously with timeout
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="millisecondsTimeout">The milliseconds timeout.</param>
        /// <returns></returns>
        /// <exception cref="TimeoutException">Read operation timed out before completing</exception>
        public static async Task<byte> ReadByteAsync(this IReadByteStream stream, int millisecondsTimeout)
        {
            var result = await TryReadByteAsync(stream, millisecondsTimeout).ConfigureAwait(false);
            if (!result.HasValue) throw new TimeoutException("Read operation timed out before completing");
            return result.Value;
        }

        /// <summary>
        /// Tries to read the byte asynchronously with timeout
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="millisecondsTimeout">The milliseconds timeout.</param>
        /// <returns></returns>
        public static async Task<byte?> TryReadByteAsync(this IReadByteStream stream, int millisecondsTimeout)
        {
            var readTask = stream.ReadByteAsync();
            var delayTask = Task.Delay(millisecondsTimeout);
            var task = await Task.WhenAny(readTask, delayTask).ConfigureAwait(false);
            if (task == delayTask) return null;
            return readTask.Result;
        }
    }
}
