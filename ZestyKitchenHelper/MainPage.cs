using System;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ZestyKitchenHelper
{
    public class MainPage : ContentPage
    {
        private const int font_size = 20;
        private const float button_width_proportional = 0.5f;
        private const float button_height_proportional = 0.05f;
        private const int button_between_margin = 20;

        private Button localAccountButton, cloudAccountButton;
        public MainPage()
        {

            // Initialize screen width and height
            ContentManager.screenHeight = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;
            ContentManager.screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;

            localAccountButton = new Button()
            {
                FontFamily = "oswald-regular",
                Text = "Local Account",
                HorizontalOptions = LayoutOptions.Center,
                WidthRequest = ContentManager.screenWidth * button_width_proportional,
                HeightRequest = ContentManager.screenHeight * button_height_proportional,
                Margin = new Thickness(0, button_between_margin, 0 ,0),
                CornerRadius = 5,
                BorderColor = Color.Black,
                BorderWidth = 2,
                BackgroundColor = Color.Goldenrod,
                TextColor = Color.Black,
                FontSize = font_size
            };
            cloudAccountButton = new Button()
            {
                FontFamily = "oswald-regular",
                Text = "Cloud Account",
                CornerRadius = 5,
                BorderColor = Color.Black,
                WidthRequest = ContentManager.screenWidth * button_width_proportional,
                HeightRequest = ContentManager.screenHeight * button_height_proportional,
                Margin = new Thickness(0, (ContentManager.screenHeight - (button_between_margin + (2 * ContentManager.screenHeight * button_height_proportional))) / 2, 0, 0),
                HorizontalOptions = LayoutOptions.Center,
                BorderWidth = 2,
                BackgroundColor = Color.Goldenrod,
                TextColor = Color.Black,
                FontSize = font_size
            };


            Content = new StackLayout()
            { 
                HeightRequest = ContentManager.screenHeight,
                WidthRequest = ContentManager.screenWidth,
                VerticalOptions = LayoutOptions.Center,
                BackgroundColor = ContentManager.ThemeColor,
                Children =
                {
                    cloudAccountButton, localAccountButton
                }
            };
        }

        public void InitializeLogin(Action loginLocal, Action loginCloud)
        {
            localAccountButton.Clicked += (o, a) => { localAccountButton.IsEnabled = false; cloudAccountButton.IsEnabled = false; loginLocal(); };
            cloudAccountButton.Clicked += (o, a) => { cloudAccountButton.IsEnabled = false; localAccountButton.IsEnabled = false; loginCloud(); };
        }
    }
}
