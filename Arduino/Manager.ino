#include "Manager.h"
#include "Ascii.h"
#include "Tape.h"
#include "Sharp.h"


namespace Manager
{
    enum Command
    {
        Init = 1,
        Ping = 2,
        DeviceSelect = 3,
        Print = 4,
        Data = 5,
        LoadTape = 6,
        SaveTape = 7,
        Disk = 8
    };

    const byte VersionHigh = 1;                  // Version numbers
    const byte VersionLow = 1;                  
    
    const int BUFFER_SIZE = 64;         // The size of the serial and tape buffers
    byte serialBuffer[BUFFER_SIZE];     // Serial receive buffer
    byte outBuffer[BUFFER_SIZE];        // The output buffer
    int serialBufferCount = 0;          // The number of bytes to read into the serial buffer (no larger than BUFFER_SIZE)
    int serialBufferIndex = 0;          // The index of the next position to read into the buffer
    int outBufferCount = 0;             // The number of items in the output buffer
    int outBufferIndex = 0;             // The next byte to process in the output buffer
    int dataRemaining = 0;              // Number of bytes remaining in incoming frame

    /**
     * @brief Waits for a single byte to become available on the serial interface
     * @return Byte read or error
     */
    Result WaitReadByte()
    {
        unsigned long startTime = millis();
        while (Serial.available() == 0) {
            // Wait for a byte to become available or until the timeout
            if ((millis() - startTime) > 1000) return ResultType::Timeout;
        }
        return Serial.read();
    }    

    /**
     * @brief Waits for 2 byte short integer to become available on the serial interface
     * @return Byte read or error
     */
    Result WaitReadWord()
    {
        unsigned long startTime = millis();
        while (Serial.available() < 2) {
            // Wait for a byte to become available or until the timeout
            if ((millis() - startTime) > 1000) return ResultType::Timeout;
        }
        return Serial.read() | (Serial.read() << 8);
    }

    /**
     * @brief Reads an escaped byte from the serial interface or resulting code.
     * @return The data to be read or error code
     */
    Result WaitReadDataByte()
    {
        Result result = WaitReadByte();
        if (!result.HasValue()) return result;      
        switch (result.Value()) {
            case Ascii::DLE:
                return WaitReadByte();
            case Ascii::NAK:
                return WaitReadByte().AsErrorCode();
            case Ascii::ETX:
                return ResultType::End;
            case Ascii::CAN:
                return ResultType::Cancelled;
            case Ascii::SYN:
                return ResultType::Unexpected;
            default:
                return result.Value();
        }
    }    

