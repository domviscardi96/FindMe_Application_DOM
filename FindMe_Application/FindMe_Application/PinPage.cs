using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace FindMe_Application
{
    public class PinPage : ContentPage
    {
        Map map;
        Position currentPosition = new Position(43.9466095604592, -78.8943450362667);
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
            map.MoveToRegion(MapSpan.FromCenterAndRadius(currentPosition, Distance.FromMiles(3)));
            //var position = new Position(43.9466095604592, -78.8943450362667);
            //place the first pin with the current time/location

            // add current pin
            var currentPin = new Pin
            {
                Type = PinType.Place,
                Position = currentPosition,
                Label = "Current",
                //Address = $"Time: {DateTime.Now.ToString("hh:mm:ss tt")}"
                Address = "Time: 11:44:17 AM"
            };
            map.Pins.Add(currentPin);

            // add "Show Current" button
            var showCurrentButton = new Button { Text = "Show Current", HorizontalOptions = LayoutOptions.FillAndExpand };
            showCurrentButton.Clicked += (sender, args) =>
            {
                map.MoveToRegion(MapSpan.FromCenterAndRadius(currentPosition, Distance.FromMiles(3)));
            };

            //add more pins for the user to see
            var morePinsButton = new Button { Text = "View more pins", HorizontalOptions = LayoutOptions.FillAndExpand };
            morePinsButton.Clicked += (sender, args) =>
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

                map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(43.9466095604592, -78.8943450362667), Distance.FromMiles(3)));
            };

            //recenter the map after using zooms in/out or moves around
            var recenterButton = new Button { Text = "Re-center", HorizontalOptions = LayoutOptions.FillAndExpand };
            recenterButton.Clicked += (sender, args) =>
            {
                map.MoveToRegion(MapSpan.FromCenterAndRadius(currentPosition, Distance.FromMiles(3)));
            };
            var buttons = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children = { morePins, reLocate }
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
    }
}