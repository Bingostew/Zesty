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


            Button scrollViewTestButton = new Button();
            Grid parentGrid = new Grid()
            {
                RowDefinitions =
                {
                    new RowDefinition(){ },
                    new RowDefinition(){ }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition(),
                }
            };
            parentGrid.Children.Add(new Label() { Text = "Left Side" }, 0, 0);
            Grid scrollGrid = GridManager.InitializeGrid(1, 10, 300, 200);
            List<View> scrollGridChildren = new List<View>();
            for(int i = 0; i<10; i++)
            {
                Button button = new Button() { Text = i.ToString(), BackgroundColor = Color.RoyalBlue };
                scrollGridChildren.Add(button);
                
            }
            GridManager.AddGridItem(scrollGrid, scrollGridChildren, false);
            ScrollView scrollView = new ScrollView() { Content = scrollGrid, Orientation = ScrollOrientation.Horizontal };
            parentGrid.Children.Add(scrollView, 0, 1);
            AbsoluteLayout absLayout = new AbsoluteLayout() { Children = { parentGrid } };
            AbsoluteLayout.SetLayoutBounds(parentGrid, new Rectangle(0, 0, 1, 1));
            AbsoluteLayout.SetLayoutFlags(parentGrid, AbsoluteLayoutFlags.All);
            scrollViewTestButton.Clicked += (o, a) => ContentManager.pageController.Content = absLayout;

            List<View> gridChildren = new List<View>(){ cabinetLabel, cabinetButton, fridgeLabel, fridgeButton, unplacedLabel, addUnplaceButton, 
                new Label() { Text = "Testing Page" }, scrollViewTestButton };
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
