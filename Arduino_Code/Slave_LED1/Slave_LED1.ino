#include <SPI.h>

#include "SoftwareSerial.h"
#include "Adafruit_Pixie.h"

// Slave_LED1.ino uses UNO
//// 참조 사이트 : https://weathergadget.wordpress.com/2016/05/19/usi-spi-slave-communication/

//#define SS 10 // uno, sPI pin
//#define SS  37 //mega spi pin
#define NUMPIXELS1 40 // Number of Pixies in the strip
#define PIXIEPIN  5 // Pin number for SoftwareSerial output to the LED chain

SoftwareSerial pixieSerial(-1, PIXIEPIN);
Adafruit_Pixie strip = Adafruit_Pixie(NUMPIXELS1, &pixieSerial);

const int bufferSize = NUMPIXELS1 * 3;

byte bufferA[bufferSize];
byte bufferB[bufferSize];

byte* receivingPointer = &bufferA[0];
byte* sendingPointer = &bufferB[0];

byte* interMediatePointer;

volatile int m_pos = 0;
volatile boolean m_process_it = false;

void setup() {

  //initialize the buffers to zero

  for (int i = 0; i < bufferSize; i++)
  {
    bufferA[i] = (byte)0;
    bufferB[i] = (byte)0;
  }
  Serial.begin(115200); // have to send on master in, *slave out*


  //Serial1.begin(115200);

  pixieSerial.begin(115200); // Pixie REQUIRES this baud rate

  SPI.begin(); //PB2 - PB4 are converted to SS/, MOSI, MISO, SCK

  // pinMode(SS, INPUT);
  pinMode(MISO, OUTPUT);

  // turn on SPI in slave mode
  SPCR |= bit(SPE);
  // SPI통신 레지스터 설정
  SPCR |= _BV(SPE);

  // get ready for an interrupt
  m_pos = 0;   // buffer empty
  m_process_it = false;

  //// 슬레이브로 동작하도록 설정
  SPCR &= ~_BV(MSTR);

  ////  인터럽트 발생을 허용
  SPCR |= _BV(SPIE);

  // now turn on interrupts
  //SPI.attachInterrupt();
  SPI.setClockDivider(SPI_CLOCK_DIV16);
  //https://www.arduino.cc/en/Tutorial/SPITransaction

  pinMode(PIXIEPIN, OUTPUT);
}

//https://forum.arduino.cc/index.php?topic=52111.0
//It is because they share the pins that we need the SS line.With multiple slaves,
//only one slave is allowed to "own" the MISO line(by configuring it as an output).So when SS is brought low
//for that slave it switches its MISO line from high - impedance to output, then it can reply to requests
//from the master.When the SS is brought high again(inactive) that slave must reconfigure that line as high - impedance,
//so another slave can use it.

// SPI interrupt routine
ISR (SPI_STC_vect) {
  byte c = SPDR;  // grab byte from SPI Data Register

  //  get the byte into the buffer pointed by receivingPointer
  //buf[ m_pos++ ]=c;
  //  if( m_pos == bufferSize )
  //  {

  //   m_process_it = true;
  // m_pos =0;
  //  }//if

  if (c == (byte)255)// frame endMarker
  { 
    //    if ( m_pos == bufferSize )
    //    { // the receiving buffer is full => wrap the index to zero

    // change the receivingPointer to the other buffer
    //receivingPointer = &bufferB[0];
    interMediatePointer = receivingPointer;
    receivingPointer = sendingPointer;

    sendingPointer = interMediatePointer;

    //receivingPointer = &bufferB[0]

    // change the sendingPointer to the other buffer
    //sendingPointer  = &bufferA[0];

    m_pos = 0;
    m_process_it = true;

  } else {
    receivingPointer[ m_pos++ ] = c; // recevingPointer points to bufferA initially
  }





 //if ( m_pos == bufferSize )
}//ISR (SPI_STC_vect)

void loop() {

  if (m_process_it)
  {
    int indexCount = 0;
    //SPI.beginTransaction(SPISettings(14000000, MSBFIRST, SPI_MODE0)); // disable interrupt
    for (int i = 0; i < NUMPIXELS1; i++)
    {
      //strip.setPixelColor(i, sendingPointer[i * 3 + 0], sendingPointer[i * 3 + 1], sendingPointer[i * 3 + 2]);

      strip.setPixelColor(i, *(sendingPointer + i * 3 + 0), *(sendingPointer + i * 3 + 1), *(sendingPointer + i * 3 + 2) );
      Serial.print(i);
      Serial.print("  : ");

      Serial.print(" r : ");
      Serial.print(sendingPointer[i * 3 + 0]);
      Serial.print(" g : ");
      Serial.print(sendingPointer[i * 3 + 1]);
      Serial.print(" b : ");
      Serial.println(sendingPointer[i * 3 + 2]);

    }

    strip.show(); // show command has been  recieved
    m_pos = 0;
    m_process_it = false;

    //delay(5);

    //SPI.endTransaction();// // enable interrupt
  } // if



} // loop()
