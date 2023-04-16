
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

const long IN_DATAREADY_TIMEOUT = 50000;

namespace Ascii {
    const byte NUL = 0;   // Null char
    const byte SOH = 1;   // Start of Heading
    const byte STX = 2;   // Start of Text
    const byte ETX = 3;   // End of Text
    const byte EOT = 4;   // End of Transmission
    const byte ENQ = 5;   // Enquiry
    const byte ACK = 6;   // Acknowledgment
    const byte BEL = 7;   // Bell
    const byte BS = 8;   // Back Space
    const byte HT = 9;   // Horizontal Tab
    const byte LF = 10;  // Line Feed
    const byte VT = 11;  // Vertical Tab
    const byte FF = 12;  // Form Feed
    const byte CR = 13;  // Carriage Return
    const byte SO = 14;  // Shift Out / X-On
    const byte SI = 15;  // Shift In / X-Off
    const byte DLE = 16;  // Data Line Escape
    const byte DC1 = 17;  // Device Control 1 (oft. XON)
    const byte DC2 = 18;  // Device Control 2
    const byte DC3 = 19;  // Device Control 3 (oft. XOFF)
    const byte DC4 = 20;  // Device Control 4
    const byte NAK = 21;  // Negative Acknowledgement
    const byte SYN = 22;  // Synchronous Idle
    const byte ETB = 23;  // End of Transmit Block
    const byte CAN = 24;  // Cancel
    const byte EM = 25;  // End of Medium
    const byte SUB = 26;  // Substitute
    const byte ESC = 27;  // Escape
    const byte FS = 28;  // File Separator
    const byte GS = 29;  // Group Separator
    const byte RS = 30;  // Record Separator
    const byte US = 31;  // Unit Separator
    const byte ENTER = CR;
    const byte SPACE = 32;
    const byte DELETE = 127;
    const byte SINGLEQUOTE = 39;
}

namespace Command
{
    const int Ping = 1;
    const int StartTape = 2;
    const int TapeHeaderBlock = 3;
    const int TapeDataBlock = 4;
    const int EndType = 5;
    const int StartTapeStream = 6;
}

// For CLOAD test
//------------------
const int pulse8 = 125;  // μs, short pulse bei CSAVE/CLOAD (8 x pulse8 = HIGH)
const int pulse4 = 250;  // μs, long  pulse bei CSAVE/CLOAD (4 x pulse4 = LOW)

const byte TAPE[] = {0x70, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x5F, 0x14, 0x00, 0x0A, 0x0F, 0xDE, 0x22, 0x48, 0x45, 0x4C, 0x4C, 0x4F, 0x20, 0x57, 0x4F, 0x52, 0x4C, 0x44, 0x22, 0x0D, 0xFF, 0xFF, 0xEF };
const int tapesize = sizeof(TAPE);

/*
const byte RAW[] = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 
1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 
1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 
1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 
0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 
1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 1, 1, 1, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 1, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 
0, 1, 0, 0, 1, 1, 0, 1, 1, 1, 0, 1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 1, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 0, 0, 1, 0, 1, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 1, 0, 1, 0, 0, 0, 1, 1, 1, 
1, 0, 0, 0, 1, 0, 1, 0, 1, 1, 1, 1, 1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 1, 0, 1, 1, 0, 0, 0, 1, 0, 1, 0, 1, 1, 1, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 0, 1, 1, 0, 0, 0, 1, 0, 1, 0, 0, 0, 1, 1, 
1, 1, 0, 0, 0, 1, 0, 1, 0, 0, 0, 1, 0, 1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 1, 0, 1, 0, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 0, 1, 0, 1, 
0, 1, 1, 1, 1};
*/
const byte RAW[] = {
    1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 
    1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 
    1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 
    1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 
    1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 
    1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 
    1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 
    1, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 
    0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 
    1, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 0, 0, 0, 1, 
    0, 0, 0, 1, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 1, 1, 1, 1, 0, 0, 
    0, 0, 0, 1, 0, 1, 1, 1, 1, 1, 1, 1, 0, 1, 0, 1, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 1, 1, 
    0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 1, 0, 0, 0, 1, 0, 1, 0, 0, 0, 1, 1, 1, 
    1, 1, 0, 0, 0, 1, 0, 1, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 1, 0, 1, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 
    0, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 0, 0, 1, 0, 1, 0, 1, 1, 1, 1, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 0, 
    1, 0, 0, 1, 1, 1, 0, 0, 0, 1, 0, 1, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 1, 0, 1, 0, 0, 0, 1, 0, 1, 1, 1, 0, 0, 1, 0, 0, 1, 
    0, 0, 1, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 1, 0, 1, 0, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 
    1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1
};
const int rawsize = sizeof(RAW);

const int PACKET_BUFFER_SIZE = 8;        // Size of the serial receive buffer
byte serialBuffer[PACKET_BUFFER_SIZE];      // Serial receive buffer

const int TAPE_BUFFER_SIZE = 32;
byte tapeBuffer[TAPE_BUFFER_SIZE];      // Serial receive buffer
int tapeCount = 0;
int tapeHeaderCount = 0;


