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
                    new RowDefinition() {Height = icon_size}
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition(){Width = icon_size },
                    new ColumnDefinition()
                }
            };


            userIcon = new ImageButton() { Source = user.IconImage, WidthRequest = icon_size, HeightRequest = icon_size, CornerRadius = icon_size / 2, Margin = new Thickness(5) };
            ContentManager.sessionUserProfile.AddOnProfileChangedListener(u => userIcon.Source = u.IconImage);
            usernameLabel = new Label() { Text = user.Name, FontFamily = main_font, FontSize = 30, TextColor = Color.Black, Margin = new Thickness(0, 30, 0, 0) };
            ContentManager.sessionUserProfile.AddOnProfileChangedListener(u => usernameLabel.Text = u.Name);
            userEmailLabel = new Label() { FontFamily = main_font, FontSize = small_font_size, TextColor = Color.Black };
            ContentManager.sessionUserProfile.AddOnProfileChangedListener(u => userEmailLabel.Text = u.Email == null || u.Email.Equals("") ? "Local Account" : u.Email);
            userEmailLabel.Text = user.Email == null || user.Email.Equals("") ? "Local Account" : user.Email;
            var changeUsernameButton = new Button() { Text = "Edit Profile", FontFamily = main_font, FontSize = 15, CornerRadius = 2, Margin = new Thickness(0, 0, 0, 30) };
            var profileDivider = new Button() { BackgroundColor = Color.Gray, HeightRequest = divider_height, Margin = new Thickness(side_margin, 0) };
            changeUsernameButton.Clicked += (o, a) => ContentManager.pageController.SetViewOverlay(GetEditUserView(), .75, .75);

            userProfileSection.Children.Add(userIcon, 0, 0);
            userProfileSection.Children.Add(new StackLayout()
            {
                Spacing = 0,
                HeightRequest = icon_size,
                Children = { usernameLabel, userEmailLabel, changeUsernameButton }
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
                    BorderColor = Color.Black, BorderWidth = 2, CornerRadius = theme_square_size / 2, Margin = new Thickness(20)  };
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
                if(a.CenterItemIndex != currentThemeIndex)
                {
                    currentThemeIndex = a.CenterItemIndex;
                    ContentManager.ThemeColor = themeList[currentThemeIndex].Color;
                }
            };

            var notificationLabel = new Label() { Text = "Notification", FontFamily = main_font, FontSize = main_font_size, TextColor = Color.Black, Margin = new Thickness(side_margin, 0) };

            preferenceSection.Children.Add(preferenceLabel, 0, 0);
            Grid.SetColumnSpan(preferenceLabel, 2);
            preferenceSection.Children.Add(themeLabel, 0, 1);
            preferenceSection.Children.Add(themeCarousel, 1, 1);

            content = new ScrollView()
            {
                Content = new StackLayout()
                {
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
                BackgroundColor = Color.Lavender,
                VerticalOptions = LayoutOptions.EndAndExpand,
                RowDefinitions =
                {
                    new RowDefinition() {Height = 100},
                    new RowDefinition() {Height = 100},
                    new RowDefinition() {Height = 100}
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition(),
                    new ColumnDefinition()
                }
            };
            var userIconCarousel = new CarouselView() { HeightRequest = icon_size, HorizontalOptions = LayoutOptions.Center, PeekAreaInsets = new Thickness(icon_size / 4, 0), Loop = false};
            userIconCarousel.ItemTemplate = new DataTemplate(() =>
            {
                ImageButton icon = new ImageButton() { WidthRequest = icon_size, HeightRequest = icon_size, CornerRadius = icon_size / 2 };
                icon.SetBinding(ImageButton.SourceProperty, "Source");

                return icon;
            });
            List<ProfileIcon> iconList = new List<ProfileIcon>();
            foreach(var icon in ContentManager.ProfileIcons)
            {
                iconList.Add(new ProfileIcon() { Source = icon });
            }
            userIconCarousel.ItemsSource = iconList;
            var currentIconIndex = ContentManager.ProfileIcons.IndexOf(ContentManager.ProfileIcons.Find(s => s.Equals(ContentManager.sessionUserProfile.IconImage)));
           
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
                    await FireBaseController.UpdateUser(new UserProfile() { Name = usernameEntry.Text, Email = emailEntry.Text, IconImage = (userIconCarousel.CurrentItem as ProfileIcon).Source }) ;
                }

                ContentManager.pageController.RemoveViewOverlay(stackLayout);
            };

            stackLayout.Children.Add(userIconCarousel);
            stackLayout.Children.Add(grid);
            stackLayout.BackgroundColor = Color.Beige;
            userIconCarousel.ScrollTo(currentIconIndex);

            return stackLayout;
        }
    }
}
