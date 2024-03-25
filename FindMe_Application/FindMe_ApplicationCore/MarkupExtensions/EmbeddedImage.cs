using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

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
