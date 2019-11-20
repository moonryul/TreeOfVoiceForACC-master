#include <SPI.h>

int ss1 = 53;
int ss2 = 49;
 
void setup (void) {
  pinMode(ss1, OUTPUT);
  pinMode(ss2, OUTPUT);
  digitalWrite(ss1, HIGH);
  digitalWrite(ss2, HIGH);
  Serial.begin(9600);
  SPI.begin();
  SPI.setClockDivider(SPI_CLOCK_DIV16);
}
 
void loop (void) {
//  // send test string
//  SPI.transfer('1');
//  delay(2000);
//  SPI.transfer('0');

  byte LEDarray[6]; // 10pixie * RGB 

  for (int i=0; i<6; i++){
    LEDarray[i]=i*10;
  }
  for (int i=0; i<6; i++){ 
//      int bytesSPI = LEDarray[i];
      digitalWrite(ss1, LOW);
      SPI.transfer(LEDarray[i]);
      Serial.println(LEDarray[i]);
      digitalWrite(ss1, HIGH);
  }
//  for (int i=15; i<30; i++){ 
//      char bytesSPI = char(LEDarray[i]);
//      digitalWrite(ss2, LOW);
//      SPI.transfer(bytesSPI);
//      digitalWrite(ss2, HIGH);
//  }
// 
  delay (10);
}
