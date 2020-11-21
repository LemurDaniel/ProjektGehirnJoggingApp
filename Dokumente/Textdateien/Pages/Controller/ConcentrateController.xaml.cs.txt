using System;
using GehirnJogging.Code;
using GehirnJogging.Code.Spiele;
using GehirnJogging.Pages.ResourceDictionaries;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace GehirnJogging.Pages.Controller
{
    public sealed partial class ConcentrateController : UserControl, ISpielController
    {
        private SpielLogik spiellogik;
        private ConcentrateAufgabe aufgabe;

        private Button[] buttons = new Button[ConcentrateAufgabe.KETTEN_ANZAHL];
        private Button right, wrong;

        public SpielLogik SpielLogik() => spiellogik;
        public int DurchgaengeMin() => 10;
        public int TimePenalty() => 120_000;
        public bool Undoable() => false;
        public void Undo(){}

        public ConcentrateController()
        {
            this.InitializeComponent();
            spiellogik = new SpielLogik(SpielHighscores.GetInstance().spiele[SpielHighscores.CONCENTRATE], this, ()=> new ConcentrateAufgabe());

            for (int i=0; i<buttons.Length; i++)
            {
                buttons[i] = new Button();
                buttons[i].Style = (Style)new ResourceDictionary { Source = new Uri("ms-appx:///Pages/ResourceDictionaries/MainButton.xaml") }["MainButtonStyle"];
                buttons[i].FontSize = (Double)App.ResourceDict["NormalFont"];
                int zahl = i;
                buttons[i].Click += (sender, e) => Go(zahl);

                Border border = new Border
                {
                    CornerRadius = new CornerRadius(15),
                    Margin = new Thickness(0, 15, 0, 15),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Child = buttons[i]
                };
                Container.Children.Add(border);
            }
            right = new ColorButton(ColorButton.GREEN);
            wrong = new ColorButton(ColorButton.RED);
            right.FontSize = (Double)App.ResourceDict["NormalFont"];
            wrong.FontSize = (Double)App.ResourceDict["NormalFont"];
        }

        public void ZeigeSpielobjekt(SpielLogik.SpielObjekt objekt)
        {
            if (aufgabe != null)
            {
                if(right.Parent!=null)((Border)right.Parent).Child = buttons[aufgabe.pos];
                if (wrong.Parent != null) ((Border)wrong.Parent).Child = buttons[aufgabe.Nutzereingabe];
            }

            Count.Text = objekt.Nummer + "/" + DurchgaengeMin();
            aufgabe = objekt as ConcentrateAufgabe;

            Vorgabe.Text = aufgabe.zeichenkette;
            for (int i = 0; i < buttons.Length; i++) buttons[i].Content = aufgabe.zeichenketten[i];

            if (!aufgabe.Finished) foreach (Button button in buttons) button.IsEnabled = true;
            else
            {
                foreach (Button button in buttons) button.IsEnabled = false;
                ((Border)buttons[aufgabe.pos].Parent).Child = right;
                right.Content = aufgabe.zeichenkette;
                if (!aufgabe.Correct)
                {
                    ((Border)buttons[aufgabe.Nutzereingabe].Parent).Child = wrong;
                    wrong.Content = aufgabe.zeichenketten[aufgabe.Nutzereingabe];
                }
            }
        }

        private void Go(int i) {
            aufgabe.Nutzereingabe = i;
            aufgabe.PruefeEingabe();        
        }
    }
}
