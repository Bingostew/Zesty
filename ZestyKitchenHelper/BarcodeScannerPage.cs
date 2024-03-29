﻿using System;
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

        public BarcodeScannerPage(AddView addView) : base()
        {
            scannerView = new ZXingScannerView()
            {
                AutomationId = "zxingScannerView"
            };
            scannerView.OnScanResult += async r =>
            {
                scannerView.IsAnalyzing = false;
                scannerView.IsScanning = false;

                HttpClient httpClient = new HttpClient();
                Uri uri = new Uri(string.Format("https://api.upcitemdb.com/prod/trial/lookup?upc={0}", r.Text));
                HttpResponseMessage response = await httpClient.GetAsync(uri);
                Console.WriteLine("BarcodeScannerPage 31 got http response");

                if (response.IsSuccessStatusCode)
                {
                    JsonTextReader jsonTextReader = new JsonTextReader(new System.IO.StringReader(await response.Content.ReadAsStringAsync()));
                    JsonSerializer jsonSerializer = new JsonSerializer();
                    BarcodeItem barcodeItem = jsonSerializer.Deserialize<BarcodeItem>(jsonTextReader);

                    if (barcodeItem == null || barcodeItem.items.Length == 0)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            ContentManager.pageController.ReturnToPrevious();
                            ContentManager.pageController.ShowAlert("Scan Result", "Unfortunately, the barcode cannot be found in the databse.", "Help Expanding Barcode Database", "Exit", null, null);
                        });
                        return;
                    }
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        var itemName = barcodeItem.items[0].title;
                        ContentManager.pageController.ReturnToPrevious();
                        ContentManager.pageController.ShowAlert("Scan Result", "Item Name: " + barcodeItem.items[0].title, "Use", "Cancel", () => { addView.SetProductName(itemName); }, null);
                    });
                }

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
            scannerView.IsAnalyzing = false;
        }

        public void StartScanning()
        {
            scannerView.IsScanning = true;
            scannerView.IsAnalyzing = true;
        }
    }
}
