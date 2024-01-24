// Extended Kalman Filter (EKF):
// For orientation tracking using an EKF, you would need to create a more involved implementation,
//--and it's recommended to use a sensor fusion library. One such library is MadgwickAHRS, 
//--which implements a Mahony filter (a simplified version of the EKF) suitable for 
//--resource-constrained environments like Arduino.

#include <Wire.h>
#include <MadgwickAHRS.h>

Madgwick filter;

void setup() {
  Serial.begin(9600);
  Wire.begin();
  filter.begin(100); // Sample rate in Hz
}

void loop() {
  // Read accelerometer, gyroscope, and magnetometer data
  // Replace the following lines with your sensor readings
  float ax = 0.0; // accelerometer x
  float ay = 0.0; // accelerometer y
  float az = 0.0; // accelerometer z
  float gx = 0.0; // gyroscope x
  float gy = 0.0; // gyroscope y
  float gz = 0.0; // gyroscope z
  float mx = 0.0; // magnetometer x
  float my = 0.0; // magnetometer y
  float mz = 0.0; // magnetometer z

  // Update filter with sensor data
  filter.updateIMU(gx, gy, gz, ax, ay, az);

  // Get the estimated orientation quaternion
  float qw, qx, qy, qz;
  filter.getQuaternion(qw, qx, qy, qz);

  // Convert quaternion to Euler angles
  float roll, pitch, yaw;
  filter.getEuler(roll, pitch, yaw);

  // Print the Euler angles
  Serial.print("Roll: ");
  Serial.print(roll * 180.0 / PI);
  Serial.print("\tPitch: ");
  Serial.print(pitch * 180.0 / PI);
  Serial.print("\tYaw: ");
  Serial.println(yaw * 180.0 / PI);

  delay(10);
}
