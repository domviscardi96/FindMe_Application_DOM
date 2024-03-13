#include <Adafruit_Sensor_Calibration.h>
#include <Adafruit_AHRS.h>
#include <OneButton.h>
#include <WiFi.h>
#include <WiFiMulti.h>


// Define Wi-Fi credentials
const char* ssid1 = "Pixel7 D";
const char* password1 = "nicola99";
const char* ssid2 = "BELL766";
const char* password2 = "471795C2D225";

Adafruit_Sensor *accelerometer, *gyroscope, *magnetometer;

#include "LSM6DS_LIS3MDL.h"  // can adjust to LSM6DS33, LSM6DS3U, LSM6DSOX...
#include "NFCtag.h"
#include "BLEconnection.h"
#include "GPS.h"


// pick your filter! slower == better quality output
//Adafruit_NXPSensorFusion filter; // slowest
Adafruit_Madgwick filter;  // faster than NXP
//Adafruit_Mahony filter;  // fastest/smalleset

#if defined(ADAFRUIT_SENSOR_CALIBRATION_USE_EEPROM)
Adafruit_Sensor_Calibration_EEPROM cal;
#else
Adafruit_Sensor_Calibration_SDFat cal;
#endif

#define FILTER_UPDATE_RATE_HZ 100
#define PRINT_EVERY_N_UPDATES 10
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
unsigned long interval_wifi = 300000; // Check every 30 seconds
const unsigned long executionInterval_gps = 60000; // Interval in milliseconds gps


bool ledState = LOW;
bool buzzerState = LOW;
bool alarmState = LOW;
bool POWERState = LOW;
bool isSetupCompleted = false;

uint32_t timestamp;
int TRIES = 5;

// Define a global variable to store the previous OwnerInfo_string
String previousOwnerInfo_string;


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

  delay(200);
}

void loop() {
  float measuredvbat = analogReadMilliVolts(VBATPIN);
  measuredvbat *= 2; // we divided by 2, so multiply back
  measuredvbat /= 1000; // convert to volts!
  
  // keep watching the push button:
  button.tick();

  // Check if the power state is LOW, if so, return immediately
  if (POWERState == LOW) {
    WiFi.disconnect();
    return;
  }

  if (POWERState == HIGH) {
    digitalWrite(STATUS_PIN, HIGH);
    delay(200);
    digitalWrite(STATUS_PIN, LOW);
    if (!isSetupCompleted) {
      // Run setup code only once
      runSetup();
      isSetupCompleted = true;
    }
    MainLoopProcess();
    
    if (deviceConnected) {
    pCharacteristic->setValue(measuredvbat);
    pCharacteristic->notify();
    Serial.print("VBat: "); Serial.println(measuredvbat);
    delay(100);

  }
}
}

void runSetup() {

  setup_BLE();
  delay(100);
  initWIFI();
  delay(100);
  setup_NFC("default","default","default");
  delay(100);
  while (!Serial) yield();

  if (!cal.begin()) {
    Serial.println("Failed to initialize calibration helper");
  } else if (!cal.loadCalibration()) {
    Serial.println("No calibration loaded/found");
  }

  if (!init_sensors()) {
    Serial.println("Failed to find sensors");
    //while (1) delay(10);
  }

  setup_sensors();
  accelerometer->printSensorDetails();
  gyroscope->printSensorDetails();
  magnetometer->printSensorDetails();


  filter.begin(FILTER_UPDATE_RATE_HZ);


  setup_GPS();
  delay(200);

  digitalWrite(STATUS_PIN, HIGH);

}

