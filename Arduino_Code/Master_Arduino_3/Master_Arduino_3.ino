/////////////////////////////////////////////////////////
//SPI Tutorial: https://learn.sparkfun.com/tutorials/serial-peripheral-interface-spi/all
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
const int ss1 = 37; // connect master pin 53 the first slave pin 53
const int ss2 = 49; // connect master pin 49 to the second slave pin 53
const int ss3 = 48; // connect master pin 48 to the third  slave pin 53
const int ss4 = 47; // connect master pin 47 to the fourth slave pin 53
//int ss5 = 46;

// A total num of LED = 186; each slave processes 40 LEDs
const int NumPixels1 = 40;
const int NumPixels2 = 44;
const int NumPixels3 = 50;
const int NumPixels4 = 52;

const int m_totalNumOfPixels = NumPixels1 +  NumPixels2 + NumPixels3 +  NumPixels4 + 2;
//const int m_totalNumOfPixels = NumPixels1 +  NumPixels2 + NumPixels3 +  NumPixels4;
// include the start 3 bytes and the end 3 bytes

const int m_totalByteSize = m_totalNumOfPixels * 3;


const int group1ByteSize = NumPixels1 * 3;
const int group2ByteSize = NumPixels2 * 3;
const int group3ByteSize = NumPixels3 * 3;
const int group4ByteSize = NumPixels4 * 3;


byte m_receiveBuffer[SERIAL_RX_BUFFER_SIZE];


byte m_totalReceiveBuffer[m_totalByteSize] ;


// SERIAL_RX_BUFFER_SIZE == 64;
// defined in C:\Program Files (x86)\Arduino\hardware\arduino\avr\cores\arduino\HardWareSerial.h

byte m_startBytes[3]  = {0, 0, 0}; // This full black color indicates the start of a single frame of LEDs.
byte m_endBytes[3]  = {255, 255, 255}; // This full white color indicates the end a single frame of LEDs.


boolean m_newFrameHasArrived = false;
// newFrameHasArrived is true when m_totalNumOfPixels of LED Pixel Data has arrived but not yet displayed or sent


SPISettings SPISettingA (4000000, MSBFIRST, SPI_MODE0); // 14MHz = speed; slave 1
SPISettings SPISettingB (4000000, MSBFIRST, SPI_MODE0); // 14MHz = speed; slave 2
SPISettings SPISettingC (4000000, MSBFIRST, SPI_MODE0); // 14MHz = speed; slave 3
SPISettings SPISettingD (4000000, MSBFIRST, SPI_MODE0); // 14MHz = speed; slave 4

//SPISettings mySettting(speedMaximum, dataOrder, dataMode)

//Parameters
//speedMaximum: The maximum speed of communication. For a SPI chip rated up to 20 MHz, use 20,000000.
//Arduino will automatically use the best speed that is equal to or less than the number you use with SPISettings.

//On Mega, default speed is 4 MHz (SPI clock divisor at 4). Max is 8 MHz (SPI clock divisor at 2).
//SPI can operate at extremely high speeds (millions of bytes per second), which may be too fast for some devices.
//To accommodate such devices, you can adjust the data rate.
//In the Arduino SPI library, the speed is set by the setClockDivider() function,
//which divides the master clock (16MHz on most Arduinos) down to a frequency between 8MHz (/2) and 125kHz (/128).
//https://www.dorkbotpdx.org/blog/paul/spi_transactions_in_arduino
//The clock speed you give to SPISettings is the maximum speed your SPI device can use,
//not the actual speed your Arduino compatible board can create.

//dataOrder: MSBFIRST or LSBFIRST : Byte transfer from the most significant bit (MSB) Transfer?
//dataMode : SPI_MODE0, SPI_MODE1, SPI_MODE2, or SPI_MODE3

//The SPISettings code automatically converts the max clock to the fastest clock your board can produce,
//which doesn't exceed the SPI device's capability.  As Arduino grows as a platform, onto more capable hardware,
//this approach is meant to allow SPI-based libraries to automatically use new faster SPI speeds.

