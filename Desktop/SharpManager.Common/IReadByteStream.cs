using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SharpManager
{
    /// <summary>
    /// Read stream interface
    /// </summary>
    public interface IReadByteStream
    {
        /// <summary>
        /// Waits for data to be available.
        /// </summary>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task WaitForDataAvailable(CancellationToken ct = default);

        /// <summary>
        /// Reads a byte from the stream
        /// </summary>
        Task<byte> ReadByteAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Clears the receive buffer.
        /// </summary>
        void ClearReceiveBuffer();
    }
}
