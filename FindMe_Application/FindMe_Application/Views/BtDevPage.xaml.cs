//** 
//This class is used to get the BLE Connection to the device ESP32
//the list of connectable devices will be displayed to the user on clicking the scan button
//referenced from: https://github.com/mo-thunderz/XamarinBleCodeBehind
//**

using Android.Bluetooth;
using Android.Util;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.LocalNotification.AndroidOption;
using Plugin.LocalNotification;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;


using System.Linq;

using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;


namespace FindMe_Application.Views
{
    [DesignTimeVisible(false)]
    public partial class BtDevPage : ContentPage
    {
        private readonly IAdapter _bluetoothAdapter;
        public readonly List<IDevice> _gattDevices = new List<IDevice>();

        // Declare a private backing field for the connected device
        public static IDevice _connectedDevice;

        // Declare an event to notify when the connected device changes
        public static event EventHandler<IDevice> ConnectedDeviceChanged;

       

        // Public property to get the connected device
        public static IDevice ConnectedDevice
        {
            get { return _connectedDevice; }
            private set
            {
                if (_connectedDevice != value)
                {
                    _connectedDevice = value;
                    // Raise the event when the connected device changes
                    ConnectedDeviceChanged?.Invoke(null, _connectedDevice);
                }
            }
        }


        //the constructor which is called when an instance of class is defined
        public BtDevPage()                                                      
        {
            InitializeComponent();

            //assign the bluetooth adapter to the current adapter on the phone 
            _bluetoothAdapter = CrossBluetoothLE.Current.Adapter;


            //once BLE device is found, add it to the list of devices
            _bluetoothAdapter.DeviceDiscovered += (sender, foundBleDevice) =>
            {
                if (foundBleDevice.Device != null && !string.IsNullOrEmpty(foundBleDevice.Device.Name))
                    _gattDevices.Add(foundBleDevice.Device);
            };

        }


        //function to ensure all the permissions are granted and approved
        private async Task<bool> PermissionsGrantedAsync()
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }

