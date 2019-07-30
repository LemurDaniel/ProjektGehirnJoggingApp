using GehirnJogging.Code;
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
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace GehirnJogging.Pages.Controller
{
    public sealed partial class WechselGeldController : UserControl, ISpielController
    {
        private SpielLogik spiellogik;
        private WechselgeldAufgabe aufgabe;

        WechselGeldEingabe[] eingaben = new WechselGeldEingabe[Enum.GetValues(typeof(Coins)).Length];

        public SpielLogik SpielLogik() => spiellogik;
        public int DurchgaengeMin() => 15;
        public int TimePenalty() => 300_000;
        public bool Undoable() => true;
        public void Undo()
        {
            if (aufgabe.Undo()) foreach (WechselGeldEingabe we in eingaben) we.Aktualisiere();
        }

        public WechselGeldController()
        {
            this.InitializeComponent();
            spiellogik = new SpielLogik(SpielHighscores.GetInstance().spiele[SpielHighscores.WECHSELGELD], this, () => new WechselgeldAufgabe());

            Coins[] coins = (Coins[])Enum.GetValues(typeof(Coins));           
            for(int i=0; i<eingaben.Length; i++)
            {
                eingaben[i] = new WechselGeldEingabe(coins[i]);
                if (i <= 7) Reihe1.Children.Add(eingaben[i]);
                else Reihe2.Children.Add(eingaben[i]);

                eingaben[i].Margin = new Thickness(10,10,10,10);
            }
        }

        public void ZeigeSpielobjekt(SpielLogik.SpielObjekt objekt)
        {
            aufgabe = objekt as WechselgeldAufgabe;
            Count.Text = objekt.Nummer + "/" + DurchgaengeMin();
            if (aufgabe.Finished)
            {
                Betrag.Text = aufgabe.betragString + " - " + aufgabe.erhaltenesGeldString + " = " + aufgabe.Convert(aufgabe.erhaltenesGeld - aufgabe.betrag);
                Erhalten.Visibility = Visibility.Collapsed;
            }
            else
            {
                Betrag.Text = "Betrag: " + aufgabe.betragString;
                Erhalten.Text = "Erhaltenes Geld: " + aufgabe.erhaltenesGeldString;
                Erhalten.Visibility = Visibility.Visible;
            }

            foreach (WechselGeldEingabe we in eingaben) we.StelleDar(aufgabe);
            Weiter.IsEnabled = !aufgabe.Finished;
        }

        private void Weiter_Click(object sender, RoutedEventArgs e) => aufgabe.PruefeEingabe();
    }
}
