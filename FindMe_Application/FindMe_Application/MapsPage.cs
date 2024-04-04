//**
//This class is called when the user clicks on the open map button on the main screen
//it shows the user options to view current location or last known locations of the device
//**

using System;
using Xamarin.Essentials;
using Xamarin.Forms;


namespace FindMe_Application
{
    public class MapsPage : ContentPage
    {
        public MapsPage()
        {
            var infoLabel = new Label
            {
                Text = "Select the Button below to view current location of the device"
            };

            //create a button for the user to open the current location of device
            var openLocation = new Button
            {
                Text = "Open Current Location of Device"
            };
            //on clicking the button, user will be navigated to a google maps page where it will show the location
            openLocation.Clicked += (sender, e) =>
            {
                if (Device.RuntimePlatform == Device.iOS)
                {
                    Launcher.OpenAsync(new Uri("http://maps.apple.com/?q=43.9466095604592+-78.8943450362667"));
                }
                else if (Device.RuntimePlatform == Device.Android)
                {
                    
                    Launcher.OpenAsync(new Uri("http://maps.google.com/?q=43.9466095604592+-78.8943450362667"));
                }
                else if (Device.RuntimePlatform == Device.UWP)
                {
                    Launcher.OpenAsync(new Uri("bingmaps:?where=43.9466095604592+-78.8943450362667"));
                }

            };

            //button to open more locations of the device
            var openPinnedPoints = new Button
            {
                Text = "Open Last 12 Locations of Device"
            };
            //when clicked, navigate to the pin page 
            openPinnedPoints.Clicked += async (sender, e) =>
            {
                await Navigation.PushAsync(new PinPage());
            };

            //stacked layout of the screen
            Content = new StackLayout
            {
                Padding = new Thickness(5, 20, 5, 0),
                HorizontalOptions = LayoutOptions.Fill,
                Children = {
                    infoLabel, openLocation, openPinnedPoints
                }
            };

        }
    }
}