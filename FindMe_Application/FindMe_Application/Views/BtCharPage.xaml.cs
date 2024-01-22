using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinEssentials = Xamarin.Essentials;

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

            bleDevice.Text = "Selected BLE device: " + _connectedDevice.Name;           // Write the selected BLE Device and Service to the UI
            bleService.Text = "Selected BLE service: " + _selectedService.Name;
        }

        protected async override void OnAppearing()                                     // When page called, shows the availabel characteristics
        {
            base.OnAppearing();
            try
            {
                if (_selectedService != null)
                {
                    var charListReadOnly = await _selectedService.GetCharacteristicsAsync();       // Read in available Characteristics

                    _charList.Clear();
                    var charListStr = new List<String>();

                    for (int i = 0; i < charListReadOnly.Count; i++)                               // Loop through available interfaces and add each to the list of characteristics
                    {
                        _charList.Add(charListReadOnly[i]);                                        
                        
                        charListStr.Add(i.ToString() + ": " + charListReadOnly[i].Name);           // Write to a list of Strings for the UI
                    }
                    foundBleChars.ItemsSource = charListStr;                                       // Write found Chars to the UI through listview 
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

        //this function is called when a characteristic is selected from this list 
        private async void FoundBleChars_ItemTapped(object sender, ItemTappedEventArgs e)       
        {

            _char = _charList[e.ItemIndex];                                                     // selected Char

            if (_char != null)                                                                  
            {
                bleChar.Text = _char.Name + "\n" +                                              //write the information of the characteristic to the UI
                    "UUID: " + _char.Uuid.ToString() + "\n" +
                    "Read: " + _char.CanRead + "\n" +                                           // indicates whether characteristic can be read from
                    "Write: " + _char.CanWrite + "\n" +                                         // indicates whether characteristic can be written to
                    "Update: " + _char.CanUpdate;                                               // indicates whether characteristics can be updated (supports notify)

                var charDescriptors = await _char.GetDescriptorsAsync();                        // get information of Descriptors defined

                bleChar.Text += "\nDescriptors (" + charDescriptors.Count + "): ";              // write Descriptor info to the GUI
                for (int i = 0; i < charDescriptors.Count; i++)
                    bleChar.Text += charDescriptors[i].Name + ", ";
            }
        }


        //function called when the register button is clicked
        //callback function will be defined that will be triggered if the selected BLE device sends information to the phone.
        private async void RegisterCommandButton_Clicked(object sender, EventArgs e)                    
        {
            try
            {
                if (_char != null)                                                                      
                {
                    if (_char.CanUpdate)                                                                // check if characteristic supports notify
                    {
                        _char.ValueUpdated += (o, args) =>                                              // define a callback function, ValueUpdated is an eventListener for when a value is changed on the characteristic
                        {
                            var receivedBytes = args.Characteristic.Value;                              // read in received bytes
                            Console.WriteLine("byte array: " + BitConverter.ToString(receivedBytes));   // write to the console for debugging


                            string _charStr = "";                                                                           // in the following section the received bytes will be displayed in different ways (you can select the method you need)
                            if (receivedBytes != null)
                            {
                                _charStr = "Bytes: " + BitConverter.ToString(receivedBytes);                                // by directly converting the bytes to strings we see the bytes themselves as they are received
                                _charStr += " | UTF8: " + Encoding.UTF8.GetString(receivedBytes, 0, receivedBytes.Length);  // This code interprets the bytes received as ASCII characters
                            }

                            if (receivedBytes.Length <= 4)
                            {                                                                                               // If only 4 or less bytes were received than it could be that an INT was sent. The code here combines the 4 bytes back to an INT
                                int char_val = 0;
                                for (int i = 0; i < receivedBytes.Length; i++)
                                {
                                    char_val |= (receivedBytes[i] << i * 8);
                                }
                                _charStr += " | int: " + char_val.ToString();
                            }
                            _charStr += Environment.NewLine;                                                                

                            XamarinEssentials.MainThread.BeginInvokeOnMainThread(() =>                                      // as this is a callback function, the "MainThread" needs to be invoked to update the UI
                            {
                                Output.Text += _charStr;
                            });

                        };
                        await _char.StartUpdatesAsync();

                        ErrorLabel.Text = GetTimeNow() + ": Notify callback function registered successfully.";
                    }
                    else
                    {
                        ErrorLabel.Text = GetTimeNow() + ": Characteristic does not have a notify function.";
                    }
                }
                else
                {
                    ErrorLabel.Text = GetTimeNow() + ": No characteristic selected.";
                }
            }
            catch
            {
                ErrorLabel.Text = GetTimeNow() + ": Error initializing UART GATT service.";
            }
        }


        //function called when the recieve button is clicked 
        private async void ReceiveCommandButton_Clicked(object sender, EventArgs e)                 
        {
            try
            {
                if (_char != null)                                                                 
                {
                    String _charStr = "";
                   
                    if (_char.CanRead)                                                              // check if characteristic supports read
           
                    {
                        
                        _char.ValueUpdated += (o, args) =>                                              // define a callback function, ValueUpdated is an eventListener for when a value is changed on the characteristic
                        {
                            var receivedBytes = args.Characteristic.Value;                              // read in received bytes


                            if (receivedBytes != null)
                            {
                                _charStr = "Bytes: " + BitConverter.ToString(receivedBytes);                                // by directly converting the bytes to strings we see the bytes themselves as they are received
                                _charStr += " | UTF8: " + Encoding.UTF8.GetString(receivedBytes, 0, receivedBytes.Length);  // This code interprets the bytes received as ASCII characters
                            }

                            if (receivedBytes.Length <= 4)
                            {                                                                                               // If only 4 or less bytes were received than it could be that an INT was sent. The code here combines the 4 bytes back to an INT
                                int char_val = 0;
                                for (int i = 0; i < receivedBytes.Length; i++)
                                {
                                    char_val |= (receivedBytes[i] << i * 8);
                                }
                                _charStr += " | int: " + char_val.ToString();
                            }
                            _charStr += Environment.NewLine;                                                                

                            XamarinEssentials.MainThread.BeginInvokeOnMainThread(() =>                                      // as this is a callback function, the "MainThread" needs to be invoked to update the GUI
                            {
                                Output.Text += _charStr;
                            });

                        };

                        
                    }
                    else
                    {
                        ErrorLabel.Text = GetTimeNow() + ": Characteristic does not support read.";
                    }
                }
                else
                    ErrorLabel.Text = GetTimeNow() + ": No Characteristic selected.";
            }
            catch
            {
                ErrorLabel.Text = GetTimeNow() + ": Error receiving Characteristic.";
            }
        }

        //this function is invoked when the send button is clicked
        private async void SendCommandButton_Clicked(object sender, EventArgs e)                    
        {
            try
            {
                if (_char != null)                                                                  
                {
                    
                    if (_char.CanWrite)                                                             // check if characteristic supports write
                    {
                        byte[] array = Encoding.UTF8.GetBytes(CommandTxt.Text);                     // Write CommandTxt.Text String to byte array in preparation of sending it over, using ASCII characters
                        await _char.WriteAsync(array);                                              // Send to the connected BLE Device
                    }
                    else
                    {
                        ErrorLabel.Text = GetTimeNow() + ": Characteristic does not support Write";
                    }
                }
            }
            catch
            {
                ErrorLabel.Text = GetTimeNow() + ": Error receiving Characteristic.";
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