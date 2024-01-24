void setup() {
  Serial.begin(115200);
  Serial1.begin(9600);
}

void loop() {
//  // Check if there is data available on Serial1
//  while (Serial1.available()) {
//    // Read incoming data from Serial1 and print it
//    char receivedChar = Serial1.read();
//    Serial.print(receivedChar);
//  }
//
//  // Check if there is data available on Serial (your original Serial)
//  while (Serial.available()) {
//    // Read incoming data from Serial and print it
//    char receivedChar = Serial.read();
//    Serial.print(receivedChar);
//  }

GPS_values();

  delay(2400); //time delay has to be multiple of 12
  Serial.println();
}


String GPS_values() {
  String message = "";  // Initialize an empty string to store the message

  // Read and process data from Serial1
  while (Serial1.available()) {
    // Read incoming character from Serial1
    char receivedChar = Serial1.read();

    // Print the character
    //Serial.print(receivedChar);
  

    // Check if the character is the start of the desired string "$GNGGA"
    if (receivedChar == '$') {
      // Read the rest of the characters until a newline character is encountered
      message = "$";  // Reset the message
      while (Serial1.available() > 0 && receivedChar != '\n') {
        receivedChar = Serial1.read();
        message += receivedChar;
        
      }
      
      // Check if the message starts with "$GNGGA"
      if (message.startsWith("$GNGGA")) {
        // Print the desired message
        Serial.println(message);
        return message;  // Return the message if it starts with "$GNGGA"
      }
    }
  }


 return "";  // Return an empty string if no valid message is found
}
