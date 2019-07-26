using GehirnJogging.Code.Spiele;
using GehirnJogging.Pages.ResourceDictionaries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace GehirnJogging.Pages.Controller
{
    public sealed partial class WechselGeldEingabe : UserControl
    {
        public static readonly BitmapImage CT_1 = new BitmapImage(new Uri("ms-appx:///Pages/AssetBilder/1c.png"));
        public static readonly BitmapImage CT_2 = new BitmapImage(new Uri("ms-appx:///Pages/AssetBilder/2c.png"));
        public static readonly BitmapImage CT_5 = new BitmapImage(new Uri("ms-appx:///Pages/AssetBilder/5c.png"));
        public static readonly BitmapImage CT_10 = new BitmapImage(new Uri("ms-appx:///Pages/AssetBilder/10c.png"));
        public static readonly BitmapImage CT_20 = new BitmapImage(new Uri("ms-appx:///Pages/AssetBilder/20c.png"));
        public static readonly BitmapImage CT_50 = new BitmapImage(new Uri("ms-appx:///Pages/AssetBilder/50c.png"));
        public static readonly BitmapImage EUR_1 = new BitmapImage(new Uri("ms-appx:///Pages/AssetBilder/1e.png"));
        public static readonly BitmapImage EUR_2 = new BitmapImage(new Uri("ms-appx:///Pages/AssetBilder/2e.png"));
        public static readonly BitmapImage EUR_5 = new BitmapImage(new Uri("ms-appx:///Pages/AssetBilder/5E.png"));
        public static readonly BitmapImage EUR_10 = new BitmapImage(new Uri("ms-appx:///Pages/AssetBilder/10E.png"));
        public static readonly BitmapImage EUR_20 = new BitmapImage(new Uri("ms-appx:///Pages/AssetBilder/20E.png"));
        public static readonly BitmapImage EUR_50 = new BitmapImage(new Uri("ms-appx:///Pages/AssetBilder/50E.png"));
        public static readonly BitmapImage EUR_100 = new BitmapImage(new Uri("ms-appx:///Pages/AssetBilder/100E.png"));

        private static Dictionary<Coins, BitmapImage> bmps = new Dictionary<Coins, BitmapImage>();
        static WechselGeldEingabe() {
            bmps.Add(Coins.CENT_1, CT_1);
            bmps.Add(Coins.CENT_2, CT_2);
            bmps.Add(Coins.CENT_5, CT_5);
            bmps.Add(Coins.CENT_10, CT_10);
            bmps.Add(Coins.CENT_20, CT_20);
            bmps.Add(Coins.CENT_50, CT_50);
            bmps.Add(Coins.EUR_1, EUR_1);
            bmps.Add(Coins.EUR_2, EUR_2);
            bmps.Add(Coins.EUR_5, EUR_5);
            bmps.Add(Coins.EUR_10, EUR_10);
            bmps.Add(Coins.EUR_20, EUR_20);
            bmps.Add(Coins.EUR_50, EUR_50);
            bmps.Add(Coins.EUR_100, EUR_100);
        }

        private void ButtonEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e) => ((sender as Button).Content as Image).Source = new BitmapImage(new Uri("ms-appx:///Pages/AssetBilder/" + (sender as Button).Tag + "-white.png"));
        private void ButtonExit(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e) => ((sender as Button).Content as Image).Source = new BitmapImage(new Uri("ms-appx:///Pages/AssetBilder/" + (sender as Button).Tag + "-mainBlue.png"));


        private Button wrong, right, right2, eingabe;
        private WechselgeldAufgabe aufgabe;
        private Coins coin;

        public WechselGeldEingabe(Coins coin)
        {
            this.InitializeComponent();

            this.coin = coin;
            bild.Source = bmps[coin];
            eingabe = new ColorButton(ColorButton.BLAU)
            {
                FontSize = (double)App.ResourceDict["NormalFont"]
            };
            right = new ColorButton(ColorButton.GREEN)
            {
                FontSize = eingabe.FontSize
            };
            right2 = new ColorButton(ColorButton.GREEN)
            {
                FontSize = eingabe.FontSize
            };
            wrong = new ColorButton(ColorButton.RED)
            {
                FontSize = eingabe.FontSize
            };

            rightContainer.Child = right;
            wrongContainer.Child = wrong;
            eingabeContainer.Child = eingabe;
        }


        public void Aktualisiere() => eingabe.Content = aufgabe.GetWechselGeld(coin);

        private void Click(object sender, RoutedEventArgs e)
        {
            if (sender == Subtract) aufgabe.AddSubCoin(coin, false);
            else if (sender == Add) aufgabe.AddSubCoin(coin, true);
            eingabe.Content = aufgabe.GetWechselGeld(coin);
        }

        public void StelleDar(WechselgeldAufgabe aufgabe)
        {
            this.aufgabe = aufgabe;
            if (aufgabe.Finished)
            {
                eingabeStack.Visibility = Visibility.Collapsed;       
                rightContainer.Visibility = Visibility.Visible;
                if (!aufgabe.Correct)
                {
                    wrongContainer.Visibility = Visibility.Visible;
                    if (aufgabe.GetLoesung(coin) != aufgabe.GetWechselGeld(coin)) wrongContainer.Child = wrong;
                    else wrongContainer.Child = right2;
                } else wrongContainer.Visibility = Visibility.Collapsed;

                right2.Content = wrong.Content = aufgabe.GetWechselGeld(coin);
                right.Content = aufgabe.GetLoesung(coin);
            }
            else
            {
                eingabeStack.Visibility = Visibility.Visible;
                rightContainer.Visibility = wrongContainer.Visibility = Visibility.Collapsed;
                eingabe.Content = aufgabe.GetWechselGeld(coin);
            }
        }

    }
}
