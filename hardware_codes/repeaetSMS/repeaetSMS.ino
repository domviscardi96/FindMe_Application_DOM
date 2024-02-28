String gpsData = ""; // Global variable to store GPS data
// Define the pin number for the LED
const int ledPin = 33;

void setup() {
  Serial.begin(115200);
  Serial1.begin(115200);
  pinMode(ledPin, OUTPUT);
  pinMode(13, OUTPUT);
  digitalWrite(13, HIGH);
  StartGPS();




}

void loop() {
  
  // Sending request for GNSS coordinates
  Serial1.println("AT+CGNSSINFO");
 
  // Waiting for response
  delay(900); // Adjust delay based on the response time of your module
// Read and display the response
  String response = "";
  while (Serial1.available()) {
    char c = Serial1.read();
    response += c;
  }
    Serial.println(response);
    
// Parse the received response
  if (parseGNSSInfo(response)) {
    // Send the GPS data via message
    sendMessage(gpsData);
  }

  delay(50000);
}

void sendCommandWithRetry(const char* command, int maxRetries) {
  int retries = 0;
  while (retries < maxRetries) {
    Serial1.println(command);
    delay(1000); // Adjust delay as needed

    // Check the response
    String response = Serial1.readStringUntil('\n');
    if (response.indexOf("OK") != -1) {
      // Command successful, break out of the loop
      break;
    } else {
      // Command failed, increment retries and try again
      retries++;
    }
  }
}

void StartGPS() {
  sendCommandWithRetry("AT+CMGF=1", 3); // messaign send
  delay(100);
  sendCommandWithRetry("AT+CGNSSPWR=1", 3); // gps on
 delay(5000);
  sendCommandWithRetry("AT+CGPSCOLD", 3); // cold start
delay(100);
sendCommandWithRetry("AT+CGNSSMODE=3", 3); // mode gps
delay(100);
sendCommandWithRetry("AT+CGNSSNMEA=1,0,0,0,0,0,0,0", 3); // gnss output
delay(100);
sendCommandWithRetry("AT+CGPSNMEARATE=5", 3); // anchor points 55Hz
delay(100);
sendCommandWithRetry("AT+CGNSSINFO=10", 3); // 
delay(100);

   digitalWrite(ledPin, HIGH);
  delay(200); // Wait for 1 second
  digitalWrite(ledPin, LOW);
  delay(200); // Wait for 1 second
   digitalWrite(ledPin, HIGH);
  delay(800); // Wait for 1 second
  digitalWrite(ledPin, LOW);
  delay(200); // Wait for 1 second
   digitalWrite(ledPin, HIGH);
  delay(200); // Wait for 1 second
  digitalWrite(ledPin, LOW);
}


bool parseGNSSInfo(String response) {
  int commaIndex = 0;
  int startIndex = response.indexOf(':') + 1;
  String values[18]; // Assuming there are 18 values separated by commas

  // Parse the response and extract values
  for (int i = startIndex; i < response.length(); i++) {
    if (response[i] == ',') {
      values[commaIndex++] = response.substring(startIndex, i);
      startIndex = i + 1;
    }
  }

  // Extracting desired values
  String available_values = values[5];
  int  smth = available_values.toInt(); // Convert to integer

  // Check if the number of GPS satellites is more than 2
  if (smth >1) {
    String latitude = values[5];
    String longitude = values[7];
    String date = values[9];
    String utc_time = values[10];
    String altitude = values[11];
    String velocity = values[12];

     // Fix utc_time format
    utc_time = utc_time.substring(0, utc_time.indexOf('.')); // Remove everything after the dot
    int hours = utc_time.substring(0, 2).toInt();
    int minutes = utc_time.substring(2, 4).toInt();


// Subtract 5 hours
    hours -= 5;
    if (hours < 0) {
      hours += 24; // Wrap around if hours becomes negative
    }

 // Ensure minutes are always two digits
    String minutesStr;
    if (minutes < 10) {
      minutesStr = "0" + String(minutes);
    } else {
      minutesStr = String(minutes);
    }

    // Construct the fixed utc_time string
    utc_time = String(hours) + minutesStr;
    
    // Construct the GPS data string
    //gpsData = utc_time + "," + latitude + ",-" + longitude + "," + date + "," + altitude + "," + velocity;
    gpsData = utc_time + "," + latitude + ",-" + longitude;
    Serial.println(gpsData);
    return true; // Indicate that GPS data is valid and ready to be sent
  }
  
  return false; // Indicate that GPS data is not valid
}

void sendMessage(String data) {
  // Send coordinates via message
  Serial1.println("AT+CMGS=\"+16478072595\""); // Use double quotes around the phone number
  delay(1000);
  if (Serial1.find(">")) { // Check for the prompt
    Serial1.println(data); // Send the GPS data
    Serial1.write(26); // ASCII code for Ctrl+Z to send the message
    delay(1000); // Wait a bit for the message to be sent
  } else {
    Serial.println("There has been an error.");
  }
}
