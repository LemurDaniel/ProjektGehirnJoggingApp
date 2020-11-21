using GehirnJogging.Code;
using GehirnJogging.Code.Spiele;
using GehirnJogging.Pages.ResourceDictionaries;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
    public sealed partial class BuchstabenController : UserControl, ISpielController
    {
        private Border[] vorgabeContainer = new Border[BuchstabenAufgabe.MAX_SIZE];
        private Border[] eingabeContainer = new Border[BuchstabenAufgabe.MAX_SIZE];
        private Button[] buttons = new Button[BuchstabenAufgabe.MAX_SIZE];

        Dictionary<Button, Border> map = new Dictionary<Button, Border>();

        private Button[] right = new Button[BuchstabenAufgabe.MAX_SIZE];
        private Button[] wrong = new Button[BuchstabenAufgabe.MAX_SIZE];

        private Button moveOverlay;
        private Button tempButton;

        private SpielLogik spiellogik;
        private BuchstabenAufgabe aufgabe;

        public SpielLogik SpielLogik() => spiellogik;
        public int DurchgaengeMin() => 20;
        public int TimePenalty() => 60_000;
        public bool Undoable() => true;
        public void Undo()
        {
            if (aufgabe.Undo()) ZeigeSpielobjekt(aufgabe);
        }
        private void Aufgeben(object sender, RoutedEventArgs e) => aufgabe.Pruefe();



        private int height = 150, width = 150;
        private int font = 50;


        public BuchstabenController()
        {
            this.InitializeComponent();
            spiellogik = new SpielLogik(SpielHighscores.GetInstance().spiele[SpielHighscores.BUCHSTABENSALAT], this, () => new BuchstabenAufgabe());

            for(int i=0; i<vorgabeContainer.Length; i++)
            {
                vorgabeContainer[i] = GetBorder();
                eingabeContainer[i] = GetBorder();

                vorgabeStack.Children.Add(vorgabeContainer[i]);
                eingabeStack.Children.Add(eingabeContainer[i]);

                buttons[i] = new ColorButton(ColorButton.BLAU)
                {
                    Content = i,
                    Height = height,
                    Width = width,
                    FontSize = font
                };
                buttons[i].PointerEntered += (sender, e) => Entered(sender as Button);

                map.Add(buttons[i], vorgabeContainer[i]);
                vorgabeContainer[i].Child = buttons[i];

                right[i] = new ColorButton(ColorButton.GREEN)
                {
                    Content = i,
                    Height = height,
                    Width = width,
                    FontSize = font
                };
                wrong[i] = new ColorButton(ColorButton.RED)
                {
                    Content = i,
                    Height = height,
                    Width = width,
                    FontSize = font
                };
            }

            moveOverlay = new ColorButton(ColorButton.ORANGE)
            {
                Height = height,
                Width = width,
                FontSize = font,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                ManipulationMode = ManipulationModes.All,
                Visibility = Visibility.Collapsed,
                RenderTransform = new CompositeTransform
                {
                    CenterX = 0.5,
                    CenterY = 0.5
                }
            };
            moveOverlay.PointerExited += (sender, e) => Exited();
            moveOverlay.ManipulationDelta += Objekt_ManipulationDelta;
            moveOverlay.ManipulationStarted += Objekt_ManipulationStarted;
            moveOverlay.ManipulationCompleted += Objekt_ManipulationCompleted;
            baseGrid.Children.Add(moveOverlay);
        }


        private Border GetBorder()
        {
            return new Border
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 10, 10, 10),
                CornerRadius = new CornerRadius(15),
                Background = new SolidColorBrush(Color.FromArgb(0x1F, 0x0, 0x0, 0x0)),
                Height = height,
                Width = width
            };
        }

        public void ZeigeSpielobjekt(SpielLogik.SpielObjekt objekt)
        {
            aufgabe = objekt as BuchstabenAufgabe;
            Count.Text = objekt.Nummer + "/" + DurchgaengeMin();
            moveOverlay.Visibility = Visibility.Collapsed;

            WortDisplay.Visibility = aufgabe.Finished ? Visibility.Visible : Visibility.Collapsed;
            Weiter.Visibility = vorgabeStack.Visibility = !aufgabe.Finished ? Visibility.Visible : Visibility.Collapsed;
            WortDisplay.Text = aufgabe.wort;

            for (int i = 0; i < vorgabeContainer.Length; i++) if (buttons[i].Parent != null) ((Border)buttons[i].Parent).Child = null;

            for (int i = 0; i < vorgabeContainer.Length; i++)
            {
                if (right[i].Parent != null) ((Border)right[i].Parent).Child = null;
                if (wrong[i].Parent != null) ((Border)wrong[i].Parent).Child = null;
                buttons[i].Visibility = Visibility.Visible;

                if (i < aufgabe.wort.Length) eingabeContainer[i].Visibility = vorgabeContainer[i].Visibility = Visibility.Visible;
                else
                {
                    eingabeContainer[i].Visibility = vorgabeContainer[i].Visibility = Visibility.Collapsed;
                    continue;
                }

                if (!aufgabe.Finished)
                {
                    if (aufgabe.vorgabe[i] != '-')
                    {
                        vorgabeContainer[i].Child = buttons[i];
                        buttons[i].Content = aufgabe.vorgabe[i];
                    }
                    if (aufgabe.nutzereingabe[i] != '-')
                    {
                        eingabeContainer[i].Child = buttons[aufgabe.pos[i]];
                        buttons[aufgabe.pos[i]].Content = aufgabe.nutzereingabe[i];
                    }
                }
                else
                {
                    if (aufgabe.nutzereingabe[i] == aufgabe.wort[i])
                    {
                        eingabeContainer[i].Child = right[i];
                        right[i].Content = aufgabe.nutzereingabe[i];
                    }
                    else
                    {
                        eingabeContainer[i].Child = wrong[i];
                        wrong[i].Content = aufgabe.nutzereingabe[i]=='-' ? ' ':aufgabe.nutzereingabe[i];
                    }
                }
            }
        }

        private void AktualisiereAufgabe()
        {
            aufgabe.SaveState();
            bool pruef = true;
            for (int i=0; i<aufgabe.wort.Length; i++)
            {
                if (eingabeContainer[i].Child != null)
                {
                    Button btn = eingabeContainer[i].Child as Button;
                    aufgabe.nutzereingabe[i] = ((char)(btn).Content);
                    for (int pos = 0; pos < aufgabe.wort.Length; pos++) if (buttons[pos] == btn) aufgabe.pos[i] = pos;
                }
                else
                {
                    pruef = false;
                    aufgabe.nutzereingabe[i] = '-';
                }
                if (vorgabeContainer[i].Child != null) aufgabe.vorgabe[i] = ((char)(vorgabeContainer[i].Child as Button).Content);
                else aufgabe.vorgabe[i] = '-';
            }
            if(pruef) aufgabe.Pruefe();
        }


        private void Entered(Button btn)
        {
            if (tempButton != null) tempButton.Visibility = Visibility.Visible;
            if (aufgabe.Finished) return;
            tempButton = btn;
           Point pos = ((Border)tempButton.Parent).TransformToVisual(baseGrid).TransformPoint(new Point(0, 0));
           ((CompositeTransform)moveOverlay.RenderTransform).TranslateX = pos.X;
           ((CompositeTransform)moveOverlay.RenderTransform).TranslateY = pos.Y;
            moveOverlay.Content = tempButton.Content;
            moveOverlay.Visibility = Visibility.Visible;
        }

        private void Exited()
        {
            moveOverlay.Visibility = Visibility.Collapsed;
            tempButton.Visibility = Visibility.Visible;
        }

        private void Objekt_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            e.Handled = true;
            moveOverlay.Visibility = Visibility.Collapsed;

            Point pos = moveOverlay.TransformToVisual(baseGrid).TransformPoint(new Point(0, 0));
            bool found = false;
            for(int i=0; i<aufgabe.wort.Length; i++)
            {
                Point container = eingabeContainer[i].TransformToVisual(baseGrid).TransformPoint(new Point(0, 0));
                double w = eingabeContainer[i].Width/2;
                double h = eingabeContainer[i].Height/2;

                if (pos.X >= container.X-w && pos.X <= container.X+w  && pos.Y >= container.Y-h && pos.Y <= container.Y+h)
                {
                    if (eingabeContainer[i].Child != null)
                    {
                       Button button = eingabeContainer[i].Child as Button;
                        eingabeContainer[i].Child = null;
                        map[button].Child = button;
                    }

                    ((Border)tempButton.Parent).Child = null;
                    eingabeContainer[i].Child = tempButton;

                    found = true;
                    break;
                }
            }
            if (!found)
            {
                ((Border)tempButton.Parent).Child = null;
                map[tempButton].Child = tempButton;
            }
            tempButton.Visibility = Visibility.Visible;
            Sounddesign.PlaySoundAsync(Sounddesign.PUTTING_DOWN);
            AktualisiereAufgabe();
        }

        private void Objekt_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            e.Handled = true;
            ((CompositeTransform)moveOverlay.RenderTransform).TranslateX += e.Delta.Translation.X;
            ((CompositeTransform)moveOverlay.RenderTransform).TranslateY += e.Delta.Translation.Y;
        }

        private void Objekt_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            e.Handled = true;
            tempButton.Visibility = Visibility.Collapsed;
            Sounddesign.PlaySoundAsync(Sounddesign.PUTTING_DOWN);
        }


    }
}
