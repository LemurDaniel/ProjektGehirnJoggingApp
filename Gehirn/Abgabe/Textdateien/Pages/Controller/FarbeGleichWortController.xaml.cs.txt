using GehirnJogging.Code;
using GehirnJogging.Code.Spiele;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace GehirnJogging.Pages.Controller
{
    public sealed partial class FarbeGleichWortController : UserControl, ISpielController
    {
        private SpielLogik spiellogik;
        private FarbeAufgabe farbeAufgabe;
        private PointerEventHandler ph;

        public SpielLogik SpielLogik() => spiellogik;
        public int DurchgaengeMin() => 80;
        public int TimePenalty() => 5_000;
        public bool Undoable() => false;
        public void Undo() { }

        public FarbeGleichWortController()
        {
            this.InitializeComponent();
            spiellogik = new SpielLogik(SpielHighscores.GetInstance().spiele[SpielHighscores.WORT_GLEICH_FARBE], this, () => new FarbeAufgabe());
            ph = new PointerEventHandler(Go);
        }
     
        public void ZeigeSpielobjekt(SpielLogik.SpielObjekt objekt)
        {
            if(farbeAufgabe!=null)farbeAufgabe.FarbButton.RemoveHandler(PointerPressedEvent, ph);

            farbeAufgabe = objekt as FarbeAufgabe;
            Count.Text = objekt.Nummer + "/" + DurchgaengeMin();

            ButtonContainer.Child = farbeAufgabe.FarbButton;
            farbeAufgabe.FarbButton.HorizontalContentAlignment = HorizontalAlignment.Center;
            farbeAufgabe.FarbButton.Width = 650;
            farbeAufgabe.FarbButton.AddHandler(PointerPressedEvent, ph, true);
        }

        private void Go(object sender, PointerRoutedEventArgs e)
        {
            if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && !e.GetCurrentPoint(this).Properties.IsRightButtonPressed) return;
            farbeAufgabe.Nutzereingabe = e.GetCurrentPoint(this).Properties.IsLeftButtonPressed;
            farbeAufgabe.FarbButton.IsEnabled = false;

            farbeAufgabe.PruefeEingabe();
        }
    }
}
