using GehirnJogging.Pages;
using GehirnJogging.Pages.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using static GehirnJogging.Code.Sounddesign;
using static GehirnJogging.Code.SpielHighscores;

namespace GehirnJogging.Code.Spiele
{
    public class SpielLogik
    {

        public readonly ISpielController spController;
        public readonly Counter counter = new Counter();
        private Spiel spiel;
        private Spiel spielOld = new Spiel();
        public Spiel Spiel { get => spiel; }
        public Spiel SpielOld { get => spielOld; }

        private Action onGameEnd;
        private Action onTimePenalty;
        public Action OnGameEnd { get => onGameEnd; set => onGameEnd = value; }
        public Action OnTimePenalty { get => onTimePenalty; set => onTimePenalty = value; }

        private int timePenalties = 0;
        private int timePenaltiesTotal = 0;
        private int ergebnis = 0;
        public int TimePenalties { get => timePenalties; }
        public int TimePenaltiesTotal { get => timePenaltiesTotal; }
        public int Ergebnis { get => ergebnis; }

        private bool gameEnded = false;
        public bool GameEnded { get => gameEnded; }

        protected int durchgaengeNow = -1;
        private Func<SpielObjekt> GetNewSpielObjekt;
        protected SpielObjekt[] spielobjekte;
        private SpielObjekt current;
        private StackPanel appbarcontent;
        public StackPanel Appbarcontent { get => appbarcontent ?? GenereateAppBarContent(); }


        public SpielLogik(Spiel spiel, ISpielController spController, Func<SpielObjekt> GetNewSpielObjekt) {
            this.spiel = spiel;
            this.spController = spController;
            this.GetNewSpielObjekt = GetNewSpielObjekt;
            spielobjekte = new SpielObjekt[spController.DurchgaengeMin()];
            for(int i=0; i<spielobjekte.Length; i++)
            {
                spielobjekte[i] = GetNewSpielObjekt.Invoke();
                spielobjekte[i].Nummer = i + 1;
                spielobjekte[i].sl = this;
            }
        }

        protected async void EndGameAsync()
        {
            if (gameEnded) return;
            ZeigeObjekt(current);
            counter.Stop();
            gameEnded = true;

            spielOld = spiel.GetCopy();

            ergebnis = (int)counter.GetTimeSpan().TotalMilliseconds + timePenaltiesTotal;

            if (spiel.bestzeit > ergebnis || spiel.bestzeit==0) spiel.bestzeit = ergebnis;
            if (spiel.schlechtesteZeit < ergebnis) spiel.schlechtesteZeit = ergebnis;
            spiel.anzahl_gespielt++;
            spiel.akummulierteSpielzeit += ergebnis;
            spiel.durchschnitt = (int)(spiel.akummulierteSpielzeit / spiel.anzahl_gespielt);

            await SpielHighscores.AktualisiereHighscoresAsync(spiel);

            if (onGameEnd != null) onGameEnd.Invoke();
        }

        protected void AddTimePenalty()
        {
            if (onTimePenalty != null) onTimePenalty.Invoke();
            timePenaltiesTotal += spController.TimePenalty();
            timePenalties++;
        }

        public void Start()
        {
            if (gameEnded) return;
            Progress();
            counter.Start();
        }
        public void Cancel()    =>  counter.Stop();



        private StackPanel GenereateAppBarContent()
        {
            appbarcontent = new StackPanel();
            appbarcontent.Orientation = Orientation.Horizontal;
            for(int i=0; i< spController.DurchgaengeMin(); i++)
            {
                Border border = new Border
                {
                    CornerRadius = new CornerRadius(50),
                    Child = spielobjekte[i].Button,
                    Margin = new Thickness(10, 10, 10, 10)
                };

                appbarcontent.Children.Add(border);
            }
            return appbarcontent;
        }

        private void Progress()
        {
            if (durchgaengeNow >= spController.DurchgaengeMin() - 1) EndGameAsync();
            else spielobjekte[++durchgaengeNow].Enabled = true;
            ZeigeObjekt(spielobjekte[durchgaengeNow]);
        }


