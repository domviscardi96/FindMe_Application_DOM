using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

using Xamarin.Forms;
using Xamarin.Forms.Maps;

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

            //recenter the map after using zooms in/out or moves around
            var recenterButton = new Button { Text = "Re-center", HorizontalOptions = LayoutOptions.FillAndExpand };
            recenterButton.Clicked += (sender, args) =>
            {
                map.MoveToRegion(MapSpan.FromCenterAndRadius(currentPosition.Value, Distance.FromMiles(3)));
                map.Pins.Clear();
            };
            var buttons = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children = { morePinsButton, recenterButton }
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
                        Children = { showCurrentButton, morePinsButton, recenterButton }
                    },
                     map
                }
            };
        }

        // Method to load currentPosition from SMS.txt
        private (Xamarin.Forms.Maps.Position? currentPosition, DateTime currentTime) LoadCurrentPosition()
        {
            // Declare variables to hold position and time
            Xamarin.Forms.Maps.Position? currentPosition = null;
            DateTime currentTime = DateTime.MinValue;

            var assembly = typeof(PinPage).GetTypeInfo().Assembly;
            string resourceName = assembly.GetManifestResourceNames().FirstOrDefault(name => name.EndsWith("SMS.txt"));

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string line;
                string lastLine = null;

                // Read the file line by line
                while ((line = reader.ReadLine()) != null)
                {
                    lastLine = line;
                }

                

                // Parse the last line to get currentPosition
                if (lastLine != null)
                {
                    // Split the line by comma and remove the surrounding quotes
                    string[] parts = lastLine.Trim('"').Split(',');

                    // Extract time, latitude, and longitude
                    if (parts.Length >= 3 &&
                        int.TryParse(parts[0].Substring(0, 2), out int hour) &&
                        int.TryParse(parts[0].Substring(2, 2), out int minute) &&
                        int.TryParse(parts[0].Substring(4, 2), out int second) &&
                        double.TryParse(parts[1], out double currentlatitude) &&
                        double.TryParse(parts[2], out double currentlongitude))
                    {
                        // Count the number of digits in the original values
                        int latnumDigits = currentlatitude.ToString().Length;
                        int longnumDigits = currentlongitude.ToString().Length;

                        // Calculate the divisor to get the desired format
                        double latdivisor = Math.Pow(10, latnumDigits - 2);
                        double longdivisor = Math.Pow(10, longnumDigits - 3);

                        // Divide the originalValue by the divisor to get the formatted value
                        double formattedLAT = currentlatitude / latdivisor;
                        double formattedLONG = currentlongitude / longdivisor;


                        // Create DateTime object from the extracted time
                        currentTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, minute, second);

                        // Set currentPosition
                        currentPosition =new Xamarin.Forms.Maps.Position(formattedLAT, formattedLONG);
                        

                        //// Display an alert with the last line of the file
                        //await DisplayAlert("Parts", parts[1], "OK");

                        //// Display an alert with the last line of the file
                        //await DisplayAlert("Parts", parts[2], "OK");


                        //// Display an alert with the parsed values
                        //await DisplayAlert("Parsed Values", $"Latitude: {formattedLAT}\nLongitude: {formattedLONG}", "OK");

                       

                    }
                }
                return (currentPosition, currentTime);
            }
        }

        private void AddMorePins()
        {
            map.Pins.Add(new Pin
            {
                Position = new Position(43.9413706450885, -78.886652062821),
                Label = "Time:  11:43:17 AM"
            });
            map.Pins.Add(new Pin
            {
                Position = new Position(43.9354395880272, -78.8800085355502),
                Label = "Time: 11:42:17 AM"
            });
            map.Pins.Add(new Pin
            {
                Position = new Position(43.9321083624444, -78.8827922355485),
                Label = "Time: 11:41:17 AM"
            });
            map.Pins.Add(new Pin
            {
                Position = new Position(43.929737722797, -78.8925129494835),
                Label = "Time: 11:40:17 AM"
            });
            map.Pins.Add(new Pin
            {
                Position = new Position(43.9276422677719, -78.9019333261595),
                Label = "Time:  11:39:17 AM"
            });
            map.Pins.Add(new Pin
            {
                Position = new Position(43.9265774660606, -78.9083019616729),
                Label = "Time: 11:38:17 AM"
            });
            map.Pins.Add(new Pin
            {
                Position = new Position(43.936199308208, -78.9123624273792),
                Label = "Time: 11:37:17 AM"
            });
            map.Pins.Add(new Pin
            {
                Position = new Position(43.941642672947, -78.9149201039987),
                Label = "Time: 11:36:17 AM"
            });
            map.Pins.Add(new Pin
            {
                Position = new Position(43.9449804253866, -78.9096744424994),
                Label = "Time: 11:35:17 AM"
            });
            map.Pins.Add(new Pin
            {
                Position = new Position(43.9467665241741, -78.9019879413369),
                Label = "Time: 11:34:17 AM"
            });
            map.Pins.Add(new Pin
            {
                Position = new Position(43.9480993889922, -78.8960205619348),
                Label = "Time: 11:33:17 AM"
            });
            map.Pins.Add(new Pin
            {
                Position = new Position(43.9464500188188, -78.8946003943102),
                Label = "Time: 11:32:17 AM"
            });
        }
    }


}