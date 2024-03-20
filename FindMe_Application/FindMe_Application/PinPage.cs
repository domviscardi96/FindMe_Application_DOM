﻿using System;
using System.Linq;
using System.Text;

using Xamarin.Forms;
using Xamarin.Essentials;
using Map = Xamarin.Forms.Maps.Map;
using System.Threading.Tasks;
using System.Timers;
using PinType = Xamarin.Forms.Maps.PinType;
using Color = Xamarin.Forms.Color;
using MapSpan = Xamarin.Forms.Maps.MapSpan;
using Position = Xamarin.Forms.Maps.Position;
using Distance = Xamarin.Forms.Maps.Distance;
using Pin = Xamarin.Forms.Maps.Pin;
using Polyline = Xamarin.Forms.Maps.Polyline;
using System.Net.Http;
using Newtonsoft.Json;
using Android.Content;


namespace FindMe_Application
{
    public class PinPage : ContentPage
    {
        Map map;
        System.Timers.Timer timer;
        bool isTimerRunning = false;
        bool isFirstPinAdded = false;
        Pin previousPin = null;

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


            //this is the most current position of the device, the map will move to this region about 3 miles from it 
            map.MoveToRegion(MapSpan.FromCenterAndRadius(initialPosition, Distance.FromMiles(30)));

            Content = new StackLayout
            {
                Spacing = 0,
                Children =
                {
                    map
                }
            };

            // Start the timer when the page appears
            this.Appearing += PinPage_Appearing;
            // Stop the timer when the page disappears
            this.Disappearing += PinPage_Disappearing;


            CheckAndRequestSmsPermission_more();

        }

        private void PinPage_Appearing(object sender, EventArgs e)
        {
            if (!isTimerRunning)
            {
                // Start the timer with a 60-second interval
                timer = new System.Timers.Timer(30000); // 30 seconds
                timer.Elapsed += Timer_Elapsed;
                timer.AutoReset = true;
                timer.Enabled = true;
                isTimerRunning = true;
            }
        }

