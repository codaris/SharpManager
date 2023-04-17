#pragma once
#ifndef __SHARP_H__
#define __SHARP_H__

/** Sharp pins */
const int SHARP_BUSY = 9;   // Pin D9, Busy   Sharp pin 4 
const int SHARP_DOUT = 8;   // Pin D8, Dout   Sharp pin 5 
const int SHARP_XIN = 7;    // Pin D7, Xin    Sharp pin 6
const int SHARP_XOUT = 6;   // Pin D6, Xout   Sharp pin 7 
const int SHARP_DIN = 5;    // Pin D5, Din    Sharp pin 8
const int SHARP_ACK = 4;    // Pin D4, ACK    Sharp pin 9 
const int SHARP_SEL2 = 3;   // Pin D3, SEL2   Sharp pin 10
const int SHARP_SEL1 = 2;   // Pin D2, SEL1   Sharp pin 11


namespace Sharp
{
    /**
     * @brief Reads the device select 
     */
    int ReadDeviceSelect();

    /**
     * @brief Reads a byte to be sent to printer from pocket computer 
     * @returns The byte read
     */
    int ReadPrintByte();

    /**
     * @brief Read a disk command from the pocket computer
     */
    void ReadDisk();
};

#endif
