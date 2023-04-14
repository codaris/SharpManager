using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpManager
{
    internal class ChecksumReadByteStream : IReadByteStream
    {
        /// <summary>The CRC16 checksum </summary>
        private ushort checksum = Checksum.InitialCRC16;

        /// <summary>The byte stream</summary>
        private readonly IReadByteStream byteStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChecksumReadByteStream"/> class.
        /// </summary>
        /// <param name="byteStream">The byte stream.</param>
        public ChecksumReadByteStream(IReadByteStream byteStream)
        {
            this.byteStream = byteStream;
        }

        /// <summary>Gets a value indicating whether data available.</summary>
        public bool DataAvailable => byteStream.DataAvailable;

        /// <summary>
        /// Reads a byte from the stream
        /// </summary>
        /// <returns></returns>
        public async Task<byte> ReadByteAsync()
        {
            byte value = await byteStream.ReadByteAsync();
            checksum = Checksum.ComputeNext(checksum, value);
            return value;
        }

        /// <summary>
        /// Reads the checksum word and compares it with the current checksum
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ReadChecksumAsync()
        {
            ushort value = await byteStream.ReadWordAsync();
            return value == checksum;
        }
    }
}
