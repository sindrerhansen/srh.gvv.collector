/*
Name:		SRH.VAR.Collector.ino
Created:	1/5/2018 8:04:09 24H
Author:	Sindre R. Hansen
*/

#include <OneWire.h>
#include <DallasTemperature.h>
#include <Wire.h>

#define MASTER_ADDRESS 0x10
#define TEMPERATURECOLLECTOR_ADDRESS 0x7
#define FLOWOUT_ADDRESS 0x6

static int ONE_WIRE_BUS = 2;
String input_String;
bool Input_stringcomplete;
String valueDevider = "|";
String sendString = "";
char msg[10];

// Setup a oneWire instance to communicate with any OneWire devices 
// (not just Maxim/Dallas temperature ICs)
OneWire oneWire(ONE_WIRE_BUS);

// Pass our oneWire reference to Dallas Temperature.
DallasTemperature TemperatureSensors(&oneWire);


// the setup function runs once when you press reset or power the board
void setup() {
	//Setting up serial
	Serial.begin(9600);
	TemperatureSensors.begin();
	//Setting up I2C communikkasjon
	Wire.begin(TEMPERATURECOLLECTOR_ADDRESS);	//Change to sensor used
	Wire.onReceive(wireReceiveEvent);
	Wire.onRequest(requestEvent);
}


void wireReceiveEvent(int howMany) {
	char charReadBuffer[30] = "";
	int charIndicator = 0;
	while (Wire.available() > 0) {
		char c = Wire.read();
		charReadBuffer[charIndicator] = c;
		charIndicator++;
	}
	String inStr(charReadBuffer);
	input_String = inStr;
	Input_stringcomplete = true;
	Serial.println(input_String);
}

void requestEvent ()
{}


// the loop function runs over and over again until power down or reset
void loop() {
	// Getting Temperatures
	TemperatureSensors.requestTemperatures();

	sendString = String(TEMPERATURECOLLECTOR_ADDRESS);
	sendString += valueDevider + String(TemperatureSensors.getTempCByIndex(0));
	sendString += valueDevider + String(TemperatureSensors.getTempCByIndex(1));
	sendString += valueDevider + String(TemperatureSensors.getTempCByIndex(2));
	sendString += valueDevider;
	// Sending on I2C buss
	char charSendBuffer[30];

	sendString.toCharArray(charSendBuffer, 30);
	Wire.beginTransmission(MASTER_ADDRESS);
	Wire.write(charSendBuffer);
	Wire.endTransmission();
	Serial.println(sendString);
	Serial.println(charSendBuffer);
}
