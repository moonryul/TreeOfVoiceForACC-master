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
//COM11



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

int ss1 = 37; // connect master pin 53 the first slave pin 53
int ss2 = 49; // connect master pin 49 to the second slave pin 53
int ss3 = 48; // connect master pin 48 to the third  slave pin 53
int ss4 = 47; // connect master pin 47 to the fourth slave pin 53

// A total num of LED = 186; each slave processes 40 LEDs

const int NumPixels1 = 40;
const int NumPixles2 = 44;
const int NumPixels3 = 50;
const int NumPixels4 = 52;

const int group1ByteSize = NumPixels1 * 3;
const int group2ByteSize = NumPixles2 * 3;
const int group3ByteSize = NumPixels3 * 3;
const int group4ByteSize = NumPixels4 * 3;

const int m_totalByteSize = group1ByteSize + group2ByteSize + group3ByteSize + group4ByteSize; // 3 bytes for each of 186 LEDS
byte m_totalRecieveBuffer[m_totalByteSize];

byte LED1[group1ByteSize];
byte LED2[group2ByteSize];
byte LED3[group3ByteSize];
byte LED4[group4ByteSize];

//SPISettings SPISettingA (4000000, MSBFIRST, SPI_MODE0); // 14MHz = speed; slave 1
//SPISettings SPISettingB (4000000, MSBFIRST, SPI_MODE0); // 14MHz = speed; slave 2
//SPISettings SPISettingC (4000000, MSBFIRST, SPI_MODE0); // 14MHz = speed; slave 3
//SPISettings SPISettingD (4000000, MSBFIRST, SPI_MODE0); // 14MHz = speed; slave 4

void setup (void) {


  // set the Slave Select Pins (SS)  as outputs:

  pinMode(ss1, OUTPUT);
  pinMode(ss2, OUTPUT);
  pinMode(ss3, OUTPUT);
  pinMode(ss4, OUTPUT);

  digitalWrite(ss1, HIGH);
  digitalWrite(ss2, HIGH);
  digitalWrite(ss3, HIGH);
  digitalWrite(ss4, HIGH);

  SPI.begin(); // set up:
  //To condition the hardware you call SPI.begin () which configures the SPI pins (SCK, MOSI, SS) as outputs.
  //It sets SCK and MOSI low, and SS high.
  //It then enables SPI mode with the hardware in "master" mode. This has the side-effect of setting MISO as an input.

  // Slow down the master a bit
 SPI.setClockDivider(SPI_CLOCK_DIV16);
  // The default setting is SPI_CLOCK_DIV4,
  // which sets the SPI clock to one-quarter the frequency of the system clock (4 Mhz for the boards at 16 MHz).

  //Serial1.begin(9600); // Use Serial1 to send message to the Serial1 Monitor

 // Serial.begin(9600); // increase the serial comm speed; Unity Script also sets this speed
  //Serial.begin(115200); // To read bytes from the PC Unity Script

  //Define another serial port:
  //https://www.arduino.cc/reference/en/language/functions/communication/serial/
  //
  //https://m.blog.naver.com/PostView.nhn?blogId=darknisia&logNo=220569815020&proxyReferer=https%3A%2F%2Fwww.google.com%2F
  // Mega has 4 Serial Ports: Serial, Serial1, Serial2, Serial3.
  // Serial ports are defined by Pin 0 and 1; Serial1 is defined by pins 19(RX), 18(TX).
  // Connect the first USB cable  to Pin 0 and 1 by the ordinary method; Connect the second USB cable from the second
  // USB port in the PC to Pin 19 and 18; Also open another arduino IDE for the second serial port, Serial1.
  // Use the first arduino IDE to upload the arduino code, and use the second arduino IDE to report messages.
  for (int i = 0; i < group1ByteSize/3; i++) {
    LED1[3*i+0]=250;
    LED1[3*i+1]=0;
    LED1[3*i+2]=0;
  }
  for (int i = 0; i < group2ByteSize/3; i++) {
    LED2[3*i+0]=0;
    LED2[3*i+1]=250;
    LED2[3*i+2]=0;
  }
  for (int i = 0; i < group3ByteSize/3; i++) {
    LED3[3*i+0]=0;
    LED3[3*i+1]=0;
    LED3[3*i+2]=250;
  }
  for (int i = 0; i < group4ByteSize/3; i++) {
    LED4[3*i+0]=250;
    LED4[3*i+1]=250;
    LED4[3*i+2]=0;
  }
}


void loop (void) {

//  for(int i = 0; i < m_totalByteSize/3; i++){
//    m_totalRecieveBuffer[3 * i + 0] = 3 * i + 0;
//    m_totalRecieveBuffer[3 * i + 1] = 3 * i + 1;
//    m_totalRecieveBuffer[3 * i + 2] = 3 * i + 2;
//  }

  //sendLEDBytesToSlaves( *m_totalRecieveBuffer,  m_totalByteSize  );
  //printLEDBytesToSerialMonitor( *m_totalRecieveBuffer,  m_totalByteSize  );
  //sendShowByte();
  digitalWrite(ss1, LOW); // select the first SS line
  SPI.transfer((byte)255);
  for (int i = 0; i < group1ByteSize; i++) {
    SPI.transfer(LED1[i]);
  }
  digitalWrite(ss1, HIGH);


  digitalWrite(ss2, LOW); // select the second SS Line
  SPI.transfer((byte)255);
  for (int i = 0; i < group2ByteSize; i++) {
    SPI.transfer(LED2[i]);
  }
  digitalWrite(ss2, HIGH);


  digitalWrite(ss3, LOW); // select the third SS line
  SPI.transfer((byte)255);
  for (int i = 0; i < group3ByteSize; i++) {
    SPI.transfer(LED3[i]);
  }
  digitalWrite(ss3, HIGH);


  digitalWrite(ss4, LOW);   // select the fourth SS line
  SPI.transfer((byte)255);
  for (int i = 0; i < group4ByteSize; i++) {
    SPI.transfer(LED4[i]);
  }
  digitalWrite(ss4, HIGH);

  delay(50);
}
