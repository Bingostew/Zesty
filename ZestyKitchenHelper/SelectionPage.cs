using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Utility;
using Xamarin.Forms;
using ZXing.Mobile;
using ZXing.Net.Mobile.Forms;

namespace ZestyKitchenHelper
{
    public class SelectionPage : ContentPage
    {
        private const int grid_margin = 10;
        private const string main_label_font = "Raleway_Regular";

        StackLayout content;
        private ImageButton cabinetButton = new ImageButton()
        {
            Margin = new Thickness(0, grid_margin),
            Source = ContentManager.pantryIcon,
            Aspect = Aspect.AspectFill,
            BackgroundColor = Color.Transparent,
            WidthRequest = 300, HeightRequest = 300
        };
        private ImageButton fridgeButton = new ImageButton()
        {
            Margin = new Thickness(0, grid_margin),
            Source = ContentManager.refridgeIcon,
            Aspect = Aspect.AspectFill,
            BackgroundColor = Color.Transparent,
            WidthRequest = 300,
            HeightRequest = 300
        };
        private ImageButton addUnplaceButton = new ImageButton()
        {
            Margin = new Thickness(0, grid_margin),
            Source = ContentManager.addIcon,
            BackgroundColor = Color.Transparent
        };
        private Label cabinetLabel = new Label()
        {
            Margin = new Thickness(grid_margin),
            FontSize = 30,
            TextColor = Color.Black,
            HorizontalOptions = LayoutOptions.Center,
            Text = "My Pantries",
            FontFamily = main_label_font
        };
        private Label fridgeLabel = new Label()
        {
            Margin = new Thickness(grid_margin),
            FontSize = 30,
            TextColor = Color.Black,
            HorizontalOptions = LayoutOptions.Center,
            Text = "My Fridges",
            FontFamily = main_label_font
        };
        private Label unplacedLabel = new Label()
        {
            Margin = new Thickness(grid_margin),
            FontSize = 30,
            HorizontalOptions = LayoutOptions.Center,
            TextColor = Color.Black,
            Text = "All Items",
            FontFamily = main_label_font
        };



        public SelectionPage()
        {
            var titleGrid = new TopPage("Main Page", null, true, false).GetGrid();
            titleGrid.HeightRequest = ContentManager.screenHeight * TopPage.top_bar_height_proportional;

            Grid grid = new Grid()
            {
                BackgroundColor = ContentManager.ThemeColor,
                Margin = new Thickness(grid_margin),
                RowDefinitions =
                {
                    new RowDefinition(){Height = GridLength.Auto },
                    new RowDefinition(){Height = GridLength.Star },
                    new RowDefinition(){Height = GridLength.Auto },
                    new RowDefinition(){Height = GridLength.Star },
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition(),
                    new ColumnDefinition()
                }
            };
            ContentManager.AddOnBackgroundChangeListener(c => grid.BackgroundColor = c);
            List<View> gridChildren = new List<View>(){ cabinetLabel, cabinetButton, fridgeLabel, fridgeButton, unplacedLabel, addUnplaceButton, 
                new Label() { Text = "Testing Page" }};
            grid.OrganizeGrid(gridChildren, GridOrganizer.OrganizeMode.TwoRowSpanLeft);

            void SetSelection(ContentManager.StorageSelection selection)
            {
                ContentManager.storageSelection = selection;
            }

            cabinetButton.Clicked += (obj, args) => SetSelection(ContentManager.StorageSelection.cabinet);
            cabinetButton.Clicked += (obj, args) => ContentManager.pageController.ToSingleSelectionPage();
            fridgeButton.Clicked += (obj, args) => SetSelection(ContentManager.StorageSelection.fridge);
            fridgeButton.Clicked += (obj, args) => ContentManager.pageController.ToSingleSelectionPage();
            addUnplaceButton.Clicked += (o,a) => ContentManager.pageController.ToUnplacedPage();

            content = new StackLayout()
            {
                WidthRequest = ContentManager.screenWidth,
                HeightRequest = ContentManager.screenHeight,
                Children =
                {
                    titleGrid, grid
                }
            };
            Content = content;
        }
    }
}
