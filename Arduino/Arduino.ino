#include "Ascii.h"
#include "Manager.h"
#include "Sharp.h"
#include "Tape.h"


/** Physical button */
const int BUTTON = 14;

unsigned long ledMillis = 0;        // will store last time LED was updated
const long ledInterval = 1000;      // LED blink interval

/**
 * @brief Setup the Arduino for connection to Sharp Pocket Computer
 */
void setup() 
{
    Serial.begin(115200);
    pinMode(SHARP_ACK, OUTPUT);
    pinMode(SHARP_XOUT, INPUT);
    pinMode(SHARP_DOUT, INPUT);
    pinMode(SHARP_BUSY, INPUT);
    pinMode(SHARP_XIN, OUTPUT);
    pinMode(SHARP_DIN, INPUT);
    pinMode(SHARP_SEL2, INPUT);
    pinMode(SHARP_SEL1, INPUT);
    pinMode(LED_BUILTIN, OUTPUT);
    pinMode(BUTTON, INPUT_PULLUP);
}


/**
 * @brief The main loop
 */
void loop() 
{   
    // Process manager operations
    Manager::Task();

    // Read the Xout PIN
    bool xout = digitalRead(SHARP_XOUT);
    
    // if Xout is high, read device select
    if (xout) {
        Result device = Sharp::ReadDeviceSelect();       
        if (device.HasValue()) {
            int deviceCode = device.Value();
            if (deviceCode == 0x0F || deviceCode == 0x10 || deviceCode == 0x45 || deviceCode == 0x41) {
                digitalWrite(SHARP_ACK, 1);
                delayMicroseconds(9000);
                digitalWrite(SHARP_ACK, 0);

                Manager::SendDeviceSelect(deviceCode);
                // if (device == 0x10) SendRawData();
                if (deviceCode == 0x41) Sharp::ProcessDiskCommand();
            }
        }
    }

    // If busy pin and not Xout, read and print printer byte
    if (digitalRead(SHARP_BUSY) && !xout) {
        Result printChar = Sharp::ReadPrintByte();
        if (printChar.HasValue()) Manager::SendPrintChar(printChar.Value());
    }

    // Blink the LED
    unsigned long currentMillis = millis();
    if (currentMillis - ledMillis >= ledInterval) {
        // save the last time you blinked the LED
        ledMillis = currentMillis;
        // Flip the LED
        digitalWrite(LED_BUILTIN, digitalRead(LED_BUILTIN) == LOW);
    }      
}
