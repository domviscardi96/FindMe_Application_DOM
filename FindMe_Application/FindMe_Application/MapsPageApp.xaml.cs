using Xamarin.Forms;
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