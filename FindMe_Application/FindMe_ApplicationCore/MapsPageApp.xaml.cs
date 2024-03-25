using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace FindMe_Application
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapsPageApp : ContentPage
    {
        public MapsPageApp()
        {
            InitializeComponent();

            new TabbedPage()
            {
                Children = {
                    new MapsPage(),
                    new PinPage(),
                    //new MapsApi(),
                }
            };
        }
    }
}