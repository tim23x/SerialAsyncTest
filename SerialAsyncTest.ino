/*
    Name:       SerialAsyncTest.ino
    Created:	16/02/2019 17:39:24
    Author:     TIM-7730\tim23
*/


#include <Streaming.h>
void setup()
{
	Serial.begin(115200);
}

void loop()
{
	for (int i = 0; i < 32000; i++)
	{
		Serial << i << '#';
	}
	delay(20);
}
