#include "Sharp.h"
#include "Manager.h"

namespace Sharp
{
    /** The disk read timeout */
    const long IN_DATAREADY_TIMEOUT = 50000;

    /** Default wait timtout */
    const int TIMEOUT = 10000;           // 1 second timeout

    /**
     * @brief Wait for the specified pin to go low
     * 
     * @param pin       The pin to wait on
     * @param timeout   The timeout amount in milliseconds
     * @return True if success or false for timeout
     */
    bool WaitForLow(uint8_t pin, int timeout = TIMEOUT) 
    {
        unsigned long startTime = millis();
        while(digitalRead(pin)) {
            if ((millis() - startTime) > timeout) return false;
        }
        return true;
    }

    /**
     * @brief Wait for the specified pin to go high
     * 
     * @param pin       The pin to wait on
     * @param timeout   The timeout amount in milliseconds
     * @return True if success or false for timeout
     */
    bool WaitForHigh(uint8_t pin, int timeout = TIMEOUT) 
    {
        unsigned long startTime = millis();
        while(!digitalRead(pin)) {
            if ((millis() - startTime) > timeout) return false;
        }
        return true;
    }

    /**
     * @brief Reads the device select 
     */
    Result ReadDeviceSelect()
    {
        int dataByte = 0;
        delayMicroseconds(50);
        for (int i = 0; i < 8; i++) {
            digitalWrite(SHARP_ACK, HIGH);
            if (!WaitForHigh(SHARP_BUSY, 50)) return ResultType::Timeout;
            delayMicroseconds(50);
            int dataBit = digitalRead(SHARP_DOUT);
            dataByte = dataByte | (dataBit << i);        
            digitalWrite(SHARP_ACK, LOW);
            if (!WaitForLow(SHARP_BUSY)) return ResultType::Timeout;
            delayMicroseconds(150);
        }    
        return dataByte;
    }


    /**
     * @brief Reads a byte to be sent to printer from pocket computer 
     * @returns The byte read
     */
    Result ReadPrintByte()
    {
        int dataByte = 0;
        for (int i = 0; i < 8; i++) {        
        if (!WaitForHigh(SHARP_BUSY)) return ResultType::Timeout;
            delayMicroseconds(50);
            int dataBit = digitalRead(SHARP_DOUT);
            digitalWrite(SHARP_ACK, HIGH);
            if (!WaitForLow(SHARP_BUSY)) return ResultType::Timeout;
            delayMicroseconds(50);
            digitalWrite(SHARP_ACK, LOW);
            dataByte = dataByte | (dataBit << i);
        } 

        // Convert the bytes to ASCII
        if (dataByte == 48) dataByte = 'O'; // Letter "O"
        else if (dataByte == 240) dataByte = '0'; // Digital zero

        return dataByte;
    }


    /**
     * @brief Read a disk command from the pocket computer and sends data frame directly to desktop
     */
    void ProcessDiskCommand()
    {
        Manager::StartDiskCommand();
        Manager::StartFrame();
        long timeout = micros();
        while (true) {
            int data = 0;
            while (!digitalRead(SHARP_BUSY))               // Wait for busy to go HIGH
            {
                if (micros() - timeout > IN_DATAREADY_TIMEOUT) {
                    Manager::EndFrame();
                    return;
                }
            }
            digitalWrite(SHARP_ACK, HIGH);                  // Acknowlege to HIGH (NAK)
            bitWrite(data, 0, digitalRead(SHARP_SEL1));
            bitWrite(data, 1, digitalRead(SHARP_SEL2));
            bitWrite(data, 2, digitalRead(SHARP_DOUT));
            bitWrite(data, 3, digitalRead(SHARP_DIN));
            if (!WaitForLow(SHARP_BUSY)) {                  // Wait for busy to go low
                Manager::SendFailure(ResultType::Timeout);
                return;    
            }
            delayMicroseconds(100);
            digitalWrite(SHARP_ACK, LOW);                   // Acknowledge to LOW (ACK)
            if (!WaitForHigh(SHARP_BUSY)) {                 // Wait for busy to go HIGH
                Manager::SendFailure(ResultType::Timeout);
                return;    
            }
            digitalWrite(SHARP_ACK, HIGH);                  // Acknowlege to HIGH (NAK)
            bitWrite(data, 4, digitalRead(SHARP_SEL1));
            bitWrite(data, 5, digitalRead(SHARP_SEL2));
            bitWrite(data, 6, digitalRead(SHARP_DOUT));
            bitWrite(data, 7, digitalRead(SHARP_DIN));
            if (!WaitForLow(SHARP_BUSY)) {                  // Wait for busy to go low
                Manager::SendFailure(ResultType::Timeout);
                return;    
            }
            delayMicroseconds(100);
            digitalWrite(SHARP_ACK, LOW);                   // Acknowledge to LOW (ACK)        
            timeout = micros();                             // Reset timeout
            Manager::SendFrameByte(data);                   // Sends the data        
        }
    }

    /**
     * @brief Sends a disk byte to the pocket computer
     * @param value         Byte to send
     * @return Result of sending the byte
     */
    ResultType SendDiskByte(byte value)
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
        if (!WaitForHigh(SHARP_BUSY)) return ResultType::Timeout;   // Wait for busy to go HIGH
        digitalWrite(SHARP_ACK, LOW);
        delayMicroseconds(50);
        if (!WaitForLow(SHARP_BUSY)) return ResultType::Timeout;    // Wait for busy to go LOW

        // Set the values
        digitalWrite(SHARP_SEL1, bitRead(value, 4));
        digitalWrite(SHARP_SEL2, bitRead(value, 5));
        digitalWrite(SHARP_DOUT, bitRead(value, 6));
        digitalWrite(SHARP_DIN, bitRead(value, 7));     
        // Set ACK to high
        digitalWrite(SHARP_ACK, HIGH);
        if (!WaitForHigh(SHARP_BUSY)) return ResultType::Timeout;   // Wait for busy to go HIGH
        digitalWrite(SHARP_ACK, LOW);
        delayMicroseconds(50);
        if (!WaitForLow(SHARP_BUSY)) return ResultType::Timeout;    // Wait for busy to go LOW

        // Restore input pin mode
        pinMode(SHARP_SEL1, INPUT);    
        pinMode(SHARP_SEL2, INPUT);    
        pinMode(SHARP_DOUT, INPUT);    
        pinMode(SHARP_DIN, INPUT);    
        return ResultType::Ok;
    }
}