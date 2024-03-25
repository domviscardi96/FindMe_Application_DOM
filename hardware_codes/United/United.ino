//#include <Adafruit_Sensor_Calibration.h>
//#include <Adafruit_AHRS.h>
#include <OneButton.h>
#include <WiFi.h>
#include <WiFiMulti.h>


// Define Wi-Fi credentials
const char* ssid1 = "Pixel7 D";
const char* password1 = "nicola99";
const char* ssid2 = "Kinjal's iPhone";
const char* password2 = "capstone";
const char* ssid3 = "SM-G930W81459";
const char* password3 = "dote8639";
const char* ssid4 = "Harly's iPhone 14 Pro Max";
const char* password4 = "aaaaaaaa";
const char* ssid5 = "BELL766";
const char* password5 = "471795C2D225";

//Adafruit_Sensor *accelerometer, *gyroscope, *magnetometer;

//#include "LSM6DS_LIS3MDL.h"  // can adjust to LSM6DS33, LSM6DS3U, LSM6DSOX...
#include "NFCtag.h"
#include "BLEconnection.h"
#include "GPS.h"


// pick your filter! slower == better quality output
//Adafruit_NXPSensorFusion filter; // slowest
//Adafruit_Madgwick filter;  // faster than NXP
//Adafruit_Mahony filter;  // fastest/smalleset
//
//#if defined(ADAFRUIT_SENSOR_CALIBRATION_USE_EEPROM)
//Adafruit_Sensor_Calibration_EEPROM cal;
//#else
//Adafruit_Sensor_Calibration_SDFat cal;
//#endif

//#define FILTER_UPDATE_RATE_HZ 100
//#define PRINT_EVERY_N_UPDATES 10
//#define AHRS_DEBUG_OUTPUT
#define LED_PIN 33
//#define BUZZER_PIN 27
#define STATUS_PIN 13
#define PIN_INPUT 32
#define PIN_power 12
#define VBATPIN A13

const int TONE_OUTPUT_PIN = 27;
const int TONE_PWM_CHANNEL = 7;


OneButton button(PIN_INPUT, true);

// Initialize Wi-Fi
WiFiMulti wifiMulti;
String connectionDetails; // Variable to store connection details
String phoneNumber = "6478072595";
//String phoneNumber_updt = "";

unsigned long lastExecutionTime = 0;
unsigned long previousMillis = 0;
unsigned long previousMillis1 = 0;
unsigned long previousMillis2 = 0;
unsigned long previousMillis3 = 0;
unsigned long previousMillis_wifi = 0;

const long interval = 200;   // 200 millisecond
const long interval1 = 1000;  // 1 second
const long interval2 = 2000;  // 2 seconds
const long interval3 = 500;   // 0.5 seconds
unsigned long interval_wifi = 240000; // Check every 30 seconds
const unsigned long executionInterval_gps = 120000; // Interval in milliseconds gps


bool ledState = LOW;
bool buzzerState = LOW;
bool alarmState = LOW;
bool POWERState = LOW;
bool isSetupCompleted = false;

uint32_t timestamp;
int TRIES = 5;


// Define a global variable to store the previous OwnerInfo_string
//String previousOwnerInfo_string;
bool isDisconnected = false;

void setup() {
  Serial.begin(115200);
  Serial1.begin(115200);



  pinMode(LED_PIN, OUTPUT);
  //pinMode(BUZZER_PIN, OUTPUT);
  pinMode(STATUS_PIN, OUTPUT);

  timestamp = millis();

  Wire.setClock(400000); // 400KHz

  // Initialize button
  pinMode(PIN_power, OUTPUT);
  pinMode(LED_PIN, OUTPUT);
  // Set initial power state
  digitalWrite(PIN_power, POWERState);
  button.attachLongPressStart(longClick);
  button.attachDoubleClick(doubleClick);

  delay(200);
}

void loop() {
  float measuredvbat = analogReadMilliVolts(VBATPIN);
  measuredvbat *= 2; // we divided by 2, so multiply back
  measuredvbat /= 1000; // convert to volts!

  // keep watching the push button:
  button.tick();
  
  if (POWERState == HIGH) {
    digitalWrite(STATUS_PIN, HIGH);
    delay(200);
    digitalWrite(STATUS_PIN, LOW);
    if (!isSetupCompleted) {
      // Run setup code only once
      runSetup();
      isSetupCompleted = true;
       isDisconnected = false;
    }
    
    MainLoopProcess();
    button.tick();

    if (deviceConnected) {
      pCharacteristic->setValue(measuredvbat);
      pCharacteristic->notify();
      //Serial.print("VBat: "); Serial.println(measuredvbat);
      delay(100);

    }
  }
}

void runSetup() {

  setup_GPS();
  delay(200);
  setup_NFC("John", "Doe", phoneNumber);
  delay(100);
  initWIFI();
  delay(100);
  setup_BLE();
  delay(100);

  //digitalWrite(STATUS_PIN, HIGH);

  sound(NOTE_C);
  delay(100);
  sound(NOTE_C);
}

