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
        private Image expWarningImage1 = new Image()
        {
            IsVisible = false,
            Source = ContentManager.expWarningIcon,
            WidthRequest =  ContentManager.exp_warning_size,
            HeightRequest =  ContentManager.exp_warning_size,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Start,
        };
        private Image expWarningImage2 = new Image()
        {
            IsVisible = false,
            Source = ContentManager.expWarningIcon,
            WidthRequest =  ContentManager.exp_warning_size,
            HeightRequest =  ContentManager.exp_warning_size,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Start
        };
        private Image expWarningImage3 = new Image()
        {
            IsVisible = false,
            Source =ContentManager.expWarningIcon,
            WidthRequest =  ContentManager.exp_warning_size,
            HeightRequest =  ContentManager.exp_warning_size,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Start
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

            // Check if any items in the given categories are expired. If so, show expiration warning.
            grid.Children.Add(expWarningImage1, 0, 1);
            grid.Children.Add(expWarningImage2, 1, 1);
            grid.Children.Add(expWarningImage3, 0, 3);
            var expiredCabinets = new List<string>();
            var expiredFridges = new List<string>();
            var expiredItems = new List<int>();
            ContentManager.GetItemExpirationInfo(expiredCabinets, expiredFridges, expiredItems);

            if (expiredCabinets.Count > 0)
            {
                expWarningImage1.IsVisible = true;
                AnimateExpirationWarning(expWarningImage1);
            }
            if (expiredFridges.Count > 0)
            {
                expWarningImage2.IsVisible = true;
                AnimateExpirationWarning(expWarningImage2);
            }
            if (expiredItems.Count > 0)
            {
                expWarningImage3.IsVisible = true;
                AnimateExpirationWarning(expWarningImage3);
            }

            
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

        private void AnimateExpirationWarning(View view)
        {
            view.QuadraticInterpolator(1.3, 2000, (t) => { if (t >= 1) { view.Scale = t; } }, null, true);
        }
    }
}
