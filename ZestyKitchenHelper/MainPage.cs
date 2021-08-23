using System.Security.Cryptography.X509Certificates;
using Xamarin.Forms;

namespace ZestyKitchenHelper
{
    public class MainPage
    {
        private static TabbedPage mainPage;

        public static TabbedPage Create(SingleSelectionPage cabinetSelectPage, SingleSelectionPage fridgeSelectPage, UnplacedPage itemPage)
        {
            if(mainPage!= null)
            {
                return mainPage;
            }
            mainPage = new TabbedPage()
            {
                Children =
               {
                    cabinetSelectPage, fridgeSelectPage, itemPage
               }
            };

            return mainPage;
        }
    }
}