void MainLoopProcess () {
  button.tick();

  static uint8_t counter = 0;
  unsigned long currentTime = millis();

  //-------------------GPS-------------------//
  //every executioninterval_gps, read gps and send sms
  // Check if the specified interval has passed since the last execution
  if (currentTime - lastExecutionTime >= executionInterval_gps) {
    // Update the last execution time
    lastExecutionTime = currentTime;
    GPS_values();
    button.tick();
  }

  //-----------------WIFI--------------------//

  //every interval_wifi and bleutooth not connected, look for the strongest wifi available
  if (!deviceConnected && (currentTime - previousMillis_wifi >= interval_wifi)) {
    Serial.println("Connecting to Wi-Fi...");
    int attempts = 0;
    bool connected = false;
    button.tick();
    while (!connected && attempts < 1) {
      connected = (wifiMulti.run() == WL_CONNECTED);
      if (!connected) {
        delay(100);
        Serial.print(".");
        button.tick();
        attempts++;
      }
    }

    previousMillis_wifi = currentTime;
    Serial.println("");

    if (connected) {
      Serial.println("Wi-Fi connected");

      String connectionDetails = "SSID: " + WiFi.SSID() + " " + "MAC: " + WiFi.macAddress();

      // Print the connection details
      Serial.print("SSID:"); Serial.println(String(WiFi.SSID()));
      Serial.print("MAC: "); Serial.println(String(WiFi.macAddress()));

      sendMessage(connectionDetails);
      delay(1000);
    } else {
      Serial.println("Wi-Fi not connected");
      button.tick();
    }
  }


  ////----------------SET_UP NFC-----------------//
  //if (deviceConnected) {
  //
  //    std::string OwnerInfo = pCharacteristic_3->getValue();
  //    String OwnerInfo_string = String(OwnerInfo.c_str());
  //
  //// Check if OwnerInfo_string is different from the previous one
  //    if (OwnerInfo_string != previousOwnerInfo_string) {
  //        previousOwnerInfo_string = OwnerInfo_string; // Update previousOwnerInfo_string
  //
  //        // Find the positions of commas
  //        int firstCommaPos = OwnerInfo_string.indexOf(',');
  //        int secondCommaPos = OwnerInfo_string.indexOf(',', firstCommaPos + 1);
  //
  //        // Extract substrings
  //        String firstname = OwnerInfo_string.substring(0, firstCommaPos);
  //        String lastName = OwnerInfo_string.substring(firstCommaPos + 1, secondCommaPos);
  //        phoneNumber_updt = OwnerInfo_string.substring(secondCommaPos + 1);
  //
  //        // Print the extracted substrings
  //        Serial.print("Name: ");
  //        Serial.println(firstname);
  //        Serial.print("Last Name: ");
  //        Serial.println(lastName);
  //        Serial.print("Phone Number: ");
  //        Serial.println(phoneNumber_updt);
  //
  //        setup_NFC(firstname, lastName, phoneNumber_updt);
  //    }
  //
  //}




  //--------------BUZZER_LIGHT--------------//
  //bluetooth reading value from Characteristic2 for buzzer/light
  if (deviceConnected) {
    std::string rxValue = pCharacteristic_2->getValue();
    String rxValue_string = String(rxValue.c_str());
    int rxValue_int = rxValue_string.toInt();
    //    Serial.print("Characteristcs 2 (getValue): ");
    //    Serial.println(rxValue_int);

    switch (rxValue_int) {
      case 1:
        digitalWrite(LED_PIN, LOW);
        //tone(BUZZER_PIN, 0);
        ledcDetachPin(TONE_OUTPUT_PIN);
        break;

      case 2:
        // Toggle LED at 1 sec interval
        if (millis() - previousMillis1 >= interval) {
          previousMillis1 = millis();
          for (int i = 0; i < 10; i++) {
            digitalWrite(LED_PIN, HIGH);
            delay(200);
            digitalWrite(LED_PIN, LOW);
            delay(200);
          }
          ledcDetachPin(TONE_OUTPUT_PIN);
        }
        break;
      case 3:
        // Toggle Buzzer at 1 sec interval
        if (millis() - previousMillis2 >= interval1) {
          previousMillis2 = millis();
          buzzerState = !buzzerState;
          //digitalWrite(BUZZER_PIN, buzzerState);
          digitalWrite(LED_PIN, LOW);
          //tone(BUZZER_PIN, buzzerState ? 1500 : 0);
          ledcAttachPin(TONE_OUTPUT_PIN, TONE_PWM_CHANNEL);
          ledcWriteNote(TONE_PWM_CHANNEL, NOTE_C, 4);
          delay(500);
          ledcWriteTone(TONE_PWM_CHANNEL, NOTE_F);
          delay(1000);
          ledcDetachPin(TONE_OUTPUT_PIN);
        }
        break;
      case 4:
        // Toggle Buzzer and Alarm at 0.5 sec interval
        if (millis() - previousMillis3 >= interval3) {
          previousMillis3 = millis();
          buzzerState = !buzzerState;
          alarmState = !alarmState;
          //tone(BUZZER_PIN, buzzerState ? 1500 : 0);
          ledcAttachPin(TONE_OUTPUT_PIN, TONE_PWM_CHANNEL);
          ledcWriteNote(TONE_PWM_CHANNEL, NOTE_C, 4);
          delay(500);
          ledcWriteTone(TONE_PWM_CHANNEL, NOTE_F);
          for (int i = 0; i < 10; i++) {
            digitalWrite(LED_PIN, HIGH);
            delay(200);
            digitalWrite(LED_PIN, LOW);
            delay(200);
          }
          delay(100);
          ledcDetachPin(TONE_OUTPUT_PIN);
        }
        break;
    }

  }

  // disconnecting
  if (!deviceConnected && oldDeviceConnected) {
    delay(500); // give the bluetooth stack the chance to get things ready
    ledcAttachPin(TONE_OUTPUT_PIN, TONE_PWM_CHANNEL);
    ledcWriteNote(TONE_PWM_CHANNEL, NOTE_E, 1);
    delay(500);
    ledcDetachPin(TONE_OUTPUT_PIN);
    delay(5000);
    pServer->startAdvertising(); // restart advertising
    Serial.println("start advertising");
    oldDeviceConnected = deviceConnected;

  }

  // connecting
  if (deviceConnected && !oldDeviceConnected) {
    // do stuff here on connecting
    oldDeviceConnected = deviceConnected;
  }

}

