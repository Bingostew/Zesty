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

        private List<View> newSelectionButton = new List<View>();
        private ScrollView scrollView;
        private Grid mainGrid;
        private ImageButton returnButton;

        private List<List<View>> gridList = new List<List<View>>();

        private Action<string> deleteStorageLocal, deleteStorageBase;

        public SingleSelectionPage(Action<string> _deleteStorageLocal, Action<string> _deleteStorageBase)
        {
            deleteStorageLocal = _deleteStorageLocal;
            deleteStorageBase = _deleteStorageBase;
            mainGrid = new Grid()
            {
                RowDefinitions =
                {
                    new RowDefinition(){Height = 200 },
                    new RowDefinition(){Height = 200}
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition(),
                    new ColumnDefinition()
                }
            };

            var newButton = new ImageButton() { Source = ContentManager.addIcon, Aspect = Aspect.Fill, BackgroundColor = Color.Transparent };
            newButton.Clicked += (obj, args) => ContentManager.pageController.ToStorageCreationPage(true);
            newSelectionButton.Add(newButton);
            returnButton = new ImageButton() { Source = ContentManager.backButton, WidthRequest = 80, HorizontalOptions = LayoutOptions.StartAndExpand };
            returnButton.Clicked += (o, a) => ContentManager.pageController.ToMainSelectionPage();

            scrollView = new ScrollView()
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Always,
                Content = mainGrid
            };

            scrollView.Scrolled += (obj, args) => Console.WriteLine("scrolled");

            SetView();

            Content = new StackLayout()
            {
                Children =
                {
                    returnButton,
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
            var itemBase = ContentManager.storageSelection == ContentManager.StorageSelection.cabinet ? ContentManager.cabinetInfo : ContentManager.fridgeInfo;
            var itemInfoBase = ContentManager.storageSelection == ContentManager.StorageSelection.cabinet ? ContentManager.cabinetItemBase : ContentManager.fridgeItemBase;
            foreach (var key in itemBase.Keys)
            {
                var metaName = key;
                var name = new Label() { Text = key.ToString(), TextColor = Color.Black, FontSize = 25, HorizontalTextAlignment = TextAlignment.Center };
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
                    WidthRequest = 60,
                    HeightRequest = 40,
                    TranslationX = -40,
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    IsVisible = false
                };

                var viewButton = new Button()
                {
                    BackgroundColor = Color.Red,
                    Text = "View",
                    WidthRequest = 60,
                    HeightRequest = 40,
                    TranslationX = 40,
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
                    Text = "Name",
                    HeightRequest = 30,
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
                    HeightRequest = 50,
                    ScaleX = 0,
                    IsEnabled = false
                };
                deleteButton.Clicked += async (obj, args) =>
                {
                    var confirm = await DisplayAlert("Caution", "Are you sure you want to delete this layout?", "Delete", "Cancel");
                    if (confirm && itemBase.ContainsKey(key) && itemInfoBase.ContainsKey(key))
                    {
                        foreach (var index in itemBase[key].Keys)
                        {
                            foreach (var cabButton in itemInfoBase[key][index].Keys)
                            {
                                foreach (var item in itemInfoBase[key][index][cabButton])
                                {
                                    ContentManager.UnplacedItemBase.Add(item.ItemData.ID, item);
                                }
                            }
                        }
                        deleteStorageBase.Invoke(key);
                        deleteStorageLocal.Invoke(key);
                        itemInfoBase.Remove(key);
                        itemBase.Remove(key);
                        foreach (var child in mainGridChildren[key])
                        {
                            mainGrid.Children.Remove(child);
                        }
                    }
                };
                changeNameButton.Clicked += async (obj, args) =>
                {
                    changeNameField.IsEnabled = true;
                    await ViewExtensions.LinearInterpolator(changeNameField, 1, 100, i => changeNameField.ScaleX = i);
                    changeNameField.Focus();
                };
                void onNameChanged()
                {
                    if (changeNameField.Text != null && !itemBase.ContainsKey(changeNameField.Text))
                    {
                        var itemInfoHistory = itemBase[metaName];
                        var itemBaseHistory = itemInfoBase[metaName];
                        metaName = changeNameField.Text;
                        itemBase.Remove(metaName); itemInfoBase.Remove(metaName);
                        itemBase.Add(changeNameField.Text, itemInfoHistory);
                        itemInfoBase.Add(changeNameField.Text, itemBaseHistory);
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
                    button.ToggleEffects(new ImageTint() { tint = Color.FromHsla(1, .1, .5, .5) }, new List<VisualElement>() { addButton, viewButton, deleteButton, changeNameButton});
                List<View> views = new List<View>() { preview, button, addButton, viewButton, deleteButton, changeNameButton, changeNameField };
                if(!mainGridChildren.ContainsKey(metaName)) mainGridChildren.Add(metaName, views);
                gridList.Insert(1, views);
            }
            mainGrid.OrganizeGrid(gridList, GridOrganizer.OrganizeMode.HorizontalLeft);
        }
    }
}
