using GehirnJogging.Code;
using GehirnJogging.Code.Spiele;
using GehirnJogging.Pages.Controller;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static GehirnJogging.Code.Spiele.SpielLogik;
using static GehirnJogging.Code.SpielHighscores;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace GehirnJogging.Pages
{
    public sealed partial class SchiebepuzzleController : UserControl, ISpielController 
    {

        private SpielLogik spiellogik;
        private Schiebepuzzle puzzle;

        public SpielLogik SpielLogik() => spiellogik;
        public int DurchgaengeMin() => 1;
        public int TimePenalty() => Int32.MaxValue;
        public bool Undoable() => true;
        public void Undo() => puzzle.Undo();

        public SchiebepuzzleController(Spiel spiel,int tiles, StorageFile file, bool schneideQuadratisch, Action onFinished)
        {
            this.InitializeComponent();
            spiellogik = new SpielLogik(spiel, this, () => new Schiebepuzzle(tiles, file, schneideQuadratisch, onFinished));
        }


            // SpieleLogik
        public void ZeigeSpielobjekt(SpielObjekt objekt) {

            if (puzzle == objekt) return;

            puzzle = objekt as Schiebepuzzle;
            int size = puzzle.Tiles;

            PuzzleFeld.Children.Clear();
            PuzzleFeld.RowDefinitions.Clear();
            PuzzleFeld.ColumnDefinitions.Clear();

            for (int i = 0; i < size; i++)
            {
                PuzzleFeld.RowDefinitions.Add(new RowDefinition());
                PuzzleFeld.ColumnDefinitions.Add(new ColumnDefinition());
            }

            Dictionary<Image, Pos> mapPos = puzzle.getMapPos();
            foreach (Image bild in mapPos.Keys)
            {
                PuzzleFeld.Children.Add(bild);
                bild.Margin = new Thickness(1, 1, 1, 1);
                bild.PointerReleased += (sender, e) => puzzle.SchiebeBild((Image)sender);
                Grid.SetRow(bild, mapPos[bild].row);
                Grid.SetColumn(bild, mapPos[bild].col);
            }
        }
    }
}
