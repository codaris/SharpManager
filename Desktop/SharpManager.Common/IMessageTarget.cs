using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpManager
{
    public interface IMessageTarget
    {
        /// <summary>
        /// Write the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        void Write(string message);

        /// <summary>
        /// Write the specified message with newline.
        /// </summary>
        /// <param name="message">The message.</param>
        void WriteLine(string message) => Write(message + Environment.NewLine);

        /// <summary>
        /// Writes a newline
        /// </summary>
        void WriteLine() => Write(Environment.NewLine);
    }
}