    /**
     * @brief Reads a single byte from the serial port and compares with the specified value
     * @return Data if expected value returned or error code 
     */
    Result Expect(int value)
    {
        auto result = WaitReadByte();
        if (result.HasValue() && result.Value() != value) return ResultType::Unexpected;
        return result;
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
    void SendFailure(ResultType errorCode)
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

    /**
     * @brief Start a data frame transmission
    */
    void StartFrame()
    {
        Serial.write(Ascii::STX);
    }

    /**
     * @brief Sends an escaped data packet byte
     * @param value Byte to send
    */
    void SendFrameByte(int data)
    {
        switch (data)
        {
            case Ascii::DLE:
            case Ascii::SYN:
            case Ascii::CAN:
            case Ascii::ETX:
            case Ascii::NAK:
                Serial.write(Ascii::DLE);
                break;
        }
        Serial.write(data);
    }

    /**
     * @brief End data frame transmission
    */
    void EndFrame()
    {
        Serial.write(Ascii::ETX);
    }

    /**
     * @brief Send the print character
     * @param value Character to print
    */
    void SendDataByte(int data)
    {
        if (data == -1) return;
        Serial.write(Ascii::SOH);
        Serial.write(Command::Data);
        Serial.write(data);
    }

    /**
     * @brief Send the device select to the manager
     * @param device The device code
    */
    void SendDeviceSelect(int device)
    {
        Serial.write(Ascii::SOH);
        Serial.write(Command::DeviceSelect);
        Serial.write(device); 
    }

    /**
     * @brief Send the print character
     * @param value Character to print
    */
    void SendPrintChar(int data)
    {
        Serial.write(Ascii::SOH);
        Serial.write(Command::Print);
        Serial.write(data);
    }

    /**
     * @brief Start the disk command packet
    */
    void StartDiskCommand()
    {
        Serial.write(Ascii::SOH);
        Serial.write(Command::Disk);
    }

    /**
     * @brief Sends success to the manager
     * @param totalSize The total amount of data to read in BUFFER_SIZE packets
    */
    void InitializeBuffer(int totalSize)
    {
        // Reset the buffers
        outBufferCount = 0;
        outBufferIndex = 0;
        serialBufferCount = min(BUFFER_SIZE, totalSize);
        serialBufferIndex = 0;
        dataRemaining = totalSize;
    }

    /**
     * @brief Reads a byte from the buffer, fills the buffer as necessary.
     * @param timeout Number of milliseconds to wait for byte from the buffer
     * @returns A byte from the buffer or an error code
    */
    Result ReadBufferByte(int timeout = 0)
    {
        unsigned long startTime = millis();
        while (true)
        {
            // If the write buffer contains unsent bytes
            if (outBufferIndex < outBufferCount) {
                return outBuffer[outBufferIndex++];
            } else {
                // If write buffer empty and no remaining bytes, leave
                if (dataRemaining == 0) return ResultType::End;
            }

            // If bytes are available, add to read buffer
            while (Serial.available() > 0 && serialBufferIndex < serialBufferCount) {
                serialBuffer[serialBufferIndex++] = Serial.read();
            }    

            // If the read buffer is full and the write buffer is full
            // Copy the read buffer into the write buffer and receive another packet
            if (serialBufferIndex == serialBufferCount && outBufferIndex == outBufferCount) {
                dataRemaining -= serialBufferCount;
                memcpy(outBuffer, serialBuffer, serialBufferCount);
                outBufferCount = serialBufferCount;
                outBufferIndex = 0;
                serialBufferCount = min(BUFFER_SIZE, dataRemaining);
                serialBufferIndex = 0;
                // Acknowledge the serial buffer
                SendSuccess();
            }

            // Timeout if no byte returned in time
            if (timeout > 0 && (millis() - startTime) > timeout) return ResultType::Timeout;            
        }
    }

    /**
     * @brief Fills serial buffer 
    */
    void FillBuffer()
    {
        while (Serial.available() > 0 && serialBufferIndex < serialBufferCount) {
            serialBuffer[serialBufferIndex++] = Serial.read();
        }        
    }

    /**
     * @brief Process length-prefixed data packet
     * @param sendFunction  The function to call with the buffered data
     * @return True if succesful, false on error
     */
    bool ProcessDataFrame(void (*sendFunction)(byte))
    {
        // Read the data packet length
        Result length = WaitReadWord();            // 2 bytes, total length of data
        if (length.IsError()) {
            SendFailure(length.AsErrorCode());
            return false;
        }

        SendSuccess();  // Acknowledge the header

        InitializeBuffer(length);

        while (true) 
        {
            Result data = ReadBufferByte();
            if (data.IsDone()) break;
            if (data.IsError()) {
                SendFailure(data.AsErrorCode());
                return false;
            }
            sendFunction(data);
        }

        // Completed processing packet
        return true;
    }

    /**
     * @brief Sends the initialization header
     */
    void SendInitializeHeader()
    {
        Serial.write(Ascii::SOH);
        Serial.write(VersionHigh);
        Serial.write(VersionLow);
        Serial.write(SERIAL_RX_BUFFER_SIZE);
        Serial.write(Ascii::STX);
        Serial.write("Sharp Anduino Driver ");
        Serial.print((int)VersionHigh);
        Serial.print(".");
        Serial.print((int)VersionLow);
        Serial.print("\r\n");
        Serial.write(Ascii::ETX);        
    }

    /**
     * @brief Process incoming serial packets 
     */
    void Task()
    {
        if (!Serial.available()) return;
        int data = Serial.read();

        // Respond to sync byte with sync response
        if (data == Ascii::SYN) {
            Serial.write(Ascii::SYN);
            return;
        }

        // If not start of header, ignore
        if (data != Ascii::SOH) return;

        // Read the command type
        Result command = WaitReadByte();
        if (command.IsError()) {
            SendFailure(command.AsErrorCode());
            return;
        }

        switch (command.Value())
        {
            case Command::Ping:
                // Do nothing except ACK
                SendSuccess();
                break;
            case Command::Init:
                SendInitializeHeader();
                break;
            case Command::LoadTape:
                // Run the load from tape command
                Tape::Load();
                break;
            case Command::SaveTape:
                // Run the save tape command
                Tape::Save();
                break;
            case Command::Disk:
                Result capture = WaitReadByte();
                if (capture.IsError()) {
                    SendFailure(capture.AsErrorCode());
                    return;
                }
                // Disk command response
                ProcessDataFrame(Sharp::SendDiskByte);      
                if (capture.Value() == 0xFF) Sharp::ProcessDiskCommand();
                break;
            default:
                // Unknown command error
                SendFailure(ResultType::Unexpected);
                break;
        }
    }
}
