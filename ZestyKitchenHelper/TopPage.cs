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
        private const int title_font_size = 25;
        public const double top_bar_height_proportional = 0.1;

        Grid grid;
        Label pageTitle;
        public TopPage(string title, Action extraReturnAction = null)
        {
            var border = new Button() { IsEnabled = false, BackgroundColor = Color.Black, HeightRequest = border_width, WidthRequest = ContentManager.screenWidth };
            var backButton = new ImageButton() { Source = ContentManager.backButton };
            pageTitle = new Label() { Text = title, FontSize = title_font_size, TextColor = Color.Black };
            grid = new Grid()
            {
                RowDefinitions =
                {
                    new RowDefinition(){ },
                    new RowDefinition(){Height = border_width}
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition(){Width = GridLength.Star}, new ColumnDefinition(){Width = GridLength.Star},
                    new ColumnDefinition(){Width = GridLength.Star}, new ColumnDefinition(){Width = GridLength.Star},
                }
            };

            backButton.Clicked += (o, a) => { ContentManager.pageController.ReturnToPrevious(); extraReturnAction?.Invoke(); };

            grid.Children.Add(backButton, 0, 0);
            grid.Children.Add(pageTitle, 1, 0);
            grid.Children.Add(border, 0, 1);
            Grid.SetColumnSpan(pageTitle, 2);
            Grid.SetColumnSpan(border, 4);
        }

        public Grid GetGrid()
        {
            return grid;
        }
    }
}