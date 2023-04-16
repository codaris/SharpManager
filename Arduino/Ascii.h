#pragma once
#ifndef __ASCII_H__
#define __ASCII_H__

namespace Ascii {
    const byte NUL = 0;   // Null char
    const byte SOH = 1;   // Start of Heading
    const byte STX = 2;   // Start of Text
    const byte ETX = 3;   // End of Text
    const byte EOT = 4;   // End of Transmission
    const byte ENQ = 5;   // Enquiry
    const byte ACK = 6;   // Acknowledgment
    const byte BEL = 7;   // Bell
    const byte BS = 8;   // Back Space
    const byte HT = 9;   // Horizontal Tab
    const byte LF = 10;  // Line Feed
    const byte VT = 11;  // Vertical Tab
    const byte FF = 12;  // Form Feed
    const byte CR = 13;  // Carriage Return
    const byte SO = 14;  // Shift Out / X-On
    const byte SI = 15;  // Shift In / X-Off
    const byte DLE = 16;  // Data Line Escape
    const byte DC1 = 17;  // Device Control 1 (oft. XON)
    const byte DC2 = 18;  // Device Control 2
    const byte DC3 = 19;  // Device Control 3 (oft. XOFF)
    const byte DC4 = 20;  // Device Control 4
    const byte NAK = 21;  // Negative Acknowledgement
    const byte SYN = 22;  // Synchronous Idle
    const byte ETB = 23;  // End of Transmit Block
    const byte CAN = 24;  // Cancel
    const byte EM = 25;  // End of Medium
    const byte SUB = 26;  // Substitute
    const byte ESC = 27;  // Escape
    const byte FS = 28;  // File Separator
    const byte GS = 29;  // Group Separator
    const byte RS = 30;  // Record Separator
    const byte US = 31;  // Unit Separator
    const byte ENTER = CR;
    const byte SPACE = 32;
    const byte DELETE = 127;
    const byte SINGLEQUOTE = 39;
}

#endif