#include <SPI.h>

#include "SoftwareSerial.h"
#include "Adafruit_Pixie.h"

// Slave_LED1.ino uses UNO
//// 참조 사이트 : https://weathergadget.wordpress.com/2016/05/19/usi-spi-slave-communication/ (very technical)
//https://m.blog.naver.com/darknisia/220673747042 (Korean)

//#define SS 10 // uno, sPI pin
#define SS  53 //mega spi pin
#define NUMPIXELS1 40 // Number of Pixies in the strip
#define PIXIEPIN  5 // Pin number for SoftwareSerial output to the LED chain

SoftwareSerial pixieSerial(-1, PIXIEPIN);
Adafruit_Pixie strip = Adafruit_Pixie(NUMPIXELS1, &pixieSerial);

const int m_totalBufferSize = (NUMPIXELS1 + 2) * 3; // 6 bytes are for the start and end bytes

byte m_bufferA[m_totalBufferSize];
byte m_bufferB[m_totalBufferSize];

byte* m_receivingPointer = &m_bufferA[0];
byte* m_sendingPointer = &m_bufferB[0];


byte m_startBytes[3]  = {0, 0, 0}; // This full black color indicates the start of a single frame of LEDs.
byte m_endBytes[3]  = {255, 255, 255}; // This full white color indicates the end a single frame of LEDs.

//I read the Arduino reference page and understand it's used for values that might change
//and often used with interrupts, it's pulled from the RAM and not the storage register,
//but if you code a variable using "int", can't that values also change?
// cf: https://forum.arduino.cc/index.php?topic=418692.0

//That’s it: volatile simply convinces the compiler not to optimize over a variable that’s **apparently** constant.

//On its face, volatile is very simple. You use it to tell the compiler that the declared variable can change without notice,
//and this changes the way that the compiler optimizes with respect to this variable. In big-computer programming,
//you almost never end up using volatile in C. But in the embedded world,
//we end up using volatile in one trivial and two very important circumstances, so it’s worth taking a look.

// "volatile"  lets it know that it can't optimize it away because it may change from somewhere else.
//So the compiler will not optimize this code away and the if statement still works.
//Equally important, volatile also causes the compiler to generate code that always reads the variable from RAM
//and does not "cache" the last read value in a register.
//volatile should ALWAYS be used on any variable that can be modified an interrupt, or any external source.  For example, chip register addresses (think of, for example, the status register in a UART or other communications chip) should also be declared as volatile,
//so the compiler will always read the physical register.
//cf: https://hackaday.com/2015/08/18/embed-with-elliot-the-volatile-keyword/

volatile byte m_pos = 0;
volatile boolean m_process_it = false;

volatile boolean recvInProgress = false;  

// If the current code does not work, try https://sites.google.com/site/qeewiki/books/avr-guide/spi

void setup() {

  //Serial.begin(115200); // for debugging

  //Serial1.begin(115200); // for debugging

  pixieSerial.begin(115200); // Pixie REQUIRES this baud rate

  pinMode(PIXIEPIN, OUTPUT);

  // pinMode(SS, INPUT); // PIN is INPUT by default

  pinMode(MISO, OUTPUT); // this is needed to send bytes to the master

  // turn on SPI communication
  //SPCR |= bit(SPE); // SPCR = SPI Control Register; SPIE =bit 7, SPE = bit 6, MSTR = bit 4

  SPCR |= _BV(SPE); // SPE: SPI Enable; Macro _BV(n) sets bit n to 1, whereas the other bits to 0

  SPCR &= ~_BV(MSTR); // MSTR = Master Slave Select Register; MSTR = 1 => Set as master
  // MSTR =0 => Set as slave
  // The MSTR bit within the SPCR register tells the AVR whether it is a master or a slave.
  //https://sites.google.com/site/qeewiki/books/avr-guide/spi
  //cf. The SPIF bit within the SPSR sets HIGH(1) whenever data transmission is complete even if interrupts are not enabled.
  //This is useful because we could check the status of the bit in order to figure out
  //if it is safe to write the the SPDR register.
  //  Writing to the SPDR register causes data to be loaded into the shift register and automatically triggers the AVR to transmit.
  // Reading from the SPDR register causes the data to be read from the receive shift register.

  //  The WCOL bit within the SPSR register will be set HIGH(1) if you attempt to write data into the SPDR register during data transmission.
  //  WCOL will be cleared (0) when the SPDR is reed.

  // turn on interrupts
  SPCR |= _BV(SPIE); // SPIE: SPI Interrrupt Enable

  // get ready for an interrupt
  m_pos = 0;   // buffer empty
  m_process_it = false;


}//setup()

//SRs are by their nature invoked only outside of the normal program flow, and this naturally confuses the compiler.
//Indeed, ISRs look to the compiler like functions that are never called, so the definition of ISR in AVR-GCC,
//for instance, includes the special “used” attribute so that the function doesn’t get thrown away entirely.
//So you can guess that things get ugly when you want to
//modify or use a variable from within a function that the compiler doesn’t know is even going to be use

//cf:
//volatile uint16_t counter=0;
//
//ISR(timer_interrupt_vector){
//    ++counter;
//}
//
//int main(void){
//    printf(&quot;%d\n&quot;, counter);
//}
// SPI interrupt routine
ISR (SPI_STC_vect) { // SPI_STC_vect: invoked when a new byte arrives:
  
  byte c = SPDR;  // grab byte from SPI Data Register

  // move the current byte c to the receiving buffer

  m_receivingPointer[ m_pos ] = c; // recevingPointer points to bufferA initially

  // There are three cases:

  if ( realBytesInProgress == true )  {

    if ( !endMarkerArrived( m_receivingPointer, m_pos, m_startBytes) ) { // the end marker not received
      index++;
    }
    else { // the end marker received

      if ( m_pos == m_totalBufferSize - 1 ) {
        // the full buffer has been filled


        m_pos  = 0;
        realBytesInProgress = false; // made false after the end bytes arrived
         m_process_it = true;
         
    // m_process_it = true means that loop() can begin and continue to  read the sending buffer;

     // change the receivingPointer to the other buffer
       m_receivingPointer = &m_bufferB[0];
    // change the sendingPointer to the other buffer
       m_sendingPointer  = &m_bufferA[0];
    
      }// if
      else { // the number of bytes between the start bytes and the end bytes is a wrong number
             //=> ignore the read data and continue to read

        m_pos ++;
        realBytesInProgress = false; // made false after a wrong number of data has been read between the start and end bytes

      } // else
    }  // else

  } // if ( realBytesInProgress == true )
  else if ( startMarkerArrived( m_receivingPointer, m_pos, m_endBytes) ) {

    realBytesInProgress = true; // made true after the start bytes arrived
    m_pos++;

  }
  else {
    m_pos++;
  }

}//ISR (SPI_STC_vect)

void loop() {

  // The following process of reading the sending buffer can be interrupted when new bytes arrive at the SPI port

  if (m_process_it)
  { // get the bytes between the startBytes and the endBytes

    for (int i = 1; i < NUMPIXELS1; i++)
    {
      strip.setPixelColor(i, m_sendingPointer[i * 3 + 0], m_sendingPointer[i * 3 + 1], m_sendingPointer[i * 3 + 2]);
      //	Serial1.println(buf[i * 3 + 0]);
      //	Serial1.print(buf[i * 3 + 1]);
      //	Serial1.print(buf[i * 3 + 2]);
    }

    strip.show(); // show command has been  recieved

    m_process_it = false; // made false until the interrupt routine makes it true

  } // if
} // loop()