        private void PinPage_Disappearing(object sender, EventArgs e)
        {
            // Stop the timer when the page disappears
            if (isTimerRunning)
            {
                timer.Stop();
                timer.Dispose();
                isTimerRunning = false;
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Check and request SMS permission to get more pins
            Device.BeginInvokeOnMainThread(() =>
            {
                CheckAndRequestSmsPermission_more();
            });
        }

        public async void ShowCurrentLocation()
        {
            try
            {
                // Check and request SMS permission
                var status = await Permissions.CheckStatusAsync<Permissions.Sms>();

                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.Sms>();
                }

                if (status != PermissionStatus.Granted)
                {
                    await DisplayAlert("Permission Required", "SMS permission is required to access SMS messages.", "OK");
                    return;
                }

                // Read SMS messages
                var smsReader = DependencyService.Get<ISmsReader>();
                var smsList = smsReader.ReadSms();

                if (smsList.Any())
                {
                    // Process the first SMS message
                    await ProcessFirstSmsLine(smsList.First());
                }
                else
                {
                    await DisplayAlert("No SMS Messages", "There are no SMS messages available.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        public async Task ProcessFirstSmsLine(string sms)
        {
            //await DisplayAlert("firstine", sms, "ok");

            // Trim the SMS message and remove quotation marks if any
            string trimmedSms = sms.Trim().Trim('"');

            // Split the SMS message into parts
            var parts = trimmedSms.Split(',');

            // Check if the SMS message contains valid GPS coordinates
            if (parts.Length >= 4 &&
                (parts[0].Length == 3 || parts[0].Length == 4) && // Check if the time part has 3 or 4 digits

                DateTime.TryParseExact(parts[0].PadLeft(4, '0'), "HHmm", null, System.Globalization.DateTimeStyles.None, out DateTime parsedTime) &&
                double.TryParse(parts[1], out double currentLatitude) &&
                double.TryParse(parts[2], out double currentLongitude) &&
                DateTime.TryParseExact(parts[3], "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
            {
                // Formatting latitude and longitude if needed
                int latnumDigits = currentLatitude.ToString().Length;
                int longnumDigits = currentLongitude.ToString().Length;
                double latdivisor = Math.Pow(10, latnumDigits - 2);
                double longdivisor = Math.Pow(10, longnumDigits - 3);
                double formattedLAT = currentLatitude / latdivisor;
                double formattedLONG = currentLongitude / longdivisor;

                
                // Adding pin to the map
                map.Pins.Add(new Pin
                {
                    Position = new Position(formattedLAT, formattedLONG),
                    Label = $"{parsedDate.ToString("dd/MM/yyyy")} {parsedTime.ToString("HH:mm")}"
                });

                map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(formattedLAT, formattedLONG), Distance.FromMiles(0.2)));
            }
            else
            {
                // Handle invalid GPS coordinates
                await DisplayAlert("Error", "Invalid GPS coordinates", "OK");
            }
        }

        public async void AddMorePins(string gpscoordinates)
        {
            if (string.IsNullOrEmpty(gpscoordinates))
            {
                // Handle the case when there are no SMS messages
                await DisplayAlert("ALERT", "No coordinates available", "OK");
                return;
            }


            // Split the gpscoordinates string into individual lines
            string[] lines = gpscoordinates.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            //await DisplayAlert("ok", lines[1], "ok");

            // Check if there are at least two lines
            if (lines.Length >= 2)
            {
                // Process the second line to get the map region
                await ProcessMapRegion(lines[1]);
            }


            foreach (var line in lines)
            {
                // Process each line separately
                await ProcessLine(line);
            }
        }

        private async Task ProcessMapRegion(string line)
        {
           // await DisplayAlert("processsmap", line, "ok");

            // Trim the line and remove quotation marks if any
            string trimmedLine = line.Trim().Trim('"');

            // Split the trimmed line into latitude and longitude parts
            var parts = trimmedLine.Split(',');

            // Check if the line contains valid latitude and longitude
            if (parts.Length >= 2 &&
                double.TryParse(parts[1], out double regionLatitude) &&
                double.TryParse(parts[2], out double regionLongitude))
            {
                // Formatting latitude and longitude if needed
                int latnumDigits = regionLatitude.ToString().Length;
                int longnumDigits = regionLongitude.ToString().Length;
                double latdivisor = Math.Pow(10, latnumDigits - 2);
                double longdivisor = Math.Pow(10, longnumDigits - 3);
                double formattedLAT = regionLatitude / latdivisor;
                double formattedLONG = regionLongitude / longdivisor;

                
                // Set the map region
                map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(formattedLAT, formattedLONG), Distance.FromMiles(0.05)));
            }
            else
            {
                // Handle invalid map region
                await DisplayAlert("Error", "Invalid map region coordinates", "OK");
            }
        }

        private async Task ProcessLine(string line)
        {
            // Trim the line and remove quotation marks if any
            string trimmedLine = line.Trim().Trim('"');

            // Split the trimmed line into parts
            var parts = trimmedLine.Split(',');

            // Check if the line contains valid GPS coordinates and date
            if (parts.Length >= 4 &&
                (parts[0].Length == 3 || parts[0].Length == 4) && // Check if the time part has 3 or 4 digits
                DateTime.TryParseExact(parts[3], "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate) &&
                DateTime.TryParseExact(parts[0].PadLeft(4, '0'), "HHmm", null, System.Globalization.DateTimeStyles.None, out DateTime parsedTime) &&
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

                // Add pin to the map with label containing date and time
                Pin pin = new Pin
                {
                    Position = new Position(formattedLAT, formattedLONG),
                    Label = $"{parsedDate.ToString("dd/MM/yyyy")} {parsedTime.ToString("HH:mm")}"
                };

                // Change the label for the first pin
                if (!isFirstPinAdded)
                {
                    pin.Label += " (Current)";
                    pin.Type = PinType.Generic;
                    isFirstPinAdded = true;
                }

                map.Pins.Add(pin);

                if (previousPin != null)
                {
                    // Create a polyline for the entire line
                    var polyline = new Polyline();
                    polyline.StrokeWidth = 5;
                    polyline.StrokeColor = Color.Red; // Set line color
                    polyline.Geopath.Add(previousPin.Position);
                    polyline.Geopath.Add(pin.Position);
                    map.MapElements.Add(polyline);

                    // Calculate the point 90% along the line
                    var ninetyPercentPoint = new Position(
                        (9 * previousPin.Position.Latitude + pin.Position.Latitude) / 10,
                        (9 * previousPin.Position.Longitude + pin.Position.Longitude) / 10);

                    // Create a polyline for the first 90% of the line (thin red)
                    var ninetyPercentPolyline = new Polyline();
                    ninetyPercentPolyline.StrokeWidth = 20;
                    ninetyPercentPolyline.StrokeColor = Color.Black; // Set line color
                    ninetyPercentPolyline.Geopath.Add(previousPin.Position);
                    ninetyPercentPolyline.Geopath.Add(ninetyPercentPoint);
                    map.MapElements.Add(ninetyPercentPolyline);

                    // Create a polyline for the last 10% of the line (thick blue as an arrow)
                    var tenPercentPolyline = new Polyline();
                    tenPercentPolyline.StrokeWidth = 5;
                    tenPercentPolyline.StrokeColor = Color.Red; // Set arrow color
                    tenPercentPolyline.Geopath.Add(ninetyPercentPoint);
                    tenPercentPolyline.Geopath.Add(pin.Position);
                    map.MapElements.Add(tenPercentPolyline);
                }

                // Update previous pin to current pin
                previousPin = pin;
            }
            else
            {
                // Handle invalid GPS coordinates or insufficient data
                await DisplayAlert("Error", "Invalid GPS coordinates or insufficient data", "OK");
            }
        }


        public async void getSMS()
            {
            string allSms = ""; // Clear the allSms string
            string gpsCoordinates = ""; // Initialize a string for GPS coordinates messages
            string rawIpAddress = ""; // Initialize a string for IP address messages

            var smsReader = DependencyService.Get<ISmsReader>();
                var smsList = smsReader.ReadSms();

                if (smsList.Any())
                {
                StringBuilder messageBuilder = new StringBuilder();
                    foreach (var sms in smsList)
                    {

                    // Check if the SMS contains an IP address
                    if (sms.StartsWith("IP:"))
                    {
                        rawIpAddress += sms + "\n"; // Add the IP address message to the ipAddress string
                    }
                    else
                    {
                        gpsCoordinates += sms + "\n"; // Add the GPS coordinates message to the gpsCoordinates string
                    }

                    //await DisplayAlert("sms", sms, "ok");
                    //messageBuilder.AppendLine(sms);
                    }
                // Process GPS coordinates and IP address separately
                if (!string.IsNullOrEmpty(gpsCoordinates))
                {
                    // Format the GPS coordinates messages with quotation marks
                    string formattedGpsCoordinates = FormatSmsMessages(gpsCoordinates);

                    // Call AddMorePins here with the formatted GPS coordinates messages
                    AddMorePins(formattedGpsCoordinates);
                }

                if (!string.IsNullOrEmpty(rawIpAddress))
                {
                    //**NEW CODE**//
                    //split the string based on comma delimiter
                    string[] parts = rawIpAddress.Split(',');
                    //get only the ipAddress from the split string
                    string ipAddress = parts[0].Trim();
                    string ipDate = parts[1].Trim();

                    //convert the IP address to GPS coordinates
                    await ConvertIPAddtoCoordinates(ipAddress, ipDate);
                    // Display the IP address messages in an alert or handle them accordingly
                    await DisplayAlert("IP Addresses", rawIpAddress, "OK");
                }

            }
                else
                {
                    await DisplayAlert("No SMS Messages", "There are no SMS messages available.", "OK");
                }
            }

        public string FormatSmsMessages(string allSms)
        {
            StringBuilder formattedSms = new StringBuilder();

            // Split the allSms variable into individual SMS messages
            string[] smsArray = allSms.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // Iterate through each SMS message (considering each SMS contains GPS coordinates and date separately)
            for (int i = 0; i < smsArray.Length; i += 2)
            {
                // Append the formatted SMS message with both GPS coordinates and date
                formattedSms.AppendLine($"{smsArray[i].Trim()}{smsArray[i + 1].Trim()}");
            }

            // Return the formatted SMS messages as a single string
            return formattedSms.ToString();
        }

        //**NEW CODE**// //converts iPaddress to GPS coordinates
        public async Task ConvertIPAddtoCoordinates(string ipAddress, string ipDate) 
        {
            //api key for geolocation API 
            string apiKey = "AIzaSyANxOarKJTH_DkQEnE2KeTO_rFiERNeKFA";
            //construct the request URL
            string requestUrl = $"http://api.ipstack.com/{ipAddress}?access_key={apiKey}";

            //makes an HTTP GET request to a specified URL, receiving JSON data containing geographical coordinates, extracting them for use
            using(var client = new HttpClient())
            {
                var response = await client.GetStringAsync(requestUrl);
                
                dynamic locationData = JsonConvert.DeserializeObject(response);

                double latitude = locationData.latitude;
                double longitude = locationData.longitude;

                Device.BeginInvokeOnMainThread(() =>
                {
                    map.Pins.Add(new Pin
                    {
                        Position = new Position(latitude, longitude),
                        Label = ipDate,
                        Type = PinType.Place
                    });
                });
            }


        }


        public async void CheckAndRequestSmsPermission_more()
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

  


    }
}