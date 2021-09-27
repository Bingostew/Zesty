using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ZestyKitchenHelper
{
    public class PreferencePage : ContentPage
    {
        private const string main_font = "NotoSans-Regular";
        private const string title_font = "Oswald-Regular";
        private const int small_font_size = 15;
        private const int main_font_size = 20;
        private const int title_font_size = 25;

        private const int side_margin = 10;
        private const int icon_size = 150;
        private const int standard_height = 50;
        private const int theme_square_size = 20;
        private const int divider_height = 1;

        ScrollView content;
        ImageButton userIcon;
        Label usernameLabel;
        Label userEmailLabel;

        public PreferencePage()
        {
            // Register background change listener
            BackgroundColor = ContentManager.ThemeColor;
            ContentManager.AddOnBackgroundChangeListener(c => BackgroundColor = c);
            // Title section
            var titleGrid = GridManager.InitializeGrid(1, 3, 50, GridLength.Star);
            var returnButton = new ImageButton() { Source = ContentManager.backButton };
            var pageTitleLabel = new Label() { Text = "Setting", FontFamily = title_font, FontSize = 30, TextColor = Color.Black, HorizontalTextAlignment = TextAlignment.Center };
            GridManager.AddGridItem(titleGrid, new List<View> { returnButton, pageTitleLabel }, false);
            returnButton.Clicked += (o, a) => ContentManager.pageController.ReturnToPrevious();

            // User profile section
            var user = ContentManager.sessionUserProfile;
            var userProfileSection = new Grid()
            {
                RowDefinitions =
                {
                    new RowDefinition() {Height = icon_size + 10}
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition(){Width = icon_size + 10 },
                    new ColumnDefinition()
                }
            };


            userIcon = new ImageButton() { Source = user.IconImage, WidthRequest = icon_size, HeightRequest = icon_size, BackgroundColor = Color.Transparent, CornerRadius = icon_size / 2, Margin = new Thickness(5) };
            ContentManager.sessionUserProfile.AddOnProfileChangedListener(u => userIcon.Source = u.IconImage);
            usernameLabel = new Label() { Text = user.Name, FontFamily = main_font, FontSize = 30, TextColor = Color.Black, Margin = new Thickness(0, 30, 0, 0) };
            ContentManager.sessionUserProfile.AddOnProfileChangedListener(u => usernameLabel.Text = u.Name);
            userEmailLabel = new Label() { FontFamily = main_font, FontSize = small_font_size, TextColor = Color.Black };
            ContentManager.sessionUserProfile.AddOnProfileChangedListener(u => userEmailLabel.Text = u.Email == null || u.Email.Equals("") ? "Local Account" : u.Email);
            userEmailLabel.Text = user.Email == null || user.Email.Equals("") ? "Local Account" : user.Email;
            var editProfileButton = new Button() { Text = "Edit Profile", FontFamily = main_font, FontSize = 15, CornerRadius = 2, Margin = new Thickness(0, 0, 0, 30) };
            var profileDivider = new Button() { BackgroundColor = Color.Gray, HeightRequest = divider_height, Margin = new Thickness(side_margin, 0) };
            editProfileButton.Clicked += (o, a) => { ContentManager.pageController.SetViewOverlay(GetEditUserView(), .75, .75, 0.5, 0.5); ScrollToImageIcon(); };

            userProfileSection.Children.Add(userIcon, 0, 0);
            userProfileSection.Children.Add(new StackLayout()
            {
                Spacing = 0,
                HeightRequest = icon_size,
                Children = { usernameLabel, userEmailLabel, editProfileButton }
            }, 1, 0);

            // Preference section
            var preferenceSection = new Grid()
            {
                RowDefinitions =
                {
                    new RowDefinition() {Height = standard_height },
                    new RowDefinition() {Height = standard_height}

                },
                ColumnDefinitions =
                {
                    new ColumnDefinition(),
                    new ColumnDefinition()
                }
            };

            var preferenceLabel = new Label() { Text = "Preferences", FontSize = title_font_size, FontFamily = title_font, TextColor = Color.Black, Margin = new Thickness(side_margin, 0) };
            var themeLabel = new Label() { Text = "Background Theme", FontFamily = main_font, FontSize = main_font_size, TextColor = Color.Black, Margin = new Thickness(side_margin, 0) };
            var themeCarousel = new CarouselView() { HeightRequest = theme_square_size + 70, PeekAreaInsets = new Thickness(30, 0), Margin = new Thickness(0, 0, side_margin, 0), Loop = false };

            themeCarousel.ItemTemplate = new DataTemplate(() =>
            {
                ImageButton image = new ImageButton() { IsEnabled = false, WidthRequest = theme_square_size, HeightRequest = theme_square_size,
                    BorderColor = Color.Black, BorderWidth = 2, CornerRadius = theme_square_size / 2, Margin = new Thickness(20) };
                image.SetBinding(Image.SourceProperty, "Image");
                image.SetBinding(Image.BackgroundColorProperty, "Color");

                return image;
            });
            List<ThemeIcon> themeList = new List<ThemeIcon>()
            {
                new ThemeIcon(){Color = Color.Wheat, Source = ContentManager.transIcon },
                new ThemeIcon(){Color = Color.Lavender, Source = ContentManager.transIcon },
                new ThemeIcon(){Color = Color.BurlyWood, Source = ContentManager.transIcon },
                new ThemeIcon(){Color = Color.AliceBlue, Source = ContentManager.transIcon }
            };
            themeCarousel.ItemsSource = themeList;
            int currentThemeIndex = 0;
            themeCarousel.Scrolled += (o, a) =>
            {
                if (a.CenterItemIndex != currentThemeIndex)
                {
                    currentThemeIndex = a.CenterItemIndex;
                    ContentManager.ThemeColor = themeList[currentThemeIndex].Color;
                }
            };

            var notifLabel = new Label() { Text = "Notification", FontFamily = main_font, FontSize = main_font_size, TextColor = Color.Black, Margin = new Thickness(side_margin, 0) };
            var notifGrid = GridManager.InitializeGrid(3, 2, 50, GridLength.Star);
            notifGrid.Margin = new Thickness(0, 0, side_margin, 0);
            var oneDayNotif = new Switch() { IsToggled = true, WidthRequest = 80, OnColor = Color.Goldenrod };
            var threeDayNotif = new Switch() { IsToggled = true, WidthRequest = 80, OnColor = Color.Goldenrod };
            var oneWeekNotif = new Switch() { IsToggled = true, WidthRequest = 80, OnColor = Color.Goldenrod };
            var oneDayLabel = new Label() { Text = "1 day", FontSize = small_font_size, FontFamily = main_font, TextColor = Color.Black, VerticalTextAlignment = TextAlignment.Center };
            var threeDayLabel = new Label() { Text = "3 days", FontSize = small_font_size, FontFamily = main_font, TextColor = Color.Black, VerticalTextAlignment = TextAlignment.Center };
            var oneWeekLabel = new Label() { Text = "1 week", FontSize = small_font_size, FontFamily = main_font, TextColor = Color.Black, VerticalTextAlignment = TextAlignment.Center };
            oneDayNotif.Toggled += (o, a) => { ContentManager.sessionUserProfile.enableOneDayWarning = a.Value; updateUser(); };
            threeDayNotif.Toggled += (o, a) => { ContentManager.sessionUserProfile.enableThreeDayWarning = a.Value; updateUser(); };
            oneWeekNotif.Toggled += (o, a) => { ContentManager.sessionUserProfile.enableOneWeekWarning = a.Value; updateUser(); };
            GridManager.AddGridItem(notifGrid, new List<View>() { oneDayLabel, oneDayNotif, threeDayLabel, threeDayNotif, oneWeekLabel, oneWeekNotif }, true, Utility.GridOrganizer.OrganizeMode.HorizontalRight);

            async void updateUser()
            {
                if (ContentManager.isLocal)
                {
                    LocalStorageController.UpdateUser(ContentManager.sessionUserProfile);
                }
                else
                {
                    await FireBaseController.UpdateUser(ContentManager.sessionUserProfile);
                }
            }

            preferenceSection.Children.Add(preferenceLabel, 0, 0);
            Grid.SetColumnSpan(preferenceLabel, 2);
            preferenceSection.Children.Add(themeLabel, 0, 1);
            preferenceSection.Children.Add(themeCarousel, 1, 1);
            preferenceSection.Children.Add(notifLabel, 0, 2);
            preferenceSection.Children.Add(notifGrid, 1, 2);
            Grid.SetRowSpan(notifGrid, 3);


            content = new ScrollView()
            {
                Content = new StackLayout()
                {
                    HeightRequest = ContentManager.screenHeight,
                    Spacing = 5,
                    Children =
                    {
                        titleGrid,
                        userProfileSection,
                        profileDivider,
                        preferenceSection
                    }
                }
            };
            Content = content;
        }

        CarouselView userIconCarousel;
        int currentIconIndex;
        class ThemeIcon
        {
            public string Source { get; set; }
            public Color Color { get; set; }
        }
        class ProfileIcon
        {
            public string Source { get; set; }
        }
        /// <summary>
        /// Retrieves the view that allows user to edit profile image, name, and email
        /// </summary>
        /// <returns></returns>
        private View GetEditUserView()
        {
            var stackLayout = new StackLayout();
            var grid = new Grid()
            {
                Margin = new Thickness(20),
                BackgroundColor = Color.Lavender,
                VerticalOptions = LayoutOptions.EndAndExpand,
                RowDefinitions =
                {
                    new RowDefinition(),
                    new RowDefinition(),
                    new RowDefinition() 
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition(),
                    new ColumnDefinition()
                }
            };
            userIconCarousel = new CarouselView() { HeightRequest = icon_size, HorizontalOptions = LayoutOptions.Center, PeekAreaInsets = new Thickness(icon_size / 4, 0), Loop = false, IsScrollAnimated = true};
            userIconCarousel.ItemTemplate = new DataTemplate(() =>
            {
                ImageButton icon = new ImageButton() { IsEnabled = false, WidthRequest = icon_size, HeightRequest = icon_size, CornerRadius = icon_size / 2 };
                icon.SetBinding(ImageButton.SourceProperty, "Source");

                return icon;
            });
            List<ProfileIcon> iconList = new List<ProfileIcon>();
            foreach(var icon in ContentManager.ProfileIcons)
            {
                iconList.Add(new ProfileIcon() { Source = icon });
            }
            userIconCarousel.ItemsSource = iconList;
            currentIconIndex = ContentManager.ProfileIcons.IndexOf(ContentManager.ProfileIcons.Find(s => s.Equals(ContentManager.sessionUserProfile.IconImage)));
            Console.WriteLine("Preference 189 current icon index " + currentIconIndex);
            var nameLabel = new Label() { Text = "Name", FontFamily = main_font, FontSize = small_font_size, TextColor = Color.Black };
            var usernameEntry = new Entry() { Text = ContentManager.sessionUserProfile.Name };
            var emailLabel =  new Label() { Text = "Email", FontFamily = main_font, FontSize = small_font_size, TextColor = Color.Black };
            emailLabel.Text = ContentManager.sessionUserProfile.Email == null || ContentManager.sessionUserProfile.Email.Equals("") ? "" : ContentManager.sessionUserProfile.Email;
            var emailEntry = new Entry() { Text = emailLabel.Text };
            var saveButton = new Button() { Text = "Confirm", FontFamily = main_font, TextColor = Color.Black, CornerRadius = 3 };

            grid.Children.Add(nameLabel, 0, 0);
            grid.Children.Add(usernameEntry, 1, 0);
            if (!ContentManager.isLocal)
            {
                grid.Children.Add(emailLabel, 0, 1);
                grid.Children.Add(emailEntry, 1, 1);
            }
            grid.Children.Add(saveButton, 0, 2);
            Grid.SetColumnSpan(saveButton, 2);

            saveButton.Clicked += async (o, a) =>
            {
                ContentManager.sessionUserProfile.ChangeProfileWithListener(usernameEntry.Text, emailEntry.Text, (userIconCarousel.CurrentItem as ProfileIcon).Source);

                if (ContentManager.isLocal)
                {
                    LocalStorageController.UpdateUser(ContentManager.sessionUserProfile);
                }
                else
                {
                    await FireBaseController.UpdateUser(new UserProfile()
                    {
                        Name = usernameEntry.Text,
                        Email = emailEntry.Text,
                        IconImage = (userIconCarousel.CurrentItem as ProfileIcon).Source,
                        IsLocal = ContentManager.sessionUserProfile.IsLocal
                    }) ;
                }

                ContentManager.pageController.RemoveViewOverlay(stackLayout);
            };

            stackLayout.Children.Add(userIconCarousel);
            stackLayout.Children.Add(grid);
            stackLayout.BackgroundColor = Color.Beige;

            return stackLayout;
        }

        private void ScrollToImageIcon()
        {
            userIconCarousel.Scrolled += (o, a) => Console.WriteLine("Preference 233 item scrolled " + a.CenterItemIndex);
            userIconCarousel.ScrollTo(currentIconIndex, -1, ScrollToPosition.Start);
        }
    }
}
