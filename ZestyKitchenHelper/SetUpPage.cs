﻿using System;
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

        StackLayout content;
        Label title;
        public SetUpPage()
        {
            title = new Label() { Text = "Welcome To Zesty", FontSize = 25, FontFamily = "Oswald-Regular", TextColor = Color.Black, HorizontalOptions = LayoutOptions.CenterAndExpand };
            var accountName = new Label() { Text = "Account Name", FontSize = 20, FontFamily = "Raleway-Regular", TextColor = Color.Black, 
                HorizontalOptions = LayoutOptions.CenterAndExpand, VerticalOptions = LayoutOptions.CenterAndExpand, Margin = new Thickness(side_margin,0, 0, 0)};
            var accountInput = new Entry() { Placeholder = "Star Chef", PlaceholderColor = Color.Gray, WidthRequest = ContentManager.screenWidth / 2, Margin = new Thickness(0,0,side_margin,0) };
            var confirmButton = new Button() { Text = "Confirm", FontFamily = "Oswald-Regular", FontSize = 20, TextColor = Color.White, Margin = new Thickness(side_margin, vert_margin), BackgroundColor = Color.WhiteSmoke };
            confirmButton.Clicked += async (obj, arg) =>
            {
                if (ContentManager.isLocal)
                {
                    ContentManager.sessionUserProfile = new UserProfile()
                    {
                        Name = accountInput.Text,
                        IconImage = ContentManager.addIcon,
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
                        IconImage = ContentManager.addIcon,
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
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                WidthRequest = ContentManager.screenWidth,
                Children =
                {
                    title,
                    new StackLayout()
                    {
                        Orientation = StackOrientation.Horizontal,
                        Children =
                        {
                            accountName, accountInput
                        }
                    },
                    confirmButton
                }
            };

            Content = content;
        }
    }
}