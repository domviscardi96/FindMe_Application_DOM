
#include <BTAddress.h>
#include <BTAdvertisedDevice.h>
#include <BTScan.h>

/*
  Based on Neil Kolban example for IDF: https://github.com/nkolban/esp32-snippets/blob/master/cpp_utils/tests/BLE%20Tests/SampleNotify.cpp
  Ported to Arduino ESP32 by Evandro Copercini
  updated by chegewara and MoThunderz
*/
#include <BLEDevice.h>
#include <BLEServer.h>
#include <BLEUtils.h>
#include <BLE2902.h>



//initialize the values
BLEServer* pServer = NULL;
BLECharacteristic* pCharacteristic = NULL;
BLECharacteristic* pCharacteristic_2 = NULL;
BLEDescriptor *pDescr;
BLE2902 *pBLE2902;

bool deviceConnected = false;
bool oldDeviceConnected = false;
uint32_t value = 0;

// See the following for generating UUIDs:
// https://www.uuidgenerator.net/

#define SERVICE_UUID        "633ae448-8b19-11ee-b9d1-0242ac120002" //set up main service uuid
#define CHAR1_UUID          "6e9d7936-8b19-11ee-b9d1-0242ac120002"
#define CHAR2_UUID          "786d85f0-8b19-11ee-b9d1-0242ac120002"

#define LED_PIN 33
#define BUZZER_PIN 13

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

//connect via mobile, both accessin the server
class MyServerCallbacks: public BLEServerCallbacks {
    void onConnect(BLEServer* pServer) {
      deviceConnected = true;
    };

    void onDisconnect(BLEServer* pServer) {
      deviceConnected = false;
    }
};

class CharacteristicCallBack: public BLECharacteristicCallbacks {
    void onWrite(BLECharacteristic *pChar) override {
      std::string pChar2_value_stdstr = pChar->getValue();
      String pChar2_value_string = String(pChar2_value_stdstr.c_str());
      int pChar2_value_int = pChar2_value_string.toInt();
      Serial.println("pChar2: " + String(pChar2_value_int));
    }
};


void setup() {
  delay(500);
  Serial.begin(115200);

  pinMode(LED_PIN, OUTPUT);
  pinMode(BUZZER_PIN, OUTPUT);

  // Create the BLE Device
  BLEDevice::init("AntiLoss Device");

  // Create the BLE Server
  pServer = BLEDevice::createServer();
  pServer->setCallbacks(new MyServerCallbacks());

  // Create the BLE Service
  BLEService *pService = pServer->createService(SERVICE_UUID);

  // Create a BLE Characteristic
  pCharacteristic = pService->createCharacteristic(   //mobile can only read this value, GPS.
                      CHAR1_UUID,
                      BLECharacteristic::PROPERTY_NOTIFY
                    );

  pCharacteristic_2 = pService->createCharacteristic( //mobile/esp can override
                        CHAR2_UUID,
                        BLECharacteristic::PROPERTY_READ   |
                        BLECharacteristic::PROPERTY_WRITE
                      );

  // Create a BLE Descriptor

  pDescr = new BLEDescriptor((uint16_t)0x2901);
  pDescr->setValue("A very interesting variable");
  pCharacteristic->addDescriptor(pDescr);

  pBLE2902 = new BLE2902();
  pBLE2902->setNotifications(true);

  // Add all Descriptors here
  pCharacteristic->addDescriptor(pBLE2902);
  pCharacteristic_2->addDescriptor(new BLE2902());

  // After defining the desriptors, set the callback functions
  pCharacteristic_2->setCallbacks(new CharacteristicCallBack());


  // Start the service
  pService->start();

  // Start advertising
  BLEAdvertising *pAdvertising = BLEDevice::getAdvertising();
  pAdvertising->addServiceUUID(SERVICE_UUID);
  pAdvertising->setScanResponse(false);
  pAdvertising->setMinPreferred(0x0);  // set value to 0x00 to not advertise this parameter
  BLEDevice::startAdvertising();
  Serial.println("Waiting a client connection to notify...");

  //      xTaskCreatePinnedToCore (
  //    loop2,     // Function to implement the task
  //    "loop2",   // Name of the task
  //    3000,      // Stack size in words
  //    NULL,      // Task input parameter
  //    0,         // Priority of the task
  //    NULL,      // Task handle.
  //    0          // Core where the task should run
  //  );
}


void loop() {
  //    // notify changed value
  //    if (deviceConnected) { //every ten seconds notify(send) via CHAR1 the increased value
  //        pCharacteristic->setValue(value);
  //        pCharacteristic->notify();
  //        value++;
  //        delay(1000);
  //    }

  if (deviceConnected) {
    std::string rxValue = pCharacteristic_2->getValue();
    String rxValue_string = String(rxValue.c_str());
    int rxValue_int = rxValue_string.toInt();
    Serial.print("Characteristcs 2 (getValue): ");
    Serial.println(rxValue_int);

   switch (rxValue_int) {
      case 1:
        // Toggle LED at 1 sec interval
        if (millis() - previousMillis1 >= interval) {
          previousMillis1 = millis();
          ledState = !ledState;
          digitalWrite(LED_PIN, ledState);
        }
        break;
      case 2:
        // Toggle Buzzer at 1 sec interval
        if (millis() - previousMillis2 >= interval1) {
          previousMillis2 = millis();
          buzzerState = !buzzerState;
          digitalWrite(BUZZER_PIN, buzzerState);
          //tone(BUZZER_PIN, buzzerState ? 1500 : 0);
        }
        break;
      case 3:
        // Toggle Buzzer and Alarm at 0.5 sec interval
        if (millis() - previousMillis3 >= interval3) {
          previousMillis3 = millis();
          buzzerState = !buzzerState;
          alarmState = !alarmState;
          //tone(BUZZER_PIN, buzzerState ? 1500 : 0);
          digitalWrite(LED_PIN, alarmState);
          digitalWrite(BUZZER_PIN, buzzerState);
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

//void loop2 (void* pvParameters) {
//while(1) {
//
//  }
//}
