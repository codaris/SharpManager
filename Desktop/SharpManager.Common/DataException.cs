using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpManager
{
    public class DataException : System.Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VectorException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DataException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public DataException(string message, DataException innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Expects the specified received value.
        /// </summary>
        /// <param name="received">The received.</param>
        /// <param name="expected">The expected.</param>
        /// <exception cref="SharpManager.DataException">Expected {expected:X2} but received {received:X2}</exception>
        public static void Expect(byte received, byte expected)
        {
            if (received == expected) return;
            throw new DataException($"Expected {expected:X2} but received {received:X2}");
        }

        /// <summary>
        /// Expects the specified received value.
        /// </summary>
        /// <param name="received">The received.</param>
        /// <param name="expected">The expected.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <exception cref="SharpManager.DataException"></exception>
        public static void Expect(byte received, byte expected, string errorMessage)
        {
            if (received == expected) return;
            throw new DataException(errorMessage);
        }

    }
}
