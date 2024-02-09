//** 
//This class is the main page UI of the application. The user has multiple options that they can select from. 
//The main page consists of the title of the application and its functionalities 
//**

using FindMe_Application.Views;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: ExportFont("MontereyFLF-BoldItalic.ttf", Alias = "MyFont")]
[assembly: ExportFont("Prototype.ttf", Alias = "MyFont2")]

namespace FindMe_Application
{
    public partial class MainPage : ContentPage
    {
        public IDevice _connectedDevice;
        private bool isAlarmSwitchToggledOn = false;
        private bool isLightSwitchToggledOn = false;
        private bool isBuzzerSwitchToggledOn = false;
        private BtDevPage _btDevPage;

        public MainPage()
        {
            InitializeComponent();
            BtDevPage.ConnectedDeviceChanged += (sender, device) => _connectedDevice = device;
            _btDevPage = new BtDevPage();

        }
        // method invoked when the bluetooth switch is toggled
         public async void Bluetooth_Toggled(object sender, ToggledEventArgs e)
        {
            Image bluetoothImage = (Image)this.Content.FindByName("bluetoothImage");

            // Get references to other switches and their corresponding images
            Switch swAlarm = (Switch)this.Content.FindByName("swAlarm");
            Image alarmImage = (Image)this.Content.FindByName("alarmImage");

            Switch swBuzzer = (Switch)this.Content.FindByName("swBuzzer");
            Image buzzerImage = (Image)this.Content.FindByName("buzzerImage");

            Switch swLight = (Switch)this.Content.FindByName("swLight");
            Image lightImage = (Image)this.Content.FindByName("lightImage");

            if (bluetoothImage != null && swAlarm != null && alarmImage != null && swBuzzer != null && buzzerImage != null && swLight != null && lightImage != null)
            {
                if (e.Value && swBluetooth.IsToggled) //check for bluetooth toggled
                {
                    // Switch is toggled ON, change the image source to bluetooth_on.png
                    bluetoothImage.Source = ImageSource.FromResource("FindMe_Application.Embedded_Resources.Images.bluetooth_ON.png");
                    HandleBluetoothToggled(sender, e);
                }
                else
                {
                    // Switch is toggled OFF, revert to the original image source
                    bluetoothImage.Source = ImageSource.FromResource("FindMe_Application.Embedded_Resources.Images.bluetooth_OFF.png");
                    //called the function in BtDevPage to disconnect the device 
                    await _btDevPage.DisconnectFromBLEDevice();

                    // Untoggle other switches and set their images to OFF mode
                    swAlarm.IsToggled = false;
                    alarmImage.Source = ImageSource.FromResource("FindMe_Application.Embedded_Resources.Images.alarm_OFF.png");

                    swBuzzer.IsToggled = false;
                    buzzerImage.Source = ImageSource.FromResource("FindMe_Application.Embedded_Resources.Images.volume_OFF.png");

                    swLight.IsToggled = false;
                    lightImage.Source = ImageSource.FromResource("FindMe_Application.Embedded_Resources.Images.bulb_OFF.png");
                    
                   

                }
            }
        }


        //method is called on bluetooth toggle on, navigates user to the bluetooth device connection page 
        async void HandleBluetoothToggled(object sender, ToggledEventArgs e)
        {

            await Navigation.PushAsync(new BtDevPage());
            //await DisplayAlert("Bluetooth", "Bluetooth is connected", "OK");
        }


