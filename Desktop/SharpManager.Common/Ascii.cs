using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpManager
{
    internal static class Ascii
    {
        public const byte NUL = 0;   // Null char
        public const byte SOH = 1;   // Start of Heading
        public const byte STX = 2;   // Start of Text
        public const byte ETX = 3;   // End of Text
        public const byte EOT = 4;   // End of Transmission
        public const byte ENQ = 5;   // Enquiry
        public const byte ACK = 6;   // Acknowledgment
        public const byte BEL = 7;   // Bell
        public const byte BS = 8;   // Back Space
        public const byte HT = 9;   // Horizontal Tab
        public const byte LF = 10;  // Line Feed
        public const byte VT = 11;  // Vertical Tab
        public const byte FF = 12;  // Form Feed
        public const byte CR = 13;  // Carriage Return
        public const byte SO = 14;  // Shift Out / X-On
        public const byte SI = 15;  // Shift In / X-Off
        public const byte DLE = 16;  // Data Line Escape
        public const byte DC1 = 17;  // Device Control 1 (oft. XON)
        public const byte DC2 = 18;  // Device Control 2
        public const byte DC3 = 19;  // Device Control 3 (oft. XOFF)
        public const byte DC4 = 20;  // Device Control 4
        public const byte NAK = 21;  // Negative Acknowledgement
        public const byte SYN = 22;  // Synchronous Idle
        public const byte ETB = 23;  // End of Transmit Block
        public const byte CAN = 24;  // Cancel
        public const byte EM = 25;  // End of Medium
        public const byte SUB = 26;  // Substitute
        public const byte ESC = 27;  // Escape
        public const byte FS = 28;  // File Separator
        public const byte GS = 29;  // Group Separator
        public const byte RS = 30;  // Record Separator
        public const byte US = 31;  // Unit Separator
        public const byte ENTER = CR;
        public const byte SPACE = 32;
        public const byte DELETE = 127;
        public const byte SINGLEQUOTE = 39;

        /// <summary>
        /// Determines whether the specified value is printable.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the specified value is printable; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsPrintable(byte value)
        {
            if (value >= 32 && value <= 127) return true;
            if (value >= BS && value <= CR) return true;
            return false;
        }
    }
}
