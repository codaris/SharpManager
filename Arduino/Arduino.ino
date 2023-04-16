#include "Ascii.h"
#include "SharpManager.h"

/** Sharp pins */
const int SHARP_BUSY = 9;   // Pin D9, Busy   Sharp pin 4 
const int SHARP_DOUT = 8;   // Pin D8, Dout   Sharp pin 5 
const int SHARP_XIN = 7;    // Pin D7, Xin    Sharp pin 6
const int SHARP_XOUT = 6;   // Pin D6, Xout   Sharp pin 7 
const int SHARP_DIN = 5;    // Pin D5, Din    Sharp pin 8
const int SHARP_ACK = 4;    // Pin D4, ACK    Sharp pin 9 
const int SHARP_SEL2 = 3;   // Pin D3, SEL2   Sharp pin 10
const int SHARP_SEL1 = 2;   // Pin D2, SEL1   Sharp pin 11

/** Physical button */
const int BUTTON = 14;

/** The disk read timeout */
const long IN_DATAREADY_TIMEOUT = 50000;

/** The serial and tape buffers */
const int TAPE_BUFFER_SIZE = 64;        // The size of the serial and tape buffers
byte serialBuffer[TAPE_BUFFER_SIZE];    // Serial receive buffer
byte tapeBuffer[TAPE_BUFFER_SIZE];      // The tape buffer
int serialBufferCount = 0;              // The number of bytes to read into the serial buffer (no larger than TAPE_BUFFER_SIZE)
int serialBufferIndex = 0;              // The index of the next position to read into the buffer
int tapeBufferCount = 0;                // The number of items in the tape buffer
int tapeBufferIndex = 0;                // The next byte to process in the tape buffer

const int TEST_BUFFER_SIZE = 256;
byte testBuffer[TEST_BUFFER_SIZE];
int testBufferIndex = 0;


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

const int x = SERIAL_RX_BUFFER_SIZE;

/**
 * @brief The main loop
 */
void loop() 
{   
    // Process a packet from the serial port
    if (Serial.available()) ProcessSerialPacket();

    // If button is pressed send tape data
    if (digitalRead(BUTTON) == LOW) ReadTape();

    // Read the Xout PIN
    bool xout = digitalRead(SHARP_XOUT);
    
    // if Xout is high, read device select
    if (xout) {
        int device = ReadDeviceSelect();       
        if (device == 0x0F || device == 0x10 || device == 0x45 || device == 0x41) {
            digitalWrite(SHARP_ACK, 1);
            delayMicroseconds(9000);
            digitalWrite(SHARP_ACK, 0);

            Serial.write(Ascii::SOH);
            Serial.write(Command::DeviceSelect);
            Serial.write(device);

            // if (device == 0x10) SendRawData();
            // if (device == 0x41) ReadDisk();
        }
    }

    // If busy pin and not Xout, read and print printer byte
    if (digitalRead(SHARP_BUSY) && !xout) {
        OutputPrintByte(ReadPrintByte());
    }
} 

/**
 * @brief Process the serial packets coming from SharpManager
 */
void ProcessSerialPacket()
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
            LoadFromTape();
            break;
        default:
            // Unknown command error
            Serial.write(Ascii::NAK);
            Serial.write(ErrorCode::Unexpected);
            return;
    }
}


/**
 * @brief Reads the device select 
 */
int ReadDeviceSelect()
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
int ReadPrintByte()
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
    return dataByte;
}


/**
 * @brief Outputs the printer byte to the serial connection performing any transformations
 * @param data  The byte to output
 */
void OutputPrintByte(int data)
{
    if (data == 48) data = 'O'; // Letter "O"
    else if (data == 240) data = '0'; // Digital zero
    Serial.write(Ascii::SOH);
    Serial.write(Command::Print);
    Serial.write(data);
}


/**
 * @brief Read a disk command from the pocket computer
 */
void ReadDisk()
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