void sendCommandWithRetry(const char* command, int maxRetries) {
  button.tick();
  int retries = 0;
  while (retries < maxRetries) {
    Serial1.println(command);
    delay(1000); // Adjust delay as needed

    // Check the response
    String response = Serial1.readStringUntil('\n');
    if (response.indexOf("OK") != -1) {
      // Command successful, break out of the loop
      break;
    } else {
      // Command failed, increment retries and try again
      retries++;
    }
  }
}

void setup_GPS() {
  Serial.println("inside setup_gps");
  button.tick();
  sendCommandWithRetry("AT+CGPSCOLD", TRIES); // cold start
  delay(100);
  sendCommandWithRetry("AT+CGNSSMODE=3", TRIES);// mode gps
  delay(100);
  sendCommandWithRetry("AT+CGNSSNMEA=1,0,0,0,0,0,0,0", TRIES); // gnss output
  delay(100);
  sendCommandWithRetry("AT+CGPSNMEARATE=5", TRIES); // anchor points 55Hz
  delay(100);
  sendCommandWithRetry("AT+CGNSSINFO=10", TRIES); //
  delay(100);
  Serial.println("done setup_gps");
}

// this function will be called when the button was long pressed
void longClick() {

  // Toggle power state
  POWERState = !POWERState;
  digitalWrite(PIN_power, POWERState);

  // Produce different beep patterns based on the power state
  if (POWERState == HIGH) {
    Serial.println("clicked on");
    // Power turned on, produce a long beep
    digitalWrite(LED_PIN, HIGH);
    delay(1000);
    sound(NOTE_A);
    digitalWrite(LED_PIN, LOW);
    sendCommandWithRetry("AT+CMGF=1", TRIES); // messaign send
    delay(100);
    sendCommandWithRetry("AT+CGNSSPWR=1", TRIES); // gps on
    delay(100);

  } else {
    Serial.println("clicked off");
    for (int i = 0; i < 3; i++) {
      sound(NOTE_A);
      digitalWrite(LED_PIN, HIGH);
      delay(200);
      digitalWrite(LED_PIN, LOW);
      delay(200); // Delay between beeps
    }
    delay(100);
    // Power turned off, produce three short beeps
    sendCommandWithRetry("AT+CMGF=0", TRIES); // messaign send
    delay(100);
    sendCommandWithRetry("AT+CGNSSPWR=0", TRIES); // gps off
    delay(100);

    digitalWrite(STATUS_PIN, LOW);
    isSetupCompleted = false;
    ESP.restart();
  }
}

void initWIFI() {
  WiFi.mode(WIFI_STA);
  // Initialize Wi-Fi
  wifiMulti.addAP(ssid1, password1);
  wifiMulti.addAP(ssid2, password2);
  wifiMulti.addAP(ssid3, password3);
  wifiMulti.addAP(ssid4, password4);
  //wifiMulti.addAP(ssid5, password5);
  delay(200);

}

void doubleClick()
{
  Serial.println("RESET");
  for (int i = 0; i < 3; i++ ) {
    sound(NOTE_E);
    delay(100);
  }
  ESP.restart();
}

void sound(note_t note) {
  ledcAttachPin(TONE_OUTPUT_PIN, TONE_PWM_CHANNEL);
  ledcWriteNote(TONE_PWM_CHANNEL, note, 8);
  delay(50);
  ledcDetachPin(TONE_OUTPUT_PIN);
}
