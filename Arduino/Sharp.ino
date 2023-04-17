#include "Sharp.h"

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
void Sharp::ReadDisk()
{
    long timeout = micros();
    while (true) {
        if (micros() - timeout > IN_DATAREADY_TIMEOUT) break;
        int data = 0;
        while (!digitalRead(SHARP_BUSY));
        digitalWrite(SHARP_ACK, HIGH);
        bitWrite(data, 0, digitalRead(SHARP_SEL1));
        bitWrite(data, 1, digitalRead(SHARP_SEL2));
        bitWrite(data, 2, digitalRead(SHARP_DOUT));
        bitWrite(data, 3, digitalRead(SHARP_DIN));
        delayMicroseconds(50);
        digitalWrite(SHARP_ACK, LOW);
        while (!digitalRead(SHARP_BUSY));    
        digitalWrite(SHARP_ACK, HIGH);
        bitWrite(data, 4, digitalRead(SHARP_SEL1));
        bitWrite(data, 5, digitalRead(SHARP_SEL2));
        bitWrite(data, 6, digitalRead(SHARP_DOUT));
        bitWrite(data, 7, digitalRead(SHARP_DIN));
        delayMicroseconds(50);
        digitalWrite(SHARP_ACK, LOW);
        Serial.print("Disk: ");
        Serial.print(data, HEX);
        Serial.println();    
    }
}

