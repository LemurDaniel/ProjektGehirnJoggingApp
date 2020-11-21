using GehirnJogging.Code.Spiele;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static GehirnJogging.Code.Spiele.SpielLogik;
using GehirnJogging.Code;
using GehirnJogging.Pages.ResourceDictionaries;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace GehirnJogging.Pages.Controller
{
    public sealed partial class KopfrechnenController : UserControl, ISpielController
    {
        private SpielLogik spiellogik;
        private Rechenaufgabe aufgabe;

        Button right, wrong;

        public SpielLogik SpielLogik() => spiellogik;
        public int DurchgaengeMin() => 25;
        public int TimePenalty() => 20_000;
        public bool Undoable() => false;
        public void Undo() { }

        public KopfrechnenController()
        {
            this.InitializeComponent();
            spiellogik = new SpielLogik(SpielHighscores.GetInstance().spiele[SpielHighscores.KOPFRECHNEN], this, () => new Rechenaufgabe());
            right = new ColorButton(ColorButton.GREEN)
            {
                FontSize = Eingabe.FontSize,
                Height = Eingabe.Height,
                Width = Eingabe.Width
            };
            wrong = new ColorButton(ColorButton.RED)
            {
                FontSize = Eingabe.FontSize,
                Height = Eingabe.Height,
                Width = Eingabe.Width
            };
            RightContainer.Child = right;
            WrongContainer.Child = wrong;
        }

        private void Go(object sender, RoutedEventArgs e)
        {
            if (aufgabe.Finished) return;
            aufgabe.Nutzereingabe = Eingabe.Text;
            aufgabe.PruefeEingabe();
        }
        public void ZeigeSpielobjekt(SpielObjekt objekt)
        {
            aufgabe = objekt as Rechenaufgabe;
            Count.Text = objekt.Nummer + "/" + DurchgaengeMin();
            Term.Text = aufgabe.Term;
            Eingabe.Text = "";
            wrong.Content = aufgabe.Nutzereingabe;
            right.Content = aufgabe.erg;

            WrongContainer.Visibility = aufgabe.Finished && !aufgabe.Correct ? Visibility.Visible : Visibility.Collapsed;
            RightContainer.Visibility = aufgabe.Finished ? Visibility.Visible:Visibility.Collapsed;            
            EingabeContainer.Visibility = !aufgabe.Finished ? Visibility.Visible : Visibility.Collapsed;
        }

    }
}
