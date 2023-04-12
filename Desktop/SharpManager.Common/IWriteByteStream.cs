using System;
using System.Collections.Generic;
using System.Text;

namespace SharpManager
{
    /// <summary>
    /// Write stream interface
    /// </summary>
    public interface IWriteByteStream
    {
        /// <summary>
        /// Writes the byte to the stream
        /// </summary>
        void WriteByte(byte value);
    }
}
