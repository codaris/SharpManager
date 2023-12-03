#include "SerialChannel.h"
#include "Ascii.h"

enum ErrorCode
{
    Unknown = 0,
    Timeout = 1,
    InvalidData = 2,
    Cancelled = 3,
    Unexpected = 4,
    Overflow = 5
};

namespace SerialChannel 
{
    /**
     * @brief Waits for a single byte to become available on the serial interface
     * @return int      Byte read or -1 if timed out
     */
    Result WaitReadByte()
    {
        unsigned long startTime = millis();
        while (Serial.available() == 0) {
            // Wait for a byte to become available or until the timeout
            if ((millis() - startTime) > 1000) return Result(ErrorCode::Timeout);
        }
        return Result(Serial.read());
    }    


    /**
     * @brief Waits for 2 byte short integer to become available on the serial interface
     * @return int      Byte read or -1 if timed out
     */
    int WaitReadWord()
    {
        unsigned long startTime = millis();
        while (Serial.available() < 2) {
            // Wait for a byte to become available or until the timeout
            if ((millis() - startTime) > 1000) return -1;
        }
        return Serial.read() | (Serial.read() << 8);
    }

    /**
     * @brief Waits for a single byte to become available on the serial interface
     * @return int      Byte read or -1 if timed out
     */
    int WaitReadDataByte()
    {
        byte data = WaitReadByte();
        switch (data) {
            case Ascii::DLE:
                return WaitReadByte();
            case Ascii::NAK:
                return -WaitReadByte();
            case Ascii::CAN:
                return -ErrorCode::Cancelled;
            case Ascii::SYN:
            case Ascii::ETX:
                return -ErrorCode::Unexpected;
            default:
                return data;
        }
    }    

    /**
     * @brief Sees if there is a byte available on the serial line that will cancel the operation
     * @return bool     True if cancelled
     */
    bool ReadCancel()
    {
        if (Serial.available() == 0) return false;
        int value = Serial.read(); 
        if (value == Ascii::CAN) return true;
        if (value == Ascii::ESC) return true;
        return false;
    }    

    /**
     * @brief Sends failure to the manager
     * @param errorCode Error code to send
    */
    void SendFailure(ErrorCode errorCode)
    {
        Serial.write(Ascii::NAK);
        Serial.write(errorCode);
    }


    /**
     * @brief Sends success to the manager
    */
    void SendSuccess()
    {
        Serial.write(Ascii::ACK);
    }


    void Task(void (*commandFunction)(int))
    {
        // Wait for data
        int data = Serial.read();

        // Respond to sync byte with sync response
        if (data == Ascii::SYN) {
            Serial.write(Ascii::SYN);
            return;
        }

        // If not start of header, ignore
        if (data != Ascii::SOH) return;

        // Read the command type
        int command = WaitReadByte();
        if (command == -1) {
            SendFailure(ErrorCode::Timeout);
            return;
        }
        
        commandFunction(command);
    }

    /**
     * @brief Resets the incoming packet buffer
     * @param remaining     The number of bytes remaining to read total
    */
    void StartDataRead()
    {
        // Reset the buffers
        outBufferCount = 0;
        outBufferIndex = 0;
        serialBufferIndex = 0;    


    }


    bool DataProcess()
    {

    }
}