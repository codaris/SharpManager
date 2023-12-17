#pragma once
#ifndef __MANAGER_H__
#define __MANAGER_H__

#include "Result.h"

namespace Manager
{
    /**
     * @brief Waits for a single byte to become available on the serial interface
     * @return Byte read or error
     */
    Result WaitReadByte();

    /**
     * @brief Waits for 2 byte short integer to become available on the serial interface
     * @return Byte read or error
     */
    Result WaitReadWord();

    /**
     * @brief Reads an escaped byte from the serial interface or resulting code.
     * @return The data to be read or error code
     */
    Result WaitReadDataByte();

    /**
     * @brief Reads a single byte from the serial port and compares with the specified value
     * @return Data if expected value returned or error code 
     */
    Result Expect(int value);

    /**
     * @brief Sees if there is a byte available on the serial line that will cancel the operation
     * @return bool     True if cancelled
     */
    bool ReadCancel();

    /**
     * @brief Sends failure to the manager
     * @param errorCode Error code to send
    */
    void SendFailure(ResultType errorCode);

    /**
     * @brief Sends success/acknowledgement to the manager
    */
    void SendSuccess();

    /**
     * @brief Start a data packet transmission
    */
    void StartFrame();

    /**
     * @brief Sends an escaped data packet byte
     * @param value Byte to send
    */
    void SendFrameByte(int data);

    /**
     * @brief End data packet transmission
    */
    void EndFrame();

    /**
     * @brief Send a byte to the manager
     * @param data  Value to send
    */
    void SendDataByte(int data);

    /**
     * @brief Send the device select to the manager
     * @param device The device code
    */
    void SendDeviceSelect(int device);

    /**
     * @brief Send the print character
     * @param data Character to print
    */
    void SendPrintChar(int data);

    /**
     * @brief Sends the disk command start
    */
    void StartDiskCommand();

    /**
     * @brief Initializes the buffer for reading 
     * @param totalSize The total amount of data to read in BUFFER_SIZE packets
    */
    void InitializeBuffer(int totalSize);

    /**
     * @brief Reads a byte from the buffer, fills the buffer as necessary.
     * @param timeout Number of milliseconds to wait for byte from the buffer
     * @returns A byte from the buffer or an error code
    */
    Result ReadBufferByte(int timeout = 0);

    /**
     * @brief Fills serial buffer 
    */
    void FillBuffer();

    /**
     * @brief Process incoming commands 
     * @return The processed command
     */
    void Task();
}

#endif