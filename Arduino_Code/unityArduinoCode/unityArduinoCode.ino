#include <SPI.h>

const int slaveSelectPin = 37;
const int sendSize = 558;	// number of bytes for one slice signal
byte tempBuffer[SERIAL_RX_BUFFER_SIZE];
byte sendBuffer[sendSize];
int index = sendSize - 1;

void setup() {
  pinMode(slaveSelectPin, OUTPUT);

  SPI.begin();
  SPI.setBitOrder(MSBFIRST);
  Serial.begin(57600);
  Serial1.begin(57600);

  Reset();
}

// ���۸� �ʱ�ȭ �ϰ�, ��� �����մ� ��ȣ�� ������.
void Reset() {
  memset(sendBuffer, 0, sendSize);
  digitalWrite(slaveSelectPin, LOW);
  SPI.transfer(sendBuffer, sendSize);
  digitalWrite(slaveSelectPin, HIGH);

}

void loop() {
  // ���� �ø��� ���ۿ� �����Ͱ� �ִ��� Ȯ���Ѵ�.
  int count = Serial.available();
  if (count == 0)
    return;

  // ���ۿ� �ִ¸�ŭ �о ���� ���ۿ� ��´�.
  Serial.readBytes(tempBuffer, count);
  for (int j = 0; j < count; ++j) {
    sendBuffer[index - j] = tempBuffer[j];
  }

  index -= count;


  if (index == -1) {
    int idx = 0;
    for (int i = 0; i < sendSize; i++) {

      // print the received data from PC to the serial monitor via Serial1 of Mega
      if ( i % 3 == 0) {
        Serial1.print(idx);
        Serial1.print("    ");
        Serial1.print("r:");
        Serial1.print((byte)sendBuffer[i]);
      }
      else if ( i % 3 == 1) {
        Serial1.print("g:");
        Serial1.print((byte)sendBuffer[i]);

      }

      else {
        Serial1.print("b:");
        Serial1.println(sendBuffer[i]);
        idx++;
      }

    }// for

    //		digitalWrite(slaveSelectPin, LOW);
    //		SPI.transfer(sendBuffer, sendSize);
    //		digitalWrite(slaveSelectPin, HIGH);

    index = sendSize - 1;
  } //if (index == -1) {
}
