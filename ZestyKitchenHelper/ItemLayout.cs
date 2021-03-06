﻿using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Utility;
using System.Threading;

namespace ZestyKitchenHelper
{
    public class ItemLayout : Layout<View>
    {
        public ImageButton iconImage;
        public ImageButton infoIcon;
        public Button deleteButton;
        public Label itemTitle;
        public Button expirationMark;
        public Label amountLabel;
        private Func<string, Layout<View>> storageEvent;
        private double width, height;

        public Item ItemData;
        public int ParentCellIndex { get; set; }
        public string StorageName { get; set; }
        public int StorageIndex { get; set; }
        public double DefaultTranslationX { get; set; }
        public ImageButton ParentButton { get; set; }
        /// <summary>
        /// Default parameter- Must set the ItemProperty binding manually
        /// </summary>
        public ItemLayout()
        {
            GetAbsoluteLayout = new AbsoluteLayout()
            {
                WidthRequest = width,
                HeightRequest = height
            };
            Children.Add(GetAbsoluteLayout);
        }
        public ItemLayout(double _width, double _height, Item _item)
        {
            width = _width;
            height = _height;
            ItemData = _item;
            GetAbsoluteLayout = new AbsoluteLayout()
            {
                WidthRequest = width,
                HeightRequest = height
            };
            Children.Add(GetAbsoluteLayout);

        }

        public void BindCabinetInfo(double defaultTransX, int parentCellIndex, ImageButton parentButton, string storageName, Func<string, Layout<View>> storagePointer)
        {
            ParentCellIndex = parentCellIndex;
            DefaultTranslationX = defaultTransX;
            ParentButton = parentButton;
            StorageName = storageName;
            storageEvent = storagePointer;
        }
        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            foreach(View child in Children)
            {
                LayoutChildIntoBoundingRegion(child, new Rectangle(x, y, width, height));
            }
        }
        private int GetVisibleChildren()
        {
            int count = 0;
            foreach(View child in Children)
            {
                if(child.IsVisible == true)
                {
                    count++;
                }
            }
            return count;
        }
        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        { 
            if(GetVisibleChildren() > 0)
            {
                Size size = new Size(width, height);
                return new SizeRequest(size);
            }
            else
            {
                return new SizeRequest();
            }
        }

        protected override void OnChildMeasureInvalidated()
        {
            base.OnChildMeasureInvalidated();
        }
        public AbsoluteLayout GetAbsoluteLayout { get; }

        public ItemLayout AddExpirationMark()
        {
            expirationMark = new Button()
            {
                CornerRadius = 10,
                Text = DateCalculator.SubtractDateStringed(ItemData.expYear, ItemData.expMonth, ItemData.expDay),
                HorizontalOptions = LayoutOptions.End,
            };
            ChangeExpirationMarkColor();
            GetAbsoluteLayout.Children.Add(expirationMark, new Rectangle(0, 0, 40, 40));
            return this;
        }

        public void ChangeExpirationMarkColor()
        {
            var expDay = ItemData.daysUntilExp;
            if(expDay <= 0)
            {
                expirationMark.BackgroundColor = Color.Red;
            }
            if (expDay < 7)
            {
                expirationMark.BackgroundColor = Color.Orange;
            }
            else if (expDay < 30)
            {
                expirationMark.BackgroundColor = Color.Yellow;
            }
            else
            {
                expirationMark.BackgroundColor = Color.Gray;
            }

        }
        public ItemLayout AddAmountMark()
        {
            amountLabel = new Label() { Text = "X" + ItemData.amount.ToString(), WidthRequest = 10, HeightRequest = 5, HorizontalTextAlignment = TextAlignment.Center,
                BackgroundColor = Color.White, TextColor = Color.Black, FontAttributes = FontAttributes.Bold };

            GetAbsoluteLayout.Children.Add(amountLabel, new Rectangle(1, 1, 30, 20), AbsoluteLayoutFlags.PositionProportional);
            return this;
        }
        public ItemLayout AddMainImage()
        {
            ImageSource source = ItemData.icon.Substring(6);
            iconImage = new ImageButton { Source = source, Aspect = Aspect.Fill };
            iconImage.BorderColor = Color.Black;
            iconImage.BorderWidth = 2;
            iconImage.Clicked += (obj, args) => Console.WriteLine("clickitydoo wee");
            AbsoluteLayout.SetLayoutBounds(iconImage, new Rectangle(0, 0, 1, 1));
            AbsoluteLayout.SetLayoutFlags(iconImage, AbsoluteLayoutFlags.All);

            GetAbsoluteLayout.Children.Add(iconImage);
            return this;
        }

