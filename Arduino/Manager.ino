#include "Manager.h"
#include "Ascii.h"
#include "Tape.h"
#include "Sharp.h"

const byte VersionHigh = 1;                  // Version number
const byte VersionLow = 1;                  

namespace Manager
{
    const int BUFFER_SIZE = 64;         // The size of the serial and tape buffers
    byte serialBuffer[BUFFER_SIZE];     // Serial receive buffer
    byte outBuffer[BUFFER_SIZE];        // The output buffer
    int serialBufferCount = 0;          // The number of bytes to read into the serial buffer (no larger than BUFFER_SIZE)
    int serialBufferIndex = 0;          // The index of the next position to read into the buffer
    int outBufferCount = 0;             // The number of items in the output buffer
    int outBufferIndex = 0;             // The next byte to process in the output buffer

    bool ProcessDataPacket(void (*sendFunction)(byte));
}


/**
 * @brief Waits for a single byte to become available on the serial interface
 * @return int      Byte read or -1 if timed out
 */
int Manager::WaitReadByte()
{
    unsigned long startTime = millis();
    while (Serial.available() == 0) {
        // Wait for a byte to become available or until the timeout
        if ((millis() - startTime) > 1000) return -1;
    }
    return Serial.read();
}    


/**
 * @brief Waits for 2 byte short integer to become available on the serial interface
 * @return int      Byte read or -1 if timed out
 */
int Manager::WaitReadWord()
{
    unsigned long startTime = millis();
    while (Serial.available() < 2) {
        // Wait for a byte to become available or until the timeout
        if ((millis() - startTime) > 1000) return -1;
    }
    return Serial.read() | (Serial.read() << 8);
}

/**
 * @brief Sees if there is a byte available on the serial line that will cancel the operation
 * @return bool     True if cancelled
 */
bool Manager::ReadCancel()
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
void Manager::SendFailure(ErrorCode errorCode)
{
    Serial.write(Ascii::NAK);
    Serial.write(errorCode);
}


/**
 * @brief Sends success to the manager
*/
void Manager::SendSuccess()
{
    Serial.write(Ascii::ACK);
}

/**
 * @brief Send the device select to the manager
 * @param device The device code
*/
void Manager::SendDeviceSelect(int device)
{
    Serial.write(Ascii::SOH);
    Serial.write(Command::DeviceSelect);
    Serial.write(device); 
}


/**
 * @brief Send the print character
 * @param value Character to print
*/
void Manager::SendPrintChar(int data)
{
    Serial.write(Ascii::SOH);
    Serial.write(Command::Print);
    Serial.write(data);
}

/**
 * @brief Start the disk command packet
*/
void Manager::StartDiskCommand()
{
    Serial.write(Ascii::SOH);
    Serial.write(Command::Disk);
    Serial.write(Ascii::STX);
}

/**
 * @brief End the disk command packet
*/
void Manager::EndDiskCommand()
{
    Serial.write(Ascii::ETX);
}

/**
 * @brief Process the serial packets coming from SharpManager
 */
void Manager::ProcessPacket()
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
    int command = WaitReadByte();
    switch (command)
    {
        case Command::Ping:
            // Do nothing except ACK
            Serial.write(Ascii::ACK);
            break;
        case Command::Init:
            Serial.write(Ascii::SOH);
            Serial.write("SM");
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
        case Command::LoadTape:
            // Run the load from tape command
            Tape::Load();
            break;
        case Command::SaveTape:
            // Run the save tape command
            Tape::Save();
            break;
        case Command::Disk:
            // Disk command response
            ProcessDataPacket(Sharp::SendDiskByte);            
            break;
        default:
            // Unknown command error
            SendFailure(ErrorCode::Unexpected);
            return;
    }
}


/**
 * @brief Sends an escaped table data byte
 * @param value Byte to send
*/
void Manager::SendTapeByte(int data)
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
 * @brief Send the print character
 * @param value Character to print
*/
void Manager::SendDataByte(int data)
{
    if (data == -1) return;
    Serial.write(Ascii::SOH);
    Serial.write(Command::Data);
    Serial.write(data);
}

/**
 * @brief Resets the incoming packet buffer
 * @param remaining     The number of bytes remaining to read total
*/
void Manager::ResetBuffer(int remaining)
{
    // Reset the buffers
    outBufferCount = 0;
    outBufferIndex = 0;
    serialBufferCount = min(BUFFER_SIZE, remaining);
    serialBufferIndex = 0;    
}

/**
 * @brief Return true if there is any data in the buffer to process
 * @return  True if buffer contains a byte
*/
bool Manager::BufferHasData()
{
    return (outBufferIndex < outBufferCount);
}

/**
 * @brief Reads a single byte from the buffer
 * @note There must be a least one byte in the buffer @see BufferHasData()
 * @return  A byte from the buffer
*/
byte Manager::ReadFromBuffer()
{
    if (!BufferHasData()) return 0;
    return outBuffer[outBufferIndex++];
}

/**
 * @brief   Fills the buffer from the serial port.  Should be called in a loop.
 * @param   remaining     The number of bytes remaining to read total
 * @return  The number of bytes read into the buffer (subtract from remaining)
*/
int Manager::FillBuffer(int remaining)
{
    int result = 0;

    // Read a byte from the serial port into the buffer
    if (Serial.available() > 0 && serialBufferIndex < serialBufferCount) {
        serialBuffer[serialBufferIndex++] = Serial.read();
    }    

    // If the read buffer is full and the write buffer is full
    // Copy the read buffer into the write buffer and receive another packet
    if (serialBufferIndex == serialBufferCount && outBufferIndex == outBufferCount) {
        result = serialBufferCount;
        memcpy(outBuffer, serialBuffer, serialBufferCount);
        outBufferCount = serialBufferCount;
        outBufferIndex = 0;
        serialBufferCount = min(BUFFER_SIZE, remaining);
        serialBufferIndex = 0;
        // Acknowledge the read buffer
        Manager::SendSuccess();
    }    

    return result;
}

bool Manager::ProcessDataPacket(void (*sendFunction)(byte))
{
    // Read the data packet length
    int length = Manager::WaitReadWord();            // 2 bytes, total length of data
    if (length == -1) {
        Manager::SendFailure(ErrorCode::Timeout);
        return false;
    }

    int remaining = length;                 // Number of bytes remaining to be read from PC

    // Reset the buffers
    outBufferCount = 0;
    outBufferIndex = 0;
    serialBufferCount = min(BUFFER_SIZE, remaining);
    serialBufferIndex = 0;

    while (true) 
    {
        // If the write buffer contains unsent bytes
        if (outBufferIndex < outBufferCount) {
            sendFunction(outBuffer[outBufferIndex++]);
        }
        else
        {
            // If write buffer empty and no remaining bytes, leave
            if (remaining == 0) break;
        }

        // If a byte is available, add to read buffer
        if (Serial.available() > 0 && serialBufferIndex < serialBufferCount) {
            serialBuffer[serialBufferIndex++] = Serial.read();
        }    

        // If the read buffer is full and the write buffer is full
        // Copy the read buffer into the write buffer and receive another packet
        if (serialBufferIndex == serialBufferCount && outBufferIndex == outBufferCount) {
            remaining -= serialBufferCount;
            memcpy(outBuffer, serialBuffer, serialBufferCount);
            outBufferCount = serialBufferCount;
            outBufferIndex = 0;
            serialBufferCount = min(BUFFER_SIZE, remaining);
            serialBufferIndex = 0;
            // Acknowledge the read buffer
            Manager::SendSuccess();
        }
    }

    return true;
}
