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
  //Serial.begin(115200); // have to send on master in, *slave out*
  for (int i = 0; i < bufferSize / 3; i++) {
    buf[3 * i] = 255;
    buf[3 * i + 1] = 0;
    buf[3 * i + 2] = 0;
  }

  //Serial1.begin(115200);

  pixieSerial.begin(115200); // Pixie REQUIRES this baud rate
  //SPI.begin(); //PB2 - PB4 are converted to SS/, MOSI, MISO, SCK



  pinMode(PIXIEPIN, OUTPUT);
}

//https://forum.arduino.cc/index.php?topic=52111.0
//It is because they share the pins that we need the SS line.With multiple slaves,
//only one slave is allowed to "own" the MISO line(by configuring it as an output).So when SS is brought low
//for that slave it switches its MISO line from high - impedance to output, then it can reply to requests
//from the master.When the SS is brought high again(inactive) that slave must reconfigure that line as high - impedance,
//so another slave can use it.

// SPI interrupt routine



void loop() {

  //SPI.beginTransaction(SPISettings(14000000, MSBFIRST, SPI_MODE0)); // disable interrupt
  for (int i = 0; i < NUMPIXELS1; i++)
  {
    strip.setPixelColor(i, buf[i * 3 + 0], buf[i * 3 + 1], buf[i * 3 + 2]);

  }

  strip.show(); // show command has been  recieved

  delay(5);
  //SPI.endTransaction();// // enable interrupt

} // loop()
