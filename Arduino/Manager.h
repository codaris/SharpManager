#pragma once
#ifndef __MANAGER_H__
#define __MANAGER_H__

namespace Command
{
    const int Ping = 1;
    const int DeviceSelect = 2;
    const int LoadTape = 3;
    const int Print = 4;
    const int SaveTape = 5;
    const int Data = 6;
    const int Disk = 7;
    const int Cancel = 99;
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
     * @brief Sees if there is a byte available on the serial line that will cancel the operation
     * @return bool     True if cancelled
     */
    bool ReadCancel();

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
     * @brief Start the disk command packet
     * @note Use SendTapeData to send data
    */
    void StartDiskCommand();

    /**
     * @brief End the disk command packet
    */
    void EndDiskCommand();

    /**
     * @brief Sends an escaped table data byte
     * @param value Byte to send
    */
    void SendTapeByte(int data);

    /**
     * @brief Sends an byte wrapped in packet for testing
     * @param value Byte to send
    */
    void SendDataByte(int data);

    /**
     * @brief Process a packet from the Serial port
     * @note There should be one character available in the serial buffer
    */
    void ProcessPacket();

    /**
     * @brief Resets the incoming packet buffer
    */
    void ResetBuffer();

    /**
     * @brief Resets the incoming packet buffer
     * @param remaining     The number of bytes remaining to read total
    */
    void ResetBuffer(int remaining);

    /**
     * @brief Return true if there is any data in the buffer to process
     * @return  True if buffer contains a byte
    */
    bool BufferHasData();

    /**
     * @brief Reads a single byte from the buffer
     * @note There must be a least one byte in the buffer @see BufferHasData()
     * @return  A byte from the buffer
    */
    byte ReadFromBuffer();

    /**
     * @brief   Fills the buffer from the serial port.  Should be called in a loop.
     * @param   remaining     The number of bytes remaining to read total
     * @return  The number of bytes read into the buffer (subtract from remaining)
    */
    int FillBuffer(int remaining);
}

#endif