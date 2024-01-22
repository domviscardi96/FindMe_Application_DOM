using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

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