        public void NextObjekt() => ZeigeObjekt( spielobjekte[Math.Min(current.Nummer, spielobjekte.Length-1)] );
        public void PrevObjekt() => ZeigeObjekt(spielobjekte[Math.Max(current.Nummer-2, 0)]);
        public void ZeigeObjekt(SpielObjekt objekt)
        {
            if (objekt == null || !objekt.Enabled) return;
            current = objekt;
            spController.ZeigeSpielobjekt(current);
        }

        abstract public class SpielObjekt
        {
            public static readonly BitmapImage CORRECT_GREEN = new BitmapImage(new Uri("ms-appx:///Pages/AssetBilder/accept-circular-button-outline-green.png"));
            public static readonly BitmapImage CORRECT_WHITE = new BitmapImage(new Uri("ms-appx:///Pages/AssetBilder/accept-circular-button-outline-white.png"));
            public static readonly BitmapImage WRONG_RED = new BitmapImage(new Uri("ms-appx:///Pages/AssetBilder/cancel-button-red.png"));
            public static readonly BitmapImage WRONG_WHITE = new BitmapImage(new Uri("ms-appx:///Pages/AssetBilder/cancel-button-white.png"));
            public static readonly BitmapImage UNDEFINED_BLUE = new BitmapImage(new Uri("ms-appx:///Pages/AssetBilder/information-mainBlue.png"));
            public static readonly BitmapImage UNDEFINED_WHITE = new BitmapImage(new Uri("ms-appx:///Pages/AssetBilder/information-white.png"));

            public static readonly Style ICON = (Style)new ResourceDictionary { Source = new Uri("ms-appx:///Pages/ResourceDictionaries/IconButton.xaml") }["IconButtonStyle"];
            public static readonly Style ICON_RED = (Style)new ResourceDictionary { Source = new Uri("ms-appx:///Pages/ResourceDictionaries/IconButton.xaml") }["IconButtonStyleRed"];
            public static readonly Style ICON_GREEN = (Style)new ResourceDictionary { Source = new Uri("ms-appx:///Pages/ResourceDictionaries/IconButton.xaml") }["IconButtonStyleGreen"];

            protected static Random rand = new Random();

            internal SpielLogik sl;
            private int nummer;
            private bool finished = false , correct = false, enabled = false;
            private Button button;
            public int Nummer { get => nummer; set => nummer = value; }
            public bool Finished { get => finished; set => finished = value; }
            public bool Enabled { get => enabled; set => SetEnable(value); }
            public bool Correct { get => correct; set => SetCorrect(value); }
            public Button Button { get => button; set => button = value; }

            protected Sound RightSound = Sounddesign.RIGHT;
            protected Sound WrongSound = Sounddesign.WRONG1;

            protected SpielObjekt()
            {
                button = new Button
                {
                    Content = new Image { Source = SpielObjekt.UNDEFINED_WHITE, Height = 50, Width = 50, Stretch = Stretch.Uniform },
                    IsEnabled = false,
                    Style = ICON
                };
                button.PointerEntered += (s, e) => ((Image)button.Content).Source = SpielObjekt.UNDEFINED_WHITE;
                button.PointerExited += (s, e) => ((Image)button.Content).Source = SpielObjekt.UNDEFINED_BLUE;
                button.Click += (sender, e) => { Sounddesign.PlaySoundAsync(Sounddesign.BUTTON_PUSH1); sl.ZeigeObjekt(sl.spielobjekte[nummer-1]); };
            }

            private void SetEnable(bool value)
            {
                ((Image)button.Content).Source = value ? UNDEFINED_BLUE : UNDEFINED_WHITE;
                Button.IsEnabled = value;
                enabled = value;
            }

            private void SetCorrect(bool value)
            {
                if (Finished) return;

                finished = true;
                ((Image)button.Content).Source = value ? CORRECT_GREEN : WRONG_RED;
                button.PointerEntered += (s, e) => ((Image)button.Content).Source = value ? CORRECT_WHITE : WRONG_WHITE;
                button.PointerExited += (s, e) => ((Image)button.Content).Source = value ? CORRECT_GREEN : WRONG_RED;
                button.Style = value ? ICON_GREEN : ICON_RED;
                button.IsEnabled = true;
                correct = value;
                if (correct) Sounddesign.PlaySoundAsync(RightSound);
                else
                {
                    Sounddesign.PlaySoundAsync(WrongSound);
                    sl.AddTimePenalty();
                }
                sl.Progress();
            }
        }
    }
}
