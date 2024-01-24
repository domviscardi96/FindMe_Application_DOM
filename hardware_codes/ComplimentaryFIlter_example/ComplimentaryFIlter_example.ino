#include <Wire.h>
#include <MPU6050.h>

MPU6050 mpu;

float accel_angle_x, accel_angle_y; // Accelerometer-based angle estimates
float gyro_angle_x, gyro_angle_y;   // Gyroscope-based angle estimates
float alpha = 0.98;                 // Complementary filter constant

void setup() {
  Serial.begin(9600);
  Wire.begin();
  mpu.initialize();
}

void loop() {
  // Read accelerometer and gyroscope data
  mpu.getMotion6();
  int16_t ax = mpu.getAccelerationX();
  int16_t ay = mpu.getAccelerationY();
  int16_t az = mpu.getAccelerationZ();
  int16_t gx = mpu.getRotationX();
  int16_t gy = mpu.getRotationY();
  int16_t gz = mpu.getRotationZ();

  // Convert accelerometer data to angles
  accel_angle_x = atan2(ay, az) * 180.0 / PI;
  accel_angle_y = atan2(-ax, sqrt(ay * ay + az * az)) * 180.0 / PI;

  // Integrate gyroscope data to get angles
  gyro_angle_x += gx / 131.0 * 0.01; // Replace 131.0 with your gyroscope sensitivity
  gyro_angle_y += gy / 131.0 * 0.01; // 0.01 is the time interval between measurements

  // Apply complementary filter
  float angle_x = alpha * gyro_angle_x + (1 - alpha) * accel_angle_x;
  float angle_y = alpha * gyro_angle_y + (1 - alpha) * accel_angle_y;

  // Print the angles
  Serial.print("Angle X: ");
  Serial.print(angle_x);
  Serial.print("\tAngle Y: ");
  Serial.println(angle_y);

  delay(10);
}
