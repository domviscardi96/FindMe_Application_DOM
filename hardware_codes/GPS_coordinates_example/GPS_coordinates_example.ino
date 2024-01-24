#include <WiFi.h>


boolean stringComplete = false;
String inputString = "";
String fromGSM = "";
int c = 0;


bool CALL_END = 1;
char* response = " ";
String res = "";
void setup()
{

  // Making Radio OFF for power saving
  WiFi.mode(WIFI_OFF);  // WiFi OFF
  btStop();   // Bluetooth OFF


  Serial.begin(115200); // For Serial Monitor
  Serial1.begin(9600); // For A9G Board

  // Waiting for A9G to setup everything for 20 sec
  delay(20000);



  Serial1.println("AT");               // Just Checking
  delay(1000);

  Serial1.println("AT+GPS = 1");      // Turning ON GPS
  delay(1000);

}

void loop()
{
 

  // read from port 0, send to port 1:
  if (Serial.available()) {
    int inByte = Serial.read();
    Serial1.write(inByte);
  }

      //-------------------------------------  Getting Location and making Google Maps link of it


      delay(1000);
      Serial1.println("AT+LOCATION = 2");
      Serial.println("AT+LOCATION = 2");

      while (!Serial1.available());
      while (Serial1.available())
      {
        char add = Serial1.read();
        res = res + add;
        delay(1);
      }

      res = res.substring(17, 38);
      response = &res[0];

      Serial.print("Recevied Data - "); Serial.println(response); // printin the String in lower character form
      Serial.println("\n");

      if (strstr(response, "GPS NOT"))
      {
        Serial.println("No Location data");
      }
      else
      {

        int i = 0;
        while (response[i] != ',')
          i++;

        String location = (String)response;
        String lat = location.substring(2, i);
        String longi = location.substring(i + 1);
        Serial.println(lat);
        Serial.println(longi);

        String Gmaps_link = ( "http://maps.google.com/maps?q=" + lat + "+" + longi); //http://maps.google.com/maps?q=38.9419+-78.3020


     

        Serial1.println ("I'm here " + Gmaps_link);
        delay(1000);
        Serial1.println((char)26);
        delay(1000);
      }
      response = "";
      res = "";



}
