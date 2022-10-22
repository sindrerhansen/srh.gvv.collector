/*
Name:		SRH.VAR.Collector.ino
Created:	1/5/2018 8:04:09 24H
Author:	Sindre R. Hansen
*/

#include <Wire.h>

#define MASTER_ADDRESS 0x10
#define TEMPERATURECOLLECTOR_ADDRESS 0x7
#define FLOWOUT_ADDRESS 0x6

volatile unsigned long  total_flow =0;
unsigned long lastTotalFlow;

double TotalLiter;

double liter_hour;							// Calculated litres/hour                      
const byte flowmeter = 2;						// Flow Meter Pin number
static double SpesificEnergyWather = 4.184;		//MJ/kg
unsigned long cloopTime;
static double PulsesPerLiter = 440.0;

String RecivedString;
bool dataRecived;

String input_String;
bool input_StringComplete = false;

static int SerialSendRate = 500; //mS
static char valueDevider = '|';
String sendString = "";

bool ResetFlag;

// the setup function runs once when you press reset or power the board
void setup() {
	//Setting up serial
	Serial.begin(9600);
	pinMode(flowmeter, INPUT_PULLUP);
	//Setting up I2C communikkasjon
	Wire.begin(FLOWOUT_ADDRESS);	//Change to sensor used
	Wire.onReceive(wireReceiveEvent);
	Wire.onRequest(requestEvent);
	attachInterrupt(digitalPinToInterrupt(flowmeter), flow, RISING);			// Setup Interrupt 

	ResetFlag = true;
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

void flow()										// Interruot function
{
	total_flow++;
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

void requestEvent()
{}

void loop() {
	if (dataRecived)
	{
		RecivedString.trim();

		if (RecivedString.startsWith("SetTotFlow"))
		{
			RecivedString.remove(0, 10);
			unsigned long floC = atol(RecivedString.c_str());
			total_flow += floC;
			lastTotalFlow = total_flow;
			ResetFlag = false;
		}


		RecivedString = "";
		dataRecived = false;
	}

	if (millis() >= (cloopTime + SerialSendRate))
	{
		long int time = millis();
		TotalLiter = total_flow / PulsesPerLiter;
		int difFlow = total_flow - lastTotalFlow;

		if (difFlow > 0)
			liter_hour = (difFlow / PulsesPerLiter) * (3600000 / (time - cloopTime));
		else
			liter_hour = 0;

		lastTotalFlow = total_flow;
		cloopTime = time;
		char charSendBuffer[30];
		sendString = String(FLOWOUT_ADDRESS);
		sendString += valueDevider + String(ResetFlag);
		sendString += valueDevider + String(TotalLiter);
		sendString += valueDevider + String(liter_hour);
		sendString += valueDevider + String(total_flow);
		//Ending
		sendString += valueDevider;
		sendString.toCharArray(charSendBuffer, 30);

		Wire.beginTransmission(MASTER_ADDRESS);
		Wire.write(charSendBuffer);
		Wire.endTransmission();

		Serial.println(sendString);
	}
  
}