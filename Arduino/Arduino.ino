#include "Ascii.h"
#include "Manager.h"
#include "Sharp.h"
#include "Tape.h"


/** Physical button */
const int BUTTON = 14;


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
    // Process a packet from the serial port
    if (Serial.available()) Manager::ProcessPacket();

    // If button is pressed send tape data
    if (digitalRead(BUTTON) == LOW) Tape::Save();

    // Read the Xout PIN
    bool xout = digitalRead(SHARP_XOUT);
    
    // if Xout is high, read device select
    if (xout) {
        int device = Sharp::ReadDeviceSelect();       
        if (device == 0x0F || device == 0x10 || device == 0x45 || device == 0x41) {
            digitalWrite(SHARP_ACK, 1);
            delayMicroseconds(9000);
            digitalWrite(SHARP_ACK, 0);

            Manager::SendDeviceSelect(device);
            // if (device == 0x10) SendRawData();
            // if (device == 0x41) ReadDisk();
        }
    }

    // If busy pin and not Xout, read and print printer byte
    if (digitalRead(SHARP_BUSY) && !xout) {
        Manager::SendPrintChar(Sharp::ReadPrintByte());
    }
}
