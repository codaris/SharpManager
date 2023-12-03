using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpManager
{
    public class ArduinoException : System.Exception
    {
        public ErrorCode ErrorCode { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorException"/> class.
        /// </summary>
        public ArduinoException() : base()
        {
            ErrorCode = ErrorCode.Ok;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ArduinoException(string message) : base(message)
        {
            ErrorCode = ErrorCode.Ok;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public ArduinoException(string message, DataException innerException) : base(message, innerException)
        {
            ErrorCode = ErrorCode.Ok;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArduinoException"/> class.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        public ArduinoException(ErrorCode errorCode) : base(ErrorCodeToMessage(errorCode))
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArduinoException"/> class.
        /// </summary>
        /// <param name="errorCode">The error code as byte.</param>
        public ArduinoException(byte errorCode) : this((ErrorCode)errorCode)
        {
        }

        /// <summary>
        /// Converts Error code to message.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <returns>Error message</returns>
        private static string ErrorCodeToMessage(ErrorCode errorCode)
        {
            return errorCode switch
            {
                ErrorCode.Ok => "Unknown response received",
                ErrorCode.Cancelled => "Request cancelled",
                ErrorCode.Timeout => "Timeout occured",
                ErrorCode.SyncError => "Synchronization error",
                ErrorCode.Unexpected => "Unexpected command received",
                ErrorCode.Overflow => "Buffer overflow occurred",
                _ => $"Unexpected error code 0x{errorCode:X2}",
            };
        }

        /// <summary>
        /// Throws timeout exception
        /// </summary>
        /// <exception cref="SharpManager.ArduinoException"></exception>
        public static void Timeout()
        {
            throw new ArduinoException(ErrorCode.Timeout);
        }
    }
}
