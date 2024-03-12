#include <WiFi.h>
#include <WiFiMulti.h>

// Replace with your network credentials (STATION)
const char* ssid1 = "Pixel7 D";
const char* password1 = "nicola99";
const char* ssid2 = "BELL766";
const char* password2 = "471795C2D225";

// Initialize Wi-Fi
WiFiMulti wifiMulti;

String connectionDetails; // Variable to store connection details

void setup() {
  Serial.begin(115200);
  Serial1.begin(115200);
  delay(1000);
  
  
   WiFi.mode(WIFI_STA);
   
   Serial1.println("AT+CMGF=1"); // Use double quotes around the phone number
   delay(1000);
    // Initialize Wi-Fi
  wifiMulti.addAP(ssid1, password1);
  wifiMulti.addAP(ssid2, password2);
  //wifiMulti.addAP(ssid3, password3);
 
}

void loop() {
  // put your main code here, to run repeatedly:
      Serial.println("Connecting to Wi-Fi...");
  while (wifiMulti.run() != WL_CONNECTED) {
    delay(100);
    Serial.print(".");
  }

  Serial.println("");
  Serial.println("Wi-Fi connected");

    // Store connection details in a string
  connectionDetails = "Connected to " + String(WiFi.SSID()) + "\n"
                      + "IP address: " + WiFi.localIP().toString()
                      + " Signal Strength: " + String(WiFi.RSSI());

  // Print the connection details
  Serial.println(connectionDetails);


//  
//  Serial1.println("AT+CMGS=\"+16478072595\""); // Use double quotes around the phone number
//  delay(1000);
//  if (Serial1.find(">")) { // Check for the prompt
//    Serial1.println(connectionDetails); // Send the GPS data
//    Serial1.write(26); // ASCII code for Ctrl+Z to send the message
//    delay(1000); // Wait a bit for the message to be sent
//  } else {
//    Serial.println("There has been an error.");
//  }


  delay(5000);
}
