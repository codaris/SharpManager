#include "Sharp.h"
#include "Manager.h"

/** The disk read timeout */
const long IN_DATAREADY_TIMEOUT = 50000;


/**
 * @brief Reads the device select 
 */
int Sharp::ReadDeviceSelect()
{
    int dataByte = 0;
    digitalWrite(LED_BUILTIN, HIGH);
    delayMicroseconds(50);
    for (int i = 0; i < 8; i++) {
        digitalWrite(SHARP_ACK, HIGH);
        long Timeout = millis();
        while (!digitalRead(SHARP_BUSY))
        {
            if (millis() - Timeout > 50) break;
        }
        delayMicroseconds(50);
        int dataBit = digitalRead(SHARP_DOUT);
        dataByte = dataByte | (dataBit << i);        
        digitalWrite(SHARP_ACK, LOW);
        while (digitalRead(SHARP_BUSY));
        delayMicroseconds(150);
    }    
    digitalWrite(LED_BUILTIN, LOW);
    return dataByte;
}


/**
 * @brief Reads a byte to be sent to printer from pocket computer 
 * @returns The byte read
 */
int Sharp::ReadPrintByte()
{
    digitalWrite(LED_BUILTIN, HIGH);
    int dataByte = 0;
    for (int i = 0; i < 8; i++) {        
        while (!digitalRead(SHARP_BUSY));
        delayMicroseconds(50);
        int dataBit = digitalRead(SHARP_DOUT);
        digitalWrite(SHARP_ACK, HIGH);
        while (digitalRead(SHARP_BUSY));
        delayMicroseconds(50);
        digitalWrite(SHARP_ACK, LOW);
        dataByte = dataByte | (dataBit << i);
    } 
    digitalWrite(LED_BUILTIN, LOW);

    // Convert the bytes to ASCII
    if (dataByte == 48) dataByte = 'O'; // Letter "O"
    else if (dataByte == 240) dataByte = '0'; // Digital zero

    return dataByte;
}


/**
 * @brief Read a disk command from the pocket computer
 */
void Sharp::ProcessDiskCommand()
{
    Manager::StartDiskCommand();
    long timeout = micros();
    while (true) {
        int data = 0;
        while (!digitalRead(SHARP_BUSY))               // Wait for busy to go HIGH
        {
            if (micros() - timeout > IN_DATAREADY_TIMEOUT) {
                Manager::EndDiskCommand();
                return;
            }
        }
        digitalWrite(SHARP_ACK, HIGH);                  // Acknowlege to HIGH (NAK)
        bitWrite(data, 0, digitalRead(SHARP_SEL1));
        bitWrite(data, 1, digitalRead(SHARP_SEL2));
        bitWrite(data, 2, digitalRead(SHARP_DOUT));
        bitWrite(data, 3, digitalRead(SHARP_DIN));
        while (digitalRead(SHARP_BUSY));                // Wait for busy to go low
        delayMicroseconds(100);
        digitalWrite(SHARP_ACK, LOW);                   // Acknowledge to LOW (ACK)
        while (!digitalRead(SHARP_BUSY));               // Wait for busy to go HIGH
        digitalWrite(SHARP_ACK, HIGH);                  // Acknowlege to HIGH (NAK)
        bitWrite(data, 4, digitalRead(SHARP_SEL1));
        bitWrite(data, 5, digitalRead(SHARP_SEL2));
        bitWrite(data, 6, digitalRead(SHARP_DOUT));
        bitWrite(data, 7, digitalRead(SHARP_DIN));
        while (digitalRead(SHARP_BUSY));                // Wait for busy to go low
        delayMicroseconds(100);
        digitalWrite(SHARP_ACK, LOW);                   // Acknowledge to LOW (ACK)        
        timeout = micros();                             // Reset timeout
        Manager::SendTapeByte(data);                    // Sends the data        
    }
}

void Sharp::SendDiskByte(byte value)
{
    // Initialize
    digitalWrite(SHARP_SEL1, 0);
    digitalWrite(SHARP_SEL2, 0);
    digitalWrite(SHARP_DOUT, 0);
    digitalWrite(SHARP_DIN, 0); 
    digitalWrite(SHARP_ACK, LOW); 
    delayMicroseconds(10);
    pinMode(SHARP_SEL1, OUTPUT);    
    pinMode(SHARP_SEL2, OUTPUT);    
    pinMode(SHARP_DOUT, OUTPUT);    
    pinMode(SHARP_DIN, OUTPUT);    

    // Set the values
    digitalWrite(SHARP_SEL1, bitRead(value, 0));
    digitalWrite(SHARP_SEL2, bitRead(value, 1));
    digitalWrite(SHARP_DOUT, bitRead(value, 2));
    digitalWrite(SHARP_DIN, bitRead(value, 3));     
    // Set ACK to high
    digitalWrite(SHARP_ACK, HIGH);
    while (!digitalRead(SHARP_BUSY));    // Wait for busy to go HIGH
    digitalWrite(SHARP_ACK, LOW);
    delayMicroseconds(50);
    while (digitalRead(SHARP_BUSY));    // Wait for busy to go LOW

    // Set the values
    digitalWrite(SHARP_SEL1, bitRead(value, 4));
    digitalWrite(SHARP_SEL2, bitRead(value, 5));
    digitalWrite(SHARP_DOUT, bitRead(value, 6));
    digitalWrite(SHARP_DIN, bitRead(value, 7));     
    // Set ACK to high
    digitalWrite(SHARP_ACK, HIGH);
    while (!digitalRead(SHARP_BUSY));    // Wait for busy to go HIGH
    digitalWrite(SHARP_ACK, LOW);
    delayMicroseconds(50);
    while (digitalRead(SHARP_BUSY));    // Wait for busy to go LOW

    // Restore input pin mode
    pinMode(SHARP_SEL1, INPUT);    
    pinMode(SHARP_SEL2, INPUT);    
    pinMode(SHARP_DOUT, INPUT);    
    pinMode(SHARP_DIN, INPUT);    
}