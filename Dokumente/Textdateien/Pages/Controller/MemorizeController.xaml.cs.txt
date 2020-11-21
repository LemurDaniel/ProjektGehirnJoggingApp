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
    public sealed partial class MemorizeController : UserControl, ISpielController
    {
        private SpielLogik spiellogik;
        private MemorizeAufgabe aufgabe;

        private Border[] btnContainer = new Border[MemorizeAufgabe.MEMANZAHL];
        private Button[] buttons = new Button[MemorizeAufgabe.MEMANZAHL];
        private Button[] right = new Button[MemorizeAufgabe.MEMANZAHL];

        private Button wrong;

        public SpielLogik SpielLogik() => spiellogik;
        public int DurchgaengeMin() => 25;
        public int TimePenalty() => 30_000;
        public bool Undoable() => false;
        public void Undo() { }

        private int height = 120;
        private int font = 80;

        public MemorizeController()
        {
            this.InitializeComponent();
            spiellogik = new SpielLogik(SpielHighscores.GetInstance().spiele[SpielHighscores.MEMORIZE], this, () => new MemorizeAufgabe());

            for (int i = 0; i < buttons.Length; i++)
            {
                SpielFeld.RowDefinitions.Add(new RowDefinition());
                SpielFeld.ColumnDefinitions.Add(new ColumnDefinition());

                buttons[i] = new Button();
                buttons[i].Style = (Style)new ResourceDictionary { Source = new Uri("ms-appx:///Pages/ResourceDictionaries/MainButton.xaml") }["MainButtonStyle"];
                buttons[i].FontSize = font;
                int zahl = i;
                buttons[i].Click += (sender, e) => Go(buttons[zahl], zahl);
                buttons[i].Height = buttons[i].Width = height;

                btnContainer[i] = new Border
                {
                    CornerRadius = new CornerRadius(15),
                    Margin = new Thickness(15, 15, 15, 15),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Child = buttons[i]
                };
                SpielFeld.Children.Add(btnContainer[i]);

                right[i] = new ColorButton(ColorButton.GREEN);
                right[i].Content = i + 1 + "";
                right[i].FontSize = buttons[0].FontSize;
                right[i].Height = right[i].Width = height;
            }
            wrong = new ColorButton(ColorButton.RED);
            wrong.FontSize = buttons[0].FontSize;
            wrong.Height = wrong.Width = height;
        }

        public void ZeigeSpielobjekt(SpielLogik.SpielObjekt objekt)
        {
            aufgabe = objekt as MemorizeAufgabe;
            Count.Text = objekt.Nummer + "/" + DurchgaengeMin();

            for (int i = 0; i < MemorizeAufgabe.MEMANZAHL; i++)
            {
                if (buttons[i].Parent != null) ((Border)buttons[i].Parent).Child = null;
                if (right[i].Parent != null) ((Border)right[i].Parent).Child = null;

                Grid.SetColumn(btnContainer[i], aufgabe.positionen[i].col);
                Grid.SetRow(btnContainer[i], aufgabe.positionen[i].row);
                buttons[i].Content = aufgabe.Finished || aufgabe.Nutzereingabe == 0 ? i + 1 + "" : " ";
                buttons[i].IsEnabled = !aufgabe.Finished && aufgabe.Status[i] == 0;

                if (aufgabe.Status[i] == 0 || !aufgabe.Finished) btnContainer[i].Child = buttons[i];
                else if (aufgabe.Status[i] == 1) btnContainer[i].Child = right[i];
                else if (aufgabe.Status[i] == -1)
                {
                    if (wrong.Parent != null) ((Border)wrong.Parent).Child = null;
                    btnContainer[i].Child = wrong;
                    wrong.Content = i + 1 + "";
                }

            }
        }

        private void Go(Button sender, int zahl)
        {
            if (zahl == 0) foreach (Button button in buttons) button.Content = "";
            sender.IsEnabled = false;
            aufgabe.Nutzereingabe = zahl;
        }


    }
}
