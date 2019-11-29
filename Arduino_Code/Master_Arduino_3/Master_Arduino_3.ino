/////////////////////////////////////////////////////////
// https://forum.arduino.cc/index.php?topic=558963.0
// Arduino UNO
// 10 (SS)
// 11 (MOSI)
// 12 (MISO)
// 13 (SCK)
//
// +5v(if required)
// GND(for signal return)
//
// Arduino Mega (master)  - Arduino Mega (slave)
// 53 (SS)              -- 53 (SS)
// 50 (MISO)            -- 50 (MISO)
// 51 (MOSI)            -- 51 (MOSI)
// 52 (SCK)            --  52 (SCK)
// https://m.blog.naver.com/PostView.nhn?blogId=yuyyulee&logNo=220331139392&proxyReferer=https%3A%2F%2Fwww.google.com%2F
/////////////////////////////////////////////////////////
// In C:\Program Files (x86)\Arduino\hardware\arduino\avr\libraries\SPI\src: SPI.h constructs SPI object as extern SPIClass SPI

// SPI.cpp defines SPI, and is included as part of the whole program.
#include <SPI.h>

// The built-in pin number of the slave, which is used within SPI.Begin()
int ss1 = 53; // connect master pin 53 the first slave pin 53
int ss2 = 49; // connect master pin 49 to the second slave pin 53
int ss3 = 48; // connect master pin 48 to the third  slave pin 53
int ss4 = 47; // connect master pin 47 to the fourth slave pin 53
//int ss5 = 46;

// A total num of LED = 177; each slave processes 40 LEDs
const int NumPixels1 = 30;
const int NumPixles2 = 44;
const int NumPixels3 = 50;
const int NumPixels4 = 53;

const int group1ByteSize = NumPixels1 * 3;
const int group2ByteSize = NumPixles2 * 3;
const int group3ByteSize = NumPixels3 * 3;
const int group4ByteSize = NumPixels4 * 3;
//const int group5ByteSize = 40 * 3;

const int m_totalByteSize = group1ByteSize + group2ByteSize + group3ByteSize + group4ByteSize; // 3 bytes for each of 177 LEDS

int m_accumByteCount = 0;

byte m_recieveBuffer[SERIAL_RX_BUFFER_SIZE];

byte m_totalRecieveBuffer[m_totalByteSize];

// SERIAL_RX_BUFFER_SIZE == 64;
// defined in C:\Program Files (x86)\Arduino\hardware\arduino\avr\cores\arduino\HardWareSerial.h

byte m_showByte = 0;

int m_currentSize = 0;
int m_currentIndex = 0;

void setup (void) {
  // set the Slave Select Pins as outputs:
  pinMode(ss1, OUTPUT);
  pinMode(ss2, OUTPUT);
  pinMode(ss3, OUTPUT);
  pinMode(ss4, OUTPUT);
  //pinMode(ss5, OUTPUT);

  digitalWrite(ss1, HIGH);
  digitalWrite(ss2, HIGH);
  digitalWrite(ss3, HIGH);
  digitalWrite(ss4, HIGH);
  //digitalWrite(ss5, HIGH);

  SPI.begin();
  //To condition the hardware you call SPI.begin () which configures the SPI pins (SCK, MOSI, SS) as outputs.
  //It sets SCK and MOSI low, and SS high.
  //It then enables SPI mode with the hardware in "master" mode. This has the side-effect of setting MISO as an input.

  // Slow down the master a bit
  //SPI.setClockDivider(SPI_CLOCK_DIV8);
  SPI.setClockDivider(SPI_CLOCK_DIV16);
  // Sets the SPI clock divider relative to the system clock.
  // On AVR based boards, the dividers available are 2, 4, 8, 16, 32, 64 or 128.
  // The default setting is SPI_CLOCK_DIV4,
  // which sets the SPI clock to one-quarter the frequency of the system clock (4 Mhz for the boards at 16 MHz).
  // SPI.setBitOrder(MSBFIRST);

  //Serial.begin(9600); // increase the serial comm speed; Unity Script also sets this speed
  Serial.begin(115200); // To read bytes from the PC Unity Script

  //Define another serial port:
  //https://www.arduino.cc/reference/en/language/functions/communication/serial/
  //
  //https://m.blog.naver.com/PostView.nhn?blogId=darknisia&logNo=220569815020&proxyReferer=https%3A%2F%2Fwww.google.com%2F
  // Mega has 4 Serial Ports: Serial, Serial1, Serial2, Serial3.
  // Serial ports are defined by Pin 0 and 1; Serial1 is defined by pins 19(RX), 18(TX).
  // Connect the first USB cable  to Pin 0 and 1 by the ordinary method; Connect the second USB cable from the second
  // USB port in the PC to Pin 19 and 18; Also open another arduino IDE for the second serial port, Serial1.
  // Use the first arduino IDE to upload the arduino code, and use the second arduino IDE to report messages.

  Serial1.begin(115200); // Use Serial1 to send message to the Serial1 Monitor

}

