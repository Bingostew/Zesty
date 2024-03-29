﻿using System;
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
    public class SingleSelectionPage : ContentPage, IMainPage
    {
        private const int spacing = 5;
        private const int storage_name_margin = 5;
        private const int button_radius = 5;
        private double grid_cell_width;
        private double add_view_button_width;
        private double change_name_field_height;

        private ScrollView scrollView;
        private Grid mainGrid;
        private ImageButton newButton;
        private AbsoluteLayout content;
        private ContentManager.StorageSelection currentStorageSelection;

        private Action<string> deleteStorageLocal, deleteStorageBase;

        public SingleSelectionPage(Action<string> _deleteStorageLocal, Action<string> _deleteStorageBase, ContentManager.StorageSelection storageType)
        {
            deleteStorageLocal = _deleteStorageLocal;
            deleteStorageBase = _deleteStorageBase;
            currentStorageSelection = storageType;
            // Calculate sizes
            grid_cell_width = (ContentManager.screenWidth / 2) - (spacing / 3);
            add_view_button_width = grid_cell_width / 3;
            change_name_field_height = grid_cell_width / 6;

            string title = storageType == ContentManager.StorageSelection.cabinet ? "My Pantry" : "My Fridge";
            var titleGrid = new TopPage(title, useReturnButton:false).GetGrid();
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

            scrollView = new ScrollView()
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Always,
                Content = mainGrid
            };

            UpdateLayout();

            content = new AbsoluteLayout();
            content.Children.Add(titleGrid, new Rectangle(0, 0, 1, 0.1), AbsoluteLayoutFlags.All);
            content.Children.Add(scrollView, new Rectangle(0, 1, 1, 0.9), AbsoluteLayoutFlags.All);
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

        Dictionary<string, List<View>> mainGridChildren = new Dictionary<string, List<View>>();
        public void UpdateLayout()
        {
            mainGridChildren.Clear(); // Need to clear children to prevent the same children to be assigned to two different views, causing invisibility bugs
            mainGrid.Children.Clear();
            var itemBase = new List<string>(); 
            var expiredStorages = new List<string>();
            var expiredItems = new List<int>();
            if (currentStorageSelection == ContentManager.StorageSelection.cabinet) {
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
                var model = ContentManager.GetStorageView(currentStorageSelection, key);
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
                    BackgroundColor = Color.WhiteSmoke,
                    Text = "Add",
                    CornerRadius = button_radius,
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
                    BackgroundColor = Color.WhiteSmoke,
                    Text = "View",
                    BorderColor = Color.Black,
                    BorderWidth = 2,
                    CornerRadius = button_radius,
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
                    Text = "X",
                    FontAttributes = FontAttributes.Bold,
                    BorderWidth = 2,
                    TextColor = Color.WhiteSmoke,
                    BackgroundColor = Color.Transparent,
                    FontSize = 20,
                    FontFamily = "oswald-medium",
                    Padding = 0,
                    WidthRequest = 30,
                    HeightRequest = 30,
                    HorizontalOptions = LayoutOptions.EndAndExpand,
                    VerticalOptions = LayoutOptions.StartAndExpand,
                    IsVisible = false
                };
                var changeNameButton = new ImageButton()
                {
                    Source = ContentManager.changeNameIcon,
                    BackgroundColor = Color.Transparent,
                    HeightRequest = change_name_field_height,
                    WidthRequest = change_name_field_height,
                    Aspect = Aspect.AspectFill,
                    Padding = 0,
                    HorizontalOptions = LayoutOptions.EndAndExpand,
                    VerticalOptions = LayoutOptions.StartAndExpand,
                    TranslationX = -40,
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
                            var gridChildrenList = mainGridChildren.Values.ToList();
                            gridChildrenList.Insert(0, new List<View>() { newButton });
                            // Re-layout grid after deletion
                             mainGrid.OrganizeGrid(gridChildrenList, GridOrganizer.OrganizeMode.HorizontalLeft);  
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
                    button.ToggleEffects(new ImageTint() { tint = Color.FromRgba(0,0,0,180), ImagePath = ContentManager.buttonTintImage  }, new List<VisualElement>() { addButton, viewButton, deleteButton, changeNameButton});

               
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
            }
            var gridChildren = mainGridChildren.Values.ToList();
            gridChildren.Insert(0, new List<View>() { newButton });
            mainGrid.OrganizeGrid(gridChildren, GridOrganizer.OrganizeMode.HorizontalLeft);
            Console.WriteLine("SIngle Selection 275 main grid children length " + mainGridChildren.Values.Count);
        }
    }
}
