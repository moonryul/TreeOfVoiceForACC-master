//COM20

#include <SPI.h>
#include "SoftwareSerial.h"
#include "Adafruit_Pixie.h"

#define NUMPIXELS1 40 // Number of Pixies in the strip
#define PIXIEPIN  5 // Pin number for SoftwareSerial output to the LED chain

SoftwareSerial pixieSerial(-1, PIXIEPIN);
Adafruit_Pixie strip = Adafruit_Pixie(NUMPIXELS1, &pixieSerial);

const int m_bufferSize = NUMPIXELS1 * 3; // 6 bytes are for the start and end bytes
byte m_buffer[m_bufferSize];

volatile int m_pos = 0;
volatile boolean m_process_it = false;
volatile boolean m_frameInProgess = false;

void setup() {
  Serial.begin(57600);
  pixieSerial.begin(115200); // Pixie REQUIRES this baud rate

  //SPI.setClockDivider(SPI_CLOCK_DIV16);
  pinMode(PIXIEPIN, OUTPUT);
  pinMode(MISO, OUTPUT); // this is needed to send bytes to the master

  SPCR |= _BV(SPE); // SPE: SPI Enable; Macro _BV(n) sets bit n to 1, whereas the other bits to 0
  SPCR &= ~_BV(MSTR); // MSTR = Master Slave Select Register; MSTR = 1 => Set as master
  // turn on interrupts
  SPCR |= _BV(SPIE); // SPIE: SPI Interrrupt Enable
}

// SPI interrupt routine
ISR (SPI_STC_vect) { // SPI_STC_vect: invoked when data arrives:

  byte c = SPDR;  // grab byte from SPI Data Register

  if ( !m_process_it) { // ignore the incoming byte while loop() is processing the fully filled buffer
                        // m_process_it becomes false when loop() finishes processing the filled buffer
    // !m_process has two cases:
    if ( !m_frameInProgess ) { // when frameInProfess is false, try to check the start byte, 255


      if ( c == 255 ) {
        m_pos = 0;
        m_frameInProgess = true;
        return;
      }
      else { // continue to read until the start byte arrives while frameInPgoress is false
        return;
      }
    }//if ( !m_frameInProgress )

    else  { // m_frameInProgess is true: the start byte has arrived

      m_buffer[m_pos] = c;

      if ( m_pos == m_bufferSize - 1) {
        m_process_it = true;
        m_frameInProgess = false;
        return;
      } // if

      else {
        m_pos ++; // the next location to fill in the buffer
        return;
      }
    } // else: m_frameInProgess is true: the start byte has arrived

  } //  if ( !m_process_it )

  // now the process_it is true: loop() is reading the buffer => ignore the incoming byte
   // until loop() has read the buffer and turns m_process_it false
  return;

} // ISR

void loop() {

  // The following process of reading the sending buffer can be interrupted when new bytes arrive at the SPI port
  
  if ( m_process_it )
  
  { // get the bytes in the fully filled buffer

    for (int i = 0; i < NUMPIXELS1; i++)
    {
      strip.setPixelColor(i, m_buffer[ i * 3 + 0], m_buffer[i * 3 + 1], m_buffer[i * 3 + 2] );
      Serial.print(i);
      Serial.print("th  ");
      Serial.print("  r:  ");
      Serial.print(m_buffer[i * 3 + 0]);
      Serial.print("  g:  ");

      Serial.print(m_buffer[i * 3 + 1]);
      Serial.print("  b:  ");
      Serial.println(m_buffer[i * 3 + 2]);
    }

    strip.show(); // show command has been  recieved
    m_process_it = false;
  } //if ( m_process_it )
  //delay(1);
}// loop()