/**
 * @brief Loads data from the serial port to the pocket computer
 */
void LoadFromTape()
{
    // Read the load tape packet information
    int length = WaitReadWord();            // 2 bytes, total length of program
    if (length == -1) {
        Serial.write(Ascii::NAK);
        Serial.write(ErrorCode::Timeout);
        return;
    }

    int remaining = length;                 // Number of bytes remaining to be read from PC

    int headerCount = WaitReadByte();       // Number of bytes that make up the header
    if (length == -1) {
        Serial.write(Ascii::NAK);
        Serial.write(ErrorCode::Timeout);
        return;
    }

    Serial.write(Ascii::ACK);               // Acknowledge the packet

    // Reset the buffers
    tapeBufferCount = 0;
    tapeBufferIndex = 0;
    serialBufferCount = min(TAPE_BUFFER_SIZE, remaining);
    serialBufferIndex = 0;

    // Has the tape prefix been sent
    bool sentPrefix = false;

    while (true) 
    {
        // If the write buffer contains unsent bytes
        if (tapeBufferIndex < tapeBufferCount) {
            // Send the prefix if it isn't already sent
            if (!sentPrefix) {
                StartTape();
                sentPrefix = true;
            }
            // Send a single byte to the pocket computer
            SendTapeByte(tapeBuffer[tapeBufferIndex++], headerCount > 0);
            // Decrement the header count
            if (headerCount > 0) headerCount--;
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
        if (serialBufferIndex == serialBufferCount && tapeBufferIndex == tapeBufferCount) {
            remaining -= serialBufferCount;
            memcpy(tapeBuffer, serialBuffer, serialBufferCount);
            tapeBufferCount = serialBufferCount;
            tapeBufferIndex = 0;
            serialBufferCount = min(TAPE_BUFFER_SIZE, remaining);
            serialBufferIndex = 0;
            // Acknowledge the read buffer
            Serial.write(Ascii::ACK);
        }
    }

    // End the tape after buffer completely sent
    EndTape();
}


/**
 * @brief Sends the initial 250 1 bits at the start of the tape
 */
void StartTape()
{
    digitalWrite(LED_BUILTIN, HIGH);  
    for (int i = 0; i < 250; i++) SendTapeBit(1);    
}


/**
 * @brief Sends 2 stop bits to end the transmission
 */
void EndTape()
{
    // End with 2 stop bits
    for (int sb = 0; sb < 2; sb++) SendTapeBit(1);
    digitalWrite(LED_BUILTIN, LOW);      
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
 * @brief Sends a single bit over the tap interface
 * @param bit   The bit to send
 */
void SendTapeBit(bool bit)
{
    const int pulse8 = 125;  // μs, short pulse CSAVE/CLOAD (8 x pulse8 = HIGH)
    const int pulse4 = 250;  // μs, long  pulse CSAVE/CLOAD (4 x pulse4 = LOW)

    // For every bit, attempt to read one byte from the buffer
    if (Serial.available() > 0 && serialBufferIndex < serialBufferCount) serialBuffer[serialBufferIndex++] = Serial.read();

    // Pulse the bits
    if (bit) { // Bit = 1
        for (int z = 0; z < 8; z++) TapePulseOut(pulse8); // 8 short pulses = HIGH
    }
    if (!bit) { // Bit = 0
        for (int z = 0; z < 4; z++) TapePulseOut(pulse4); // 4 long pulses = LOW
    }    
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
 * @brief Waits for a single byte to become available on the serial interface
 * @return int      Byte read or -1 if timed out
 */
int WaitReadByte()
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
 * @brief Synchronize and return the timestamp of the start of the tape data
 * @return unsigned long 
 */
bool TapeReadSync2()
{
    while (true) {
        // Wait for HIGH of XOUT and return zero if timed out
        unsigned long startTimeout = millis();
        while (!digitalRead(SHARP_XOUT)) {      
            if ((millis() - startTimeout) > 1000) return false;       
        }  
        // Potential start time of the data
        unsigned long startTime = micros();
        // Wait for low transition
        while (digitalRead(SHARP_XOUT));       

        // Wait for HIGH of XOUT and return zero if timed out
        startTimeout = millis();
        while (!digitalRead(SHARP_XOUT)) {    
            if ((millis() - startTimeout) > 1000) return false;       
        }  

        // Calculate the duration of the pulse
        unsigned long duration = micros() - startTime;
        // If duration is greater than 270 then sync over and return start time
        if (duration >= 270) return true;
    }
}


unsigned long TapeReadSync()
{
    while (true) {
        // Wait for HIGH of XOUT and return zero if timed out
        unsigned long startTimeout = millis();
        while (!digitalRead(SHARP_XOUT)) {      
            if ((millis() - startTimeout) > 1000) return 0;       
        }  
        // Potential start time of the data
        unsigned long startTime = micros();
        // Wait for low transition
        while (digitalRead(SHARP_XOUT));       

        // Wait for HIGH of XOUT and return zero if timed out
        startTimeout = millis();
        while (!digitalRead(SHARP_XOUT)) {    
            if ((millis() - startTimeout) > 1000) return 0;       
        }  

        // Calculate the duration of the pulse
        unsigned long duration = micros() - startTime;
        // If duration is greater than 270 then sync over and return start time
        if (duration >= 270) return startTime;
    }
}

bool ReadTapeBit(unsigned long startTime = micros());
bool ReadStopBit(unsigned long startTime = micros());
bool ReadStartBit(unsigned long startTime = micros());

int ReadTapeByte()
{
    int result = 0;
    for (int j = 4; j < 8; j++) bitWrite(result, j, ReadTapeBit());
    if (!ReadStopBit()) return -1;      // Stop bit
    if (!ReadStartBit()) return -2;       // Start bit
    for (int j = 0; j < 4; j++) bitWrite(result, j, ReadTapeBit());
    if (!ReadStopBit()) return -3;      // Stop bit      
    return result;  
}


bool ReadTapeBit(unsigned long startTime = micros())
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


inline bool ReadStartBit(unsigned long startTime = micros()) { return !ReadTapeBit(startTime); }
inline bool ReadStopBit(unsigned long startTime = micros()) { return ReadTapeBit(startTime); }


void ReadTape()
{
    Serial.println("Waiting for CSAVE...");

    // Wait for xout to go high
    unsigned long startTimeout = millis();
    while (!digitalRead(SHARP_XOUT)) {      
        if ((millis() - startTimeout) > 10000) {
            Serial.println("Timeout");
            return;
        }
    }  

    // Read the device select
    int device = ReadDeviceSelect(); 
    Serial.print("Device select: 0x");
    Serial.println(device, HEX);

    Serial.println("Starting sync...");
    unsigned long startTime = TapeReadSync();
    if (startTime == 0) {
        Serial.println("Timeout");
        return;
    }

    // Serial.println("Reading tape data...");
    bool headerMarker = false;                      // Have we see the end of header byte
    bool header = true;                             // Are we in the header portion
    int value = 0;

    while (true) {
        // Start bit
        if (ReadStartBit(startTime)) {
            value = ReadTapeByte();
            if (headerMarker) header = false;           // Read one byte past header marker
            if (value == 0x5F) headerMarker = true;     // Read the header marker
            if (value >= 0) {
                Serial.print(value, HEX);
                Serial.print(" ");
            } else {
                if (value == -2) Serial.println("Timeout");
                else if (value == -1) Serial.println("error");
                Serial.println(value);
                break;
            }
        }
        // Resync
        unsigned long syncTime = micros();
        startTime = TapeReadSync();
        if ((micros() - syncTime) > 10000) {
            Serial.println();
            Serial.println("Done.");
            return;
        }
    }
}

