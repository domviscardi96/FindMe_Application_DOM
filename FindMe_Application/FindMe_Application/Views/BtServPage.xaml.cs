//** 
//This class is used to show the availabel services of the BLE Device
//the list of services related to the devices will be displayed to the user on selecting the BLE device
//referenced from: https://github.com/mo-thunderz/XamarinBleCodeBehind
//**

using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FindMe_Application.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BtServPage : ContentPage
    {
        //initialize readonly variables for the connected device and list of services
        private readonly IDevice _connectedDevice;                              
        private readonly List<IService> _servicesList = new List<IService>();   

        public BtServPage(IDevice connectedDevice)                              // this class is called after user has selected a BLE Device, the BLE device is passed from the BtDevPage
        {
            InitializeComponent();

            
            _connectedDevice = connectedDevice;                                 //store the connected device to the readonly variable to be used within this class
            bleDevice.Text = "Selected BLE device: " + _connectedDevice.Name;   // Write the name of the connected device to the user interface
        }

        protected async override void OnAppearing()                             // When the page is called we would like to see the services available
        {
            base.OnAppearing();

            try
            {
                var servicesListReadOnly = await _connectedDevice.GetServicesAsync();           // Read in the list of available Services for the connected device

                _servicesList.Clear();
                var servicesListStr = new List<String>();

                for (int i = 0; i < servicesListReadOnly.Count; i++)                             // Loop through the found interfaces and add each service to the list
                {
                    _servicesList.Add(servicesListReadOnly[i]);                                  
                    servicesListStr.Add(servicesListReadOnly[i].Name + ", UUID: " + servicesListReadOnly[i].Id.ToString());    // Append the name of the services seperately to an array of strings that can be used to populate the list in the UI
                }
                foundBleServs.ItemsSource = servicesListStr;                                    // Write the found names to the list in the UI
            }
            catch
            {
                await DisplayAlert("Error initializing", $"Error initializing UART GATT service.", "OK");
            }
        }


        //Function is called when user selects a service to see the characteristics, navigates the user to the characteristics page 
        private async void FoundBleServs_ItemTapped(object sender, ItemTappedEventArgs e)       
        {
            var selectedService = _servicesList[e.ItemIndex];
            if (selectedService != null)                                                        
            {
                await Navigation.PushAsync(new BtCharPage(_connectedDevice, selectedService, null));  
            }
        }
    }
}