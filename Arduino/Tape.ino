#include "Tape.h"
#include "Ascii.h"
#include "Manager.h"
#include "Sharp.h"
#include "Result.h"


namespace Tape
{
    /** Default wait timeout */
    const int TIMEOUT = 1000;           // 1 second timeout


    /**
     * @brief Wait for the XOUT pin to go out high or cancel from manager
     * @returns True if Xout goes high; false if cancelled
    */
    bool WaitForXoutHighOrCancel()
    {
        while (!digitalRead(SHARP_XOUT)) {      
            if (Manager::ReadCancel()) return false;       
        }  
        return true;
    }


    /**
     * @brief Wait for the XOUT pin to go out high
     * @param timeout   The amount to timeout
     * @returns True if Xout goes high; false if timed out
    */
    bool WaitForXoutHigh(int timeout = TIMEOUT)
    {
        unsigned long startTimeout = millis();
        while (!digitalRead(SHARP_XOUT)) {      
            if ((millis() - startTimeout) > timeout) return false;       
        }  
        return true;
    }


    /**
     * @brief Wait for XOUT pin to go low
    */
    bool WaitForXoutLow(int timeout = TIMEOUT)
    {
        unsigned long startTimeout = millis();
        while (digitalRead(SHARP_XOUT)) {      
            if ((millis() - startTimeout) > timeout) return false;       
        }  
        return true;
    }


    /**
     * @brief Sends a square wave pulse to the pocket computer
     * @param duration Number of microseconds to be on and off
     */
    void TapePulseOut(int duration) 
    {
        digitalWrite(SHARP_XIN, HIGH);
        delayMicroseconds(duration);
        digitalWrite(SHARP_XIN, LOW);
        delayMicroseconds(duration);
    }


    /**
     * @brief Sends a single bit over the tap interface
     * @param bit   The bit to send
     */
    void SendTapeBit(bool bit)
    {
        const int pulse8 = 125;  // μs, short pulse CSAVE/CLOAD (8 x pulse8 = HIGH)
        const int pulse4 = 250;  // μs, long  pulse CSAVE/CLOAD (4 x pulse4 = LOW)

        // For every bit, attempt to read from the buffer
        Manager::FillBuffer();

        // Pulse the bits
        if (bit) { // Bit = 1
            for (int z = 0; z < 8; z++) TapePulseOut(pulse8); // 8 short pulses = HIGH
        }
        if (!bit) { // Bit = 0
            for (int z = 0; z < 4; z++) TapePulseOut(pulse4); // 4 long pulses = LOW
        }    
    }


    /**
     * @brief Sends a single byte over the tape interface
     * @param value     Byte to send
     * @param header    True if header byte else data byte
     */
    void SendTapeByte(byte value, bool header)
    {
        SendTapeBit(0);
        for (int j = 4; j < 8; j++) SendTapeBit(bitRead(value, j));
        SendTapeBit(1);
        SendTapeBit(0);
        for (int j = 0; j < 4; j++) SendTapeBit(bitRead(value, j));
        // Send 5 or 2 stop bits
        for (int sb = 0; sb < (header ? 5 : 2); sb++) SendTapeBit(1);
    }


    /**
     * @brief Reads the synchronization 
     * @param startTime The start of the next bit
     * @returns True if successful; false if timed out
    */
    bool ReadSync(unsigned long &startTime)
    {
        int timeout = 10000;  // Initial timeout - 10 seconds
        while (true) {
            // Wait for HIGH of XOUT and return false if timed out
            if (!WaitForXoutHigh(timeout)) return false;
            // Potential start time of the data
            startTime = micros();
            // Wait for low transition
            if (!WaitForXoutLow()) return false;
            // Wait for HIGH of XOUT and return false if timed out
            if (!WaitForXoutHigh()) return false;
            // Calculate the duration of the pulse
            unsigned long duration = micros() - startTime;
            // If duration is greater than 270 then sync over and return start time
            if (duration >= 270) return true;
            // Otherwise continue sync loop
            // After initial loop reset timeout to 1 second
            timeout = 1000;
        }
    }


    /**
     * @brief Reads the synchronization 
     * @param startTime The start of the next bit
     * @param totalSync The amount of time the sync took
     * @returns True if successful; false if timed out
    */
    bool ReadSync(unsigned long &startTime, unsigned long &totalSync)
    {
        unsigned long syncStart = micros();
        bool result = ReadSync(startTime);
        totalSync = micros() - syncStart;
        return result;
    }


