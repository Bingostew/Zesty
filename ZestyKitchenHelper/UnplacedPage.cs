using System;
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
    public class UnplacedPage : ContentPage, IMainPage
    {
        private const int side_margin = 5;
        private const int between_margin = 5;

        public SearchBar searchAllBar;
        private ScrollView gridScroll;
        private static Grid metaGrid;
        const string expIndicatorString = "Expiration Date";
        const string alphaIndicatorString = "A-Z";

        private AbsoluteLayout content;
        public UnplacedPage(Action<Item> localUnplacedEvent, Action<Item> baseUnplaceEvent, Action<Item> deleteItemLocal, Action<Item> deleteItemBase)
        {
            var titleGrid = new TopPage("Items", useReturnButton: false).GetGrid();
            var addNewButton = new ImageButton() { Source = ContentManager.addIcon, BackgroundColor = Color.Transparent, Margin = new Thickness(side_margin, between_margin) };
            // Renewing contents in meta grid
            metaGrid = GridManager.GetGrid(ContentManager.metaGridName);
            GridManager.AddGridItem(metaGrid, ContentManager.MetaItemBase.Values, true);

            var addView = new AddView(localUnplacedEvent, baseUnplaceEvent, "", false);
            searchAllBar = new SearchBar() { Margin = new Thickness(side_margin, 0) };
            searchAllBar.Text = ContentManager.defaultSearchAllBarText;
            searchAllBar.TextColor = Color.Black;
            searchAllBar.Focused += (obj, args) => searchAllBar.Text = "";
            searchAllBar.Unfocused += (obj, args) => { if (searchAllBar.Text.Length == 0) searchAllBar.Text = ContentManager.defaultSearchAllBarText; };
            searchAllBar.Unfocused += (obj, args) => GridManager.FilterItemGrid(ContentManager.MetaItemBase.Values, metaGrid, searchAllBar.Text);
            addNewButton.Clicked += (obj, args) => { addView.ResetForm(); ContentManager.pageController.ToAddView(addView); };

            var sortSelectorIcon = new Image() { Source = ContentManager.sortIcon };
            var sortSelector = new Picker()
            {
                Margin = new Thickness(side_margin, between_margin),
                ItemsSource = new List<string>() { expIndicatorString, alphaIndicatorString },
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
                Margin = new Thickness(side_margin),
                VerticalScrollBarVisibility = ScrollBarVisibility.Always,
                HeightRequest = Height * 0.8,
                Content = metaGrid
            };

            AbsoluteLayout.SetLayoutBounds(titleGrid, new Rectangle(0, 0, 1, TopPage.top_bar_height_proportional));
            AbsoluteLayout.SetLayoutFlags(titleGrid, AbsoluteLayoutFlags.All);
            AbsoluteLayout.SetLayoutBounds(searchAllBar, new Rectangle(0, 0.13, 0.8, .1));
            AbsoluteLayout.SetLayoutFlags(searchAllBar, AbsoluteLayoutFlags.All);
            AbsoluteLayout.SetLayoutBounds(addNewButton, new Rectangle(0, .25, 100,100));
            AbsoluteLayout.SetLayoutFlags(addNewButton, AbsoluteLayoutFlags.PositionProportional);
            AbsoluteLayout.SetLayoutBounds(sortSelector, new Rectangle(1, 0.13, .2, .1));
            AbsoluteLayout.SetLayoutFlags(sortSelector, AbsoluteLayoutFlags.All);
            AbsoluteLayout.SetLayoutBounds(sortSelectorIcon, AbsoluteLayout.GetLayoutBounds(sortSelector));
            AbsoluteLayout.SetLayoutFlags(sortSelectorIcon, AbsoluteLayoutFlags.All);
            AbsoluteLayout.SetLayoutBounds(gridScroll, new Rectangle(0, 1, 1, .625));
            AbsoluteLayout.SetLayoutFlags(gridScroll, AbsoluteLayoutFlags.All);
            AbsoluteLayout.SetLayoutBounds(addView, new Rectangle(0, 0, 1, 1));
            AbsoluteLayout.SetLayoutFlags(addView, AbsoluteLayoutFlags.All);
            content = new AbsoluteLayout()
            {
                Children =
                {
                    titleGrid,
                    searchAllBar,
                    addNewButton,
                    sortSelectorIcon,
                    sortSelector,
                    gridScroll
                }
            };

            Content = content;
        }

        public AbsoluteLayout GetLayout()
        {
            return content;
        }
        public void SetLayout(AbsoluteLayout layout)
        {
            content = layout;
            Content = content;
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            GridManager.FilterItemGrid(ContentManager.MetaItemBase.Values, metaGrid, "");
        }
        public void UpdateLayout()
        {
            metaGrid = GridManager.GetGrid(ContentManager.metaGridName);
            GridManager.AddGridItem(metaGrid, ContentManager.MetaItemBase.Values, true);
        }
        }
}
