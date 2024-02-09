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

namespace FindMe_Application
{
    public class PinPage : ContentPage
    {
        Map map;
        
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

           // add "Smsbtn" button
           var smsbutton = new Button { Text = "smsbutton", HorizontalOptions = LayoutOptions.FillAndExpand };
            smsbutton.Clicked += (sender, args) =>
            {

                CheckAndRequestSmsPermission();


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
                AddMorePins();
                

                map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(43.9466095604592, -78.8943450362667), Distance.FromMiles(3)));
            };

         
            var buttons = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children = { morePinsButton}
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
                        Children = { showCurrentButton, morePinsButton, smsbutton }
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
                while((reader.ReadLine()) is string line)                                   //this while loop will get the last line in the file hold it in a variable
                {
                    lastLine = line;
                }

                if(lastLine != null)
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
        //            messageBuilder.AppendLine(sms);
        //        }
        //        allSms = messageBuilder.ToString().Trim();

        //        await DisplayAlert("SMS Messages", allSms, "OK");
        //    }
        //    else
        //    {
        //        await DisplayAlert("No SMS Messages", "There are no SMS messages available.", "OK");
        //    }
        //}

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
                    var smsTimePart = sms.Split(new[] { ',' }, 2);
                    if (smsTimePart.Length > 1)
                    {
                        var smsTimeStamp = smsTimePart[0];
                        DateTime smsTime;

                        if (DateTime.TryParse(smsTimeStamp, out smsTime))
                        {
                            smsTimeStamp = smsTime.ToString("HH:mm:ss");
                        }

                        var smsFormatted = $"{smsTimeStamp}:{smsTimePart[1]}";
                        messageBuilder.AppendLine(smsFormatted);
                    }
                    else
                    {
                        messageBuilder.AppendLine(sms);
                    }
                }
                allSms = messageBuilder.ToString().Trim();

                await DisplayAlert("SMS Messages", allSms, "OK");
            }
            else
            {
                await DisplayAlert("No SMS Messages", "There are no SMS messages available.", "OK");
            }
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

        private void AddMorePins()
        {
            

            //get the file that is to be read for the pins
            var assembly = typeof(PinPage).GetTypeInfo().Assembly;
            string resourceName = assembly.GetManifestResourceNames().FirstOrDefault(name => name.EndsWith("SMS.txt")); 
            var resourceStream = assembly.GetManifestResourceStream(resourceName);

            if (resourceStream == null) return;

            //initialize an array to hold the reversed lines from the file 
            ArrayList revLineslist = new ArrayList();

            //using the streamreader, add every line in the file to the array, then reverse the array
            using(StreamReader reader = new StreamReader(resourceStream))
            {
                string line;
                while((line = reader.ReadLine()) != null)
                {
                    revLineslist.Add(line);
                }
            }
            revLineslist.Reverse();

            //this loop is to take each line in the array, parse it for the information needed to add a new pin 
            foreach (string line in revLineslist)
            {
                var parts = line.Trim('"').Split(',');
                if (parts.Length >= 3 &&
                        DateTime.TryParseExact(parts[0], "HHmmss", null, System.Globalization.DateTimeStyles.None, out DateTime parsedTime) &&            //directly parse the time from the string
                        double.TryParse(parts[1], out double currentLatitude) &&                                                                                 //get the latitude and longitude
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


                    map.Pins.Add(new Pin
                    {
                        Position = new Position(formattedLAT, formattedLONG),
                        //Position = new Position(currentLatitude, currentLongitude),
                        Label = parsedTime.ToString("G")
                        //Label = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, parsedTime.Hour, parsedTime.Minute, parsedTime.Second).ToString("G")
                    });
                }
            }
            



        }

    }


}