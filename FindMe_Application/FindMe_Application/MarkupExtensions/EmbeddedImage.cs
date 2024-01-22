using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FindMe_Application.MarkupExtensions
{
    public class EmbeddedImage : IMarkupExtension
    {
        public string ResourceID {get; set;}
        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if(string.IsNullOrWhiteSpace(ResourceID))
                return null;
            return ImageSource.FromResource(ResourceID);
        }
    }
}
