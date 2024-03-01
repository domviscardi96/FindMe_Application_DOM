#include <Adafruit_Sensor_Calibration.h>
#include <Adafruit_AHRS.h>
#include <OneButton.h>


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
#define BUZZER_PIN 27
#define STATUS_PIN 13
#define PIN_INPUT 32
#define PIN_power 12

OneButton button(PIN_INPUT, true);

unsigned long lastExecutionTime = 0;
const unsigned long executionInterval_gps = 150000; // Interval in milliseconds gps

unsigned long previousMillis = 0;
unsigned long previousMillis1 = 0;
unsigned long previousMillis2 = 0;
unsigned long previousMillis3 = 0;

const long interval = 1000;   // 1 second
const long interval1 = 1000;  // 1 second
const long interval2 = 2000;  // 2 seconds
const long interval3 = 500;   // 0.5 seconds

bool ledState = LOW;
bool buzzerState = LOW;
bool alarmState = LOW;
int POWERState = LOW;
bool isSetupCompleted = false;

uint32_t timestamp;
int TRIES = 5;


void setup() {
  Serial.begin(115200);
  Serial1.begin(115200);

  
//  setup_NFC();
  setup_BLE();
//  
//  while (!Serial) yield();
//
//  if (!cal.begin()) {
//    Serial.println("Failed to initialize calibration helper");
//  } else if (! cal.loadCalibration()) {
//    Serial.println("No calibration loaded/found");
//  }
//
//  if (!init_sensors()) {
//    Serial.println("Failed to find sensors");
//    while (1) delay(10);
//  }
  
  pinMode(LED_PIN, OUTPUT);
  pinMode(BUZZER_PIN, OUTPUT);
  pinMode(STATUS_PIN, OUTPUT);
  
  
//  accelerometer->printSensorDetails();
//  gyroscope->printSensorDetails();
//  magnetometer->printSensorDetails();
//
//  setup_sensors();
//  filter.begin(FILTER_UPDATE_RATE_HZ);
  timestamp = millis();
//
  Wire.setClock(400000); // 400KHz
//  
//  setup_GPS();


 // Initialize button
  pinMode(PIN_power, OUTPUT);
  pinMode(LED_PIN, OUTPUT);
  // Set initial power state
  digitalWrite(PIN_power, POWERState);
  button.attachLongPressStart(longClick);

delay(200);
}

void loop() {
  // keep watching the push button:
  button.tick();

  // Check if the power state is LOW, if so, return immediately
  if (POWERState == LOW) {
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
  } 
}

void runSetup() {
  setup_NFC(); 

  while (!Serial) yield();

  if (!cal.begin()) {
    Serial.println("Failed to initialize calibration helper");
  } else if (!cal.loadCalibration()) {
    Serial.println("No calibration loaded/found");
  }

  if (!init_sensors()) {
    Serial.println("Failed to find sensors");
    while (1) delay(10);
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
  Serial.print("I2C took "); Serial.print(millis()-timestamp); Serial.println(" ms");
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
  Serial.print("Update took "); Serial.print(millis()-timestamp); Serial.println(" ms");
#endif

  // only print the calculated output once in a while
  if (counter++ <= PRINT_EVERY_N_UPDATES) {
    return;
  }
  // reset the counter
  counter = 0;

#if defined(AHRS_DEBUG_OUTPUT)
  Serial.print("Raw: ");
  Serial.print(accel.acceleration.x, 4); Serial.print(", ");
  Serial.print(accel.acceleration.y, 4); Serial.print(", ");
  Serial.print(accel.acceleration.z, 4); Serial.print(", ");
  Serial.print(gx, 4); Serial.print(", ");
  Serial.print(gy, 4); Serial.print(", ");
  Serial.print(gz, 4); Serial.print(", ");
  Serial.print(mag.magnetic.x, 4); Serial.print(", ");
  Serial.print(mag.magnetic.y, 4); Serial.print(", ");
  Serial.print(mag.magnetic.z, 4); Serial.println("");
#endif

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
  Serial.print("Took "); Serial.print(millis()-timestamp); Serial.println(" ms");
#endif


//every executioninterval_gps, read gps and send sms
    // Check if the specified interval has passed since the last execution
    if (currentTime - lastExecutionTime >= executionInterval_gps) {
        // Update the last execution time
        lastExecutionTime = currentTime;
        GPS_values();
    }

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
          tone(BUZZER_PIN, 0);
        
        break;
    
      case 2:
        // Toggle LED at 1 sec interval
        if (millis() - previousMillis1 >= interval) {
          previousMillis1 = millis();
          ledState = !ledState;
          digitalWrite(LED_PIN, ledState);
          tone(BUZZER_PIN, 0);
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
          tone(BUZZER_PIN, buzzerState ? 1500 : 0);
        }
        break;
      case 4:
        // Toggle Buzzer and Alarm at 0.5 sec interval
        if (millis() - previousMillis3 >= interval3) {
          previousMillis3 = millis();
          buzzerState = !buzzerState;
          alarmState = !alarmState;
          tone(BUZZER_PIN, buzzerState ? 1500 : 0);
          digitalWrite(LED_PIN, alarmState);
       //  digitalWrite(BUZZER_PIN, buzzerState);
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
  sendCommandWithRetry("AT+CMGF=1", TRIES); // messaign send
  delay(100);
  sendCommandWithRetry("AT+CGNSSPWR=1", TRIES); // gps on
 delay(5000);
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
  } else {
    Serial.println("clicked off");
    // Power turned off, produce three short beeps
    for (int i = 0; i < 3; i++) {
      digitalWrite(LED_PIN, HIGH);
    delay(200);
    digitalWrite(LED_PIN, LOW);
    delay(200); // Delay between beeps
    }
    delay(1000);
    digitalWrite(STATUS_PIN, LOW);
    isSetupCompleted = false;
    //POWERState=LOW;
  }
}
