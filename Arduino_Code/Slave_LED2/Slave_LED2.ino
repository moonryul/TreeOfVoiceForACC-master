#include <SPI.h>
//This gives you an SPIClass, and an instance of that class called SPI in SPI.cpp.
#include "SoftwareSerial.h"
#include "Adafruit_Pixie.h"

//Slave_LED2.ino uses MEGA
//https://forum.arduino.cc/index.php?topic=52111.0
//http://gammon.com.au/forum/?id=10892

#define SS 53
#define NUMPIXELS2 44 // Number of Pixies in the strip
#define PIXIEPIN  5 // Pin number for SoftwareSerial output to the LED chain

SoftwareSerial pixieSerial(-1, PIXIEPIN);
Adafruit_Pixie strip = Adafruit_Pixie(NUMPIXELS2, &pixieSerial);

const int bufferSize = NUMPIXELS2 * 3;
byte showByte = 1; 
byte buf[bufferSize];
volatile byte m_pos = 0;
volatile boolean m_process_it = false;
 
void setup() 
{
  Serial.begin(115200); // have to send on master in, *slave out*
  pixieSerial.begin(115200); // Pixie REQUIRES this baud rate
  SPI.begin(); //PB2 - PB4 are converted to SS/, MOSI, MISO, SCK

  pinMode(SS, INPUT);
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
}//void setup() 
 
 
// SPI interrupt routine
ISR (SPI_STC_vect) {

  byte c = SPDR;  // grab byte from SPI Data Register

// commented out by Moon
//if( c == 0 ){
//    showByte = 0;
//    Serial.println("show command");
//    }
//  else if( m_pos < sizeof(buf)){
//    buf[ m_pos++ ]=c;  
//  }
//  else if( m_pos ==  sizeof(buf) ){
//    m_process_it = true;
//  }
//}
 
 // replace the above by the following

//if( c == 0 )
// {
//	m_process_it = true;
//    Serial1.println("show command");
// }
// else if( m_pos < sizeof(buf))
//  {
//    buf[ m_pos++ ]=c;  
//  }

    if (m_pos < bufferSize ) 
    { 
    buf[ m_pos++ ]=c;  
    if( m_pos == bufferSize )
    {
 
     m_process_it = true;
     m_pos =0;
    }
    }
    
} // ISR (SPI_STC_vect) 

void loop() 
{

	// commented out by Moon; replace it by the following code
 // if( m_process_it ){

	// //SPI.beginTransaction(SPISettings(14000000, MSBFIRST, SPI_MODE0)); // disable interrupt
 //   for(int i=0; i<NUMPIXELS2; i++) { //NUMPIXELS
 //     strip.setPixelColor (i, buf[i*3+0], buf[i*3+1], buf[i*3+2] );
 //     Serial.println( buf[i*3+0]);
 //     Serial.println( buf[i*3+1]);
 //     Serial.println( buf[i*3+2]);
 //     }
 // }

 //if(showByte == 0){
 //   strip.show(); // show command has been  recieved
 //   showByte = 1;
 //   m_pos = 0;
 //   m_process_it = false;
 //   }
 // delay(10);
 // //SPI.endTransaction();// // enable interrupt

// replace the above by the following

  if( m_process_it )
  {

	 //SPI.beginTransaction(SPISettings(14000000, MSBFIRST, SPI_MODE0)); // disable interrupt
    for(int i=0; i<NUMPIXELS2; i++) 
	{ 
      strip.setPixelColor (i, buf[i*3+0], buf[i*3+1], buf[i*3+2] );
    ///  Serial1.println( buf[i*3+0]);
   //   Serial1.println( buf[i*3+1]);
   //   Serial1.println( buf[i*3+2]);
    }

	strip.show(); // show command has been  recieved => update the LED colors in the chain
//	m_pos = 0;
	m_process_it = false;

	//SPI.endTransaction();// // enable interrupt

  } // if( m_process_it )

}// void loop() 
