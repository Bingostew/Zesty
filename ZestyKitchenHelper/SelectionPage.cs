using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Utility;
using Xamarin.Forms;

namespace ZestyKitchenHelper
{
    public class SelectionPage : ContentPage
    {
        private ImageButton cabinetButton = new ImageButton()
        {
            Source = ContentManager.pantryIcon,
            Aspect = Aspect.AspectFill,
            BackgroundColor = Color.Transparent,
            WidthRequest = 300, HeightRequest = 300
        };
        private ImageButton fridgeButton = new ImageButton()
        {
            Source = ContentManager.refridgeIcon,
            Aspect = Aspect.AspectFill,
            BackgroundColor = Color.Transparent,
            WidthRequest = 300,
            HeightRequest = 300
        };
        private ImageButton addUnplaceButton = new ImageButton()
        {
            Source = ContentManager.addIcon,
            BackgroundColor = Color.Transparent
        };
        private Label cabinetLabel = new Label()
        {
            FontSize = 30,
            TextColor = Color.Black,
            HorizontalOptions = LayoutOptions.Center,
            Text = "My Pantries"
        };
        private Label fridgeLabel = new Label()
        {
            FontSize = 30,
            TextColor = Color.Black,
            HorizontalOptions = LayoutOptions.Center,
            Text = "My Fridges"
        };
        private Label unplacedLabel = new Label()
        {
            FontSize = 30,
            HorizontalOptions = LayoutOptions.Center,
            TextColor = Color.Black,
            Text = "All Items"
        };


        public SelectionPage()
        {
            Grid grid = new Grid()
            {
                BackgroundColor = Color.Wheat,
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

            List<View> gridChildren = new List<View>(){ cabinetLabel, cabinetButton, fridgeLabel, fridgeButton, unplacedLabel, addUnplaceButton };
            grid.OrganizeGrid(gridChildren, GridOrganizer.OrganizeMode.TwoRowSpanLeft);

            void SetSelection(ContentManager.StorageSelection selection)
            {
                ContentManager.storageSelection = selection;
            }

            cabinetButton.Clicked += (obj, args) => SetSelection(ContentManager.StorageSelection.cabinet);
            cabinetButton.Clicked += (obj, args) => ContentManager.pageController.InitializeSingleSelectionPage(ContentManager.StorageSelection.cabinet);
            fridgeButton.Clicked += (obj, args) => SetSelection(ContentManager.StorageSelection.fridge);
            fridgeButton.Clicked += (obj, args) => ContentManager.pageController.InitializeSingleSelectionPage(ContentManager.StorageSelection.fridge);
            addUnplaceButton.Clicked += (o,a) => ContentManager.pageController.ToUnplacedPage();
            Content = grid;
        }
    }
}
