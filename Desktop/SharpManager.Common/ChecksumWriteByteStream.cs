using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpManager
{
    internal class ChecksumWriteByteStream : IWriteByteStream
    {
        /// <summary>The CRC16 checksum </summary>
        private ushort checksum = Checksum.InitialCRC16;

        /// <summary>The byte stream</summary>
        private readonly IWriteByteStream byteStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChecksumWriteByteStream"/> class.
        /// </summary>
        /// <param name="byteStream">The byte stream.</param>
        public ChecksumWriteByteStream(IWriteByteStream byteStream)
        {
            this.byteStream = byteStream; 
        }

        /// <summary>
        /// Writes the byte to the stream
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public void WriteByte(byte value)
        {
            byteStream.WriteByte(value);
            checksum = Checksum.ComputeNext(checksum, value);
        }

        /// <summary>
        /// Resets the checksum.
        /// </summary>
        public void ResetChecksum()
        {
            checksum = Checksum.InitialCRC16;
        }

        /// <summary>
        /// Writes the checksum.
        /// </summary>
        public void WriteChecksum()
        {
            byteStream.WriteWord(checksum);
        }
    }
}
