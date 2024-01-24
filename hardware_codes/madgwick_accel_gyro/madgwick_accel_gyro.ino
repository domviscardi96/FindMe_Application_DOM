#include <MadgwickAHRS.h>

#include <Adafruit_LSM6DS3TRC.h>
Adafruit_LSM6DS3TRC lsm6ds; //accel + gyro

Madgwick filter;
const float sensorRate = 104.00;

void setup(void) {
  
 Serial.begin(115200);
 Wire.begin();
 while (!Serial)
 delay(50); // will pause Zero, Leonardo, etc until serial console opens
 Serial.println("Adafruit LSM6DS+LIS3MDL test!");
 bool lsm6ds_success, lis3mdl_success;
 // hardware I2C mode, can pass in address & alt Wire
 lsm6ds_success = lsm6ds.begin_I2C(0X6A);

 if (!lsm6ds_success){
 Serial.println("Failed to find LSM6DS chip");
 }

 if (!lsm6ds_success) {
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

 //start filter
 filter.begin(sensorRate);


}

void loop() {
 sensors_event_t accel, gyro, temp ;
 // /* Get new normalized sensor events */
 lsm6ds.getEvent(&accel, &gyro, &temp);
 /* Display the results (acceleration is measured in m/s^2) */
// Serial.print("\t\tAccel X: ");
// Serial.print(accel.acceleration.x, 2);
// Serial.print(" \tY: ");
// Serial.print(accel.acceleration.y, 2);
// Serial.print(" \tZ: ");
// Serial.print(accel.acceleration.z, 2);
// Serial.println(" \tm/s^2 ");
// /* Display the results (rotation is measured in rad/s) */
// Serial.print("\t\tGyro X: ");
// Serial.print(gyro.gyro.x, 2);
// Serial.print(" \tY: ");
// Serial.print(gyro.gyro.y, 2);
// Serial.print(" \tZ: ");
// Serial.print(gyro.gyro.z, 2);
// Serial.println(" \tradians/s ");
// Serial.println();

//values for orientation
float roll,pitch,heading;
//update filter
filter.updateIMU(gyro.gyro.x,gyro.gyro.y,gyro.gyro.z,accel.acceleration.x,accel.acceleration.y,accel.acceleration.z);

roll = filter.getRoll();
pitch = filter.getPitch();
heading = filter.getYaw();

 Serial.print("\t\tOrientation: ");
 Serial.print(heading);
 Serial.print(" ");
  Serial.print(pitch);
  Serial.print(" ");
  Serial.println(roll);
 delay(100);
}
