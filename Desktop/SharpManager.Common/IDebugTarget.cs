using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpManager
{
    public interface IDebugTarget : IMessageTarget
    {
        /// <summary>
        /// Write the specified debug message.
        /// </summary>
        /// <param name="message">The message.</param>
        void DebugWrite(string message);

        /// <summary>
        /// Write the specified debug message with newline.
        /// </summary>
        /// <param name="message">The message.</param>
        void DebugWriteLine(string message) => DebugWrite(message + Environment.NewLine);

        /// <summary>
        /// Write a newline to debug
        /// </summary>
        void DebugWriteLine() => DebugWrite(Environment.NewLine);

        /// <summary>
        /// Dump the specified bytes to debug
        /// </summary>
        /// <param name="data">The data.</param>
        void Dump(IEnumerable<byte> data)
        {
            StringBuilder hex = new StringBuilder(49);      // 16 * 3 - 1 (two chars for byte and one for space, minus one space at the end)
            StringBuilder ascii = new StringBuilder(16);
            int offset = 0;

            foreach (byte b in data)
            {
                if (offset % 16 == 0 && offset > 0)
                {
                    DebugWriteLine($"{hex}  |{ascii}|");
                    hex.Clear();
                    ascii.Clear();
                }

                if (offset % 16 == 0)
                {
                    DebugWrite($"  {offset:X8}  ");
                }

                hex.AppendFormat("{0:X2} ", b);
                ascii.Append((b >= 32 && b <= 126) ? (char)b : '.');
                offset++;
            }

            if (hex.Length > 0)
            {
                DebugWriteLine($"{hex,-48}  |{ascii}|");
            }
        }
    }
}
