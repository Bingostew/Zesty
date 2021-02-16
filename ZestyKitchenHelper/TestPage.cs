using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.IO;
using System.Runtime.CompilerServices;
using System.Timers;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Essentials;
using System.Diagnostics.SymbolStore;
using System.Diagnostics;
using System.Numerics;
using System.Collections;
using Xamarin.Forms.Xaml;
using System.Net.Http;

namespace ZestyKitchenHelper
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    // *** Get Width by using "Layout.width", not measured in pixels
    [DesignTimeVisible(true)]
    public class TestPage : ContentPage
    {
       
        AbsoluteLayout layout = new AbsoluteLayout();
        public TestPage()
        {
            Title = "No";
            Label label = new Label
            {
                Text = "Click the Button below",
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.Center,
                TranslationX = 100
            };

            Button button = new Button
            {
                Text = "Click to Rotate Text!",
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.Center,
                TranslationY = 200
            };
            AddContent(label, button);
            button.Released += OnButtonReleaseEvent;


            void OnButtonReleaseEvent(object sender, EventArgs args)
            {
                //if (holdEvent != null) holdEvent.EndEvent();
                
            }

            Point GetAbsolutePosition(VisualElement element)
            { 
                var y = element.Y;
                var x = element.X;
                var parent = (VisualElement)element.Parent;
                while (parent != null && parent.Parent.GetType() == typeof(VisualElement)) { y += parent.Y; x += parent.X; parent = (VisualElement)parent.Parent; }
                return new Point(x, y);
            }

            void AddContent(params View[] element)
            {
                foreach (View v in element)
                {
                    Content = layout;
                    layout.Children.Add(v);
                }
            }
        }
    }
}

