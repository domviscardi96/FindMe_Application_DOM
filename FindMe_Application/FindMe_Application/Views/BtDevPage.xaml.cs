//** 
//This class is used to get the BLE Connection to the device ESP32
//the list of connectable devices will be displayed to the user on clicking the scan button
//referenced from: https://github.com/mo-thunderz/XamarinBleCodeBehind
//**

using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using static Xamarin.Essentials.Permissions;
using XamarinEssentials = Xamarin.Essentials;

namespace FindMe_Application.Views
{
    [DesignTimeVisible(false)]
    public partial class BtDevPage : ContentPage
    {
        //instantiate readonly variables for the BLE adapter and a list to hold the BLE devices connected
        private readonly IAdapter _bluetoothAdapter;                            
        private readonly List<IDevice> _gattDevices = new List<IDevice>();
        private IDevice _connectedDevice; //to keep track of the connected devices

        public BtDevPage()                                                      //the constructor which is called when an instance of class is defined
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

        ////this function is called on selecting any of the found devices in the device list
        //public async void FoundBluetoothDevicesListView_ItemTapped(object sender, ItemTappedEventArgs e)   
        //{
        //    IsBusyIndicator.IsVisible = IsBusyIndicator.IsRunning = !(ScanButton.IsEnabled = false);            //switch on IsBusy indicator
        //    IDevice selectedItem = e.Item as IDevice;                                                           //selected item is an Idevice, so need to cast to that, IDevice is an interface

        //    if (selectedItem.State == DeviceState.Connected)                                                    //check connection to the device
        //    {
        //        await Navigation.PushAsync(new BtServPage(selectedItem));                                       //navigate to the service page to show the services of the selected BLE
        //    }
        //    else
        //    {
        //        try
        //        {
        //            var connectParameters = new ConnectParameters(false, true);                                //if not connected, try to connect to the BLE device selected
        //            await _bluetoothAdapter.ConnectToDeviceAsync(selectedItem, connectParameters);             //Navigate to the service page to show the services of the selected BLE device
        //            await Navigation.PushAsync(new BtServPage(selectedItem));
        //        }
        //        catch
        //        {
        //            await DisplayAlert("Error Connecting", $"Error connecting to BLE device: {selectedItem.Name ?? "N/A"}", "Retry");       //error message shown when unable to connect
        //        }
        //    }
        //    IsBusyIndicator.IsVisible = IsBusyIndicator.IsRunning = !(ScanButton.IsEnabled = true);             

        //}


        // Add this method to disconnect from the BLE device
        private async void DisconnectFromBLEDevice()
        {
            if (_connectedDevice != null)
            {
                try
                {
                    await _bluetoothAdapter.DisconnectDeviceAsync(_connectedDevice);
                }
                catch
                {
                    // Handle any exceptions that may occur during disconnection
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
                    
                    await DisplayAlert("Succesfull Connection", $"Connected to BLE device: {selectedItem.Name ?? "N/A"}", "OK");
                }
                catch
                {
                    await DisplayAlert("Error Connecting", $"Error connecting to BLE device: {selectedItem.Name ?? "N/A"}", "Retry");
                }
            }
            IsBusyIndicator.IsVisible = IsBusyIndicator.IsRunning = !(ScanButton.IsEnabled = true);
        }


    }
}