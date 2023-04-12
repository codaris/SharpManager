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
        /// Gets a value indicating whether data available.
        /// </summary>
        bool DataAvailable { get; }

        /// <summary>
        /// Reads a byte from the stream
        /// </summary>
        Task<byte> ReadByteAsync();
    }
}
