#include <SPI.h>
#include "SoftwareSerial.h"
#include "Adafruit_Pixie.h"
#define NUMPIXELS 2 // Number of Pixies in the strip
#define PIXIEPIN  6 // Pin number for SoftwareSerial output
SoftwareSerial pixieSerial(-1, PIXIEPIN);
Adafruit_Pixie strip = Adafruit_Pixie(NUMPIXELS, &pixieSerial);

const int bufferSize = NUMPIXELS * 3 ;
byte buf[bufferSize];
volatile byte pos = 0;
volatile boolean process_it = false;
 
void setup() {
  Serial.begin(9600);
  // have to send on master in, *slave out*
  pixieSerial.begin(115200); // Pixie REQUIRES this baud rate
  strip.setBrightness(200);  // Adjust as necessary to avoid blinding
  pinMode(MISO, OUTPUT);
  // turn on SPI in slave mode
  
  // SPI통신을 사용할 수 있도록 레지스터 설정
  SPCR |= _BV(SPE);
  // SPI통신에서 슬레이브로 동작하도록 설정
  SPCR &= ~_BV(MSTR);
  // SPI 통신으로 문자가 수신될 경우 인터럽트 발생을 허용
  SPCR |= _BV(SPIE);
  
  // now turn on interrupts
//  SPI.attachInterrupt();
  SPI.setClockDivider(SPI_CLOCK_DIV16);
 
  pinMode(PIXIEPIN, OUTPUT);
}
 
 
// SPI interrupt routine
ISR (SPI_STC_vect) {
  byte c = SPDR;  // grab byte from SPI Data Register
  if(pos < sizeof(buf)){
    buf[pos++]=c;
  }
  if(pos == (sizeof(buf)-1) ){
    process_it = true;
  }
}
 
void loop() {
  if(process_it){
    for(int i=0; i<NUMPIXELS; i++) { //NUMPIXELS
      strip.setPixelColor(i, buf[i*3+0], buf[i*3+1], buf[i*3+2]);
      Serial.println(buf[i*3+0]);
      Serial.println(buf[i*3+1]);
      Serial.println(buf[i*3+2]);
      }
    strip.show();
    pos = 0;
    process_it = false;
  }
  delay(10);
}
