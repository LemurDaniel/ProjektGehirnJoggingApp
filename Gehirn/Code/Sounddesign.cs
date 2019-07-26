using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace GehirnJogging.Code
{
    public class Sounddesign
    {

        private static MediaElement ambient = new MediaElement();
        private static MediaElement soundEffect = new MediaElement();
        private static StorageFolder sounds;

        private static int ambientCount = 0;
        public static MediaElement Ambient { get => ambient;  }

        private static Sound overrideNext = null;
        private static int overrideNextN = 0;
        public static Sound OverrideNext { get => overrideNext; set => overrideNext = value; }
        public static int OverrideNextN { get => overrideNextN; set => overrideNextN = value; }

        /* Static Vars */
        public static Sound BUTTON_PUSH1 = new Sound("FreeSounds\\button-1.wav");
        public static Sound BUTTON_PUSH2 = new Sound("FreeSounds\\button-2.wav");

        public static Sound LIST_SELECT1 = new Sound("FreeSounds\\ListSelect-1.wav");

        public static Sound GAME_START = new Sound("FreeSounds\\GameStart.wav");

        public static Sound PUZZLE_TILE_PUSH = new Sound("FreeSounds\\PuzzleTilePush.wav");
        public static Sound PUZZLE_TILE_BLOCK = new Sound("FreeSounds\\PuzzleTileBlock.wav");

        public static Sound WOOSH = new Sound("FreeSounds\\Woosh.wav");
        public static Sound WOOSH_SMALL = new Sound("FreeSounds\\WooshSmall.wav");

        public static Sound WRONG1 = new Sound("FreeSounds\\Wrong1.wav");
        public static Sound WRONG2 = new Sound("FreeSounds\\Wrong2.wav");

        public static Sound RIGHT = new Sound("FreeSounds\\Right.wav");

        public static Sound GAME_COMPLETE = new Sound("FreeSounds\\GameEnd.wav");

        public static Sound RECORD = new Sound("FreeSounds\\record.wav");
        public static Sound RECORD_BIG = new Sound("FreeSounds\\recordBig.wav");

        public static Sound COIN = new Sound("FreeSounds\\Coin.wav");
        public static Sound MONEY = new Sound("FreeSounds\\Money.wav");
        public static Sound CASH_REGISTER = new Sound("FreeSounds\\CashRegister.wav");

        public static Sound PUTTING_DOWN = new Sound("FreeSounds\\PuttingDown.wav");

        public static Sound AMBIENT1 = new Sound("FreeSounds\\Ambient1.wav");
        public static Sound AMBIENT2 = new Sound("FreeSounds\\Ambient2.wav");
        public static Sound AMBIENT3 = new Sound("FreeSounds\\Ambient3.wav");

        public static Sound CURRENT_AMBIENT = null;

        /* Initialize */
        static Sounddesign() => InitializeAsync();
        private static async void InitializeAsync() => sounds = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Sounds");
        private Sounddesign() { }


        /* Play Sounds*/
        public static async void PlaySoundAsync(Sound sound)
        {
            if (overrideNextN > 0)
            {
                sound = overrideNext;
                overrideNext = --overrideNextN == 0 ? null:overrideNext;
            }
            try
            {
                StorageFile datei = await sound.DateiAsync();
                soundEffect.SetSource(await datei.OpenAsync(FileAccessMode.Read), "");
                soundEffect.Play();
            }
            catch { };
        }

        /* Ambient Loop */
        public static async void SetAmbientAsync()
        {
            try
            {
                StorageFile datei = await CURRENT_AMBIENT.DateiAsync();
                ambient.SetSource(await datei.OpenAsync(FileAccessMode.Read), "");
            }
            catch {}
        }
        public static void CycleAmbient()
        {
            ambientCount = ++ambientCount % 4;
            if (ambientCount == 0)
            {
                CURRENT_AMBIENT = null;
                ambient.Stop();
                return;
            }
            else if (ambientCount == 1) CURRENT_AMBIENT = AMBIENT1;
            else if (ambientCount == 2) CURRENT_AMBIENT = AMBIENT2;
            else if (ambientCount == 3) CURRENT_AMBIENT = AMBIENT3;
            SetAmbientAsync();
            ambient.Volume = 0.4;
            ambient.IsLooping = true;
            ambient.Play();
        }


        public class Sound
        {
            internal string path;
            public Sound(string path) => this.path = path;

            private StorageFile datei;
            public async Task<StorageFile> DateiAsync()
            {
                if (datei == null) datei = await StorageFile.GetFileFromPathAsync(sounds.Path + "\\" + path);
                return datei;
            }
        };

    }

}