        public ItemLayout AddTitle()
        {
            itemTitle = new Label() 
            { Text = ItemData.name, HorizontalTextAlignment = TextAlignment.Center, BackgroundColor = Color.White, TextColor = Color.Black, FontSize = 12 };
            GetAbsoluteLayout.Children.Add(itemTitle, new Rectangle(0, 1, .6, .3), AbsoluteLayoutFlags.All);

            return this;
        }

        public ItemLayout AddDeleteButton()
        {
            deleteButton = new Button() { BackgroundColor = Color.Red, CornerRadius = 2, Text = "X", FontSize = 20 };
            GetAbsoluteLayout.Children.Add(deleteButton);
            
            AbsoluteLayout.SetLayoutBounds(deleteButton, new Rectangle(1, 0, 35, 35));
            AbsoluteLayout.SetLayoutFlags(deleteButton, AbsoluteLayoutFlags.PositionProportional);

            return this;
        }

        public ItemLayout AddInfoIcon()
        {
            infoIcon = new ImageButton() { Source = ContentManager.addIcon };
            GetAbsoluteLayout.Children.Add(infoIcon);
            AbsoluteLayout.SetLayoutBounds(infoIcon, new Rectangle(1, 0, 30, 30));
            AbsoluteLayout.SetLayoutFlags(infoIcon, AbsoluteLayoutFlags.PositionProportional);
            infoIcon.Clicked += (obj, args) => 
            {
                InfoPage infoPage = new InfoPage(ItemData);
                ContentManager.navigateToInfoPageEvent(infoPage);
                if(StorageName != null) { infoPage.BindCabinetInfo(ParentButton, storageEvent, StorageName); }
                infoPage.SetCabinetView(); 
            };
            return this;
        }

        public void SubtractAmount()
        {
            ItemData.amount--;
            amountLabel.Text = "X" + ItemData.amount.ToString();
        }

        public void RecalculateDate()
        {
            ItemData.daysUntilExp = DateCalculator.SubtractDate(ItemData.expYear, ItemData.expMonth, ItemData.expDay);
            expirationMark.Text = DateCalculator.SubtractDateStringed(ItemData.expYear, ItemData.expMonth, ItemData.expDay);
            if(ItemData.daysUntilExp <= 0) { expirationMark.Text = "Exp"; }
        }

        public void SetMarkingVisibility(bool visible)
        {
            foreach(var child in GetAbsoluteLayout.Children)
            {
                if(child != iconImage)
                {
                    child.IsVisible = visible;
                }
            }
        }
    }

    public class IconLayout : Layout<View>
    {
        ImageSource imageSource = "";
        string iconName = "";
        AbsoluteLayout layout = new AbsoluteLayout();
        public Action<ImageButton> OnClickIconAction;
        public ImageButton imageButton;

        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            foreach (View child in Children)
            {
                LayoutChildIntoBoundingRegion(child, new Rectangle(x, y, width, height));
            }
        }
        private int GetVisibleChildren()
        {
            int count = 0;
            foreach (View child in Children)
            {
                if (child.IsVisible == true)
                {
                    count++;
                }
            }
            return count;
        }
        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            if (GetVisibleChildren() > 0)
            {
                return new SizeRequest(new Size(widthConstraint, heightConstraint));
            }
            else
            {
                return new SizeRequest();
            }
        }

        protected override void OnChildMeasureInvalidated()
        {
            base.OnChildMeasureInvalidated();
        }
        public string GetImageSource()
        {
            return imageSource.ToString();
        }
        public string GetIconName()
        {
            return iconName;
        }
        public IconLayout(ImageSource source, string iconName)
        {
            imageSource = source;
            imageButton = new ImageButton() { Source = source };
            var label = new Label() { Text = iconName, HorizontalTextAlignment = TextAlignment.Center, TextColor = Color.Black };
            this.iconName = iconName;
            imageButton.Clicked += (obj, args) =>
            {
                OnClickIconAction?.Invoke(imageButton);
            };
            layout.Children.Add(imageButton, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);
            layout.Children.Add(label, new Rectangle(0, .7, 1, .1), AbsoluteLayoutFlags.All);
            Children.Add(layout);
        }
    }
}
