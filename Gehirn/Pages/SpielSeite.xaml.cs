using GehirnJogging.Code;
using GehirnJogging.Code.Spiele;
using GehirnJogging.Pages.Controller;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
using static GehirnJogging.Code.SpielHighscores;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace GehirnJogging.Pages
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class SpielSeite : Page
    {

        private Page previous = App.currentPage;
        private SpielLogik spiellogik;
        private Popup popup;

        public SpielSeite()
        {
            this.InitializeComponent();
            volumeOn.Visibility = Sounddesign.CURRENT_AMBIENT == null ? Visibility.Collapsed : Visibility.Visible;
            volumeOff.Visibility = Sounddesign.CURRENT_AMBIENT == null ? Visibility.Visible : Visibility.Collapsed;
            Sounddesign.PlaySoundAsync(Sounddesign.GAME_START);
        }

        private void ButtonEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e) => ((sender as Button).Content as Image).Source = new BitmapImage(new Uri("ms-appx:///Pages/AssetBilder/" + (sender as Button).Tag + "-white.png"));
        private void ButtonExit(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e) => ((sender as Button).Content as Image).Source = new BitmapImage(new Uri("ms-appx:///Pages/AssetBilder/" + (sender as Button).Tag + "-mainBlue.png"));

        protected override void OnNavigatedTo(NavigationEventArgs e) => Start(e.Parameter as ISpielController);
        private void Click(object sender, RoutedEventArgs e)
        {
            Sounddesign.PlaySoundAsync(Sounddesign.BUTTON_PUSH1);
            if (sender == Undo) spiellogik.spController.Undo();
            else if (sender == PrevOb) spiellogik.PrevObjekt();
            else if (sender == NextOb) spiellogik.NextObjekt();
            else if (sender == Forward) MoveToHighScore();
            else if (sender == Menu) popup.IsOpen = !popup.IsOpen;
            else if (sender == volumeOff || sender == volumeOn)
            {
                Sounddesign.PlaySoundAsync(Sounddesign.BUTTON_PUSH1);
                Sounddesign.CycleAmbient();
                volumeOn.Visibility = Sounddesign.CURRENT_AMBIENT == null ? Visibility.Collapsed : Visibility.Visible;
                volumeOff.Visibility = Sounddesign.CURRENT_AMBIENT == null ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (sender == Back)
            {
                spiellogik.Cancel();
                Frame.Navigate(previous.GetType());
            }
        }

        private void Start(ISpielController sp)
        {
            this.spiellogik = sp.SpielLogik();

            Undo.Visibility = sp.Undoable() ? Visibility.Visible : Visibility.Collapsed;
            PrevOb.Visibility  = NextOb.Visibility = sp.DurchgaengeMin()>1 ? Visibility.Visible : Visibility.Collapsed;

            SpielFeld.Child = (UserControl)spiellogik.spController;
            BotAppBar.Content = spiellogik.Appbarcontent;

            globalBest.Text = "WR: " + Counter.GetFormatedCount(spiellogik.Spiel.globaleBestzeit);
            best.Text = "PR: " + Counter.GetFormatedCount(spiellogik.Spiel.bestzeit);

            if (spiellogik.GameEnded)
            {
                Timer(count, best, globalBest, spiellogik.SpielOld);
                GameEndAsync();
                return;
            }

            spiellogik.counter.OnIncrement += async () => await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => Timer(count, best, globalBest, spiellogik.Spiel));
            spiellogik.OnGameEnd += GameEndAsync;
            spiellogik.Start();
        }


        // Counter Erhöht
        public void Timer(TextBlock count, TextBlock best, TextBlock globalBest, Spiel spiel)
        {
            count.Text = "Zeit: " + spiellogik.counter.GetFormatedCount() + (spiellogik.TimePenaltiesTotal == 0 ? "" : " + " + Counter.GetFormatedCount(spiellogik.TimePenaltiesTotal));
            int time = (int)spiellogik.counter.GetTimeSpan().TotalMilliseconds;

            if (time > spiel.globaleBestzeit || spiel.globaleBestzeit == 0) globalBest.Visibility = Visibility.Collapsed;
            else globalBest.Opacity = 1 - time / (double)spiel.globaleBestzeit;

            if (time > spiel.bestzeit || spiel.bestzeit == 0) best.Visibility = Visibility.Collapsed;
            else best.Opacity = 1 - time / (double)spiel.bestzeit;
        }


        // Game Ended
        private async void GameEndAsync()
        {
            Forward.Visibility = Visibility.Visible;
            Menu.Visibility = Visibility.Visible;

            if (popup == null)
            {
                popup = Spielende.GetPopUp(spiellogik, MoveToHighScore);
                BaseGrid.Children.Add(popup);
            }

            if (popup.IsOpen) return;

            popup.Opacity = 0;
            popup.IsOpen = true;
            await Task.Delay(10);
            popup.VerticalOffset = ((Border)popup.Child).ActualWidth / -2;
            popup.HorizontalOffset = ((Border)popup.Child).ActualHeight / -2;
            popup.Opacity = 1;
            Sounddesign.PlaySoundAsync(Sounddesign.GAME_COMPLETE);

        }

        private void MoveToHighScore()
        {
            BotAppBar.Content = null;
            BaseGrid.Children.Clear();
            SpielFeld.Child = null;
            Frame.Navigate(typeof(Spielende), spiellogik);
        }
    }
}
