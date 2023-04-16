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
    }
}