        //method is invoked on alarm switch toggled
        void Alarm_Toggled(object sender, ToggledEventArgs e)
        {
            isAlarmSwitchToggledOn = e.Value;

            if (swBluetooth.IsToggled)
            {
                // Access the connected device from BtDevPage
                _connectedDevice = BtDevPage.ConnectedDevice;


                HandleAlarmToggled(sender, e);
            }
            else
            {   // Bluetooth is not connected, handle it accordingly (e.g., display an alert)
                DisplayAlert("Bluetooth Not Connected", "Please connect to a FindMe device first.", "OK");
                // Optionally, you can toggle the switch back to off if needed
                swAlarm.IsToggled = false;
            }

        }
    // this function handles the alarm toggle on
    async void HandleAlarmToggled(object sender, ToggledEventArgs e)
        {
            Image alarmImage = (Image)this.Content.FindByName("alarmImage");


            if (alarmImage != null)
            {
                if (e.Value && swBluetooth.IsToggled)
                {
                    // Switch is toggled ON, change the image source to alarm_on.png
                    alarmImage.Source = ImageSource.FromResource("FindMe_Application.Embedded_Resources.Images.Alarm_ON.png");

                    // Navigate to the BtServPage
                    // await Navigation.PushAsync(new BtServPage(_connectedDevice));

                    await PerformAlarmOperations(_connectedDevice, isAlarmSwitchToggledOn);
                }
                else
                {
                    // Switch is toggled OFF, revert to the original image source
                    alarmImage.Source = ImageSource.FromResource("FindMe_Application.Embedded_Resources.Images.alarm_OFF.png");
                    // Add any additional logic for switch OFF if needed
                    await PerformAlarmOperations(_connectedDevice, false);
                }
            }
         }

        // Function to perform alarm-related operations
        private async Task PerformAlarmOperations(IDevice connectedDevice, bool isSwitchToggledOn)
        {
            try
            {

                // Get services of the connected device
                var services = await connectedDevice.GetServicesAsync();

                //Find the alarm service based on its UUID
                var alarmService = services.FirstOrDefault(s => s.Id == Guid.Parse("4fafc201-1fb5-459e-8fcc-c5c9c331914b"));

                if (alarmService != null)
                {
                    // Get characteristics of the alarm service
                    var characteristics = await alarmService.GetCharacteristicsAsync();

                    // Find the alarm characteristic based on its UUID
                    var alarmCharacteristic = characteristics.FirstOrDefault(c => c.Id == Guid.Parse("e3223119-9445-4e96-a4a1-85358c4046a2"));

                    if (alarmCharacteristic != null)
                    {
                        // Example: Perform the necessary write

                        // For write operation:
                        byte[] alarmData = Encoding.UTF8.GetBytes(isSwitchToggledOn ? "4" : "1"); // 2 (alarm on) , 1 (alarrm off)
                        await alarmCharacteristic.WriteAsync(alarmData);

                    }
                 }
             }   
            catch
            {
                await DisplayAlert("Error", "Error performing alarm operations.", "OK");
            }
     }

        //this method invoked on buzzer switch being toggled
        void Buzzer_Toggled(object sender, ToggledEventArgs e)
        {
            isBuzzerSwitchToggledOn = e.Value;

            if (swBluetooth.IsToggled)
            {
                // Access the connected device from BtDevPage
                _connectedDevice = BtDevPage.ConnectedDevice;


                HandleBuzzerToggled(sender, e);
            }
            else
            {   // Bluetooth is not connected, handle it accordingly (e.g., display an alert)
                DisplayAlert("Bluetooth Not Connected", "Please connect to a FindMe device first.", "OK");
                // Optionally, you can toggle the switch back to off if needed
                swBuzzer.IsToggled = false;
            }
        }


        //this method handles the toggle being on
        async void HandleBuzzerToggled(object sender, ToggledEventArgs e)
        {
            Image buzzerImage = (Image)this.Content.FindByName("buzzerImage");

            if (buzzerImage != null)
            {
                if (e.Value && swBluetooth.IsToggled) //add AND swBluetooth.IsToggled
                {
                    // Switch is toggled ON, change the image source to bluetooth_on.png
                    buzzerImage.Source = ImageSource.FromResource("FindMe_Application.Embedded_Resources.Images.volume_ON.png");
                    await PerformBuzzerOperations(_connectedDevice, isBuzzerSwitchToggledOn);
                }
                else
                {
                    // Switch is toggled OFF, revert to the original image source
                    buzzerImage.Source = ImageSource.FromResource("FindMe_Application.Embedded_Resources.Images.volume_OFF.png");
                    // Add any additional logic for switch OFF if needed
                    await PerformBuzzerOperations(_connectedDevice, false);

                }
            }
        }

