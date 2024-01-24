// Include necessary libraries
#include <HardwareSerial.h>

// Define the Serial channels
#define DEVICE_SERIAL Serial1
#define LAPTOP_SERIAL Serial

void setup() {
  // Start serial communication with the laptop
  LAPTOP_SERIAL.begin(115200);
  // Start serial communication with the device on Serial1
  DEVICE_SERIAL.begin(9600);
}

void loop() {
  // Check if there is data available on the device's Serial1 channel
  if (DEVICE_SERIAL.available()) {
    // Read the data from the device
    String dataFromDevice = DEVICE_SERIAL.read();

    // Display the data on the laptop's Serial monitor
    LAPTOP_SERIAL.print("Received from device: ");
    LAPTOP_SERIAL.println(dataFromDevice);

    // Process the received data if needed
    // Add your logic here...

    // Send the data back to the device if needed
    // DEVICE_SERIAL.write(dataFromDevice);
  }

  // Check if there is data available on the laptop's Serial channel
  if (LAPTOP_SERIAL.available()) {
    // Read the data from the laptop
    String dataFromLaptop = LAPTOP_SERIAL.read();

    // Display the data on the device's Serial1 channel
    DEVICE_SERIAL.print("Received from laptop: ");
    DEVICE_SERIAL.println(dataFromLaptop);

    // Process the received data if needed
    // Add your logic here...

    // Send the data back to the laptop if needed
    // LAPTOP_SERIAL.write(dataFromLaptop);
  }

  delay(1000);

  // Add any additional code as needed for your application
}
