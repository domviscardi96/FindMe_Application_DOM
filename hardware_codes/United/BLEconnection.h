/*
  Based on Neil Kolban example for IDF: https://github.com/nkolban/esp32-snippets/blob/master/cpp_utils/tests/BLE%20Tests/SampleNotify.cpp
  Ported to Arduino ESP32 by Evandro Copercini
  updated by chegewara and MoThunderz
*/
#include <BTAddress.h>
#include <BTAdvertisedDevice.h>
#include <BTScan.h>

#include <BLEDevice.h>
#include <BLEServer.h>
#include <BLEUtils.h>
#include <BLE2902.h>

//initialize the values
BLEServer* pServer = NULL;
BLECharacteristic* pCharacteristic = NULL;
BLECharacteristic* pCharacteristic_2 = NULL;
BLECharacteristic* pCharacteristic_3 = NULL;
BLEDescriptor *pDescr;
BLE2902 *pBLE2902;

bool deviceConnected = false;
bool oldDeviceConnected = false;
uint32_t value = 0;

// See the following for generating UUIDs:
// https://www.uuidgenerator.net/

#define SERVICE_UUID        "4fafc201-1fb5-459e-8fcc-c5c9c331914b" //set up main service uuid
#define CHAR1_UUID          "beb5483e-36e1-4688-b7f5-ea07361b26a8"
#define CHAR2_UUID          "e3223119-9445-4e96-a4a1-85358c4046a2"
#define CHAR3_UUID          "5106139b-9250-4533-aae9-63929fc4f86c"

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

void setup_BLE(void) {

  // Create the BLE Device
  BLEDevice::init("ESP32");

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

  pCharacteristic_2 = pService->createCharacteristic( //character for light and buzzer
                      CHAR2_UUID,
                      BLECharacteristic::PROPERTY_READ   |
                      BLECharacteristic::PROPERTY_WRITE  
                    );  

  pCharacteristic_3 = pService->createCharacteristic( //character for owner's information
                      CHAR3_UUID,
                      BLECharacteristic::PROPERTY_READ   
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
  pCharacteristic_3->addDescriptor(new BLE2902());
  
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
}
