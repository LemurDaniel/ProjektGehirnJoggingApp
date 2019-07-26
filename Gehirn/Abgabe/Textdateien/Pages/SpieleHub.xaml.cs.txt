using GehirnJogging.Code;
using GehirnJogging.Code.Spiele;
using GehirnJogging.Pages.Controller;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using static GehirnJogging.Code.SpielHighscores;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace GehirnJogging.Pages
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class SpieleHub : Page
    {
        public SpieleHub()
        {
            this.InitializeComponent();
            App.currentPage = this;

            volumeOn.Visibility = Sounddesign.CURRENT_AMBIENT == null ? Visibility.Collapsed : Visibility.Visible;
            volumeOff.Visibility = Sounddesign.CURRENT_AMBIENT == null ? Visibility.Visible : Visibility.Collapsed;
            Sounddesign.OverrideNext = Sounddesign.WOOSH;
            Sounddesign.OverrideNextN = 2;

            SpielHighscores sp = SpielHighscores.GetInstance();
            AddStats(SchiebePuzzle0Left, SchiebePuzzle0Right, sp.spiele[0]);
            AddStats(SchiebePuzzle1Left, SchiebePuzzle1Right, sp.spiele[1]);
            AddStats(SchiebePuzzle2Left, SchiebePuzzle2Right, sp.spiele[2]);
            AddStats(WortGzahlLeft, WortGzahlRight, sp.spiele[3]);
            AddStats(KopfrechnenLeft, KopfrechnenRight, sp.spiele[4]);
            AddStats(WechselGeldLeft, WechselGeldRight, sp.spiele[5]);
            AddStats(BuchstabenSalatLeft, BuchstabenSalatRight, sp.spiele[6]);
            AddStats(MemorizeLeft, MemorizeRight, sp.spiele[7]);
            AddStats(ConcentrateLeft, ConcentrateRight, sp.spiele[8]);
        }



        private void ButtonEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e) => ((sender as Button).Content as Image).Source = new BitmapImage(new Uri("ms-appx:///Pages/AssetBilder/" + (sender as Button).Tag + "-white.png"));
        private void ButtonExit(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e) => ((sender as Button).Content as Image).Source = new BitmapImage(new Uri("ms-appx:///Pages/AssetBilder/" + (sender as Button).Tag + "-mainBlue.png"));
        private void Click(object sender, RoutedEventArgs e)
        {
            if (sender == Logout) App.Abmelden(null);
            else if (sender == volumeOn || sender == volumeOff)
            {
                Sounddesign.PlaySoundAsync(Sounddesign.BUTTON_PUSH1);
                Sounddesign.CycleAmbient();
                volumeOn.Visibility = Sounddesign.CURRENT_AMBIENT == null ? Visibility.Collapsed : Visibility.Visible;
                volumeOff.Visibility = Sounddesign.CURRENT_AMBIENT == null ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void PivotItemLoaded(Pivot sender, PivotItemEventArgs args) => Sounddesign.PlaySoundAsync(Sounddesign.WOOSH);
        private void PivotItemLoadedSmall(Pivot sender, PivotItemEventArgs args) => Sounddesign.PlaySoundAsync(Sounddesign.WOOSH_SMALL);
        private void StarteSpiel(object sender, RoutedEventArgs e)
        {
            Sounddesign.PlaySoundAsync(Sounddesign.BUTTON_PUSH1);
            Button btn = (Button)sender;
            if (sender == SpuzzleBtn) this.Frame.Navigate(typeof(SchiebePuzzleOptions));
            else if (sender == KopfrechnenBTN) this.Frame.Navigate(typeof(SpielSeite), new KopfrechnenController());
            else if (sender == MemorizeBTN) this.Frame.Navigate(typeof(SpielSeite), new MemorizeController());
            else if (sender == ConcentrateBTN) this.Frame.Navigate(typeof(SpielSeite), new ConcentrateController());
            else if (sender == WortGZahlBTN) this.Frame.Navigate(typeof(SpielSeite), new FarbeGleichWortController());
            else if (sender == BuchstabenSalatBTN) this.Frame.Navigate(typeof(SpielSeite), new BuchstabenController());
            else if (sender == WechselGeldBTN) this.Frame.Navigate(typeof(SpielSeite), new WechselGeldController());
        }


        public static void AddStats(StackPanel left, StackPanel right, Spiel spiel)
        {
            left.Children.Add(NewTBlock("Name: "));
            left.Children.Add(NewTBlock("Spiele: "));
            left.Children.Add(NewTBlock("Bestzeit: "));
            left.Children.Add(NewTBlock("Schlechteste Zeit: "));
            left.Children.Add(NewTBlock("Durchschnitt: "));
            left.Children.Add(NewTBlock(""));
            left.Children.Add(NewTBlock("Rekordhalter"));
            left.Children.Add(NewTBlock("Globale Bestzeit"));
            left.Children.Add(NewTBlock("Globaler Durchschnitt"));

            right.Children.Add(NewTBlock(spiel.spiel));
            right.Children.Add(NewTBlock(spiel.anzahl_gespielt));
            right.Children.Add(NewTBlock(spiel.bestzeit));
            right.Children.Add(NewTBlock(spiel.schlechtesteZeit));
            right.Children.Add(NewTBlock(spiel.durchschnitt));
            right.Children.Add(NewTBlock(""));
            right.Children.Add(NewTBlock(spiel.rekordhalter));
            right.Children.Add(NewTBlock(spiel.globaleBestzeit));
            right.Children.Add(NewTBlock(spiel.globalerDurschnitt));
        }

        private static TextBlock NewTBlock(int z) => NewTBlock(Counter.GetFormatedCount(z));
        private static TextBlock NewTBlock(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontSize = (double)App.ResourceDict["BigFont"],
                Foreground = App.ResourceDict["SecondaryTextColor"] as SolidColorBrush,
                Margin = new Thickness(0,5,0,5)
            };
        }    
    }
}
