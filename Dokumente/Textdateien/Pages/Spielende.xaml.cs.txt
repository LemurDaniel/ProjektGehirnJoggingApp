using GehirnJogging.Code;
using GehirnJogging.Code.Spiele;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace GehirnJogging.Pages
{

    public sealed partial class Spielende : Page
    {
        private SpielLogik spielLogik;

        public Spielende()
        {
            this.InitializeComponent();
            Sounddesign.PlaySoundAsync(Sounddesign.WOOSH);
        }
        private void ButtonEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e) => ((sender as Button).Content as Image).Source = new BitmapImage(new Uri("ms-appx:///Pages/AssetBilder/" + (sender as Button).Tag + "-white.png"));
        private void ButtonExit(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e) => ((sender as Button).Content as Image).Source = new BitmapImage(new Uri("ms-appx:///Pages/AssetBilder/" + (sender as Button).Tag + "-mainBlue.png"));
        private void PivotItemLoadedSmall(Pivot sender, PivotItemEventArgs args) => Sounddesign.PlaySoundAsync(Sounddesign.WOOSH_SMALL);

        private void Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Sounddesign.PlaySoundAsync(Sounddesign.BUTTON_PUSH1);
            if (sender == Back) Frame.Navigate(typeof(SpielSeite), spielLogik.spController);
            if (sender == BackToMenu) Frame.Navigate(typeof(SpieleHub));
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            spielLogik = e.Parameter as SpielLogik;
            if (spielLogik == null) return;

            SpieleHub.AddStats(NewScoreLeft, NewScoreRight, spielLogik.Spiel);
            SpieleHub.AddStats(OldScoreLeft, OldScoreRight, spielLogik.SpielOld);

            if (spielLogik.SpielOld.globaleBestzeit > spielLogik.Spiel.globaleBestzeit || spielLogik.SpielOld.globaleBestzeit==0) WorldRecordPopUpAsync(500);
            else if (spielLogik.SpielOld.bestzeit > spielLogik.Spiel.bestzeit || spielLogik.SpielOld.bestzeit==0) RecordPopUpAsync(500);
        }

        private async void WorldRecordPopUpAsync(int delay)
        {
            WorldRecordPopUp.IsOpen = true;
            WorldRecordPopUp.Opacity = 0;
            await Task.Delay(delay);
            WorldRecordPopUp.VerticalOffset = PopUpBorder.ActualHeight / -2;
            WorldRecordPopUp.HorizontalOffset = PopUpBorder.ActualWidth / -2;
            WorldRecordPopUp.Opacity = 1;  
            Ccontrol.IsEnabled = false;

            RecordOldName.Text = spielLogik.SpielOld.rekordhalter;
            RecordOld.Text = Counter.GetFormatedCount(spielLogik.SpielOld.globaleBestzeit);
            RecordNewName.Text = spielLogik.Spiel.rekordhalter;
            RecordNew.Text = Counter.GetFormatedCount(spielLogik.Spiel.globaleBestzeit);

            Sounddesign.PlaySoundAsync(Sounddesign.RECORD_BIG);

            PopUpClose.Click += Handler;
        }
        private void Handler(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Ccontrol.IsEnabled = true;
            WorldRecordPopUp.IsOpen = false;
            RecordPopUpAsync(500);
            PopUpClose.Click -= Handler;
        }

        private async void RecordPopUpAsync(int delay)
        {
            WorldRecordPopUp.IsOpen = true;
            WorldRecordPopUp.Opacity = 0;
            await Task.Delay(delay);
            WorldRecordPopUp.VerticalOffset = PopUpBorder.ActualHeight / -2;
            WorldRecordPopUp.HorizontalOffset = PopUpBorder.ActualWidth / -2;
            WorldRecordPopUp.Opacity = 1;  // Wenn Unsichtbar ist die Höhe Null, Bei Opacity 0 ist richtige Höhe verfügbar
            Ccontrol.IsEnabled = false;

            WRecordT1.Text = "Du hast deinen Persönlichen Rekord gebrochen";

            RecordOldName.Text = "Alter Rekord:";
            RecordOld.Text = Counter.GetFormatedCount(spielLogik.SpielOld.bestzeit);
            RecordNewName.Text = "Neuer Rekord:";
            RecordNew.Text = Counter.GetFormatedCount(spielLogik.Spiel.bestzeit);

            Sounddesign.PlaySoundAsync(Sounddesign.RECORD);
            PopUpClose.Click += (sender, e) =>
            {
                WorldRecordPopUp.IsOpen = false;
                Ccontrol.IsEnabled = true;
             };
        }


        private static TextBlock NtextBlock(string text) => NtextBlock(text, 20, 10, 20, 10);
        private static TextBlock NtextBlock(string text, int i, int i2, int i3, int i4)
        {
            return new TextBlock
            {
                TextAlignment = TextAlignment.Left,
                Foreground = (SolidColorBrush)App.ResourceDict["PopupText"],
                FontSize = 40,
                Margin = new Thickness(i, i2, i3, i4),
                Text = text
            };
        }

        public static Popup GetPopUp(SpielLogik spiel, Action onClick)
        {
            Popup popup = new Popup();
            popup.VerticalAlignment = VerticalAlignment.Center;
            popup.HorizontalAlignment = HorizontalAlignment.Center;
            popup.Opacity = 1;

            Border border = new Border();
            border.CornerRadius = new CornerRadius(50);
            border.Padding = new Thickness(50, 50, 50, 50);
            border.BorderThickness = (Thickness)App.ResourceDict["PopupBoderThickness"];
            border.Background = (ImageBrush)App.ResourceDict["PopupBackground"];
            border.BorderBrush = (SolidColorBrush)App.ResourceDict["PopupBorderColor"];
            border.Name = "PopUpBorderInfo";

            StackPanel stack = new StackPanel();
            stack.Children.Add(NtextBlock("Herlichen Glückwunsch"));
            stack.Children.Add(NtextBlock("Du hast das Spiel beendet"));


            StackPanel stack2 = new StackPanel();
            stack2.HorizontalAlignment = HorizontalAlignment.Left;
            stack2.Margin = new Thickness(0,20,0,20);
            stack2.Children.Add(NtextBlock("Ergebnis"));
            stack2.Children.Add(NtextBlock("Zeitstrafen"));
            stack2.Children.Add(NtextBlock("ZeitstrafenSumme"));
            stack2.Children.Add(NtextBlock("Endergebnis"));


            StackPanel stack3 = new StackPanel();
            stack3.HorizontalAlignment = HorizontalAlignment.Right;
            stack3.Margin = new Thickness(0, 20, 0, 20);
            stack3.Children.Add(NtextBlock(spiel.counter.GetFormatedCount()));
            stack3.Children.Add(NtextBlock(spiel.TimePenalties+" Mal"));
            stack3.Children.Add(NtextBlock(Counter.GetFormatedCount(spiel.TimePenaltiesTotal)));
            stack3.Children.Add(NtextBlock(Counter.GetFormatedCount(spiel.Ergebnis)));

            StackPanel stack4 = new StackPanel();
            stack4.Orientation = Orientation.Horizontal;
            stack4.Children.Add(stack2);
            stack4.Children.Add(stack3);


            Border border2 = new Border();
            border2.HorizontalAlignment = HorizontalAlignment.Center;
            border2.CornerRadius = new CornerRadius(15);
            border2.Margin = new Thickness(0, 20, 0, 20);
            border2.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xF0, 0xF8, 0xFF));

            Button button = new Button();
            button.Style = (Style)new ResourceDictionary {Source = new Uri("ms-appx:///Pages/ResourceDictionaries/MainButton.xaml")}["MainButtonStyle"];
            button.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x78, 0xD7));
            button.Click += (sender, e) => onClick.Invoke();
            button.FontSize = 40;
            button.Content = "Weiter zu den Highscores";


            popup.Child = border;
            border.Child = stack;

            stack.Children.Add(stack4);

            stack.Children.Add(border2);
            border2.Child = button;

            return popup;
        }

    }
}
