using System;
using System.Collections.Generic;
using System.Text;
using Utility;
using Xamarin.Forms;

namespace ZestyKitchenHelper
{
    public class InfoPage : ContentPage
    {
        Func<string,Layout<View>> storageAction;
        string storageName;
        ImageButton parentButton;
        Label locationLabel;
        StackLayout pageContainer;
        public bool cabinetBinded;
        public InfoPage(Item item)
        {
            var backButton = new ImageButton() { Source = ContentManager.backButton, Aspect = Aspect.Fill, WidthRequest = 50, HeightRequest = 50, HorizontalOptions = LayoutOptions.StartAndExpand };
            backButton.Clicked += (obj, args) => ContentManager.navigateToUnplacedPageEvent();
            var itemLabel = new Label() { Text = item.name, TextColor = Color.Black, FontSize = 20, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center };
            var itemImage = new Image() { Source = item.icon.Substring(6), Aspect = Aspect.Fill, WidthRequest = 150, HeightRequest = 150, HorizontalOptions = LayoutOptions.StartAndExpand};
            var expirationDateLabel = new Label() { Text = "Expiration Date: " + item.expMonth + "/" + item.expDay + "/" + item.expYear, TextColor = Color.Black, FontSize = 20  };
            var amountLabel = new Label() { Text = "Amount: " + item.amount.ToString(), TextColor = Color.Black, FontSize = 20 };
            locationLabel = new Label() { Text = "Location: This item has not been placed", TextColor = Color.Black, FontSize = 20 };
            var deleteButton = new Button() { BackgroundColor = Color.FromRgba(0, 100, 20, 80), Text = "Use", WidthRequest = 100, HorizontalOptions = LayoutOptions.CenterAndExpand };
            deleteButton.Clicked += (obj, args) => { ContentManager.MetaItemBase.Remove(item.ID); UnplacedPage.UpdateGrid(item); ContentManager.navigateToUnplacedPageEvent(); };
            pageContainer = new StackLayout()
            {
                Children = { backButton, itemLabel, itemImage, expirationDateLabel, amountLabel, locationLabel, deleteButton }
            };
            Content = pageContainer;
        }

        public void BindCabinetInfo(ImageButton _parentButton, Func<string, Layout<View>> getStorageAction, string _storageName)
        {
            storageAction = getStorageAction;
            parentButton = _parentButton;
            storageName = _storageName;
            cabinetBinded = true;
       }

        public void SetCabinetView()
        {
            if (storageAction != null)
            {
                locationLabel.Text = "Location: " + storageName;
                var storageView = storageAction(storageName);
                // ImageTint tintEffect = new ImageTint() { tint = Color.FromRgba(100, 20, 20, 90) };
                // parentButton.ToggleEffects(tintEffect, null);
                pageContainer.Children.Insert(pageContainer.Children.Count - 1, storageView);
            }

        }
    }
}
