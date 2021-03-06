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
        private static Grid unplacedGrid;
        const string expIndicatorString = "Expiration Date";
        const string alphaIndicatorString = "A-Z";
        public UnplacedPage(Action<Item> localUnplacedEvent, Action<Item> baseUnplaceEvent, Action<Item> deleteItemLocal, Action<Item> deleteItemBase)
        {
            var returnButton = new ImageButton() { Source = ContentManager.backButton };
            returnButton.Clicked += (o,a) => ContentManager.pageController.ToMainSelectionPage();
            var addNewButton = new ImageButton() { Source = ContentManager.addIcon };
            unplacedGrid = AddView.InitializeNewGrid(4, -1);
            var addForm = AddView.GetAddForm(unplacedGrid, localUnplacedEvent, baseUnplaceEvent, "", false);
            searchAllBar.Text = ContentManager.defaultSearchAllBarText;
            searchAllBar.Focused += (obj, args) => searchAllBar.Text = "";
            searchAllBar.Unfocused += (obj, args) => { if (searchAllBar.Text.Length == 0) searchAllBar.Text = ContentManager.defaultSearchAllBarText; };
            searchAllBar.Unfocused += (obj, args) => ListSorter.OnSearchUnplacedGrid(unplacedGrid, searchAllBar.Text);
            var sortSelector = new Picker()
            {
                ItemsSource = new List<string>() { expIndicatorString, alphaIndicatorString },
                Title = "Sort Order",
            };
            sortSelector.SelectedIndexChanged += (obj, args) =>
            {
                unplacedGrid.SetGridChildrenList(unplacedGrid.Children.Cast<ItemLayout>().ToList());
                switch (sortSelector.SelectedItem)
                {
                    case expIndicatorString: GridOrganizer.SortItemGrid(unplacedGrid, GridOrganizer.SortingType.Expiration_Close); break;
                    case alphaIndicatorString: GridOrganizer.SortItemGrid(unplacedGrid, GridOrganizer.SortingType.A_Z); break;
                }
                unplacedGrid.SetGridChildrenList(unplacedGrid.Children.Cast<ItemLayout>().ToList());
            };


            addNewButton.Clicked += (obj, args) => { addForm.IsVisible = true; };
            gridScroll = new ScrollView()
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Always,
                Content = unplacedGrid
            };

            foreach(var placed in ContentManager.MetaItemBase.Values)
            {
                placed.SetMarkingVisibility(true);
                if (!ContentManager.UnplacedItems.Contains(placed.ItemData))
                    AddView.AddUnplacedGrid(unplacedGrid, null, null, placed, false);
            }
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

        public static void UpdateGrid(Item removed)
        {
            ItemLayout removedLayout = unplacedGrid.Children.Where(i => (i as ItemLayout).ItemData.ID == removed.ID).FirstOrDefault() as ItemLayout;
            AddView.RemoveUnplacedGrid(unplacedGrid, removedLayout);
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
