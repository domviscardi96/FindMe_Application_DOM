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
using System.Threading.Tasks;
using Javax.Security.Auth;

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


            //this is the most current position of the device, the map will move to this region about 3 miles from it 
            map.MoveToRegion(MapSpan.FromCenterAndRadius(initialPosition, Distance.FromMiles(30)));

            // add "Show Current" button
            var showCurrentButton = new Button { Text = "Show Current", HorizontalOptions = LayoutOptions.FillAndExpand };
            showCurrentButton.Clicked += (sender, args) =>
            {
                map.Pins.Clear();

                ShowCurrentLocation();


            };

            //add more pins for the user to see
            var morePinsButton = new Button { Text = "View more pins", HorizontalOptions = LayoutOptions.FillAndExpand };
            morePinsButton.Clicked += (sender, args) =>
            {
                map.Pins.Clear();
                CheckAndRequestSmsPermission_more();
                

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

                map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(formattedLAT, formattedLONG), Distance.FromMiles(0.5)));
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
                map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(formattedLAT, formattedLONG), Distance.FromMiles(1)));
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
                map.Pins.Add(new Pin
                {
                    Position = new Position(formattedLAT, formattedLONG),
                    Label = $"{parsedDate.ToString("dd/MM/yyyy")} {parsedTime.ToString("HH:mm")}"
                });
            }
            else
            {
                // Handle invalid GPS coordinates or insufficient data
               // await DisplayAlert("Error", "Invalid GPS coordinates or insufficient data", "OK");
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
                //await DisplayAlert("SMS Messages", formattedSms, "OK");

                // Call AddMorePins here with the formatted SMS messages
                AddMorePins(formattedSms);



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