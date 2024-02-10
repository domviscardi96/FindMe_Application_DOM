using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

using Xamarin.Forms;
using Xamarin.Forms.Maps;
using System.Collections;
using Android.Content;
using Android.Database;
using Android.App;
using Android.Graphics;
using Xamarin.Essentials;
using Map = Xamarin.Forms.Maps.Map;
using static System.Net.Mime.MediaTypeNames;
using Android.Locations;
using static Android.Graphics.ImageDecoder;
using static Android.Graphics.Paint;

namespace FindMe_Application
{
    public class PinPage : ContentPage
    {
        Map map;
        private string gpscoordinates = "";

        Position initialPosition = new Position(43.8971, -78.8658); //oshawa when open map
        public PinPage()
        {
            //new map object 
            map = new Map
            {
                IsShowingUser = true,
                HeightRequest = 100,
                WidthRequest = 960,
                VerticalOptions = LayoutOptions.FillAndExpand
            };

            // Call LoadCurrentPosition to get currentPosition and currentTime
            var (currentPosition, currentTime) = LoadCurrentPosition();


            //this is the most current position of the device, the map will move to this region about 3 miles from it 
            map.MoveToRegion(MapSpan.FromCenterAndRadius(initialPosition, Distance.FromMiles(30)));
            //var position = new Position(43.9466095604592, -78.8943450362667);
            //place the first pin with the current time/location

            // Add current pin
            var currentPin = new Pin
            {
                Type = PinType.Place,
                Position = currentPosition.Value,
                Label = "Current",
                Address = $"Time: {currentTime.ToString("hh:mm:ss tt")}"
            };


            // add "Show Current" button
            var showCurrentButton = new Button { Text = "Show Current", HorizontalOptions = LayoutOptions.FillAndExpand };
            showCurrentButton.Clicked += (sender, args) =>
            {
                map.MoveToRegion(MapSpan.FromCenterAndRadius(currentPosition.Value, Distance.FromMiles(3)));
                map.Pins.Clear();

                map.Pins.Add(currentPin);


            };

            //add more pins for the user to see
            var morePinsButton = new Button { Text = "View more pins", HorizontalOptions = LayoutOptions.FillAndExpand };
            morePinsButton.Clicked += (sender, args) =>
            {
                map.Pins.Clear();
                CheckAndRequestSmsPermission();
                


                map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(43.9466095604592, -78.8943450362667), Distance.FromMiles(3)));

            };