// https://arduino.stackexchange.com/questions/8457/serial-read-vs-serial-readbytes
// readBytes() is blocking until the determined length has been read, or it times out (see Serial.setTimeout()).
// Where read() grabs what has come, if it has come in. Hence available is used to query if it has.
//
// This is why you see the Serial.read() inside a while or if Serial.available.
// Hence I typically employ something like the following: Which emulates readBytes (for the most part).
//
//    #define TIMEOUT = 3000;
//    loop {
//        char inData[20];
//        unsigned long timeout = millis() + TIMEOUT;
//        uint8_t inIndex = 0;
//        while ( ((int32_t)(millis() - timeout) < 0) && (inIndex < (sizeof(inData)/sizeof(inData[0])))) {
//            if (Serial1.available() > 0) {
//                read the incoming byte:
//                inData[inIndex] = Serial.read();
//                if ((c == '\n') || (c == '\r')) {
//                    break;
//                }
//                Serial.write(inData[inIndex++]);
//            }
//        }
//    }
//
//SO: I will stick with using readBytes() because it seems to produce consistent results
//and I can predict the number of bytes I should receive back. –

void loop (void) {
  //https://arduino.stackexchange.com/questions/1726/how-does-the-arduino-handle-serial-buffer-overflow

  // If Serial.read() == -1, it means that head == tail, i.e. there are no bytes to read, that is, underflow happened

  int availCount = Serial.available(); // get the number of bytes already received in the receive buffer of the serial port

  //int HardwareSerial::available(void)
  //    {
  //    return ((unsigned int)(SERIAL_RX_BUFFER_SIZE + _rx_buffer_head - _rx_buffer_tail)) % SERIAL_RX_BUFFER_SIZE;
  //    }
  // 0<= countToRead < SERIAL_RX_BUFFER_SIZE = 64; countToRead = 0 means  head == tail, that is,  when the buffer is empty or full
  //int HardwareSerial::available(void)
  //{
  //	return   ( (unsigned int)(SERIAL_RX_BUFFER_SIZE + _rx_buffer_head - _rx_buffer_tail) ) % SERIAL_RX_BUFFER_SIZE;
  //}

  //https://www.nutsvolts.com/magazine/article/july2011_smileysworkshop;
  //UART uses a ring buffer where head index is incremented when a new byte is written into the buffer
  //https://arduino.stackexchange.com/questions/11710/does-data-coming-in-on-arduino-serial-port-store-for-some-time
  //What happens if the buffer is full and my PC writes an extra character? Does the PC block until there is buffer space,
  //is an old character dropped or is the next character dropped? – Kolban Jun 19 '15 at 12:55
  //2. The next(incoming) character is dropped.– Majenko♦ Jun 19 '15 at 13:17
  // SUM: Yes. The receive ring buffer is 64 bytes and will discard anything past that until the program reads them out of the buffer.


  if ( availCount == 0)
    return;

  // Read countToRead  bytes from the serail port buffer to a buffer; terminated if the determined number (count) is read or it times out
  // The timeout delay is set by Serial.setTimeout(); default to 1000 ms

  //
  if (  (m_accumByteCount + availCount) < m_totalByteSize) {

    int readCount = Serial.readBytes(m_recieveBuffer, availCount);


    // transfer from receiveBuffer to totalReceiveBuffer

    for (int i = 0; i < availCount; i++ )
    {
      m_totalRecieveBuffer[m_accumByteCount + i] = m_recieveBuffer[i];
    }
    //update the current accumulatedByteCount
    m_accumByteCount = m_accumByteCount + availCount;

  }//if (  (m_accumByteCount + availCount) < m_totalByteSize)

  // read count bytes from the tail of the buffer; head == tail when the buffer is empty or full

  // readCount < countToRead  means that timeout has happened.  the 1 s of timeout seems enough to read that.
  // It is assumed that there arises no timeout while trying to read availCount, because that amount of
  // bytes is already in the input ring buffer.


  // report the read bytes to the serial monitor
  //  size_t println(const char[]);
  //		Serial1.println(" read bytes:" + availCount);
  //		for (int i = 0; i < availCount; i++) {
  //			Serial1.println( totalRecieveBuffer[m_accumByteCount + i] );
  //		}


  if ((m_accumByteCount + availCount) >= m_totalByteSize) {

    //If you read availCount, then the total number of bytes exceed m_totalByteSize.
    // So  read less than availCount so that the accumulated bytes become  m_totalByteSize
    // The size to read, countToRead, is determined so that:
    //m_accumByteCount + countToRead =  m_totalByteSize

    int countToRead = m_totalByteSize - m_accumByteCount;

    int readCount = Serial.readBytes(m_recieveBuffer, countToRead); // read count bytes from the tail of the buffer; head == tail when the buffer is empty or full

    // transfer from receiveBuffer to totalReceiveBuffer

    for (int i = 0; i <  countToRead; i++ )
    {
      m_totalRecieveBuffer[m_accumByteCount + i] = m_recieveBuffer[i];
    }
    //update the current accumulatedByteCount
    m_accumByteCount = m_accumByteCount +  countToRead;

    // readCount < countToRead  means that timeout has happened.  the 1 s of timeout seems enough. So it is assumed that
    // timeout does not occur:



    //		Serial1.println(" read bytes:" + countToRead);
    //		for (int i = 0; i < countToRead; i++) {
    //		Serial1.println(totalRecieveBuffer[m_accumByteCount + i]);
    //		}

  }//if ((m_accumByteCount + availCount) >= m_totalByteSize)

  //
  // Now  the m_totalByteSize of bytes is equal to the totalByteSize;
  // So  send the read bytes to  the slaves via SPI communications.


  sendLEDBytesToSlaves(m_totalRecieveBuffer,  m_totalByteSize );

  // print the ledBytes to the serial monitor via Serial1.
  printLEDBytesToSerialMonitor(m_totalRecieveBuffer,  m_totalByteSize);

  m_accumByteCount = 0;

}// loop


