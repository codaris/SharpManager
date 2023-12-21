using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpManager
{
    public static class ByteListExtensions
    {
        /// <summary>
        /// Adds the integer to the list as a byte
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="value">The value.</param>
        public static void Add(this List<byte> list, int value)
        {
            list.Add((byte)value);
        }

        /// <summary>
        /// Adds the string toe the byte list
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="value">The value.</param>
        public static void AddString(this List<byte> data, string value)
        {
            foreach (char c in value) data.Add((byte)c);
        }

        /// <summary>
        /// Adds a size to the byte array.  Sizes are 3 bytes long.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="size">The size.</param>
        public static void AddSize(this List<byte> data, int size)
        {
            data.Add((byte)(size & 0xFF));
            data.Add((byte)(size >> 8 & 0xFF));
            data.Add((byte)(size >> 16 & 0xFF));
        }

        /// <summary>
        /// Adds the frame.
        /// </summary>
        /// <param name="data">The data.</param>
        public static void AddFrame(this List<byte> data)
        {
            // First byte is always zero                
            data.Insert(0, 0);
            // Add checksum
            data.AddChecksum(data);
        }

        /// <summary>
        /// Adds the frame.
        /// </summary>
        /// <param name="data">The data.</param>
        public static byte[] ToFrame(this List<byte> data)
        {
            data.AddFrame();
            return data.ToArray();   
        }

        /// <summary>
        /// Adds a block of data
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="block">The block.</param>
        public static void AddBlock(this List<byte> data, IEnumerable<byte> block)
        {
            data.AddRange(block);
            data.AddChecksum(block);
        }

        /// <summary>
        /// Adds the checksum.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="values">The values.</param>
        public static void AddChecksum(this List<byte> data, IEnumerable<byte>? values = null)
        {
            values ??= data;
            // Add checksum
            int checksum = 0;
            foreach (var value in values) checksum = (checksum + value) & 0xFF;
            data.Add((byte)checksum);
        }
    }
}
