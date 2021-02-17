using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ZestyKitchenHelper
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = ContentManager.pageController;
        }

        protected override void OnStart()
        {

        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