// write back the received bytes for further testing:
// Serial.write(totalRecieveBuffer, totalByteSize);
//If the transmit buffer is full then Serial.write() will block until there is enough space in the buffer.
//To avoid blocking calls to Serial.write(), you can first check the amount of free space in the transmit buffer using availableForWrite().





// set random color values to totalRecieveBuffer
//https://gamedev.stackexchange.com/questions/32681/random-number-hlsl

//for test LED
//for (int i = 0; i < totalByteSize/3; i++) {
//  totalRecieveBuffer[3 * i] = (byte) random(10, 255); // from 10 to 254
//  totalRecieveBuffer[3 * i +1] = (byte)random(10, 255);
//  totalRecieveBuffer[3 * i +2] = (byte)random(10, 255);



void  sendLEDBytesToSlaves( byte *totalRecieveBuffer, int m_totalByteSize )
{

  SPI.beginTransaction(SPISettings(14000000, MSBFIRST, SPI_MODE0));


  // SPISettings (which contains SPCR and SPSR)
  //SPISettings mySettting(speedMaximum, dataOrder, dataMode)

  //Parameters
  //speedMaximum: The maximum speed of communication. For a SPI chip rated up to 20 MHz, use 20,000000.
  //Arduino will automatically use the best speed that is equal to or less than the number you use with SPISettings.

  //dataOrder: MSBFIRST or LSBFIRST : Byte transfer from the most significant bit (MSB) Transfer?

  //dataMode : SPI_MODE0, SPI_MODE1, SPI_MODE2, or SPI_MODE3

  //  call SPI.beginTransaction(spiSettings)


  //https://www.dorkbotpdx.org/blog/paul/spi_transactions_in_arduino
  //The clock speed you give to SPISettings is the maximum speed your SPI device can use,
  //not the actual speed your Arduino compatible board can create.

  //The SPISettings code automatically converts the max clock to the fastest clock your board can produce,
  //which doesn't exceed the SPI device's capability.  As Arduino grows as a platform, onto more capable hardware,
  //this approach is meant to allow SPI-based libraries to automatically use new faster SPI speeds.


  // send the first group of data to the first slave:



  digitalWrite(ss1, LOW); // select the first SS line
  digitalWrite(ss2, HIGH);
  digitalWrite(ss3, HIGH);
  digitalWrite(ss4, HIGH);
  //digitalWrite(ss5, HIGH);

  SPI.transfer( &totalRecieveBuffer[0], group1ByteSize);
  digitalWrite(ss1, HIGH);

  SPI.endTransaction();

  // send the second group of data to the second slave:
  SPI.beginTransaction(SPISettings(14000000, MSBFIRST, SPI_MODE0));

  digitalWrite(ss1, HIGH);
  digitalWrite(ss2, LOW); // select the second SS Line
  digitalWrite(ss3, HIGH);
  digitalWrite(ss4, HIGH);
  //digitalWrite(ss5, HIGH);

  SPI.transfer( &totalRecieveBuffer[group1ByteSize], group2ByteSize);
  digitalWrite(ss2, HIGH);

  SPI.endTransaction();

  // send the third group of data to the third slave:
  SPI.beginTransaction(SPISettings(14000000, MSBFIRST, SPI_MODE0));

  digitalWrite(ss1, HIGH);
  digitalWrite(ss2, HIGH);
  digitalWrite(ss3, LOW); // select the third SS line
  digitalWrite(ss4, HIGH);
  //digitalWrite(ss5, HIGH);

  SPI.transfer( &totalRecieveBuffer[group1ByteSize + group2ByteSize], group3ByteSize);
  digitalWrite(ss3, HIGH);

  SPI.endTransaction();

  // send the fourth group of data to the fourth slave:
  //On Mega, default speed is 4 MHz (SPI clock divisor at 4). Max is 8 MHz (SPI clock divisor at 2).
  SPI.beginTransaction(SPISettings(14000000, MSBFIRST, SPI_MODE0));

  digitalWrite(ss1, HIGH);
  digitalWrite(ss2, HIGH);
  digitalWrite(ss3, HIGH);
  digitalWrite(ss4, LOW);   // select the fourth SS line
  //digitalWrite(ss5, HIGH);

  SPI.transfer( &totalRecieveBuffer[group1ByteSize + group2ByteSize  + group3ByteSize ], group4ByteSize);
  digitalWrite(ss4, HIGH);

  SPI.endTransaction();


  // If other libraries use SPI from interrupts,
  // they will be prevented from accessing SPI until you call SPI.endTransaction().


  // delay (10); // delay between LED activation; at least 1 ms
  delay(2);
} //  sendLEDBytesToSlaves(totalRecieveBuffer,  m_totalByteSize )


void printLEDBytesToSerialMonitor( byte * totalRecieveBuffer,  int m_totalByteSize  )
{
  //Serial1.println(" read bytes:" + countToRead);
  for(int i=0; i<m_totalByteSize; i++){
    Serial1.println(totalRecieveBuffer[i]);
  
  }


} //printLEDBytesToSerialMonitor( byte[] totalRecieveBuffer,  int m_totalByteSize  )
