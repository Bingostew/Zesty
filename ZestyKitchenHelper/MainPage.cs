using System.Security.Cryptography.X509Certificates;
using Xamarin.Forms;

namespace ZestyKitchenHelper
{
    public class MainPage : ContentPage
    {
        public Button scanningButton = new Button { Text = "scan" };
        public Grid grid;
        public MainPage()
        {
            grid = new Grid
            {
                RowDefinitions = {
                    new RowDefinition(),
                    new RowDefinition(),
                    new RowDefinition()
                },
                ColumnDefinitions = {
                    new ColumnDefinition(),
                    new ColumnDefinition()
                }
            };

            Button view = new Button() { BackgroundColor = Color.BlanchedAlmond, CornerRadius = 20, Margin = new Thickness(5) };
            Button view1 = new Button() { BackgroundColor = Color.BlanchedAlmond, CornerRadius = 20, Margin = new Thickness(5) };
            Button view2 = new Button() { BackgroundColor = Color.BlanchedAlmond, CornerRadius = 20, Margin = new Thickness(5) };


            Content = grid;
        }
    }
}
