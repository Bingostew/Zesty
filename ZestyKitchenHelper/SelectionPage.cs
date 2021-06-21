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
        public class TestCarousel
        {
            public string Image { get; set; }
            public string Name { get; set; }
        }
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

            var carouselView = new CarouselView();
            carouselView.ItemsSource = new List<View>()
            { new Frame(){ Content = new StackLayout(){ Children = { new Button()} } } };
            carouselView.ItemTemplate = new DataTemplate(() =>
            {
                Frame frame = new Frame();
                frame.HeightRequest = 200;
                frame.WidthRequest = 200;
                frame.BackgroundColor = Color.DarkGoldenrod;
                frame.SetBinding(ContentView.ContentProperty, "Content");

                return frame;
            }
            );

            Button carouselButton = new Button();
            carouselButton.Clicked += (o, a) => ContentManager.pageController.Content = carouselView;

            List<View> gridChildren = new List<View>(){ cabinetLabel, cabinetButton, fridgeLabel, fridgeButton, unplacedLabel, addUnplaceButton, 
                new Label() { Text = "Carousel Test" }, carouselButton };
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
            Content = grid;
        }
    }
}
