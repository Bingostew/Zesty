using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;
using Utility;
using Newtonsoft.Json;

namespace ZestyKitchenHelper
{
    class BarcodeScannerPage : ContentPage
    {
        ZXingScannerView scannerView;
        ZXingDefaultOverlay scannerOverlay;

        public BarcodeScannerPage(Action toAddPage) : base()
        {
            scannerView = new ZXingScannerView()
            {
                AutomationId = "zxingScannerView"
            };
            scannerView.OnScanResult += async r =>
            {
                scannerView.IsAnalyzing = false;

                HttpClient httpClient = new HttpClient();
                Uri uri = new Uri(string.Format("https://api.upcitemdb.com/prod/trial/lookup?upc={0}", r.Text));
                HttpResponseMessage response = await httpClient.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    JsonTextReader jsonTextReader = new JsonTextReader(new System.IO.StringReader(await response.Content.ReadAsStringAsync()));
                    JsonSerializer jsonSerializer = new JsonSerializer();
                    BarcodeItem barcodeItem = jsonSerializer.Deserialize<BarcodeItem>(jsonTextReader);
                    Device.BeginInvokeOnMainThread(async () => await ContentManager.pageController.DisplayAlert("Result", barcodeItem.items[0].title, "Cancel"));    
                }
                toAddPage?.Invoke();
            };
            scannerOverlay = new ZXingDefaultOverlay()
            {
                TopText = "Zesty scanner top text",
                BottomText = "Zesty scanner bottom text",
                ShowFlashButton = scannerView.HasTorch,
                AutomationId = "zxingDefaultOverlay"
            };
            var grid = new Grid();
            grid.Children.Add(scannerView);
            grid.Children.Add(scannerOverlay);

            Content = grid;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            Console.WriteLine("BarcodeScannerPage 56 page appeared");
            scannerView.IsScanning = true;
            scannerView.IsAnalyzing = true;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            scannerView.IsScanning = false;
        }

        public void StartScanning()
        {
            Console.WriteLine("BarcodeScanner 69 start scanning ");
            scannerView.IsScanning = true;
            scannerView.IsAnalyzing = true;
        }
        public void StopScanning()
        {
            scannerView.IsScanning = false;
        }

    }
}
