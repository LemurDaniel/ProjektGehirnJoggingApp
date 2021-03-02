using Gehirn;
using GehirnJogging.Code;
using GehirnJogging.Code.Spiele;
using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using static GehirnJogging.Code.SpielHighscores;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace GehirnJogging.Pages
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class SchiebePuzzleOptions : Page
    {

        private Dictionary<Border, StorageFile> dateien = new Dictionary<Border, StorageFile>();
        private Dictionary<StorageFile, Border> dateien_reverse = new Dictionary<StorageFile, Border>();

        private ISet<StorageFile> Add = new HashSet<StorageFile>();
        private ISet<StorageFile> Delete = new HashSet<StorageFile>();
        private Stack<StorageFile> Moves = new Stack<StorageFile>();

        private SchiebepuzzleController puzzle;

        public SchiebePuzzleOptions()
        {      
            this.InitializeComponent();
            if (App.OFFLINE_MODE) Standard.Visibility = Visibility.Collapsed;
            App.currentPage = this;
            Sounddesign.PlaySoundAsync(Sounddesign.WOOSH);
            tb1.IsChecked = true;
            volumeOn.Visibility = Sounddesign.CURRENT_AMBIENT == null ? Visibility.Collapsed : Visibility.Visible;
            volumeOff.Visibility = Sounddesign.CURRENT_AMBIENT == null ? Visibility.Visible : Visibility.Collapsed;
            InitializeAsync();
        }
        private void ButtonEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e) => ((sender as Button).Content as Image).Source = new BitmapImage(new Uri("ms-appx:///Pages/AssetBilder/" + (sender as Button).Tag + "-white.png"));
        private void ButtonExit(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e) => ((sender as Button).Content as Image).Source = new BitmapImage(new Uri("ms-appx:///Pages/AssetBilder/" + (sender as Button).Tag + "-mainBlue.png"));

        public async void InitializeAsync()
        {
            DownloadWaitRing.IsActive = true;
            StorageFolder ordner = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Puzzlebilder");
            foreach(StorageFile f in await ordner.GetFilesAsync())   FuegeBildZuListeHinzu(f);


            if (Nutzer.Getinstance().StManager.IsDownloading)
            {
                Nutzer.Getinstance().StManager.DownloadAction += async (file) => await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => FuegeBildZuListeHinzu(file));
                Nutzer.Getinstance().StManager.DownloadEndedAction += async () => await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => DownloadWaitRing.IsActive = false);
            }
            else DownloadWaitRing.IsActive = false;
        }

        private Border FuegeBildZuListeHinzu(StorageFile f)
        {
            if (f == null) return null;
            if (!f.FileType.ToLower().Equals(".png") && !f.FileType.ToLower().Equals(".jpg") && !f.FileType.ToLower().Equals(".jpeg") && !f.FileType.ToLower().Equals(".bmp")) return null;

            Border border = new Border
            {
                CornerRadius = new CornerRadius(5),
                Margin = new Thickness(0, 10, 0, 10)
            };
            Image img = new Image
            {
                Source = new BitmapImage(new Uri(f.Path))
            };

            border.Child = img;
            Liste.Items.Add(border);
            dateien.Add(border, f);
            dateien_reverse.Add(f, border);
            return border;
        }


        /* Button Handler */
        private void Click(object sender, RoutedEventArgs e)
        {
            if(sender.GetType().Equals(typeof(Button))) Sounddesign.PlaySoundAsync(Sounddesign.BUTTON_PUSH1);
            else                                        Sounddesign.PlaySoundAsync(Sounddesign.BUTTON_PUSH2);

            if (sender == tb1) Difficulty(sender);
            else if (sender == tb2) Difficulty(sender);
            else if (sender == tb3) Difficulty(sender);
            else if (sender == tbQuadratisch) tbQuadratisch.Content = tbQuadratisch.IsChecked.Value ? "Quadratisch" : "Seitenverhältnis";
            else if (sender == volumeOff || sender == volumeOn) ChangeMusic();
            else if (sender == EntferneBildBtn) EntferneBild();
            else if (sender == NeuesBildBtn) NeuesBildAsync();
            else if (sender == StarteSpielBtn) StarteSpiel();
            else if (sender == Back) Frame.Navigate(typeof(SpieleHub));
            else if (sender == Undo) UndoActionAsync();
            else if (sender == Standard) GetStandardBilder();
        }






        private void Liste_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Border border = (Border)Liste.SelectedItem;
            if (border == null)
            {
                vorschaubild.Visibility = Visibility.Collapsed;
                vorschauText.Visibility = Visibility.Collapsed;
            }
            else
            {
                vorschaubild.Source = ((Image)border.Child).Source;
                vorschauText.Text = dateien[border].Name;
                vorschaubild.Visibility = Visibility.Visible;
                vorschauText.Visibility = Visibility.Visible;
            }

            Sounddesign.PlaySoundAsync(Sounddesign.LIST_SELECT1);
        }



        /* Schwierigkeit */
        private void Difficulty(object sender)
        {
            tb1.IsChecked = false;
            tb2.IsChecked = false;
            tb3.IsChecked = false;
            ((ToggleButton)sender).IsChecked = true;
        }

        /* BTN Bild hinzufügen */
        private async void NeuesBildAsync()
        {
            StorageFile file = await Schiebepuzzle.PickBildAsync();
            StorageFolder ordner = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Puzzlebilder");
            try
            {
                file = await file.CopyAsync(ordner);
                Border border = FuegeBildZuListeHinzu(file);
                Liste.SelectedItem = border;
                if (Delete.Contains(file)) Delete.Remove(file);
                else Add.Add(file);

                Moves.Push(dateien[border]);
            }
            catch(NullReferenceException) { return; }
            catch
            {
                //Bereits vorhandenes raussuchen
                foreach (Border bord in dateien.Keys)
                {
                    if (dateien[bord].Name.Equals(file.Name)) continue;
                    Liste.SelectedItem = bord;
                    break;
                }
            }
        }

        /* BTN Bild Entfernen */
        private void EntferneBild()
        {
            if (Liste.SelectedIndex == -1) return;
            Border border = (Border)Liste.SelectedItem;
            Liste.Items.Remove(border);
            if (Add.Contains(dateien[border])) Add.Remove(dateien[border]);
            else Delete.Add(dateien[border]);

            Moves.Push(dateien[border]);
        }

        /* Undo action */
        private async void UndoActionAsync()
        {
            if (Moves.Count == 0) return;
            if(Add.Contains(Moves.Peek()))
            {
                StorageFile f = Moves.Pop();
                Liste.Items.Remove(dateien_reverse[f] );
                await f.DeleteAsync();
                Add.Remove(f);
            }
            else
            {
                Liste.Items.Add( dateien_reverse[Moves.Peek()] );
                Liste.SelectedItem = dateien_reverse[Moves.Peek()];
                Delete.Remove(Moves.Pop());
            }
        }


        private void GetStandardBilder()
        {
            DownloadWaitRing.IsActive = true;
            Nutzer.Getinstance().StManager.DownloadAction += async (file) => await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => FuegeBildZuListeHinzu(file));
            Nutzer.Getinstance().StManager.DownloadEndedAction += async () => await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => DownloadWaitRing.IsActive = false);
            Nutzer.Getinstance().StManager.GetStandardImagesAsync();
        }

        /* Bei Seitenwechsel den Blobstorage aktualisieren */
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (App.OFFLINE_MODE) return;
            StorageAccountManager st = Nutzer.Getinstance().StManager;
            st.UploadFileAsync(Add);
            st.DeleteFileAsync(Delete);
        }


        /* Musik ändern */
        private void ChangeMusic()
        {
            Sounddesign.PlaySoundAsync(Sounddesign.BUTTON_PUSH1);
            Sounddesign.CycleAmbient();
            volumeOn.Visibility = Sounddesign.CURRENT_AMBIENT == null ? Visibility.Collapsed : Visibility.Visible;
            volumeOff.Visibility = Sounddesign.CURRENT_AMBIENT == null ? Visibility.Visible : Visibility.Collapsed;
        }


        /* Spiel starten */
        private void StarteSpiel()
        {
            if (Liste.SelectedIndex == -1) return;

            int tilesize = 2;
            if (tb1.IsChecked.Value)      tilesize = 3;
            else if (tb2.IsChecked.Value) tilesize = 4;
            else if (tb3.IsChecked.Value) tilesize = 5;

            Spiel spiel = null;
            if (tb1.IsChecked.Value) spiel = SpielHighscores.GetInstance().spiele[SpielHighscores.SCHIEBEPUZZLE_LEICHT];
            else if (tb2.IsChecked.Value) spiel = SpielHighscores.GetInstance().spiele[SpielHighscores.SCHIEBEPUZZLE_NORMAL];
            else if (tb3.IsChecked.Value) spiel = SpielHighscores.GetInstance().spiele[SpielHighscores.SCHIEBEPUZZLE_SCHWER];

            StarteSpielBtn.IsEnabled = false;
            WaitRing.IsActive = true;

            progress.Visibility = Visibility.Visible;

            puzzle = new SchiebepuzzleController(spiel, tilesize, dateien[(Border)Liste.SelectedItem], tbQuadratisch.IsChecked.Value, ActionStarteSpiel);
        }

        private void ActionStarteSpiel() => this.Frame.Navigate(typeof(SpielSeite), puzzle);
    }
}
