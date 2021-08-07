using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Xamarin.Forms;
using Utility;
namespace ZestyKitchenHelper
{
    public class SingleSelectionPage : ContentPage
    {
        private const int spacing = 5;
        private const int storage_name_margin = 5;
        private double grid_cell_width;
        private double add_view_button_width;
        private double change_name_field_height;

        private List<View> newSelectionButton = new List<View>();
        private ScrollView scrollView;
        private Grid mainGrid;
        private ImageButton newButton;

        private List<List<View>> gridList = new List<List<View>>();

        private Action<string> deleteStorageLocal, deleteStorageBase;

        public SingleSelectionPage(Action<string> _deleteStorageLocal, Action<string> _deleteStorageBase)
        {
            deleteStorageLocal = _deleteStorageLocal;
            deleteStorageBase = _deleteStorageBase;

            // Calculate sizes
            grid_cell_width = (ContentManager.screenWidth / 2) - (spacing / 3);
            add_view_button_width = grid_cell_width / 3;
            change_name_field_height = grid_cell_width / 8;

            string title = ContentManager.GetStorageType() == ContentManager.cabinetStorageType ? "My Pantry" : "My Fridge";
            var titleGrid = new TopPage(title).GetGrid();
            titleGrid.HeightRequest = ContentManager.screenHeight * TopPage.top_bar_height_proportional;
            
            mainGrid = new Grid()
            {
                RowSpacing = spacing,
                ColumnSpacing = spacing,
                Margin = new Thickness(spacing),
                RowDefinitions =
                {
                    new RowDefinition(){Height = grid_cell_width },
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition(),
                    new ColumnDefinition()
                }
            };

            newButton = new ImageButton() { Source = ContentManager.addIcon, Aspect = Aspect.Fill, BackgroundColor = Color.Transparent };
            newButton.Clicked += (obj, args) => ContentManager.pageController.ToStorageCreationPage(true);
            newSelectionButton.Add(newButton);

            scrollView = new ScrollView()
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Always,
                Content = mainGrid
            };

            SetView();

            Content = new StackLayout()
            {
                Children =
                {
                    titleGrid,
                    scrollView
                }
            };
        }


