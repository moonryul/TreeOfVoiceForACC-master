//COM13

#include <SPI.h>
#include "SoftwareSerial.h"
#include "Adafruit_Pixie.h"

#define NUMPIXELS2 44 // Number of Pixies in the strip
#define PIXIEPIN  5 // Pin number for SoftwareSerial output to the LED chain

SoftwareSerial pixieSerial(-1, PIXIEPIN);
Adafruit_Pixie strip = Adafruit_Pixie(NUMPIXELS2, &pixieSerial);

const int bufferSize = NUMPIXELS2 * 3; // 6 bytes are for the start and end bytes
byte buf[bufferSize];

volatile int m_pos = 0;
volatile boolean m_process_it = false;
volatile boolean m_frameInProgess = false;

void setup() {
  Serial.begin(9600);
  pixieSerial.begin(115200); // Pixie REQUIRES this baud rate

  SPI.setClockDivider(SPI_CLOCK_DIV16);
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
  if( c == 255 ) {
    m_pos = 0;
    m_frameInProgess = true;
  }
  else if(m_frameInProgess){
    if (m_pos < sizeof(buf)) {
      buf[m_pos++] = c;
    }
  }
  if (m_pos == (sizeof(buf))) {
    m_process_it = true;
    m_frameInProgess = false;
  }
}

void loop() {

  // The following process of reading the sending buffer can be interrupted when new bytes arrive at the SPI port
  if (m_process_it)
  { // get the bytes between the startBytes and the endBytes

    for (int i = 0; i < NUMPIXELS2; i++)
    {
      strip.setPixelColor(i, buf[i * 3 + 0], buf[i * 3 + 1], buf[i * 3 + 2]);
      Serial.print(i);
      Serial.print("th  ");
      Serial.print("  r:  ");
      Serial.print(buf[i * 3 + 0]);
      Serial.print("  g:  ");
      
      Serial.print(buf[i * 3 + 1]);
      Serial.print("  b:  ");
      Serial.println(buf[i * 3 + 2]);
    }

    strip.show(); // show command has been  recieved
    m_process_it = false;
  } 
  delay(1);
}
