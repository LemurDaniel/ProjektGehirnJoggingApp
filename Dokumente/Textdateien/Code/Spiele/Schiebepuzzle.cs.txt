using GehirnJogging.Pages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using static GehirnJogging.Code.Spiele.SpielLogik;
using static GehirnJogging.Code.SpielHighscores;

namespace GehirnJogging.Code.Spiele
{
    public class Pos
    {
        public Pos(int col, int row)
        {
            this.col = col;
            this.row = row;
        }
        public readonly int col;
        public readonly int row;
    }


    public class Schiebepuzzle : SpielObjekt
    {
       
        // Fürs Schneiden
        private int tiles;
        private int height;
        private int width;

        public int Tiles { get => tiles; }


        // Für Spielverlauf
        private Image[] Bilder;
        private Image schieber = new Image();

        private Dictionary<Image, Pos> mapPos = new Dictionary<Image, Pos>();
        public ISet<Image> WrongPos = new HashSet<Image>();

        // Für Undo
        private bool recordMoves = true;
        private Stack<Image> Moves = new Stack<Image>();

        // Einstellungen
        private static int minRandomPasses = 10000;
        private static int maxRandomPasses = 50000;
        private static double schieberOpacity = 0.25;
        private static double minWrong = 0.7;// Min 70% Falsch angeordnet

        /* Konstruktor */
        public Schiebepuzzle(int tiles, StorageFile file, bool schneideQuadratisch, Action onFinished)
        {
            this.tiles = tiles;
            Bilder = new Image[tiles * tiles];
            KreiereAsync(file, schneideQuadratisch, onFinished);
        }

        private async void KreiereAsync(StorageFile file, bool schneideQuadratisch, Action onFinished)
        {
            Bilder = new Image[tiles * tiles];
            await SchneideBilder(file, schneideQuadratisch);
            FuelleMap();
            Randomize();
            onFinished.Invoke();
        }


        /* Spiel vorbereiten */
        private void FuelleMap()
        {
            Random r = new Random();
            int rand = r.Next(0, Bilder.Length-1);

            for(int i=0; i<Bilder.Length; i++)  mapPos.Add(Bilder[i], new Pos(i % tiles, i / tiles));

            schieber = Bilder[Bilder.Length-1];
            schieber.Opacity = schieberOpacity;
        }

        private void Randomize()
        {
            Random rand = new Random();
            Image last = Bilder[0];

            int count = 0;

            while(WrongPos.Count < (tiles * tiles) *minWrong || count < minRandomPasses)
            {
                // Random Bild
                Image bild = Bilder[rand.Next(0, Bilder.Length - 1)];
                if (!FreiesFeldUmliegend(bild)) continue;
                if (bild == last) continue;

                // Schiebe Bild
                Pos temp = mapPos[schieber];
                mapPos[schieber] = mapPos[bild];
                mapPos[bild] = temp;

                // Pruefe Position
                PruefePosition(schieber);
                PruefePosition(bild);

                //Aktualisiere Progress
                if (count++ > maxRandomPasses) break;
            }
        }



        /* Methoden für Spielverlauf */

        public Pos GetPos(Image bild) => mapPos[bild];
        public Dictionary<Image, Pos> getMapPos() => mapPos;
        public void Undo()
        {
            if (Moves.Count == 0) return;
            recordMoves = false;
            SchiebeBild(Moves.Pop());
            recordMoves = true;
        }

        public void SchiebeBild(Image bild)
        {
            if (Finished) return;
            if (!FreiesFeldUmliegend(bild))
            {
                Sounddesign.PlaySoundAsync(Sounddesign.PUZZLE_TILE_BLOCK);
                return;
            }
            Sounddesign.PlaySoundAsync(Sounddesign.PUZZLE_TILE_PUSH);

            Pos temp = mapPos[schieber];
            mapPos[schieber] = mapPos[bild];
            mapPos[bild] = temp;

            Grid.SetColumn(bild, mapPos[bild].col);
            Grid.SetRow(bild, mapPos[bild].row);

            Grid.SetColumn(schieber, mapPos[schieber].col);
            Grid.SetRow(schieber, mapPos[schieber].row);

            // Record Moves
            if (recordMoves)
            {
                if (Moves.Count!=0 && Moves.Peek() == bild) Moves.Pop();  // Selbes Bild sofort zurückgeschoben
                else Moves.Push(bild);
            }

            PruefePosition(schieber);
            PruefePosition(bild);

            if (WrongPos.Count != 0) return;
            schieber.Opacity = 1;
            Correct = true;
        }

        private void PruefePosition(Image bild)
        {
            int arrayPos = mapPos[bild].col + mapPos[bild].row * tiles;
            if (bild != Bilder[arrayPos])   WrongPos.Add(bild);
            else                            WrongPos.Remove(bild);
        }

        private bool FreiesFeldUmliegend(Image bild)
        {
            if (bild == schieber) return false;

            int col = mapPos[bild].col;
            int row = mapPos[bild].row;

            List<Pos> list = new List<Pos>();
            list.Add(new Pos(col - 1, row));
            list.Add(new Pos(col + 1, row));
            list.Add(new Pos(col, row - 1));
            list.Add(new Pos(col, row + 1));

            foreach (Pos pos in list) if (mapPos[schieber].col == pos.col && mapPos[schieber].row == pos.row) return true;
            return false;
        }


        /* Bild holen und Zerschneiden */

        public static async Task<StorageFile> PickBildAsync()
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".bmp");
            openPicker.FileTypeFilter.Add(".png");

            return await openPicker.PickSingleFileAsync();     
        }

        private async Task SchneideBilder(StorageFile bild, bool schneideQuadratisch)
        {
            using (IRandomAccessStream stream = await bild.OpenReadAsync())
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                if (schneideQuadratisch)
                {
                    if (decoder.PixelWidth >= decoder.PixelHeight) height = width = ((int)decoder.PixelHeight) / tiles;
                    else height = width = ((int)decoder.PixelWidth) / tiles;
                }
                else
                {
                    height = ((int)decoder.PixelHeight) / tiles;
                    width = ((int)decoder.PixelWidth) / tiles;
                }


                int posX = (((int)decoder.PixelWidth) - width * tiles) / 2;
                int posY = (((int)decoder.PixelHeight) - height * tiles) / 2;

                // Crop image
                for (int i = 0; i < tiles * tiles; i++)
                {
                    // Versatz berechnen
                    int X = posX + (i % tiles) * width;
                    int Y = posY + (i / tiles) * height;
                   Byte[] pixels = await GetPixelDataAsync(decoder, X, Y, width, height);

                    WriteableBitmap bitmap = new WriteableBitmap(width, height);
                    Stream pixStream = bitmap.PixelBuffer.AsStream();
                    pixStream.Write(pixels, 0, pixels.Length);

                    Bilder[i] = new Image();
                    Bilder[i].Source = bitmap;

                }
            }
        }

        private async Task<byte[]> GetPixelDataAsync(BitmapDecoder decoder, int x, int y, int width, int height)
        {
            BitmapTransform transform = new BitmapTransform();
            BitmapBounds bounds = new BitmapBounds
            {
                X = (uint)x,
                Y = (uint)y,
                Height = (uint)height,
                Width = (uint)width
            };
            transform.Bounds = bounds;

            PixelDataProvider pix = await decoder.GetPixelDataAsync(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Straight,
                transform,
                ExifOrientationMode.IgnoreExifOrientation,
                ColorManagementMode.ColorManageToSRgb);
            return pix.DetachPixelData();
        }
    }
}
