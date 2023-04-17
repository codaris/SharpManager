#pragma once
#ifndef __MANAGER_H__
#define __MANAGER_H__

namespace Command
{
    const int Ping = 1;
    const int DeviceSelect = 2;
    const int LoadTape = 3;
    const int Print = 4;
}

enum ErrorCode
{
    Unknown = 0,
    Timeout = 1,
    InvalidData = 2,
    Cancelled = 3,
    Unexpected = 4
};

namespace Manager
{
    /**
     * @brief Waits for a single byte to become available on the serial interface
     * @return int      Byte read or -1 if timed out
     */
    int WaitReadByte();


    /**
     * @brief Waits for 2 byte short integer to become available on the serial interface
     * @return int      Byte read or -1 if timed out
     */
    int WaitReadWord();

    /**
     * @brief Sends failure to the manager
     * @param errorCode Error code to send
    */
    void SendFailure(ErrorCode errorCode);

    /**
     * @brief Sends success to the manager
    */
    void SendSuccess();

    /**
     * @brief Send the device select to the manager
     * @param device The device code
    */
    void SendDeviceSelect(int device);

    /**
     * @brief Send the print character
     * @param value Character to print
    */
    void SendPrintChar(int value);

    /**
     * @brief Process a packet from the Serial port
     * @note There should be one character available in the serial buffer
    */
    void ProcessPacket();
}

#endif