void setup (void) {


  // set the Slave Select Pins (SS)  as outputs:

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

  SPI.begin(); // set up:
  //To condition the hardware you call SPI.begin () which configures the SPI pins (SCK, MOSI, SS) as outputs.
  //It sets SCK and MOSI low, and SS high.
  //It then enables SPI mode with the hardware in "master" mode. This has the side-effect of setting MISO as an input.

  //Serial.begin(9600); // increase the serial comm speed; Unity Script also sets this speed
  // Serial.begin(115200); // To read bytes from the PC Unity Script

  //Define another serial port:
  //https://www.arduino.cc/reference/en/language/functions/communication/serial/
  //
  //https://m.blog.naver.com/PostView.nhn?blogId=darknisia&logNo=220569815020&proxyReferer=https%3A%2F%2Fwww.google.com%2F
  // Mega has 4 Serial Ports: Serial, Serial1, Serial2, Serial3.
  // Serial ports are defined by Pin 0 and 1; Serial1 is defined by pins 19(RX), 18(TX).
  // Connect the first USB cable  to Pin 0 and 1 by the ordinary method; Connect the second USB cable from the second
  // USB port in the PC to Pin 19 and 18; Also open another arduino IDE for the second serial port, Serial1.
  // Use the first arduino IDE to upload the arduino code, and use the second arduino IDE to report messages.

  // Serial1.begin(115200); // Use Serial1 to send message to the Serial1 Monitor
  Serial.begin(57600);
  Serial1.begin(57600);


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
  //Serial1.print("I am within available");

  // if (Serial.available() > 0 ) {
  //
  //    //Serial1.print("I am within available");
  //
  //    char c = Serial.read();
  //
  //    Serial1.write(c);
  //  }

  // If Serial.read() == -1, it means that head == tail, i.e. there are no bytes to read, that is, underflow happened
  //Serial1.print("b:");
  readFrameWithStartEndBytes(); // read until a new frame of LED data arrives


  showNewFrame(); // display the new frame of LED data that has arrived

} // loop()