            var buttons = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children = { morePinsButton }
            };

            //create a stacked layout of the screen 
            Content = new StackLayout
            {
                Spacing = 0,
                Children =
                {
                     new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        Children = { showCurrentButton, morePinsButton }
                    },
                     map
                }
            };
        }


        private (Position? currentPosition, DateTime currentTime) LoadCurrentPosition()
        {
            //initialize variables
            Position? currentPosition = null;
            DateTime currentTime = DateTime.MinValue;


            var assembly = typeof(PinPage).GetTypeInfo().Assembly;
            string resourceName = assembly.GetManifestResourceNames().FirstOrDefault(name => name.EndsWith("SMS.txt")); //get the text file to read from as a resource

            if (resourceName == null) return (null, currentTime);

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))        //open a stream to get the resource 
            using (StreamReader reader = new StreamReader(stream))                          //use stream reader to read the file 
            {
                string lastLine = null;
                while ((reader.ReadLine()) is string line)                                   //this while loop will get the last line in the file hold it in a variable
                {
                    lastLine = line;
                }

                if (lastLine != null)
                {
                    var parts = lastLine.Trim('"').Split(',');
                    if (parts.Length >= 3 &&
                            DateTime.TryParseExact(parts[0], "HHmmss", null, System.Globalization.DateTimeStyles.None, out DateTime parsedTime) &&                  //directly parse the time from the string
                            double.TryParse(parts[1], out double currentLatitude) &&                                                                                       //get the latitude and longitude
                            double.TryParse(parts[2], out double currentLongitude))
                    {

                        // Count the number of digits in the original values
                        int latnumDigits = currentLatitude.ToString().Length;
                        int longnumDigits = currentLongitude.ToString().Length;

                        // Calculate the divisor to get the desired format
                        double latdivisor = Math.Pow(10, latnumDigits - 2);
                        double longdivisor = Math.Pow(10, longnumDigits - 3);

                        // Divide the originalValue by the divisor to get the formatted value
                        double formattedLAT = currentLatitude / latdivisor;
                        double formattedLONG = currentLongitude / longdivisor;

                        currentTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, parsedTime.Hour, parsedTime.Minute, parsedTime.Second); //get the current time and position from the parsed values
                        //currentPosition = new Position(currentLatitude, currentLongitude);
                        currentPosition = new Position(formattedLAT, formattedLONG);
                    }
                }
            }
            return (currentPosition, currentTime);

        }

        public async void AddMorePins(string gpscoordinates)
        {
            if (string.IsNullOrEmpty(gpscoordinates))
            {
                // Handle the case when there are no SMS messages
                await DisplayAlert("no", "no mssage", "ok");
                return;
            }

            // Split the allSms variable to get individual SMS messages
            string[] smsArray = gpscoordinates.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);


            foreach (var sms in smsArray)
            {
                // Process each SMS message
                var parts = sms.Trim().Split(',');
                if (parts.Length >= 3 &&
                    DateTime.TryParseExact(parts[0], "HHmmss", null, System.Globalization.DateTimeStyles.None, out DateTime parsedTime) &&
                    double.TryParse(parts[1], out double currentLatitude) &&
                    double.TryParse(parts[2], out double currentLongitude))
                {
                    // Formatting latitude and longitude if needed
                    int latnumDigits = currentLatitude.ToString().Length;
                    int longnumDigits = currentLongitude.ToString().Length;
                    double latdivisor = Math.Pow(10, latnumDigits - 2);
                    double longdivisor = Math.Pow(10, longnumDigits - 3);
                    double formattedLAT = currentLatitude / latdivisor;
                    double formattedLONG = currentLongitude / longdivisor;

                    // Adding pins to the map
                    map.Pins.Add(new Pin
                    {
                        Position = new Position(formattedLAT, formattedLONG),
                        Label = parsedTime.ToString("G")
                    });
                }
            }
        }

        public async void getSMS()
            {
                string allSms = ""; // Clear the allSms string

                var smsReader = DependencyService.Get<ISmsReader>();
                var smsList = smsReader.ReadSms();

                if (smsList.Any())
                {
                StringBuilder messageBuilder = new StringBuilder();
                    foreach (var sms in smsList)
                    {
                    //await DisplayAlert("sms", sms, "ok");
                    messageBuilder.AppendLine(sms);
                }
                    allSms = messageBuilder.ToString().Trim();

                // Format the SMS messages with quotation marks
                string formattedSms = FormatSmsMessages(allSms);

                // Display the formatted SMS messages in an alert
                await DisplayAlert("SMS Messages", formattedSms, "OK");

                // Call AddMorePins here with the formatted SMS messages
                AddMorePins(formattedSms);

            }
                else
                {
                    await DisplayAlert("No SMS Messages", "There are no SMS messages available.", "OK");
                }
            }

        //public async void getSMS()
        //{
        //    string allSms = ""; // Clear the allSms string

        //    var smsReader = DependencyService.Get<ISmsReader>();
        //    var smsList = smsReader.ReadSms();

        //    if (smsList.Any())
        //    {
        //        StringBuilder messageBuilder = new StringBuilder();

        //        foreach (var sms in smsList)
        //        {
        //            var smsTimePart = sms.Split(new[] { ',' }, 2);
        //            if (smsTimePart.Length > 1)
        //            {
        //                var smsTimeStamp = smsTimePart[0];
        //                DateTime smsTime;

        //                if (DateTime.TryParse(smsTimeStamp, out smsTime))
        //                {
        //                    smsTimeStamp = smsTime.ToString("HH:mm:ss");
        //                }

        //                var smsFormatted = $"{smsTimeStamp}:{smsTimePart[1]}";
        //                messageBuilder.AppendLine(smsFormatted);
        //            }
        //            else
        //            {
        //                messageBuilder.AppendLine(sms);
        //            }
        //        }
        //        allSms = messageBuilder.ToString().Trim();

        //        await DisplayAlert("SMS Messages", allSms, "OK");
        //    }
        //    else
        //    {
        //        await DisplayAlert("No SMS Messages", "There are no SMS messages available.", "OK");
        //    }
        //}

        public string FormatSmsMessages(string allSms)
        {
            StringBuilder formattedSms = new StringBuilder();

            // Split the allSms variable into individual SMS messages
            string[] smsArray = allSms.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // Iterate through each SMS message
            foreach (var sms in smsArray)
            {
                // Append the formatted SMS message with quotation marks to the StringBuilder
                formattedSms.AppendLine($"\"{sms.Trim()}\"");
            }

            // Return the formatted SMS messages as a single string
            return formattedSms.ToString();
        }


        public async void CheckAndRequestSmsPermission()
            {
                try
                {
                    var status = await Permissions.CheckStatusAsync<Permissions.Sms>();

                    if (status != PermissionStatus.Granted)
                    {
                        status = await Permissions.RequestAsync<Permissions.Sms>();
                    }

                    if (status != PermissionStatus.Granted)
                    {
                        await DisplayAlert("Permission Required", "SMS permission is required to access SMS messages.", "OK");
                        // Handle permission denied scenario
                    }
                    else
                    {
                        // Permission is granted, proceed with accessing SMS messages
                        getSMS();
                    }
                }
                catch (Exception ex)
                {
                    // Handle exception
                    await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
                }
            }

        //function to save sms into embedded file (TO BE FIXED)
            //public void SaveSmsToFile(string allSms)
            //{
            //    try
            //    {
            //        var assembly = IntrospectionExtensions.GetTypeInfo(typeof(App)).Assembly;
            //        string resourceName = "FindMe_Application.Embedded_Resources.GPS.txt";

            //        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            //        {
            //            if (stream != null)
            //            {
            //                using (StreamReader reader = new StreamReader(stream))
            //                {
            //                    string content = reader.ReadToEnd();
            //                    reader.Close();

            //                    // Append the new SMS messages to the existing content
            //                    string newContent = content + "\n\n" + allSms;

            //                    // Write the new content back to the file
            //                    File.WriteAllText(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GPS.txt"), newContent);
            //                }
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        // Handle exception
            //    }
            //}


       
    }
}