        // Function to perform alarm-related operations
        private async Task PerformBuzzerOperations(IDevice connectedDevice, bool isSwitchToggledOn)
        {
            try
            {

                // Get services of the connected device
                var services = await connectedDevice.GetServicesAsync();

                //Find the alarm service based on its UUID
                var buzzerService = services.FirstOrDefault(s => s.Id == Guid.Parse("4fafc201-1fb5-459e-8fcc-c5c9c331914b"));

                if (buzzerService != null)
                {
                    // Get characteristics of the alarm service
                    var characteristics = await buzzerService.GetCharacteristicsAsync();

                    // Find the alarm characteristic based on its UUID
                    var buzzerCharacteristic = characteristics.FirstOrDefault(c => c.Id == Guid.Parse("e3223119-9445-4e96-a4a1-85358c4046a2"));

                    if (buzzerCharacteristic != null)
                    {
                        // Example: Perform the necessary write

                        // For write operation:
                        byte[] alarmData = Encoding.UTF8.GetBytes(isSwitchToggledOn ? "3" : "1"); // 3 (buzzer on) , 1 ( off)
                        await buzzerCharacteristic.WriteAsync(alarmData);

                    }
                }
            }
            catch
            {
                await DisplayAlert("Error", "Error performing buzzer operations.", "OK");
            }
        }


        //this method is invoked on light switch being toggled
        void Light_Toggled(object sender, ToggledEventArgs e)
        {
            isLightSwitchToggledOn = e.Value;

            if (swBluetooth.IsToggled)
            {
                // Access the connected device from BtDevPage
                _connectedDevice = BtDevPage.ConnectedDevice;


                HandleLightToggled(sender, e);
            }
            else
            {   // Bluetooth is not connected, handle it accordingly (e.g., display an alert)
                DisplayAlert("Bluetooth Not Connected", "Please connect to a FindMe device first.", "OK");
                // Optionally, you can toggle the switch back to off if needed
                swLight.IsToggled = false;
            }
        }


        //this method handles light toggle on
        async void HandleLightToggled(object sender, ToggledEventArgs e)
        {
            Image lightImage = (Image)this.Content.FindByName("lightImage");

            if (lightImage != null)
            {
                if (e.Value && swBluetooth.IsToggled) //add AND swBluetooth.IsToggled
                {
                    // Switch is toggled ON, change the image source to bluetooth_on.png
                    lightImage.Source = ImageSource.FromResource("FindMe_Application.Embedded_Resources.Images.bulb_ON.png");
                    await PerformLightOperations(_connectedDevice, isLightSwitchToggledOn);

                }
                else
                {
                    // Switch is toggled OFF, revert to the original image source
                    lightImage.Source = ImageSource.FromResource("FindMe_Application.Embedded_Resources.Images.bulb_OFF.png");
                    
                    // Add any additional logic for switch OFF if needed
                    await PerformLightOperations(_connectedDevice, false);

                }
            }

        }

        // Function to perform light-related operations
        private async Task PerformLightOperations(IDevice connectedDevice, bool isSwitchToggledOn)
        {
            try
            {

                // Get services of the connected device
                var services = await connectedDevice.GetServicesAsync();

                //Find the alarm service based on its UUID
                var lightService = services.FirstOrDefault(s => s.Id == Guid.Parse("4fafc201-1fb5-459e-8fcc-c5c9c331914b"));

                if (lightService != null)
                {
                    // Get characteristics of the alarm service
                    var characteristics = await lightService.GetCharacteristicsAsync();

                    // Find the alarm characteristic based on its UUID
                    var lightCharacteristic = characteristics.FirstOrDefault(c => c.Id == Guid.Parse("e3223119-9445-4e96-a4a1-85358c4046a2"));

                    if (lightCharacteristic != null)
                    {
                        // Example: Perform the necessary write

                        // For write operation:
                        byte[] lightData = Encoding.UTF8.GetBytes(isSwitchToggledOn ? "2" : "1"); // 2 (light on) , 1 (light off)
                        await lightCharacteristic.WriteAsync(lightData);

                    }
                }
            }
            catch
            {
                await DisplayAlert("Error", "Error performing light operations.", "OK");
            }
        }


        //this function is invoked on the map button being clicked
        void MapButton_Clicked(object sender, EventArgs e)
        {
            HandleMapsButtonPressed(sender, e);
        }

        //this method handles the map button being clicked, it navigayes user to the maps page 
        async void HandleMapsButtonPressed(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new PinPage());
        }
    }
}


