#include "Manager.h"
#include "Ascii.h"
#include "Tape.h"


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
 * @brief Sends failure to the manager
 * @param errorCode Error code to send
*/
void Manager::SendFailure(ErrorCode errorCode)
{
    Serial.write(Ascii::NAK);
    Serial.write(ErrorCode::Timeout);
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
 * @brief Process the serial packets coming from SharpManager
 */
void Manager::ProcessPacket()
{
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
        case Command::LoadTape:
            // Run the load from tape command
            Tape::Load();
            break;
        case Command::SaveTape:
            // Run the save tape command
            Tape::Save();
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