                return status == PermissionStatus.Granted;
            }



        //function is called on scan button being clicked by user
        private async void ScanButton_Clicked(object sender, EventArgs e)           
        {
            IsBusyIndicator.IsVisible = IsBusyIndicator.IsRunning = !(ScanButton.IsEnabled = false);        //show the busy indicator while scanning for available devices
            foundBleDevicesListView.ItemsSource = null;                                                     

            if (!await PermissionsGrantedAsync())                                                           //ensure permissions to use bluetooth are granted
            {
                await DisplayAlert("Permission required", "Application needs location permission", "OK");   //if not then request
                IsBusyIndicator.IsVisible = IsBusyIndicator.IsRunning = !(ScanButton.IsEnabled = true);
                return;
            }

            _gattDevices.Clear();                                                                           

            if (!_bluetoothAdapter.IsScanning)                                                              //ensure that the Bluetooth adapter is scanning for devices
            {
                await _bluetoothAdapter.StartScanningForDevicesAsync();
            }

            foreach (var device in _bluetoothAdapter.ConnectedDevices)                                      //Ensure BLE devices are added to the _gattDevices list
                _gattDevices.Add(device);

            foundBleDevicesListView.ItemsSource = _gattDevices.ToArray();                                   // Write found BLE devices to the display
            IsBusyIndicator.IsVisible = IsBusyIndicator.IsRunning = !(ScanButton.IsEnabled = true);         
        }



        // Add this method to disconnect from the BLE device
        public async Task DisconnectFromBLEDevice()
        {
            if (_connectedDevice != null)
            {
                try
                {
                    // Get services of the connected device
                    var services = await _connectedDevice.GetServicesAsync();

                    //Find the service based on its UUID
                    var turnoffservice = services.FirstOrDefault(s => s.Id == Guid.Parse("4fafc201-1fb5-459e-8fcc-c5c9c331914b"));

                    if (turnoffservice != null)
                    {
                        // Get characteristics of the alarm service
                        var characteristics = await turnoffservice.GetCharacteristicsAsync();

                        // Find the alarm characteristic based on its UUID
                        var turnoffCharacteristic = characteristics.FirstOrDefault(c => c.Id == Guid.Parse("e3223119-9445-4e96-a4a1-85358c4046a2"));

                        if (turnoffCharacteristic != null)
                        {
                            // Example: Perform the necessary write

                            // For write operation:
                            byte[] OFFData = Encoding.UTF8.GetBytes("1"); // , 1 (turn everything off)
                            await turnoffCharacteristic.WriteAsync(OFFData);

                        }
                        await _bluetoothAdapter.DisconnectDeviceAsync(_connectedDevice);
                    }

                }
                catch(Exception ex)
                {
                    // Handle any exceptions that may occur during disconnection
                    Debug.WriteLine($"Error disconnecting from the device: {ex.Message}");
                }
                finally
                {
                    
                    _connectedDevice = null;
                }
            }
        }

        // Modify the existing method to handle connecting to BLE devices
        public async void FoundBluetoothDevicesListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            IsBusyIndicator.IsVisible = IsBusyIndicator.IsRunning = !(ScanButton.IsEnabled = false);
            IDevice selectedItem = e.Item as IDevice;

            if (selectedItem.State == DeviceState.Connected)
            {
                // Set the connected device
                _connectedDevice = selectedItem;

                await DisplayAlert("Succesfull Connection", $"BLE device {selectedItem.Name ?? "N/A"} already connected", "OK");
            }
            else
            {
                try
                {
                    var connectParameters = new ConnectParameters(false, true);
                    await _bluetoothAdapter.ConnectToDeviceAsync(selectedItem, connectParameters);

                    // Set the connected device
                    _connectedDevice = selectedItem;

                    //var result = await DisplayPromptAsync("Owner's Information", "Please provide your information in the following format: \nName,Last name,Phone number", "OK", "Cancel", "", -1, Keyboard.Default, "");

                    //if (result != null)
                    //{
                    //    // Save the entered information
                    //    string[] userInfo = result.Split(',');
                    //    string name = userInfo.Length > 0 ? userInfo[0] : "";
                    //    string lastName = userInfo.Length > 1 ? userInfo[1] : "";
                    //    string phoneNumber = userInfo.Length > 2 ? userInfo[2] : "";

                    //    // Now you can use 'name', 'lastName', and 'phoneNumber' as needed
                    //    //check if the information is proper
                    //    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(lastName) && !string.IsNullOrEmpty(phoneNumber))
                    //    {
                    //        // Information properly saved
                    //        await DisplayAlert("Information Saved", "Information saved successfully", "OK");

                    //        // Concatenate name, last name, and phone number into a single string
                    //        string ownerInfo = $"{name},{lastName},{phoneNumber}";

                    //        // Display the entered name, last name, and phone number
                    //        await DisplayAlert("Owner's Information", $"Name: {name}\nLast Name: {lastName}\nPhone Number: {phoneNumber}", "OK");

                    //        await PerformoNFCOperations(_connectedDevice,ownerInfo);
                    //    }
                    //    else
                    //    {
                    //        // Information not properly saved
                    //        await DisplayAlert("Error", "Please provide valid information for name, last name, and phone number", "OK");
                    //    }
                    //}
                    //else
                    //{
                    //    await DisplayAlert("Error", "Failed to get additional information", "OK");
                    //}

                    await DisplayAlert("Succesfull Connection", $"Connected to BLE device: {selectedItem.Name ?? "N/A"}", "OK");
                    

                }
                catch
                {
                    await DisplayAlert("Error Connecting", $"Error connecting to BLE device: {selectedItem.Name ?? "N/A"}", "Retry");
                }
            }

            IsBusyIndicator.IsVisible = IsBusyIndicator.IsRunning = !(ScanButton.IsEnabled = true);

            if (_connectedDevice != null)
            {
                await Navigation.PopAsync();
             
            }
        }


    }
}