void MainLoopProcess () {

  float roll, pitch, heading;
  float gx, gy, gz;
  static uint8_t counter = 0;
  unsigned long currentTime = millis();

  if ((millis() - timestamp) < (1000 / FILTER_UPDATE_RATE_HZ)) {
    return;
  }
  timestamp = millis();
  // Read the motion sensors
  sensors_event_t accel, gyro, mag;
  accelerometer->getEvent(&accel);
  gyroscope->getEvent(&gyro);
  magnetometer->getEvent(&mag);
#if defined(AHRS_DEBUG_OUTPUT)
  Serial.print("I2C took "); Serial.print(millis() - timestamp); Serial.println(" ms");
#endif

  cal.calibrate(mag);
  cal.calibrate(accel);
  cal.calibrate(gyro);
  // Gyroscope needs to be converted from Rad/s to Degree/s
  // the rest are not unit-important
  gx = gyro.gyro.x * SENSORS_RADS_TO_DPS;
  gy = gyro.gyro.y * SENSORS_RADS_TO_DPS;
  gz = gyro.gyro.z * SENSORS_RADS_TO_DPS;

  // Update the SensorFusion filter
  filter.update(gx, gy, gz,
                accel.acceleration.x, accel.acceleration.y, accel.acceleration.z,
                mag.magnetic.x, mag.magnetic.y, mag.magnetic.z);
#if defined(AHRS_DEBUG_OUTPUT)
  Serial.print("Update took "); Serial.print(millis() - timestamp); Serial.println(" ms");
#endif

  // only print the calculated output once in a while
  if (counter++ <= PRINT_EVERY_N_UPDATES) {
    return;
  }
  // reset the counter
  counter = 0;

  // print the heading, pitch and roll
  roll = filter.getRoll();
  pitch = filter.getPitch();
  heading = filter.getYaw();
  //  Serial.print("Orientation: ");
  //  Serial.print(heading);
  //  Serial.print(", ");
  //  Serial.print(pitch);
  //  Serial.print(", ");
  //  Serial.println(roll);

  float qw, qx, qy, qz;
  filter.getQuaternion(&qw, &qx, &qy, &qz);
  //  Serial.print("Quaternion: ");
  //  Serial.print(qw, 4);
  //  Serial.print(", ");
  //  Serial.print(qx, 4);
  //  Serial.print(", ");
  //  Serial.print(qy, 4);
  //  Serial.print(", ");
  //  Serial.println(qz, 4);

#if defined(AHRS_DEBUG_OUTPUT)
  Serial.print("Took "); Serial.print(millis() - timestamp); Serial.println(" ms");
#endif

  //-------------------GPS-------------------//
  //every executioninterval_gps, read gps and send sms
  // Check if the specified interval has passed since the last execution
  if (currentTime - lastExecutionTime >= executionInterval_gps) {
    // Update the last execution time
    lastExecutionTime = currentTime;
    GPS_values();
  }

//  //-----------------WIFI--------------------//
//  //every interval_wifi and bleutooth not connected, look for the strongest wifi wvailable
//  if (!deviceConnected && (currentTime - previousMillis_wifi >= interval_wifi)) {
//    Serial.println("Connecting to Wi-Fi...");
//    while (wifiMulti.run() != WL_CONNECTED) {
//      delay(100);
//      Serial.print(".");
//    }
//
//    previousMillis_wifi = currentTime;
//    Serial.println("");
//    Serial.println("Wi-Fi connected");
//
//    // Store connection details in a string
//    connectionDetails = "Connected to " + String(WiFi.SSID()) + "\n"
//                        + "IP address: " + WiFi.localIP().toString() + "\n"
//                        + "Signal Strength: " + String(WiFi.RSSI());
//
//    // Print the connection details
//    Serial.println(connectionDetails);
//
//    sendMessage(connectionDetails);
//    delay(2000);
//  }


//----------------SET_UP NFC-----------------//
if (deviceConnected) {

    std::string OwnerInfo = pCharacteristic_3->getValue();
    String OwnerInfo_string = String(OwnerInfo.c_str());

// Check if OwnerInfo_string is different from the previous one
    if (OwnerInfo_string != previousOwnerInfo_string) {
        previousOwnerInfo_string = OwnerInfo_string; // Update previousOwnerInfo_string

        // Find the positions of commas
        int firstCommaPos = OwnerInfo_string.indexOf(',');
        int secondCommaPos = OwnerInfo_string.indexOf(',', firstCommaPos + 1);

        // Extract substrings
        String firstname = OwnerInfo_string.substring(0, firstCommaPos);
        String lastName = OwnerInfo_string.substring(firstCommaPos + 1, secondCommaPos);
        String phoneNumber = OwnerInfo_string.substring(secondCommaPos + 1);

        // Print the extracted substrings
        Serial.print("Name: ");
        Serial.println(firstname);
        Serial.print("Last Name: ");
        Serial.println(lastName);
        Serial.print("Phone Number: ");
        Serial.println(phoneNumber);

        setup_NFC(firstname, lastName, phoneNumber);
    }
    
}




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
          ledState = !ledState;
          digitalWrite(LED_PIN, ledState);
          ledcDetachPin(TONE_OUTPUT_PIN);
          //tone(BUZZER_PIN, 0);
          //        Serial.println("im in case1");
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
          delay(500);
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
          delay(500);
          digitalWrite(LED_PIN, alarmState);

        }
        break;
    }

  }

  // disconnecting
  if (!deviceConnected && oldDeviceConnected) {
    delay(500); // give the bluetooth stack the chance to get things ready
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

  digitalWrite(LED_PIN, HIGH);
  delay(200); // Wait for 1 second
  digitalWrite(LED_PIN, LOW);
  delay(200); // Wait for 1 second
  digitalWrite(LED_PIN, HIGH);
  delay(800); // Wait for 1 second
  digitalWrite(LED_PIN, LOW);
  delay(800); // Wait for 1 second
  digitalWrite(LED_PIN, HIGH);
  delay(200); // Wait for 1 second
  digitalWrite(LED_PIN, LOW);

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
    digitalWrite(LED_PIN, LOW);
    sendCommandWithRetry("AT+CMGF=1", TRIES); // messaign send
    delay(100);
    sendCommandWithRetry("AT+CGNSSPWR=1", TRIES); // gps on
    delay(100);
    
  } else {
    Serial.println("clicked off");
    for (int i = 0; i < 3; i++) {
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
    //POWERState=LOW;
  }
}

void initWIFI() {
  WiFi.mode(WIFI_STA);
  // Initialize Wi-Fi
  wifiMulti.addAP(ssid1, password1);
  wifiMulti.addAP(ssid2, password2);
  //wifiMulti.addAP(ssid3, password3);

}
