using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace FindMe_Application.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BtCharPage : ContentPage
    {
        //initialize readonly variables for the connected device, the selected service, list to hold the available characterists of the BLE Device, and selected char
        private readonly IDevice _connectedDevice;                                      
        private readonly IService _selectedService;                                     
        private readonly List<ICharacteristic> _charList = new List<ICharacteristic>(); 
        private ICharacteristic _char;                                            

        public BtCharPage(IDevice connectedDevice, IService selectedService, String hexCommand)            // constructor called when an instance of the class is called, the BLE device and a selected Service are passed in 
        {
            InitializeComponent();

            _connectedDevice = connectedDevice;                                        
            _selectedService = selectedService;
            _char = null;                                                               

        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                if (_selectedService != null)
                {
                    var charListReadOnly = await _selectedService.GetCharacteristicsAsync();  // Read in available Characteristics

                    _charList.Clear();
                    var charListStr = new List<String>();

                    for (int i = 0; i < charListReadOnly.Count; i++)
                    {
                        _charList.Add(charListReadOnly[i]);

                        charListStr.Add(i.ToString() + ": " + charListReadOnly[i].Name);
                    }

                    //foundBleChars.ItemsSource = charListStr;

                    // Automatically select the second unknown characteristic if available
                    if (_charList.Count >= 2)
                    {
                        _char = _charList[1];  // Select the second characteristic (index 1)
                        await SendCharacter("1");  // Automatically send the character "0"
                    }
                }
                else
                {
                    ErrorLabel.Text = GetTimeNow() + "UART GATT service not found." + Environment.NewLine;
                }
            }
            catch
            {
                ErrorLabel.Text = GetTimeNow() + ": Error initializing UART GATT service.";
            }
        }

        private async Task SendCharacter(string character)
        {
            try
            {
                if (_char != null)
                {
                    if (_char.CanWrite)
                    {
                        // Write the ASCII representation of the specified character to the byte array
                        byte[] array = Encoding.UTF8.GetBytes(character);
                        await _char.WriteAsync(array);  // Send to the connected BLE Device
                    }
                    else
                    {
                        ErrorLabel.Text = GetTimeNow() + ": Characteristic does not support Write";
                    }
                }
            }
            catch
            {
                ErrorLabel.Text = GetTimeNow() + ": Error sending Characteristic.";
            }
        }
        //this function is to get the date/time for error messages
        private string GetTimeNow()
        {
            var timestamp = DateTime.Now;
            return timestamp.Hour.ToString() + ":" + timestamp.Minute.ToString() + ":" + timestamp.Second.ToString();
        }
    }
}