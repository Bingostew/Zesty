using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace ZestyKitchenHelper
{
    public class SetUpPage : ContentPage
    {
        private const int side_margin = 30;
        private const int vert_margin = 20;
        private const int title_top_margin = 100;
        private const double name_input_height_proportional = 0.07;

        StackLayout content;
        Label title;
        public SetUpPage()
        {
            title = new Label() { Text = "Welcome To Zesty", FontSize = 25, FontFamily = "Oswald-Regular", TextColor = Color.Black, 
                HorizontalOptions = LayoutOptions.CenterAndExpand, VerticalOptions = LayoutOptions.StartAndExpand, Margin = new Thickness(0, title_top_margin, 0, 0) };
            var accountName = new Label() { Text = "Username", FontSize = 18, FontFamily = "Raleway-Regular", TextColor = Color.Black, HorizontalOptions = LayoutOptions.CenterAndExpand, VerticalTextAlignment = TextAlignment.Center};
            var accountInput = new Entry() { Placeholder = "Star Chef", PlaceholderColor = Color.Gray, WidthRequest = ContentManager.screenWidth / 2};
            var confirmButton = new Button() { Text = "Confirm", FontFamily = "Oswald-Regular", FontSize = 20, TextColor = Color.Black, Margin = new Thickness(side_margin, vert_margin), 
                BackgroundColor = Color.WhiteSmoke, CornerRadius = 5, BorderColor = Color.Black, BorderWidth = 1};
            confirmButton.Clicked += async (obj, arg) =>
            {
                if (ContentManager.isLocal)
                {
                    var name = accountInput.Text == null ? "Zesty Chef" : accountInput.Text;
                    ContentManager.sessionUserProfile = new UserProfile()
                    {
                        Name = name,
                        IconImage = ContentManager.ProfileIcons[0],
                        IsLocal = true
                    };
                    LocalStorageController.AddUser(ContentManager.sessionUserProfile);
                    LocalStorageController.SetMetaUserInfo(new MetaUserInfo(true));
                }
                else
                {
                    ContentManager.sessionUserProfile.Name = accountInput.Text;
                    var remoteUser = new UserProfile()
                    {
                        Name = accountInput.Text,
                        Email = ContentManager.sessionUserProfile.Email,
                        IconImage = ContentManager.ProfileIcons[0],
                        IsLocal = false
                    };
                    await FireBaseController.AddUser(remoteUser);

                    // Need to add remote user to local file to ensure notification sends warning w/ correct user info
                    LocalStorageController.AddUser(remoteUser);
                    LocalStorageController.SetMetaUserInfo(new MetaUserInfo(false));
                }
                await content.QuadraticFlight(10, 90, -80, 100, t => content.TranslationY = t.Y, 1500);
                ContentManager.pageController.ToMainPage();

            };

            content = new StackLayout()
            {
                WidthRequest = ContentManager.screenWidth,
                BackgroundColor = ContentManager.ThemeColor,
                Children =
                {
                    title,
                    new StackLayout()
                    {
                        Orientation = StackOrientation.Horizontal,
                        HorizontalOptions = LayoutOptions.CenterAndExpand,
                        VerticalOptions = LayoutOptions.CenterAndExpand,
                        HeightRequest = ContentManager.screenHeight * name_input_height_proportional,
                        Margin = new Thickness(side_margin, ContentManager.screenHeight / 2 - title_top_margin - (vert_margin * 3), side_margin, 0),
                        Children =
                        {
                            accountName, accountInput
                        }
                    },
                    confirmButton
                }
            };

            Content = new StackLayout() { BackgroundColor = ContentManager.ThemeColor, Children = { content } };
        }
    }
}