const int BUFFER_SIZE = 64;
byte readBuffer[BUFFER_SIZE];      // Serial receive buffer
byte writeBuffer[BUFFER_SIZE];
int readBufferCount = 0;
int readBufferIndex = 0;
int writeBufferCount = 0;
int writeBufferIndex = 0;



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
    Serial.println();
    Serial.println("Ready:");
    Serial.println();
}

const int x = SERIAL_RX_BUFFER_SIZE;

/**
 * @brief The main loop
 */
void loop() 
{   
    // Read value from the serial port
    if (Serial.available()) ProcessSerialPacket();

    // If button is pressed send tape data
    if (digitalRead(BUTTON) == LOW) SendTapeDataOld(TAPE, tapesize); //SendRawData();

    // Read the Xout PIN
    bool xout = digitalRead(SHARP_XOUT);
    
    // if Xout is high, read device select
    if (xout) {
        int device = ReadDeviceSelect();
        Serial.print(device, HEX);
        Serial.println();
        if (device == 0x0F || device == 0x10 || device == 0x45 || device == 0x41) {
            digitalWrite(SHARP_ACK, 1);
            delayMicroseconds(9000);
            digitalWrite(SHARP_ACK, 0);
            // if (device == 0x10) SendRawData();
            if (device == 0x41) ReadDisk();
        }
    }

    // If busy pin and not Xout, read and print printer byte
    if (digitalRead(SHARP_BUSY) && !xout) {
        OutputPrintByte(ReadPrintByte());
    }
} 