void readFrameWithStartEndBytes()
{

  static boolean startBytesHaveArrived = false; // The boolean variable indicating whether the start bytes of a frame has arrived and thus

  static int index = 0; // index of the buffer

  byte c; // received byte



  while ( Serial.available() && m_newFrameHasArrived == false ) //  read the serial port while newFrameHasArrived is false, that while
    //a new frame of LED data has not arrived or has not been displayed or sent
    //  During this time, do not read the serial port. Continue to read the port
    //when the arrived frame of data has been displayed and thereby newFrameHasArrived beocmes false;
    // while ( Serial.available() )
  {
    byte c = Serial.read(); // the read the first byte in the serial port buffer
    m_totalReceiveBuffer[index] = c;

    if ( index == m_totalByteSize - 1) {
      // the receive buffer is full without the end bytes arriving
      // reset everything:
      m_newFrameHasArrived == true;
      //m_newFrameHasArrived == false;

      if (index % 3  == 2) {
        Serial1.print("index0 : ");
        Serial1.print(index);
        Serial1.print("     ");
        Serial1.print(m_totalReceiveBuffer[index - 2] );
        Serial1.print("     " );
        Serial1.print(m_totalReceiveBuffer[index - 1] );
        Serial1.print("     " );
        Serial1.println(m_totalReceiveBuffer[index - 0] );
      }
      Serial1.println("??????????????????????????????????????????????????????");
      index = 0;

    } //  // the receive buffer is full without the end bytes arriving

    else {

      // Serial1.println("the receive buffer is not full");
      //      c = Serial.read(); // the read the first byte in the serial port buffer
      //      m_totalReceiveBuffer[index] = c;

      if ( startBytesHaveArrived  == true )  { // true when the start bytes has been arrived

        // check if the end bytes for the current frame of data has arrived

        if ( !checkEndBytesHaveArrived( m_totalReceiveBuffer, index, m_endBytes) ) { // if the end marker has not received => continue to read the next byte
          if (index % 3  == 2) {
            Serial1.print("index1 : ");
            Serial1.print(index);
            Serial1.print("     ");
            Serial1.print(m_totalReceiveBuffer[index - 2] );
            Serial1.print("     " );
            Serial1.print(m_totalReceiveBuffer[index - 1] );
            Serial1.print("     " );
            Serial1.println(m_totalReceiveBuffer[index - 0] );
          }

          index++; // continue to read

        }
        else { // the end bytes has just arrived at the index-2, index-1 and index of the totalReceiveBuffer
          // check if the total number of bytes from the start bytes to  the end bytes is equal to m_totalByteSize.
          if ( index == m_totalByteSize  - 1 ) {   // Is the current index of the buffer  the last element of the buffer when the end bytes has arrived
            Serial1.println("************************************************");
            if (index % 3  == 2) {
              Serial1.print("index2 : ");
              Serial1.print(index);
              Serial1.print("     ");
              Serial1.print(m_totalReceiveBuffer[index - 2] );
              Serial1.print("     " );
              Serial1.print(m_totalReceiveBuffer[index - 1] );
              Serial1.print("     " );
              Serial1.println(m_totalReceiveBuffer[index - 0] );
            }

            m_newFrameHasArrived = true;

            startBytesHaveArrived  = false; // "The frame InProgress" is made false after the end bytes arrived

            index = 0; // The receive buffer is full so start to read into the first byte of the buffer in the next round

          }// (the buffer fully filled)

          else { // the total number of bytes from the start bytes to the end bytes is not equal to the required number of bytes for a correct frame.
            // => ignore the read data and re-start the process of receiving a frame of LED data
            if (index % 3  == 2) {
              Serial1.print("index3 : ");
              Serial1.print(index);
              Serial1.print("     ");
              Serial1.print(m_totalReceiveBuffer[index - 2] );
              Serial1.print("     " );
              Serial1.print(m_totalReceiveBuffer[index - 1] );
              Serial1.print("     " );
              Serial1.println(m_totalReceiveBuffer[index - 0] );
            }

            index = 0; // go to the start of the receive buffer            startBytesHaveArrived = false; // made false after a wrong number of data has been read from the start bytes to the end bytes

          }
        }  // else: the end bytes has just arrived


      }// ( startBytesHaveArrived == true )

      else // ( startBytesHaveArrived == false)
        // check if the start bytes has arrived at the index-2, index-1, and index of the receive buffer
        if ( checkStartBytesHaveArrived( m_totalReceiveBuffer, index, m_startBytes) ) {

          startBytesHaveArrived = true; // made true when the start bytes has arrived
          if (index % 3  == 2) {
            Serial1.print("index4 : ");
            Serial1.print(index);
            Serial1.print("     ");
            Serial1.print(m_totalReceiveBuffer[index - 2] );
            Serial1.print("     " );
            Serial1.print(m_totalReceiveBuffer[index - 1] );
            Serial1.print("     " );
            Serial1.println(m_totalReceiveBuffer[index - 0] );
          }

          index++; // continue to read

        }

        else { // the start bytes has not arrived
          if (index % 3  == 2) {
            Serial1.print("index5 : ");
            Serial1.print(index);
            Serial1.print("     ");
            Serial1.print(m_totalReceiveBuffer[index - 2] );
            Serial1.print("     " );
            Serial1.print(m_totalReceiveBuffer[index - 1] );
            Serial1.print("     " );
            Serial1.println(m_totalReceiveBuffer[index - 0] );
          }


          index++; // continue to read

        }
    } // // the receive buffer is being filled

  } //  while ( Serial.available() && newFrameHasArrived == false )


} // readWithStartEndMarkers()

void showNewFrame() {
  if ( m_newFrameHasArrived == true ) {

    //sendLEDBytesToSlaves(m_totalReceiveBuffer,  m_totalByteSize );

    // print the ledBytes to the serial monitor via Serial1.

    printLEDBytesToSerialMonitor(m_totalReceiveBuffer,  m_totalByteSize );

    m_newFrameHasArrived = false;
  } // ( m_newFrameHasArrived == true )


}// showNewFrame()

boolean checkStartBytesHaveArrived( byte totalReceiveBuffer[], int index, byte startBytes[] ) {


  if ( totalReceiveBuffer[index] == startBytes[2] && totalReceiveBuffer[index - 1] == startBytes[1]
       && totalReceiveBuffer[index - 2]  == startBytes[0] ) {

    return true;

  }
  else return false;

}//checkStartBytesHaveArrived()

boolean checkEndBytesHaveArrived( byte totalReceiveBuffer[], int index, byte endBytes[] ) {


  if ( totalReceiveBuffer[index] == endBytes[2] && totalReceiveBuffer[index - 1] == endBytes[1]
       && totalReceiveBuffer[index - 2]  == endBytes[0] ) {

    return true;

  }
  else return false;

}//checkEndBytesHaveArrived()


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

// set random color values to totalRecieveBuffer
//https://gamedev.stackexchange.com/questions/32681/random-number-hlsl

//for test LED
//for (int i = 0; i < totalByteSize/3; i++) {
//  totalRecieveBuffer[3 * i] = (byte) random(10, 255); // from 10 to 254
//  totalRecieveBuffer[3 * i +1] = (byte)random(10, 255);
//  totalRecieveBuffer[3 * i +2] = (byte)random(10, 255);


