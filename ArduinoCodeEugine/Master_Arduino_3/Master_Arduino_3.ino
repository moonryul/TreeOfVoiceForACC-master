#include <SPI.h>

int ss1 = 53; // connect master pin 53 the first slave pin 53
int ss2 = 49; // connect master pin 49 to the second slave pin 53
int ss3 = 48; // connect master pin 48 to the third  slave pin 53
int ss4 = 47; // connect master pin 47 to the fourth slave pin 53

const int NumPixels1 = 40;
const int NumPixles2 = 44;
const int NumPixels3 = 50;
const int NumPixels4 = 52;

const int group1ByteSize = NumPixels1 * 3;
const int group2ByteSize = NumPixles2 * 3;
const int group3ByteSize = NumPixels3 * 3;
const int group4ByteSize = NumPixels4 * 3;

byte LEDarray1[group1ByteSize+1];
byte LEDarray2[group2ByteSize];

void setup (void) {
  pinMode(ss1, OUTPUT);
  pinMode(ss2, OUTPUT);
  digitalWrite(ss1, HIGH);
  digitalWrite(ss2, HIGH);
  Serial.begin(9600);
  SPI.begin();
  SPI.setClockDivider(SPI_CLOCK_DIV16);
  for (int i = 0; i < group1ByteSize / 3; i++) {
    LEDarray1[3 * i + 0] = i;
    LEDarray1[3 * i + 1] = i+1;
    LEDarray1[3 * i + 2] =0;
  }
    LEDarray1[group1ByteSize] = (byte)255;
    
  for (int i = 0; i < group2ByteSize / 3; i++) {
    LEDarray2[3 * i + 0] = 255;
    LEDarray2[3 * i + 1] = 255;
    LEDarray2[3 * i + 2] = 0;
  }
}

void loop (void) {


  for (int i = 0; i < group1ByteSize+1; i++) {
    //      int bytesSPI = LEDarray[i];
    digitalWrite(ss1, LOW);
    SPI.transfer(LEDarray1[i]);
    digitalWrite(ss1, HIGH);
  }
  for (int i = 0; i < group2ByteSize; i++) {
    digitalWrite(ss2, LOW);
    SPI.transfer(LEDarray2[i]);
    digitalWrite(ss2, HIGH);
  }
  //
  delay (10);
}
