///** 
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
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace FindMe_Application.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BtServPage : ContentPage
    {
        private readonly IDevice _connectedDevice;
        private readonly List<IService> _servicesList = new List<IService>();
        private readonly List<ICharacteristic> _charList = new List<ICharacteristic>();

        public BtServPage(IDevice connectedDevice)
        {
            InitializeComponent();
            _connectedDevice = connectedDevice;
            bleDevice.Text = "Selected BLE device: " + _connectedDevice.Name;
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                var servicesListReadOnly = await _connectedDevice.GetServicesAsync();

                _servicesList.Clear();
                var servicesListStr = new List<String>();

                // Filter services based on the desired UUID
                foreach (var service in servicesListReadOnly)
                {
                    if (service.Id == Guid.Parse("4fafc201-1fb5-459e-8fcc-c5c9c331914b"))
                    {
                        _servicesList.Add(service);
                        servicesListStr.Add(service.Name + ", UUID: " + service.Id.ToString());
                    }
                }

                foundBleServs.ItemsSource = servicesListStr;
            }
            catch
            {
                await DisplayAlert("Error initializing", $"Error initializing UART GATT service.", "OK");
            }
        }

        private async void FoundBleServs_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var selectedService = _servicesList[e.ItemIndex];
            if (selectedService != null)
            {
                // Perform the operations directly instead of navigating to BtCharPage
                await PerformOperations(selectedService);
            }
        }

        // Function to perform operations on the selected service
        private async Task PerformOperations(IService selectedService)
        {
            try
            {
                var charListReadOnly = await selectedService.GetCharacteristicsAsync();

                _charList.Clear();
                var charListStr = new List<String>();

                for (int i = 0; i < charListReadOnly.Count; i++)
                {
                    _charList.Add(charListReadOnly[i]);

                    charListStr.Add(i.ToString() + ": " + charListReadOnly[i].Name);
                }
                // For example, automatically select the second unknown characteristic if available
                if (charListReadOnly.Count >= 2)
                {
                    var charToUse = charListReadOnly[1];
                    await SendCharacter(charToUse, "1");
                }
            }
            catch
            {
                await DisplayAlert("Error", "Error performing operations on service.", "OK");
            }
        }

        private async Task SendCharacter(ICharacteristic characteristic, string character)
        {
            try
            {
                if (characteristic != null)
                {
                    if (characteristic.CanWrite)
                    {
                        byte[] array = Encoding.UTF8.GetBytes("2"); //change the text here
                        await characteristic.WriteAsync(array);
                    }
                    else
                    {
                        await DisplayAlert("Error", "Characteristic does not support Write", "OK");
                    }
                }
            }
            catch
            {
                await DisplayAlert("Error", "Error sending Characteristic.", "OK");
            }
        }
    }
}