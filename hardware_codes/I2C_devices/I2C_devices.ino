
#include <Adafruit_LSM6DS3TRC.h>
Adafruit_LSM6DS3TRC lsm6ds;
#include <Adafruit_LIS3MDL.h>
Adafruit_LIS3MDL lis3mdl;

#include "ST25DVSensor.h"

#define SerialPort      Serial

  #define DEV_I2C         Wire
  ST25DV st25dv(12, -1, &DEV_I2C);


void setup(void) {
  
 Serial.begin(115200);
 Wire.begin();
 while (!Serial)
 delay(50); // will pause Zero, Leonardo, etc until serial console opens
 Serial.println("Adafruit LSM6DS+LIS3MDL test!");
 bool lsm6ds_success, lis3mdl_success;
 // hardware I2C mode, can pass in address & alt Wire
 lsm6ds_success = lsm6ds.begin_I2C(0X6A);
 lis3mdl_success = lis3mdl.begin_I2C(0x1E);
 if (!lsm6ds_success){
 Serial.println("Failed to find LSM6DS chip");
 }
 if (!lis3mdl_success){
 Serial.println("Failed to find LIS3MDL chip");
 }
 if (!(lsm6ds_success && lis3mdl_success)) {
 while (1) {
 delay(10);
 }
 }
 Serial.println("LSM6DS and LIS3MDL Found!");

//-----LSM6DS set up range and data rate-----//
//Set ranges
 lsm6ds.setAccelRange(LSM6DS_ACCEL_RANGE_4_G); //+-4G range
 Serial.print("Accelerometer range set to: +-4G ");
 
 lsm6ds.setAccelDataRate(LSM6DS_RATE_104_HZ);
 Serial.print("Accelerometer data rate set to: 104Hz");
 
 lsm6ds.setGyroRange(LSM6DS_GYRO_RANGE_250_DPS );
 Serial.print("Gyro range set to: 250 degrees/s ");

 lsm6ds.setGyroDataRate(LSM6DS_RATE_104_HZ);
 Serial.print("Gyro data rate set to: 104Hz ");


//-----LIS3MDL set up range and data rate-----//
 lis3mdl.setDataRate(LIS3MDL_DATARATE_155_HZ);
 // You can check the datarate by looking at the frequency of the DRDY pin
 Serial.print("Magnetometer data rate set to: 155Hz ");

 lis3mdl.setRange(LIS3MDL_RANGE_4_GAUSS);
 Serial.print("Range set to: 4 Gauss ");

 lis3mdl.setPerformanceMode(LIS3MDL_MEDIUMMODE);
 Serial.print("Magnetometer performance mode set to: MEDIUMMODE ");

 lis3mdl.setOperationMode(LIS3MDL_CONTINUOUSMODE);
 Serial.print("Magnetometer operation mode set to: CONTINUOUSMODE ");

 lis3mdl.setIntThreshold(500);
 lis3mdl.configInterrupt(false, false, true, // enable z axis
 true, // polarity
false, // don't latch
true); // enabled!

const char uri_write_message[] = "github.com/stm32duino/ST25DV";       // Uri message to write in the tag
  const char uri_write_protocol[] = URI_ID_0x01_STRING; // Uri protocol to write in the tag
  String uri_write = String(uri_write_protocol) + String(uri_write_message);

  // Initialize serial for output.
  SerialPort.begin(115200);

  // The wire instance used can be omitted in case you use default Wire instance
  if(st25dv.begin() == 0) {
    SerialPort.println("System Init done!");
  } else {
    SerialPort.println("System Init failed!");
    while(1);
  }

  if(st25dv.writeURI(uri_write_protocol, uri_write_message, "")) {
    SerialPort.println("Write failed!");
    while(1);
  }

}

void loop() {
 sensors_event_t accel, gyro, mag, temp;
 // /* Get new normalized sensor events */
 lsm6ds.getEvent(&accel, &gyro, &temp);
 lis3mdl.getEvent(&mag);
 /* Display the results (acceleration is measured in m/s^2) */
 Serial.print("\t\tAccel X: ");
 Serial.print(accel.acceleration.x, 4);
 Serial.print(" \tY: ");
 Serial.print(accel.acceleration.y, 4);
 Serial.print(" \tZ: ");
 Serial.print(accel.acceleration.z, 4);
 Serial.println(" \tm/s^2 ");
 /* Display the results (rotation is measured in rad/s) */
 Serial.print("\t\tGyro X: ");
 Serial.print(gyro.gyro.x, 4);
 Serial.print(" \tY: ");
 Serial.print(gyro.gyro.y, 4);
 Serial.print(" \tZ: ");
 Serial.print(gyro.gyro.z, 4);
 Serial.println(" \tradians/s ");
 /* Display the results (magnetic field is measured in uTesla) */
 Serial.print(" \t\tMag X: ");
 Serial.print(mag.magnetic.x, 4);
 Serial.print(" \tY: ");
 Serial.print(mag.magnetic.y, 4);
 Serial.print(" \tZ: ");
 Serial.print(mag.magnetic.z, 4);
 Serial.println(" \tuTesla ");

 Serial.println();
 delay(1000);
}