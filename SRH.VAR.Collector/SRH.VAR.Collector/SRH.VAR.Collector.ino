/*
 Name:		SRH.VAR.Collector.ino
 Created:	1/5/2018 8:04:09 24H
 Author:	Sindre R. Hansen
*/

#include <Wire.h>

#define MASTER_ADDRESS 0x10
#define TEMPERATURECOLLECTOR_ADDRESS 0x7
#define FLOWOUT_ADDRESS 0x6

String RecivedString;
bool dataRecived;
String input_String;
bool input_StringComplete = false;

void setup() {
	Serial.begin(9600);
	Wire.begin(MASTER_ADDRESS);
	Wire.onReceive(wireReceiveEvent);
}

void wireReceiveEvent(int howMany) {
	char charReadBuffer[30] = "";
	int charIndicator = 0;
	while (Wire.available()) {
		char c = Wire.read();
		charReadBuffer[charIndicator] = c;
		charIndicator++;
	}

	String resiveStr(charReadBuffer);
	resiveStr.trim();
	RecivedString = resiveStr;
	dataRecived = true;

}

void serialEvent() {
	while (Serial.available()) {
		char inChar = (char)Serial.read();
		input_String += inChar;
		if (inChar == '\n') {
			input_StringComplete = true;
		}
	}
}

void loop() {
	if (input_StringComplete)
	{
		input_String.trim();

		if (input_String.startsWith("SetTotFlow"))
		{
			// Sending on I2C buss
			char charSendBuffer[30];
			input_String.toCharArray(charSendBuffer, 30);
			Wire.beginTransmission(FLOWOUT_ADDRESS);
			Wire.write(charSendBuffer);
			Wire.endTransmission();
		}

		input_String = "";
		input_StringComplete = false;
	}
	if (dataRecived)
	{
		Serial.println(RecivedString);
		dataRecived = false;
	}
	delay(2);
}