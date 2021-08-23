using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace ZestyKitchenHelper
{
    public class TopPage
    {
        private const int border_width = 2;
        private const int title_font_size = 20;
        public const double top_bar_height_proportional = 0.1;

        PreferencePage preferencePage;
        Grid grid;
        Label pageTitle;
        Label usernameLabel;
        ImageButton profileIcon;
        public TopPage(string title, Action extraReturnAction = null, bool useLogo = false, bool useReturnButton = true)
        {
            preferencePage = new PreferencePage();
            var border = new Button() { IsEnabled = false, BackgroundColor = Color.Black, HeightRequest = border_width, WidthRequest = ContentManager.screenWidth };
            var backButton = new ImageButton() { Source = ContentManager.backButton, BackgroundColor = Color.Transparent };
            pageTitle = new Label() { Text = title, FontSize = title_font_size, FontFamily = "Oswald-Medium", TextColor = Color.Black, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
            var logo = new Image() { Source = ContentManager.addIcon};
            //usernameLabel = new Label(){ FontSize = title_font_size, FontFamily = "Raleway-Regular", TextColor = Color.Gray, HorizontalTextAlignment = TextAlignment.End};
            //usernameLabel.Text = ContentManager.sessionUserProfile != null ? ContentManager.sessionUserProfile.Name : "";
            //ContentManager.sessionUserProfile.AddOnProfileChangedListener(u => usernameLabel.Text = u.Name);
            profileIcon = new ImageButton() { Source = ContentManager.sessionUserProfile.IconImage, WidthRequest = 50, HeightRequest = 50, CornerRadius = 25, BackgroundColor = Color.Transparent };
            ContentManager.sessionUserProfile.AddOnProfileChangedListener(u => profileIcon.Source = u.IconImage);
            grid = new Grid()
            {
                RowDefinitions =
                {
                    new RowDefinition(){ },
                    new RowDefinition(){Height = border_width}
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition(){Width = GridLength.Star}, 
                    new ColumnDefinition(){Width = GridLength.Star},
                    new ColumnDefinition(){Width = new GridLength(2, GridUnitType.Star)},
                    new ColumnDefinition(){Width = GridLength.Star},
                    new ColumnDefinition(){Width = GridLength.Star}
                }
            };

            backButton.Clicked += (o, a) => { ContentManager.pageController.ReturnToPrevious(); extraReturnAction?.Invoke(); };
            profileIcon.Clicked += (o, a) => { ContentManager.pageController.ToPreferencePage(preferencePage); };

            if(useReturnButton)
                grid.Children.Add(backButton, 0, 0);
            if (!useLogo)
            {
                grid.Children.Add(pageTitle, 2, 0);
            }
            else
            {
                grid.Children.Add(logo, 2, 0);
            }
            grid.Children.Add(profileIcon, 4, 0);
            grid.Children.Add(border, 0, 1);
            Grid.SetColumnSpan(border, 5);
        }
        public Grid GetGrid()
        {
            return grid;
        }
    }
}