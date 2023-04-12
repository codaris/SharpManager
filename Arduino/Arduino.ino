
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

/**
 * @brief The main loop
 */
void loop() 
{   
    // Read value from the serial port
    if (Serial.available()) {
        if (Serial.read() == 'A') SendRawData();
    }

    // If button is pressed send tape data
    if (digitalRead(BUTTON) == LOW) SendRawData();

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
            if (device == 0x10) SendRawData();
            if (device == 0x41) ReadDisk();
        }
    }

    // If busy pin and not Xout, read and print printer byte
    if (digitalRead(SHARP_BUSY) && !xout) {
        OutputPrintByte(ReadPrintByte());
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

/**
 * @brief Sends the tape data to the pocket computer
 */
void SendTapeData()
{
    Serial.println("Sending tape data...");  
    digitalWrite(LED_BUILTIN, HIGH);  
    delay(500);
    for (int i = 0; i < 250; i++) SendBit(1);
    for (int i = 0; i < tapesize; i++)
    {
        SendBit(0);
        for (int j = 4; j < 8; j++)
        {
            SendBit(bitRead(TAPE[i], j));
        }
        SendBit(1);
        SendBit(0);
        for (int j = 0; j < 4; j++)
        {
            SendBit(bitRead(TAPE[i], j));
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
    Serial.println("Send Complete.");
    Serial.println();  
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

