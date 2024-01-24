String GPS_values() {
  String message = "";  // Initialize an empty string to store the message

  // Read and process data from Serial1
  while (Serial1.available()) {
    // Read incoming character from Serial1
    char receivedChar = Serial1.read();

    // Print the character
   // Serial.print(receivedChar);

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

  // Add a delay to make it easier to observe in the Serial Monitor
//  delay(3000);
  Serial.println();
  Serial.println();
  Serial.println();

  return "";  // Return an empty string if no valid message is found
}
