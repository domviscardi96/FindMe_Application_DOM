#include <ArduinoJson.h>
#include "ST25DVSensor.h"

#define SerialPort      Serial
#define DEV_I2C         Wire
ST25DV st25dv(12, -1, &DEV_I2C);

////Variables info coming from bluetooth
const char* firstname = "John";
const char* lastname = "Doe";
const char* phone_number = "123456789";

void  setup_NFC(void) {
   // Construct the URL with values from Bluetooth variables   
  String uri_write_message = "aabbfarf.atwebpages.com/presentation/?firstname=" + String(firstname) +
                             "&lastname=" + String(lastname) +
                             "&phone=" + String(phone_number);
  const char uri_write_protocol[] = URI_ID_0x03_STRING; // Uri protocol to write in the tag

    // Convert the String to a C-style string (const char*)
  const char* uri_message_cstr = uri_write_message.c_str();


  // The wire instance used can be omitted in case you use the default Wire instance
  if (st25dv.begin() == 0) {
    SerialPort.println("System Init done!");
  } else {
    SerialPort.println("System Init failed!");
    while (1);
  }

  if (st25dv.writeURI(uri_write_protocol, uri_message_cstr, "")) {
    SerialPort.println("Write failed!");
    while (1);
  }
}
