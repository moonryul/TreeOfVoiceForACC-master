
#include <SharpIR.h>



SharpIR sensor1( SharpIR::GP2Y0A02YK0F, A0 );
SharpIR sensor2( SharpIR::GP2Y0A02YK0F, A1 );

const int numReadings=15;
long readings1[numReadings];
long readings2[numReadings];
int readIndex=0;
long total1=0;
long total2=0;
long average1=0;
long average2=0;
int signaltounity[2];
unsigned long m_lastTime = 0;
unsigned long m_currentTime = 0;
unsigned long m_deltaTime = 20; //평균내는단위



void setup() {
    // Multiple Sharp IR Distance meter code for Robojax.com
 Serial.begin(115200);
 Serial1.begin(115200);

 for (int i=0; i<numReadings;i++){
  readings1[i]=0;
  readings2[i]=0;}
 

 // extra pin for 5V if needed
 pinMode(2,OUTPUT);// define pin 2 as output
 digitalWrite(2, HIGH);// set pin 2 HIGH so it always have 5V
  // Robojax.com 20181201
}

void loop() {
    
 
   delay(100);   
 unsigned long startTime=millis();  // takes the time before the loop on the library begins

  int dis1=sensor1.getDistance();  // this returns the distance for sensor 1
  int dis2=sensor2.getDistance();  // this returns the distance for sensor 2
  // Sharp IR code for Robojax.com 20181201

  total1 = total1 -readings1[readIndex];
  total2= total2 - readings2[readIndex];
  readings1[readIndex]=dis1;
  readings2[readIndex]=dis2;
  total1=total1+readings1[readIndex];
  total2=total2+readings2[readIndex];
  readIndex = readIndex +1;

  if(readIndex>=numReadings)
  {
    readIndex=0;
  }

    average1=total1/numReadings;
    average2=total2/numReadings;


  if(100>average1){signaltounity[0]=1;}
  else{signaltounity[0]=0;}

  if(100>average2){signaltounity[1]=1;}
  else{signaltounity[1]=0;}

//Serial.print(signaltounity[0]);
//Serial.print(",");
//Serial.print(signaltounity[1]);
//Serial.println(" ");
  Serial.print("Distance (1): ");
  Serial.print(average1);
  Serial.println("cm");
  
  Serial.print("Distance (2): ");
 Serial.print(average2);
 Serial.println("cm");
     // Sharp IR code for Robojax.com
     
}
