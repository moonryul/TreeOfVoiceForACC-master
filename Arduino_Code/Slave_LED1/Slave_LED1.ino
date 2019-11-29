#include <SPI.h>

#include "SoftwareSerial.h"
#include "Adafruit_Pixie.h"

// Slave_LED1.ino uses UNO
//// 참조 사이트 : https://weathergadget.wordpress.com/2016/05/19/usi-spi-slave-communication/

//#define SS 10 // uno, sPI pin
#define SS  53 //mega spi pin
#define NUMPIXELS1 40 // Number of Pixies in the strip
#define PIXIEPIN  5 // Pin number for SoftwareSerial output to the LED chain

SoftwareSerial pixieSerial(-1, PIXIEPIN);
Adafruit_Pixie strip = Adafruit_Pixie(NUMPIXELS1, &pixieSerial);

const int bufferSize = NUMPIXELS1 * 3; 
byte showByte = 0; 
byte buf[bufferSize];
volatile byte m_pos = 0;
volatile boolean m_process_it = false;
 
void setup() {
  Serial.begin(115200); // have to send on master in, *slave out*
  for(int i=0; i<bufferSize;i++){
    buf[i] = 0;
  }

  //Serial1.begin(115200); 

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
  
 if ( m_pos < bufferSize )
 {
    buf[ m_pos++ ]=c;	
    if( m_pos == bufferSize )
    {
 
     m_process_it = true;
    m_pos =0;
    }//if

//    else { // the buffer is not yet emptied but new byte has been arrived via interrupt
//           // we need to ignore it. Once we ignore the first byte, we need to ignore the entire array of one frame
//      m_pos ++
//      
//    }
    
 } //if
}//ISR (SPI_STC_vect) 
 
void loop() {

	if (m_process_it)
	{
		//SPI.beginTransaction(SPISettings(14000000, MSBFIRST, SPI_MODE0)); // disable interrupt
		for (int i = 0; i < NUMPIXELS1; i++)
		{
			strip.setPixelColor(i, buf[i * 3 + 0], buf[i * 3 + 1], buf[i * 3 + 2]);
		//	Serial1.println(buf[i * 3 + 0]);
		//	Serial1.print(buf[i * 3 + 1]);
		//	Serial1.print(buf[i * 3 + 2]);
		}

		strip.show(); // show command has been  recieved
	 // m_pos = 0;
		m_process_it = false;

		//SPI.endTransaction();// // enable interrupt
	} // if
} // loop()
