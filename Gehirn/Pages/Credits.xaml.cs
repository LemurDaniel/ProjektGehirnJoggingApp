using Gehirn;
using GehirnJogging.Code;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using static GehirnJogging.Code.Sounddesign;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace GehirnJogging.Pages
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class Credits : Page
    {

        private MediaElement media = new MediaElement();

        public Credits()
        {
            this.InitializeComponent();
            Sounddesign.PlaySoundAsync(Sounddesign.WOOSH);
            InitializeAsync();
            Sounddesign.Ambient.Stop();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            media.Stop();
            if (Sounddesign.CURRENT_AMBIENT != null)
            {
                Sounddesign.Ambient.IsLooping = true;
                Sounddesign.Ambient.Play();
            }
        }
        private void ButtonEntered(object sender, PointerRoutedEventArgs e) => ((sender as Button).Content as Image).Source = new BitmapImage(new Uri("ms-appx:///Pages/AssetBilder/left-arrow-white.png"));
        private void ButtonExit(object sender, PointerRoutedEventArgs e) => ((sender as Button).Content as Image).Source = new BitmapImage(new Uri("ms-appx:///Pages/AssetBilder/left-arrow-mainBlue.png"));
        private void Click(object sender, RoutedEventArgs e)
        {
            if (sender == Back)
            {
                Frame.Navigate(App.currentPage.GetType());
                return;
            }
            Random rand = new Random();
            Sound sound = null;

            switch (rand.Next(0, 15))
            {
                case 0: sound = Sounddesign.BUTTON_PUSH1; break;
                case 1: sound = Sounddesign.BUTTON_PUSH2; break;
                case 2: sound = Sounddesign.LIST_SELECT1; break;
                case 3: sound = Sounddesign.GAME_START; break;
                case 4: sound = Sounddesign.PUZZLE_TILE_PUSH; break;
                case 5: sound = Sounddesign.PUZZLE_TILE_BLOCK; break;
                case 6: sound = Sounddesign.WOOSH; break;
                case 7: sound = Sounddesign.WOOSH_SMALL; break;
                case 8: sound = Sounddesign.WRONG1; break;
                case 9: sound = Sounddesign.WRONG2; break;
                case 10: sound = Sounddesign.RIGHT; break;
                case 11: sound = Sounddesign.GAME_COMPLETE; break;
                case 12: sound = Sounddesign.RECORD; break;
                case 13: sound = Sounddesign.RECORD_BIG; break;
                    default: sound = Sounddesign.BUTTON_PUSH1; break;
            }
            Sounddesign.PlaySoundAsync(sound);
    }


        public async void InitializeAsync()
        {
            string source = "ms-appx:///Pages/AssetBilder/";
            StorageFolder folder = await (await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Pages")).GetFolderAsync("AssetBilder");
            StorageFile credits = await folder.GetFileAsync("Credits.txt");
            IList<String> list = await FileIO.ReadLinesAsync(credits);

            foreach(String line in list)
            {
                String[] arr = line.Split(',');
                for (int i = 0; i < arr.Length; i++) arr[i] = arr[i].Trim();
                StackPanel st = new StackPanel();
                st.Orientation = Orientation.Horizontal;

                Image icon = new Image();
                Debug.WriteLine(arr[0]);
                icon.Source = new BitmapImage(new Uri(source+arr[0]));
                icon.Stretch = Stretch.Fill;

                Button iconBtn = new Button();
                iconBtn.Content = icon;
                iconBtn.BorderThickness = new Thickness(0,0,0,0);
                iconBtn.Background = (SolidColorBrush)App.ResourceDict["FullyTransparent"];
                iconBtn.Style = (Style)new ResourceDictionary { Source = new Uri("ms-appx:///Pages/ResourceDictionaries/MainButton.xaml") }["MainButtonStyle"];
                iconBtn.PointerEntered += (sender, e) => (iconBtn.Content as Image).Source = new BitmapImage(new Uri(source+arr[1]));
                iconBtn.PointerExited += (sender, e) => (iconBtn.Content as Image).Source = new BitmapImage(new Uri(source+arr[0]));
                iconBtn.Click += Click;

                Border border = new Border();
                border.CornerRadius = new CornerRadius(50);
                border.Child = iconBtn;
                border.Height = 100;
                border.Width = 100;
                border.Margin = new Thickness(10,30,10,30);

                TextBlock block = new TextBlock();
                block.FontSize = (double)App.ResourceDict["BigFont"];
                block.Foreground = (SolidColorBrush)App.ResourceDict["MainTextColor"];
                block.VerticalAlignment = VerticalAlignment.Center;
                block.Margin = new Thickness(20,0,20,0);
                block.Text = "Icon made by ";

                HyperlinkButton hyperlink1 = new HyperlinkButton();
                hyperlink1.FontSize = (double)App.ResourceDict["BigFont"];
                hyperlink1.Content = arr[2];
                hyperlink1.NavigateUri = new Uri(arr[3]);

                TextBlock block2 = new TextBlock();
                block2.FontSize = (double)App.ResourceDict["BigFont"];
                block2.Foreground = (SolidColorBrush)App.ResourceDict["MainTextColor"];
                block2.VerticalAlignment = VerticalAlignment.Center;
                block2.Margin = new Thickness(20, 0, 20, 0);
                block2.Text = " from ";

                HyperlinkButton hyperlink2 = new HyperlinkButton();
                hyperlink2.FontSize = (double)App.ResourceDict["BigFont"];
                hyperlink2.Content = arr[4];
                hyperlink2.NavigateUri = new Uri(arr[5]);

                st.Children.Add(border);
                st.Children.Add(block);
                st.Children.Add(hyperlink1);
                st.Children.Add(block2);
                st.Children.Add(hyperlink2);
                listView.Items.Add(st);
            }
            InitializeSoundAsync();
        }

        public async void InitializeSoundAsync()
        {
            StorageFolder folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Sounds");
            StorageFile credits = await folder.GetFileAsync("Credits.txt");
            IList<String> list = await FileIO.ReadLinesAsync(credits);

            foreach (String line in list)
            {
                if (line.Contains("AMBIENT4")) continue;
                String[] arr = line.Split(',');
                for (int i = 0; i < arr.Length; i++) arr[i] = arr[i].Trim();
                StackPanel st = new StackPanel();
                st.Orientation = Orientation.Horizontal;


                Button iconBtn = new Button();
                iconBtn.Content = arr[0];
                iconBtn.Background = new SolidColorBrush(Color.FromArgb(0xFF,0xFF,0xFF,0xFF));
                iconBtn.Style = (Style)new ResourceDictionary { Source = new Uri("ms-appx:///Pages/ResourceDictionaries/MainButton.xaml") }["MainButtonStyle"];
                iconBtn.Click += async (sender, e) =>
                {
                    Console.WriteLine(folder.Path + "\\" + arr[1]);
                    StorageFile sound = await StorageFile.GetFileFromPathAsync(folder.Path+"\\"+arr[1]);
                    media.SetSource(await sound.OpenAsync(FileAccessMode.Read), "");
                    media.Play();
                };

                Border border = new Border();
                border.CornerRadius = new CornerRadius(15);
                border.Child = iconBtn;
                border.Margin = new Thickness(10, 30, 10, 30);

                TextBlock block = new TextBlock();
                block.FontSize = (double)App.ResourceDict["BigFont"];
                block.Foreground = (SolidColorBrush)App.ResourceDict["MainTextColor"];
                block.VerticalAlignment = VerticalAlignment.Center;
                block.Margin = new Thickness(20, 0, 20, 0);
                block.Text = "Sound made by ";

                HyperlinkButton hyperlink1 = new HyperlinkButton();
                hyperlink1.FontSize = (double)App.ResourceDict["BigFont"];
                hyperlink1.Content = arr[2];
                hyperlink1.NavigateUri = new Uri(arr[3]);

                st.Children.Add(border);
                st.Children.Add(block);
                st.Children.Add(hyperlink1);
                listView.Items.Add(st);
            }
        }

    }
}
