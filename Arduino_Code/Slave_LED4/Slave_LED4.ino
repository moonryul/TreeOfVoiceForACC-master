#include <SPI.h>

// Slave_LED4.ino uses MEGA
//static const uint8_t SS   = PIN_SPI_SS;
//static const uint8_t MOSI = PIN_SPI_MOSI;
//static const uint8_t MISO = PIN_SPI_MISO;
//static const uint8_t SCK = PIN_SPI_SCK;

#include "SoftwareSerial.h"
#include "Adafruit_Pixie.h"

#define SS 53
#define NUMPIXELS4 52// Number of Pixies in the strip
#define PIXIEPIN  5 // Pin number for SoftwareSerial output

SoftwareSerial pixieSerial(-1, PIXIEPIN);
Adafruit_Pixie strip = Adafruit_Pixie(NUMPIXELS4, &pixieSerial);

const int bufferSize = NUMPIXELS4 * 3;
byte showByte = 0;
byte buf[bufferSize];
volatile byte m_pos = 0;
volatile boolean m_process_it = false;
 
void setup() {
  Serial.begin(115200);
 // Serial1.begin(115200);
  // have to send on master in, *slave out*
  pixieSerial.begin(115200); // Pixie REQUIRES this baud rate
  //strip.setBrightness(200);  // Adjust as necessary to avoid blinding

  pinMode(SS, INPUT);
  // Master In, Slave Out
  pinMode(MISO, OUTPUT);
// turn on SPI in slave mode
  SPCR |= bit(SPE);
  // SPI통신 레지스터 설정
  //  SPCR |= _BV(SPE);
    
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
 
 
// SPI interrupt routine
ISR (SPI_STC_vect) {

  byte c = SPDR;  // grab byte from SPI Data Register
// if( c == 0 ){
//	 m_process_it = true;
//    Serial.println("show command");
//    }
//  else if( m_pos < sizeof(buf)){
//    buf[ m_pos++ ]=c;  
//  }
//  
 if ( m_pos < bufferSize ) 
    {
      buf[ m_pos++ ]=c;  
      if( m_pos == bufferSize )
      {
 
          m_process_it = true;
          m_pos =0;
      }
    }// if
}
 
void loop() {

  if(m_process_it){

	// SPI.beginTransaction(SPISettings(14000000, MSBFIRST, SPI_MODE0)); // disable interrupt

    for(int i=0; i<NUMPIXELS4; i++) { //NUMPIXELS
      strip.setPixelColor (i, buf[i*3+0], buf[i*3+1], buf[i*3+2] );

      //Serial1.println( buf[i*3+0]);
      //Serial1.println( buf[i*3+1]);
      //Serial1.println( buf[i*3+2]);

      }
  
  
    strip.show(); // show command has been  recieved
 
   // m_pos = 0;
    m_process_it = false;

	//SPI.endTransaction();// // enable interrupt
    }
 
}