       Dictionary<string,List<View>> mainGridChildren = new Dictionary<string, List<View>>();
        public void SetView()
        {
            mainGrid.Children.Clear();
            gridList.Clear();
            gridList.Add(newSelectionButton);
            var itemBase = new List<string>(); 
            var expiredStorages = new List<string>();
            var expiredItems = new List<int>();
            if (ContentManager.storageSelection == ContentManager.StorageSelection.cabinet) {
                itemBase = ContentManager.CabinetMetaBase.Keys.ToList();
                ContentManager.GetItemExpirationInfo(expiredStorages, null, expiredItems);

            }
            else
            {
                itemBase = ContentManager.FridgeMetaBase.Keys.ToList();
                ContentManager.GetItemExpirationInfo(null, expiredStorages, expiredItems);
            }
       
            foreach (var key in itemBase)
            {
                var metaName = key;
                var name = new Label() { Text = key.ToString(), TextColor = Color.Black, FontSize = 25, Margin = new Thickness(0, storage_name_margin),
                    HorizontalTextAlignment = TextAlignment.Center };
                var model = ContentManager.GetStorageView(key);
                var button = new ImageButton() { Source = ContentManager.transIcon, Aspect = Aspect.Fill, BorderColor = Color.Black, 
                    BorderWidth = 1, BackgroundColor = Color.Transparent };
                AbsoluteLayout preview = new AbsoluteLayout()
                {
                    HorizontalOptions = LayoutOptions.Center,
                };
                preview.Children.Add(name, new Rectangle(.5, 0, 1, .2), AbsoluteLayoutFlags.All);
                preview.Children.Add(model, new Rectangle(0, 1, 1, .8), AbsoluteLayoutFlags.All);

                var addButton = new Button()
                {
                    BackgroundColor = Color.Red,
                    Text = "Add",
                    TextColor = Color.Black,
                    WidthRequest = add_view_button_width,
                    HeightRequest = 40,
                    TranslationX = -add_view_button_width / 3 * 2,
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    IsVisible = false
                };

                var viewButton = new Button()
                {
                    BackgroundColor = Color.Red,
                    Text = "View",
                    TextColor = Color.Black,
                    WidthRequest = add_view_button_width,
                    HeightRequest = 40,
                    TranslationX = add_view_button_width / 3 * 2,
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    IsVisible = false
                };

                var deleteButton = new Button()
                {
                    BackgroundColor = Color.Red,
                    Text = "X",
                    TextColor = Color.White,
                    WidthRequest = 30,
                    HeightRequest = 30,
                    HorizontalOptions = LayoutOptions.EndAndExpand,
                    VerticalOptions = LayoutOptions.StartAndExpand,
                    IsVisible = false
                };
                var changeNameButton = new Button()
                {
                    BackgroundColor = Color.DeepSkyBlue,
                    Text = "Rename",
                    TextColor = Color.Black,
                    HeightRequest = change_name_field_height,
                    WidthRequest = 60,
                    HorizontalOptions = LayoutOptions.EndAndExpand,
                    VerticalOptions = LayoutOptions.StartAndExpand,
                    TranslationX = -30,
                    IsVisible = false
                };
                var changeNameField = new Entry()
                {
                    VerticalOptions = LayoutOptions.StartAndExpand,
                    BackgroundColor =Color.White,
                    HeightRequest = change_name_field_height,
                    ScaleX = 0,
                    IsEnabled = false
                };

                deleteButton.Clicked += (obj, args) =>
                {
                    ContentManager.pageController.ShowAlert("Caution", "Are you sure you want to delete this layout? All of its items will be unplaced.", "Delete", "Cancel",
                        () =>
                        {
                            if (ContentManager.isLocal)
                                deleteStorageLocal?.Invoke(key);
                            else
                                deleteStorageBase?.Invoke(key);


                            foreach (var cell in ContentManager.GetSelectedStorage(key).GetGridCells())
                            {
                                foreach (var child in cell.GetChildren())
                                {
                                    if (child.GetType() == typeof(ItemLayout))
                                    {
                                        Item item = (child as ItemLayout).ItemData;
                                        item.RemoveFromStorage();
                                        ContentManager.UnplacedItemBase.Add(item.ID, (ItemLayout)child);
                                    }
                                }
                            }

                            ContentManager.RemoveSelectedStorage(key);
                            mainGrid.Children.Clear();
                            mainGridChildren.Remove(key);
                            // Re-layout grid after deletion
                            var mainGridChildrenList = mainGridChildren.Values.ToList();
                            mainGridChildrenList.Insert(0, new List<View>() { newButton });
                            GridOrganizer.OrganizeGrid(mainGrid, mainGridChildrenList, GridOrganizer.OrganizeMode.HorizontalLeft);  
                        },
                        () => { });
                };
                changeNameButton.Clicked += async (obj, args) =>
                {
                    changeNameField.IsEnabled = true;
                    await ViewExtensions.LinearInterpolator(changeNameField, 1, 100, i => changeNameField.ScaleX = i);
                    changeNameField.Focus();
                };
                void onNameChanged()
                {
                    if (changeNameField.Text != null && !itemBase.Contains(changeNameField.Text))
                    {
                        var itemStorage = ContentManager.GetSelectedStorage(metaName);
                        ContentManager.RemoveSelectedStorage(metaName);
                        metaName = changeNameField.Text;
                        ContentManager.AddSelectedStorage(metaName, itemStorage);
                        name.Text = metaName;
                    }
                    changeNameField.ScaleX = 0;
                    changeNameField.IsEnabled = false;
                }
                changeNameField.Completed += (obj, args) => onNameChanged();
                changeNameField.Unfocused += (obj, args) => onNameChanged();
                addButton.Clicked += (obj, args) => ContentManager.pageController.ToAddItemPage(metaName);
                viewButton.Clicked += (obj, args) => ContentManager.pageController.ToViewItemPage(metaName);
                button.Clicked += (object obj, EventArgs args) =>
                    button.ToggleEffects(new ImageTint() { tint = Color.FromHsla(1, .1, .5, .5), ImagePath = ContentManager.buttonTintImage  }, new List<VisualElement>() { addButton, viewButton, deleteButton, changeNameButton});

               
                List<View> views = new List<View>() { preview, button, addButton, viewButton, deleteButton, changeNameButton, changeNameField };
                //expiration warning and animation
                if (expiredStorages.Contains(key))
                {
                    var expWarningImage = new Image()
                    {
                        Source = ContentManager.expWarningIcon,
                        WidthRequest = ContentManager.exp_warning_size,
                        HeightRequest = ContentManager.exp_warning_size,
                        HorizontalOptions = LayoutOptions.Start,
                        VerticalOptions = LayoutOptions.End,
                    };
                    views.Add(expWarningImage);
                    expWarningImage.QuadraticInterpolator(1.3, 2000, (t) => { if (t >= 1) { expWarningImage.Scale = t; } }, null, true);
                }
                if (!mainGridChildren.ContainsKey(metaName)) mainGridChildren.Add(metaName, views);
                gridList.Insert(1, views);
            }
            mainGrid.OrganizeGrid(gridList, GridOrganizer.OrganizeMode.HorizontalLeft);
        }
    }
}