void ProcessSerialPacket()
{
    int data = Serial.read();
    if (data == Ascii::SOH) {
        int command = ReadSerialByte();
        if (command == Command::Ping) { /* Success */}
        else if (command == Command::StartTapeStream) StartTapeStream();
        else if (command == Command::StartTape) StartTapeBuffer(); 
        else if (command == Command::EndType) EndTapeBuffer();
        else if (command == Command::TapeHeaderBlock || command == Command::TapeDataBlock)
        {
            int count = ReadSerialByte();
            count = Serial.readBytes(serialBuffer, count);
            SendTapeDataBuffer(serialBuffer, count, command == Command::TapeHeaderBlock);
        }       
        else 
        {
            Serial.write(Ascii::NAK);
            Serial.write(command);
            return;
        }
        Serial.write(Ascii::ACK);
        return;
    }
    if (data == Ascii::SYN) {
        Serial.write(Ascii::SYN);
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
    switch (data) {
        case 13:
            Serial.println();
            break;
        case 48:
            Serial.print("O"); // Letter "O"
            break;
        case 240:
            Serial.print("0"); // Digital zero
            break;
        default:
            if (data > 31 && data < 127) Serial.print(char(data));
    }
}

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

void StartTapeStream()
{
    // Read an acknowledge the header
    int length = ReadSerialWord();
    int remaining = length;
    int headerCount = ReadSerialByte();
    Serial.write(Ascii::ACK);

    // Reset the buffers
    writeBufferCount = 0;
    writeBufferIndex = 0;
    readBufferCount = min(BUFFER_SIZE, remaining);
    readBufferIndex = 0;

    // Has the tape prefix been sent
    bool sentPrefix = false;

    while (true) 
    {
        // If the write buffer contains unsent bytes
        if (writeBufferIndex < writeBufferCount) {
            // Send the prefix if it isn't already sent
            if (!sentPrefix) {
                StartTape();
                sentPrefix = true;
            }
            // Send a single byte to the pocket computer
            SendTapeByte(writeBuffer[writeBufferIndex++], headerCount > 0);
            // Decrement the header count
            if (headerCount > 0) headerCount--;
        }
        else
        {
            // If write buffer empty and no remaining bytes, leave
            if (remaining == 0) break;
        }

        // If a byte is available, add to read buffer
        if (Serial.available() > 0 && readBufferIndex < readBufferCount) {
            readBuffer[readBufferIndex++] = Serial.read();
        }    

        // If the read buffer is full and the write buffer is full
        // Copy the read buffer into the write buffer and receive another packet
        if (readBufferIndex == readBufferCount && writeBufferIndex == writeBufferCount) {
            remaining -= readBufferCount;
            memcpy(writeBuffer, readBuffer, readBufferCount);
            writeBufferCount = readBufferCount;
            writeBufferIndex = 0;
            readBufferCount = min(BUFFER_SIZE, remaining);
            readBufferIndex = 0;
            // Acknowledge the read buffer
            Serial.write(Ascii::ACK);
        }
    }

    // End the tape after buffer completely sent
    EndTape();
}

/**
 * @brief Delay for microseconds but keep filling 
 * @param microseconds 
 */
void DelayAndFillBuffer(unsigned int microseconds)
{
    unsigned long startTime = micros();

    while ((micros() - startTime) < microseconds) {
        if (Serial.available() > 0 && readBufferIndex < readBufferCount) {
            readBuffer[readBufferIndex++] = Serial.read();
        }
    }    
}

void StartTapeBuffer()
{
    tapeCount = 0;
    tapeHeaderCount = 0;
}

void StartTape()
{
    digitalWrite(LED_BUILTIN, HIGH);  
    for (int i = 0; i < 250; i++) SendTapeBit(1);    
}

void SendTapeDataBuffer(byte data[], int count, bool header)
{
    for (int i = 0; i < count; i++) {
        tapeBuffer[tapeCount++] = data[i];
        if (header) tapeHeaderCount++;
    }
}

void SendTapeData(const byte data[], int count, bool header)
{
    for (int i = 0; i < count; i++)
    {
        SendBit(0);
        for (int j = 4; j < 8; j++)
        {
            SendBit(bitRead(data[i], j));
        }
        SendBit(1);
        SendBit(0);
        for (int j = 0; j < 4; j++)
        {
            SendBit(bitRead(data[i], j));
        }
        if (header)
        {
            // In header send 5 stop bits
            for (int sb = 0; sb < 5; sb++) SendBit(1);
        }
        else
        {
            // Otherwise send 2 stop bits
            for (int sb = 0; sb < 2; sb++) SendBit(1);
        }
    }
}

void EndTape()
{
    // End with 2 stop bits
    for (int sb = 0; sb < 2; sb++) SendBit(1);
    digitalWrite(LED_BUILTIN, LOW);      
}

void EndTapeBuffer()
{
    SendTapeDataOld(tapeBuffer, tapeCount);
}

/**
 * @brief Sends the tape data to the pocket computer
 */
void SendTapeDataOld(const byte data[], int count)
{
    StartTape();
    SendTapeData(data, tapeHeaderCount, true);
    SendTapeData(data+tapeHeaderCount, count-tapeHeaderCount, false);
    EndTape();

    /*
    //Serial.println("Sending tape data...");  
    digitalWrite(LED_BUILTIN, HIGH);  
    delay(500);
    for (int i = 0; i < 250; i++) SendBit(1);
    for (int i = 0; i < count; i++)
    {
        SendBit(0);
        for (int j = 4; j < 8; j++)
        {
            SendBit(bitRead(data[i], j));
        }
        SendBit(1);
        SendBit(0);
        for (int j = 0; j < 4; j++)
        {
            SendBit(bitRead(data[i], j));
        }
        if (i <= 9)
        {
            // In header send 5 stop bits
            for (int sb = 0; sb < 5; sb++) SendBit(1);
        }
        else
        {
            // Otherwise send 2 stop bits
            for (int sb = 0; sb < 2; sb++) SendBit(1);
        }
    }

    // End with 2 stop bits
    for (int sb = 0; sb < 2; sb++) SendBit(1);
    digitalWrite(LED_BUILTIN, LOW);  
    //Serial.println("Send Complete.");
    //Serial.println();  
    */
}


/**
 * @brief Sends raw bits to the pocket computer
 */
void SendRawData()
{
    Serial.println("Sending raw data...");  
    delay(500);
    for (int i = 0; i < rawsize; i++) {
        SendBit(RAW[i]);    
    }    
    Serial.println("Send Complete.");
}


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

void SendTapeBit(bool bit)
{
    // For every bit, attempt to read one byte from the buffer
    if (Serial.available() > 0 && readBufferIndex < readBufferCount) readBuffer[readBufferIndex++] = Serial.read();

    if (bit) { // Bit = 1
        for (int z = 0; z < 8; z++) PulseOut(pulse8); // 8 short pulses = HIGH
    }
    if (!bit) { // Bit = 0
        for (int z = 0; z < 4; z++) PulseOut(pulse4); // 4 long pulses = LOW
    }    
}

void TapePulseOut(int duration)
{
    digitalWrite(SHARP_XIN, HIGH);
    DelayAndFillBuffer(duration);
    digitalWrite(SHARP_XIN, LOW);
    DelayAndFillBuffer(duration);    
}


/**
 * @brief Sends a bit to the pocket computer
 * @param bit The bit to send
 */
void SendBit(bool bit) 
{
    if (bit) { // Bit = 1
        for (int z = 0; z < 8; z++) PulseOut(pulse8); // 8 short pulses = HIGH
    }
    if (!bit) { // Bit = 0
        for (int z = 0; z < 4; z++) PulseOut(pulse4); // 4 long pulses = LOW
    }
}


/**
 * @brief Sends a square wave pulse to the pocket computer
 * @param duration Number of microseconds to be on and off
 */
void PulseOut(int duration) 
{
    digitalWrite(SHARP_XIN, HIGH);
    delayMicroseconds(duration);
    digitalWrite(SHARP_XIN, LOW);
    delayMicroseconds(duration);
}

int ReadSerialByte()
{
    unsigned long startTime = millis();
    while (Serial.available() == 0) {
        // Wait for a byte to become available or until the timeout
        if ((millis() - startTime) > 1000) return -1;
    }
    return Serial.read();
}    

int ReadSerialWord()
{
    unsigned long startTime = millis();
    while (Serial.available() < 2) {
        // Wait for a byte to become available or until the timeout
        if ((millis() - startTime) > 1000) return -1;
    }
    return Serial.read() | (Serial.read() << 8);
}
