using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Utility;
using System.Threading;

namespace ZestyKitchenHelper
{
    public class ItemLayout : Layout<View>
    {
        private const double default_name_height = 0.2;
        private const double long_name_height = 0.4;
        private const double corner_icon_size = 0.35;

        public ImageButton iconImage;
        public ImageButton infoIcon;
        public Button deleteButton;
        public Label itemTitle;
        public Button expirationMark;
        private Func<string, Layout<View>> storageEvent;
        private double width, height;

        public Item ItemData;
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

        public void SetStoragePointer(Func<string, Layout<View>> storagePointer)
        {
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
                TextColor = Color.Black
            };
            ChangeExpirationMarkColor();
            GetAbsoluteLayout.Children.Add(expirationMark, new Rectangle(0, 0, corner_icon_size, corner_icon_size), AbsoluteLayoutFlags.All);
            return this;
        }

        public void ChangeExpirationMarkColor()
        {
            var expDay = ItemData.daysUntilExp;
            if(expDay == 0)
            {
                expirationMark.BackgroundColor = Color.Red;
            }
            else if (expDay < 7 && expDay > 0)
            {
                expirationMark.BackgroundColor = Color.Orange;
            }
            else if (expDay < 30 && expDay > 0)
            {
                expirationMark.BackgroundColor = Color.Yellow;
            }
            else if (expDay >= 30)
            {
                expirationMark.BackgroundColor = Color.Green;
            }
            else
            {
                expirationMark.BackgroundColor = Color.Gray;
            }

        }
        public ItemLayout AddMainImage()
        {
            ImageSource source = ItemData.Icon.Substring(6);
            iconImage = new ImageButton { Source = source, Aspect = Aspect.AspectFit, BackgroundColor = Color.WhiteSmoke };
            iconImage.BorderColor = Color.Black;
            iconImage.BorderWidth = 1;
            AbsoluteLayout.SetLayoutBounds(iconImage, new Rectangle(0, 0, 1, 1 - default_name_height));
            AbsoluteLayout.SetLayoutFlags(iconImage, AbsoluteLayoutFlags.All);

            GetAbsoluteLayout.Children.Add(iconImage);
            return this;
        }

        public ItemLayout AddTitle()
        {
            var height = ItemData.Name.Length > 10 ? long_name_height : default_name_height;
            var itemTitleBackground = new Button()
            {
                IsEnabled = false,
                BorderColor = Color.Black,
                BorderWidth = 1,
                BackgroundColor = Color.White
            };
            itemTitle = new Label()
            { Text = ItemData.Name, TextColor = Color.Black, FontSize = 12, FontAttributes = FontAttributes.Bold, LineBreakMode = LineBreakMode.WordWrap, Margin = new Thickness(2, 2) };
            GetAbsoluteLayout.Children.Add(itemTitleBackground, new Rectangle(0, 1, 1, height), AbsoluteLayoutFlags.All);
            GetAbsoluteLayout.Children.Add(itemTitle, new Rectangle(0, 1, 1, height), AbsoluteLayoutFlags.All);
            AbsoluteLayout.SetLayoutBounds(iconImage, new Rectangle(0, 0, 1, 1 - height));

            return this;
        }


        public ItemLayout AddInfoIcon()
        {
            infoIcon = new ImageButton() { Source = ContentManager.addIcon, Aspect = Aspect.Fill };
            GetAbsoluteLayout.Children.Add(infoIcon);
            AbsoluteLayout.SetLayoutBounds(infoIcon, new Rectangle(1, 0, corner_icon_size, corner_icon_size));
            AbsoluteLayout.SetLayoutFlags(infoIcon, AbsoluteLayoutFlags.All);
            infoIcon.Clicked += (obj, args) =>
            {
                Console.WriteLine("ItemLayout 173 Direct Select Index: " + ItemData.StorageCellIndex);
                InfoView infoPage = new InfoView(ItemData);
                ContentManager.pageController.ShowInfoView(infoPage);
               // infoPage.SetCabinetView(); 
            };
            return this;
        }
        public void RecalculateDate()
        {
            ItemData.daysUntilExp = DateCalculator.SubtractDate(ItemData.expYear, ItemData.expMonth, ItemData.expDay);
            expirationMark.Text = DateCalculator.SubtractDateStringed(ItemData.expYear, ItemData.expMonth, ItemData.expDay);
            if(ItemData.daysUntilExp == 0) { expirationMark.Text = "Exp"; }
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
        public string[] iconNames;
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
        public IconLayout(ImageSource source, params string[] iconNames)
        {
            imageSource = source;
            imageButton = new ImageButton() { Source = source, BackgroundColor = Color.Transparent, CornerRadius = 2, BorderColor = Color.Black, BorderWidth = 1 };

            this.iconNames = iconNames;
            imageButton.Clicked += (obj, args) =>
            {
                OnClickIconAction?.Invoke(imageButton);
            };
            layout.Children.Add(imageButton, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);
            Children.Add(layout);
            BackgroundColor = Color.Wheat;
        }
    }
}