    /**
     * @brief Read a bit from the tape
     * @param startTime The time where this bit starts
     * @returns The bit value 
    */
    bool ReadBit(unsigned long startTime = micros())
    {
        const int samplePeriod = 2000;
        int previousState = 0;
        int pulseCount = 0;
        while (micros() - startTime < samplePeriod)
        {
            int currentState = digitalRead(SHARP_XOUT);
            if (previousState != currentState) {
                pulseCount++;
                previousState = currentState;
            }        
        }

        pulseCount /= 2; // Divide by 2 to get the number of cycles (full square waves)
        return pulseCount >= 6 ? 1 : 0; // If 6 or more cycles are detected, it's a 1 bit, otherwise it's a 0 bit
    }


    /**
     * @brief Reads a start bit (negative of the bit)
     * @param startTime The start time of the bit
     * @returns True if start bit
    */
    bool ReadStartBit(unsigned long startTime = micros()) { return !ReadBit(startTime); }


    /**
     * @brief Reads the stop bit
     * @returns True if stop bit
    */
    bool ReadStopBit() { return ReadBit(); }

    /**
     * @brief Reads a byte from the tape after the start bit
     * @returns The byte or error code
    */
    Result ReadByte()
    {
        int result = 0;
        for (int i = 4; i < 8; i++) bitWrite(result, i, ReadBit());
        if (!ReadStopBit()) return ResultType::SyncError;
        if (!ReadStartBit()) return ResultType::SyncError;      // Start bit
        for (int i = 0; i < 4; i++) bitWrite(result, i, ReadBit());
        if (!ReadStopBit()) return ResultType::SyncError;       // Stop bit      
        return result;  
    }


    /**
     * @brief Loads data from the serial port to the pocket computer
     */
    void Load()
    {
        // Read the load tape packet information
        Result length = Manager::WaitReadWord();            // 2 bytes, total length of program
        if (Manager::Error(length)) return;

        Manager::InitializeBuffer(length);
        Result headerCountResult = Manager::WaitReadByte();       // Number of bytes that make up the header
        if (Manager::Error(headerCountResult)) return;

        int headerCount = headerCountResult.Value();

        // Parsing the header was a success
        Manager::SendSuccess();

        // Has the tape prefix been sent
        bool sentPrefix = false;

        while (true) 
        {
            auto data = Manager::ReadBufferByte();
            if (data.IsDone()) break;
            if (Manager::Error(data)) return;

            // Send the prefix if it isn't already sent
            if (!sentPrefix) {
                digitalWrite(LED_BUILTIN, HIGH);
                // Send the prefix  
                for (int i = 0; i < 250; i++) SendTapeBit(1);    
                sentPrefix = true;
            }
            // Send a single byte to the pocket computer
            SendTapeByte(data.Value(), headerCount > 0);
            // Decrement the header count
            if (headerCount > 0) headerCount--;
        }

        // End with 2 stop bits
        for (int sb = 0; sb < 2; sb++) SendTapeBit(1);
        digitalWrite(LED_BUILTIN, LOW);      
    }

    /**
     * @brief Saves data from the pocket computer to the serial port
     */
    void Save()
    {
        // Acknowledge the save packet
        Manager::SendSuccess();

        // Wait for xout to go high
        if (!WaitForXoutHighOrCancel()) {
            Manager::SendFailure(ResultType::Cancelled);
            return;
        }

        // Read the device select
        Result device = Sharp::ReadDeviceSelect();    
        if (device.HasValue() && device.Value() != 0x10) {
            // Device is not tape
            Manager::SendFailure(ResultType::Unexpected);
            return;
        }
    
        unsigned long startTime = 0;
        if (!ReadSync(startTime)) {
            Manager::SendFailure(ResultType::Timeout);
            return;
        }

        bool started = false;                           // Have we started the frame

        while (true) {
            // Start bit
            if (ReadStartBit(startTime)) {
                Result data = ReadByte();
                if (Manager::Error(data)) return;
                if (!started) {
                    Manager::StartFrame();
                    started = true;
                }
                Manager::SendFrameByte(data);
            }

            // Resync
            unsigned long syncTotal = 0;
            if (!ReadSync(startTime, syncTotal)) {
                Manager::SendFailure(ResultType::Timeout);
                return;
            }
            if (syncTotal > 10000) {
                Manager::EndFrame();
                return;
            }
        }
    }    
}