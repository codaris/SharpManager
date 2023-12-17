using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpManager
{
    public interface IMessageLog
    {
        /// <summary>
        /// Writes a line to the message log
        /// </summary>
        /// <param name="message">The message.</param>
        void WriteLine(string message)
        {
            Write(message);
            WriteLine();
        }

        /// <summary>
        /// Writes a newline.
        /// </summary>
        void WriteLine()
        {
            Write("\r\n");
        }

        /// <summary>
        /// Writes to the message log
        /// </summary>
        /// <param name="message">The message.</param>
        void Write(string message);

        /// <summary>
        /// Dump the specified bytes.
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
                    WriteLine($"{hex}  |{ascii}|");
                    hex.Clear();
                    ascii.Clear();
                }
                
                if (offset % 16 == 0)
                {
                    Write($"  {offset:X8}  ");
                }

                hex.AppendFormat("{0:X2} ", b);
                ascii.Append((b >= 32 && b <= 126) ? (char)b : '.');
                offset++;
            }

            if (hex.Length > 0)
            {
                WriteLine($"{hex,-48}  |{ascii}|");
            }
        }
    }
}
