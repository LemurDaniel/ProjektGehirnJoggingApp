using Gehirn;
using GehirnJogging.Code;
using GehirnJogging.Code.Spiele;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using static GehirnJogging.Code.Nutzer;


// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace GehirnJogging.Pages
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    /// 

    public sealed partial class AnmeldeSeite : Page
    {
        private void ButtonEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e) => ((sender as Button).Content as Image).Source = new BitmapImage(new Uri("ms-appx:///Pages/AssetBilder/"+ (sender as Button).Tag + "-white.png"));
        private void ButtonExit(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e) => ((sender as Button).Content as Image).Source = new BitmapImage(new Uri("ms-appx:///Pages/AssetBilder/" + (sender as Button).Tag + "-mainBlue.png"));

        private bool IsLogin = true;
        private bool logingIn = false;
        public AnmeldeSeite()
        {
            this.InitializeComponent();
            App.currentPage = this;
            Sounddesign.PlaySoundAsync(Sounddesign.WOOSH);
            volumeOn.Visibility = Sounddesign.CURRENT_AMBIENT == null ? Visibility.Collapsed : Visibility.Visible;
            volumeOff.Visibility = Sounddesign.CURRENT_AMBIENT == null ? Visibility.Visible : Visibility.Collapsed;
            if(!BuchstabenAufgabe.read) BuchstabenAufgabe.ReadFile().GetAwaiter();           
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) => FehlerAusgabe.Text = e.Parameter as String ?? "";

        private void Click(object sender, RoutedEventArgs e)
        {
            if (sender == credits)
            {
                Frame.Navigate(typeof(Credits));
                return;
            }
            else if(sender == volumeOn || sender == volumeOff)
            {
                Sounddesign.PlaySoundAsync(Sounddesign.BUTTON_PUSH1);
                Sounddesign.CycleAmbient();
                volumeOn.Visibility = Sounddesign.CURRENT_AMBIENT == null ? Visibility.Collapsed : Visibility.Visible;
                volumeOff.Visibility = Sounddesign.CURRENT_AMBIENT == null ? Visibility.Visible : Visibility.Collapsed;
                return;
            }

            Sounddesign.PlaySoundAsync(Sounddesign.BUTTON_PUSH2);
            IsLogin = !IsLogin;
            ToggleAuswahl.IsChecked = true;
            ToggleAuswahl.Content = IsLogin ? "Login" : "Registrieren";
            loginButton.Content = IsLogin ? "Login" : "Registrieren";
            pwdConfirm.Visibility = IsLogin ? Visibility.Collapsed : Visibility.Visible;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Sounddesign.PlaySoundAsync(Sounddesign.BUTTON_PUSH1);
            // Prüfe Eingaben
            if (nickname.Text.Length < 3)
            {
                FehlerAusgabe.Text = "Nickname zu kurz. Min. 3 Zeichen";
                return;
            }
            if (pwd.Password.Length < 6)
            {
                FehlerAusgabe.Text = "Passwort zu kurz. Min. 6 Zeichen";
                return;
            }

            if (!IsLogin && !pwdConfirm.Password.Equals(pwd.Password))
            {
                FehlerAusgabe.Text = "Passwörter stimmen nicht überein";
                return;
            }

            if (logingIn) return;
            logingIn = true;

            WaitRing.IsActive = true;

            loginButton.IsEnabled = false;
            ToggleAuswahl.IsEnabled = false;
            nickname.IsEnabled = false;
            pwd.IsEnabled = false;
            pwdConfirm.IsEnabled = false;
            credits.IsEnabled = false;

            //Einloggen
            Request<Nutzer> request = null;
            string name = nickname.Text;
            string pass = pwd.Password;
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += (s, e1) =>
            {
                request = Nutzer.Einloggen((IsLogin ? App.FUNCTION_LOGIN : App.FUNCTION_REGISTER), name, pass).GetAwaiter().GetResult();
                if (request != null && request.Success) SpielHighscores.HoleHighscoresAsync().GetAwaiter();
            };
            bw.RunWorkerCompleted += (s, e1) => Login(request);
            bw.RunWorkerAsync();
        }


        private void Login(Request<Nutzer> request)
        {
            if(request.Success) this.Frame.Navigate(typeof(SpieleHub));
            else if (request.Errorcode == -4) FehlerAusgabe.Text = "Der Name ist bereits vergeben";
            else if (request.Errorcode == -1) FehlerAusgabe.Text = "Es wurde kein Account mit diesem Namen gefunden";
            else if (request.Errorcode == -2) FehlerAusgabe.Text = "Das angegebene Passwort ist falsch";
            else if (request.Errorcode == -3) FehlerAusgabe.Text = "Es konnte keine Verbindung mit der Datenbank hergestellt werden";
            else FehlerAusgabe.Text = "Es ist ein unerwarteter Fehler aufgetreten";

            logingIn = false;
            loginButton.IsEnabled = true;
            ToggleAuswahl.IsEnabled = true;
            nickname.IsEnabled = true;
            pwd.IsEnabled = true;
            pwdConfirm.IsEnabled = true;
            credits.IsEnabled = true;

            WaitRing.IsActive = false;
        }

    }
}
