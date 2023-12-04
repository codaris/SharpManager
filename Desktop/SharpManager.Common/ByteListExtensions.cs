using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpManager
{
    public static class ByteListExtensions
    {
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
    }
}