void  sendLEDBytesToSlaves( byte *totalReceiveBuffer, int totalByteSize )
{
  // use deviceA
  SPI.beginTransaction(SPISettingA);

  //https://forum.arduino.cc/index.php?topic=52111.0
  //It is because they share the pins that we need the SS line. With multiple slaves,
  //only one slave is allowed to "own" the MISO line(by configuring it as an output).So when SS is brought low
  //for that slave it switches its MISO line from high - impedance to output, then it can reply to requests
  //from the master.When the SS is brought high again(inactive) that slave must reconfigure that line as high - impedance,
  //so another slave can use it.


  // send the first group of data to the first slave:

  digitalWrite(ss1, LOW); // select the first SS line
  digitalWrite(ss2, HIGH);
  digitalWrite(ss3, HIGH);
  digitalWrite(ss4, HIGH);
  //digitalWrite(ss5, HIGH);

  // To send  a sequence of bytes to a slave arduiono via SPI, the sequence is marked by the start and the end
  // of the sequence with special bytes, m_startByte and m_endByte respectivley.

  SPI.transfer( m_startBytes, 3);
  SPI.transfer( &totalReceiveBuffer[3], group1ByteSize);
  SPI.transfer( m_endBytes, 3);

  digitalWrite(ss1, HIGH);

  SPI.endTransaction();

  // send the second group of data to the second slave:
  SPI.beginTransaction(SPISettingB);

  digitalWrite(ss1, HIGH);
  digitalWrite(ss2, LOW); // select the second SS Line
  digitalWrite(ss3, HIGH);
  digitalWrite(ss4, HIGH);
  //digitalWrite(ss5, HIGH);


  SPI.transfer( m_startBytes, 3);
  SPI.transfer( &totalReceiveBuffer[3 + group1ByteSize], group2ByteSize);
  SPI.transfer( m_endBytes, 3);

  digitalWrite(ss2, HIGH);

  SPI.endTransaction();

  // send the third group of data to the third slave:
  SPI.beginTransaction(SPISettingC);

  digitalWrite(ss1, HIGH);
  digitalWrite(ss2, HIGH);
  digitalWrite(ss3, LOW); // select the third SS line
  digitalWrite(ss4, HIGH);
  //digitalWrite(ss5, HIGH);

  SPI.transfer( m_startBytes, 3);
  SPI.transfer( &totalReceiveBuffer[3 + group1ByteSize + group2ByteSize], group3ByteSize);
  SPI.transfer( m_endBytes, 3);

  digitalWrite(ss3, HIGH);

  SPI.endTransaction();

  // send the fourth group of data to the fourth slave:

  SPI.beginTransaction(SPISettingD);

  digitalWrite(ss1, HIGH);
  digitalWrite(ss2, HIGH);
  digitalWrite(ss3, HIGH);
  digitalWrite(ss4, LOW);   // select the fourth SS line
  //digitalWrite(ss5, HIGH);

  SPI.transfer( m_startBytes, 3);
  SPI.transfer( &totalReceiveBuffer[3 + group1ByteSize + group2ByteSize  + group3ByteSize ], group4ByteSize );
  SPI.transfer( m_endBytes, 3);

  digitalWrite(ss4, HIGH);

  SPI.endTransaction();

  // If other libraries use the SPI (hardware resource)  from interrupts,
  // they will be prevented from accessing SPI until you call SPI.endTransaction().

  // delay (10); // delay between LED activation; at least 1 ms
  delay(2);
} //  sendLEDBytesToSlaves(totalReceiveBuffer,  m_totalByteSize )


void printLEDBytesToSerialMonitor( byte * totalReceiveBuffer,  int totalByteSize  )
{

  for (int i = 0; i < m_totalByteSize; i++) {

    // print the received data from PC to the serial monitor via Serial1 of Mega
    if ( i % 3 == 0) {
      Serial1.print("r:");
      Serial1.println(totalReceiveBuffer[i]);
    }
    else if ( i % 3 == 1) {
      Serial1.print("g:");
      Serial1.println(totalReceiveBuffer[i]);
    }

    else {
      Serial1.print("b:");
      Serial1.println(totalReceiveBuffer[i]);
    }

  }// for


} //printLEDBytesToSerialMonitor( byte[] totalReceiveBuffer,  int m_totalByteSize  )
