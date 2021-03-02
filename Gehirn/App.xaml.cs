using GehirnJogging.Code;
using GehirnJogging.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Gehirn
{
    sealed partial class App : Application
    {
        public static readonly bool OFFLINE_MODE = true;
        public static readonly string FUNCTION_KEY = "?code=IgVPa81pVjQSLlztCejHkoHjgtVCE2MtFEGkuHhkgYTqcPTLdvOq6A==";
        //public static readonly string FUNCTION_KEY = "";
        //public static readonly string FUNCTION_URI = "http://localhost:7071/api/";
        public static readonly string FUNCTION_URI = "http://brainjogging.azurewebsites.net/api/";
        public static readonly string FUNCTION_LOGIN = "Login";
        public static readonly string FUNCTION_REGISTER = "Register";
        public static readonly string FUNCTION_GET_HIGH_SCORES = "GetHighScores";
        public static readonly string FUNCTION_UPDATE_HIGHSCORE = "UpdateHighScore";
        public static readonly string FUNCTION_GET_IMAGES_FILES = "GetImageFiles";
        public static readonly string FUNCTION_SET_IMAGES_FILES = "SetImagesFiles";
        public static readonly string FUNCTION_DEL_IMAGES_FILES = "DelImageFiles";
        public static readonly string FUNCTION_SET_STANDARD_IMAGE_FILES = "GetStandardImages";

        public static readonly ResourceDictionary ResourceDict = new ResourceDictionary
        {
            Source = new Uri("ms-appx:///Pages/ResourceDictionaries/ColorBrushes.xaml")
        };

        public static Page currentPage;
        public static void Abmelden(string s)
        {
            Nutzer.Getinstance().Abmelden();
            currentPage.Frame.Navigate(typeof(AnmeldeSeite), s);
        }
        /// <summary>
        /// Initialisiert das Singletonanwendungsobjekt. Dies ist die erste Zeile von erstelltem Code
        /// und daher das logische Äquivalent von main() bzw. WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Anwendung durch den Endbenutzer normal gestartet wird. Weitere Einstiegspunkte
        /// werden z. B. verwendet, wenn die Anwendung gestartet wird, um eine bestimmte Datei zu öffnen.
        /// </summary>
        /// <param name="e">Details über Startanforderung und -prozess.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // App-Initialisierung nicht wiederholen, wenn das Fenster bereits Inhalte enthält.
            // Nur sicherstellen, dass das Fenster aktiv ist.
            if (rootFrame == null)
            {
                // Frame erstellen, der als Navigationskontext fungiert und zum Parameter der ersten Seite navigieren
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Zustand von zuvor angehaltener Anwendung laden
                }

                // Den Frame im aktuellen Fenster platzieren
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // Wenn der Navigationsstapel nicht wiederhergestellt wird, zur ersten Seite navigieren
                    // und die neue Seite konfigurieren, indem die erforderlichen Informationen als Navigationsparameter
                    // übergeben werden
                    rootFrame.Navigate(typeof(AnmeldeSeite), e.Arguments);
                }
                // Sicherstellen, dass das aktuelle Fenster aktiv ist
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Navigation auf eine bestimmte Seite fehlschlägt
        /// </summary>
        /// <param name="sender">Der Rahmen, bei dem die Navigation fehlgeschlagen ist</param>
        /// <param name="e">Details über den Navigationsfehler</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Ausführung der Anwendung angehalten wird.  Der Anwendungszustand wird gespeichert,
        /// ohne zu wissen, ob die Anwendung beendet oder fortgesetzt wird und die Speicherinhalte dabei
        /// unbeschädigt bleiben.
        /// </summary>
        /// <param name="sender">Die Quelle der Anhalteanforderung.</param>
        /// <param name="e">Details zur Anhalteanforderung.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Anwendungszustand speichern und alle Hintergrundaktivitäten beenden
            deferral.Complete();
        }
    }
}
