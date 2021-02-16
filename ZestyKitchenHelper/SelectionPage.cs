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
        public ImageButton cabinetButton = new ImageButton()
        {
            Source = ContentManager.pantryIcon,
            Aspect = Aspect.AspectFill,
            WidthRequest = 300, HeightRequest = 300
        };
        public ImageButton fridgeButton = new ImageButton()
        {
            Source = ContentManager.refridgeIcon,
            Aspect = Aspect.AspectFill,
            WidthRequest = 300,
            HeightRequest = 300
        };
        public ImageButton addUnplaceButton = new ImageButton()
        {
            Source = ContentManager.addIcon
        };
        

        public SelectionPage(EventHandler navigateUnplaceEvent)
        {
            Grid grid = new Grid()
            {
                BackgroundColor = Color.Wheat,
                RowDefinitions =
                {
                    new RowDefinition(),
                    new RowDefinition(),
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition(),
                    new ColumnDefinition()
                }
            };

            grid.Children.Add(cabinetButton, 0, 0);
            grid.Children.Add(fridgeButton, 1, 0);
            grid.Children.Add(addUnplaceButton, 0, 1);

            void SetSelection(ContentManager.StorageSelection selection)
            {
                ContentManager.storageSelection = selection;
            }

            cabinetButton.Clicked += (obj, args) => SetSelection(ContentManager.StorageSelection.cabinet); 
            fridgeButton.Clicked += (obj, args) => SetSelection(ContentManager.StorageSelection.fridge);
            addUnplaceButton.Clicked += navigateUnplaceEvent;
            Content = grid;
        }
    }
}
