# SharpManager
Applications for managing Sharp Pocket Computers 

**Sharp Manager** is an application for interfacing some vintage Sharp pocket computers with your desktop PC.  It works together with an [Arduino](https://store.arduino.cc/pages/nano-family) to connect the Sharp computer to a USB port on your computer.  With this hardware and software, Sharp Manager emulates a [CE-126P](http://pocket.free.fr/html/sharp/ce-126p_e.html) cassette and printer interface and a sharp Sharp [CE-140F](http://pocket.free.fr/html/sharp/ce-140f_e.html).  It has only been tested with a Sharp [PC-1401](http://pocket.free.fr/html/sharp/pc-1401_e.html) and a [PC-1403](http://pocket.free.fr/html/sharp/pc-1401_e.html) however the disk emulation should work on any device that supports the CE-140F.

## Requirements

To use this software you will need the following:

* An Sharp Pocket Computer
* An Arduino (Arduino Nano)
* [Sharp Pocket Tools](http://pocket.free.fr/html/soft/pocket-tools_e.html)
* A PC running Windows

## Features

* CE-126P Printer Emulator: Print from your computer to your desktop
* CE-126P Cassette Emulator: Load .tap files into PC-1401 and PC-1403.
* CE-140F Floppy drive Emiulator: Treat a folder on your desktop as a floppy disk.
  
Sharp Manager also supports uploading pre-compiled firmware to a number of different Arduino models.

## Download and Install

You can download the latest release installer from here:

* [Sharp Manager v1.0.1](https://github.com/codaris/SharpManager/releases/download/v1.0.1/SharpManager.msi)

Download the above file and open it to install Sharp Manager on your computer.

## Arduino Connection Diagram

const int SHARP_BEEP = 10;  // Pin D10, Beep
const int SHARP_BUSY = 2;   // Pin D9, Busy   Sharp pin 4 
const int SHARP_DOUT = 3;   // Pin D8, Dout   Sharp pin 5 
const int SHARP_XIN = 4;    // Pin D7, Xin    Sharp pin 6
const int SHARP_XOUT = 5;   // Pin D6, Xout   Sharp pin 7 
const int SHARP_DIN = 6;    // Pin D5, Din    Sharp pin 8
const int SHARP_ACK = 7;    // Pin D4, ACK    Sharp pin 9 
const int SHARP_SEL2 = 8;   // Pin D3, SEL2   Sharp pin 10
const int SHARP_SEL1 = 9;   // Pin D2, SEL1   Sharp pin 11



| Connector Pin | Arduino Pin | Description |
|:-------------:|:-----------:|-----------------|
|      2      |  **2**   | Output Data       |
|      3      |  **3**   | Output Clock      |
|      4      |    4     |                   |
|      5      |    5     |                   |
|      6      |    6     |                   |
|      7      |  **12**  | Input Clock       |
|      8      |  **13**  | Input Data        |
|      9      |    15    |                   |
|      10     |    11    |                   |
|      11     |    10    |                   |
|      GND    |  **25**  | Electrical Ground |

const int SHARP_BEEP = 10;  // Pin D10, Beep
const int SHARP_BUSY = 2;   // Pin D9, Busy   Sharp pin 4 
const int SHARP_DOUT = 3;   // Pin D8, Dout   Sharp pin 5 
const int SHARP_XIN = 4;    // Pin D7, Xin    Sharp pin 6
const int SHARP_XOUT = 5;   // Pin D6, Xout   Sharp pin 7 
const int SHARP_DIN = 6;    // Pin D5, Din    Sharp pin 8
const int SHARP_ACK = 7;    // Pin D4, ACK    Sharp pin 9 
const int SHARP_SEL2 = 8;   // Pin D3, SEL2   Sharp pin 10
const int SHARP_SEL1 = 9;   // Pin D2, SEL1   Sharp pin 11