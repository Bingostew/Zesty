﻿using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Utility;
using System.Threading.Tasks;
using System.Linq;
using Xamarin.Forms.Internals;
using System.Timers;

namespace ZestyKitchenHelper
{
    public class UnplacedPage : ContentPage
    {

        public SearchBar searchAllBar = new SearchBar();
        private ScrollView gridScroll;
        private static Grid metaGrid;
        const string expIndicatorString = "Expiration Date";
        const string alphaIndicatorString = "A-Z";
        public UnplacedPage(Action<Item> localUnplacedEvent, Action<Item> baseUnplaceEvent, Action<Item> deleteItemLocal, Action<Item> deleteItemBase)
        {
            var returnButton = new ImageButton() { Source = ContentManager.backButton, BackgroundColor = Color.Transparent };
            returnButton.Clicked += (o,a) => ContentManager.pageController.ToMainSelectionPage();
            var addNewButton = new ImageButton() { Source = ContentManager.addIcon, BackgroundColor = Color.Transparent };
            metaGrid = GridManager.GetGrid(ContentManager.metaGridName); 
            var addForm = AddView.GetAddForm(localUnplacedEvent, baseUnplaceEvent, "", false);
            searchAllBar.Text = ContentManager.defaultSearchAllBarText;
            searchAllBar.TextColor = Color.Black;
            searchAllBar.Focused += (obj, args) => searchAllBar.Text = "";
            searchAllBar.Unfocused += (obj, args) => { if (searchAllBar.Text.Length == 0) searchAllBar.Text = ContentManager.defaultSearchAllBarText; };
            searchAllBar.Unfocused += (obj, args) => GridManager.FilterItemGrid(metaGrid, searchAllBar.Text);
            searchAllBar.TextChanged += (obj, args) => GridManager.FilterItemGrid(metaGrid, searchAllBar.Text);
            addNewButton.Clicked += (obj, args) => { addForm.IsVisible = true; };

            var sortSelector = new Picker()
            {
                ItemsSource = new List<string>() { expIndicatorString, alphaIndicatorString },
                Title = "Sort Order",
            };
            sortSelector.SelectedIndexChanged += (obj, args) =>
            {
                switch (sortSelector.SelectedItem)
                {
                    case expIndicatorString: GridOrganizer.SortItemGrid(metaGrid, GridOrganizer.ItemSortingMode.Expiration_Close); break;
                    case alphaIndicatorString: GridOrganizer.SortItemGrid(metaGrid, GridOrganizer.ItemSortingMode.A_Z); break;
                }
            };


            gridScroll = new ScrollView()
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Always,
                HeightRequest = Height * 0.8,
                Content = metaGrid
            };

            var pageWidth = Application.Current.MainPage.Width;
            AbsoluteLayout.SetLayoutBounds(returnButton, new Rectangle(0, 0, 70, 70));
            AbsoluteLayout.SetLayoutFlags(returnButton, AbsoluteLayoutFlags.PositionProportional);
            AbsoluteLayout.SetLayoutBounds(searchAllBar, new Rectangle(100, 10, pageWidth - 100, 40));
            AbsoluteLayout.SetLayoutFlags(searchAllBar, AbsoluteLayoutFlags.None);
            AbsoluteLayout.SetLayoutBounds(addNewButton, new Rectangle(.2, 50, 100, 50));
            AbsoluteLayout.SetLayoutFlags(addNewButton, AbsoluteLayoutFlags.XProportional);
            AbsoluteLayout.SetLayoutBounds(sortSelector, new Rectangle(.7, 50, 200, 50));
            AbsoluteLayout.SetLayoutFlags(sortSelector, AbsoluteLayoutFlags.XProportional);
            AbsoluteLayout.SetLayoutBounds(gridScroll, new Rectangle(0, 100, 1, .8));
            AbsoluteLayout.SetLayoutFlags(gridScroll, AbsoluteLayoutFlags.SizeProportional);
            AbsoluteLayout.SetLayoutBounds(addForm, new Rectangle(0, 0, 1, 1));
            AbsoluteLayout.SetLayoutFlags(addForm, AbsoluteLayoutFlags.All);
            Content = new AbsoluteLayout()
            {
                Children =
                {
                    returnButton,
                    searchAllBar,
                    addNewButton,
                    sortSelector,
                    gridScroll,
                    addForm
                }
            };
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            GridManager.FilterItemGrid(metaGrid, "");
        }
        public static void UpdateGrid(Item removed)
        {
            ItemLayout removedLayout = metaGrid.Children.Where(i => (i as ItemLayout).ItemData.ID == removed.ID).FirstOrDefault() as ItemLayout;
            GridManager.RemoveGridItem(metaGrid, removedLayout);
            /*
            if(removed != null)
            {
                var list = unplacedGrid.GetGridChilrenList() as List<View>;
                var removedLayout = list.Where(l => (l as ItemLayout).ItemData.ID == removed.ID).FirstOrDefault();
                if(removedLayout != null) list.Remove(removedLayout);
                unplacedGrid.SetGridChildrenList(list);
            }
            AddView.ChangePageUnplacedGrid(unplacedGrid, 0, -1, ""); */
        }

    }
}
