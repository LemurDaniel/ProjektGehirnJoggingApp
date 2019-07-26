using Gehirn;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace GehirnJogging.Pages.ResourceDictionaries
{
    public class ColorButton : Button
    {
        public static readonly string RED = "Red,Rot";
        public static readonly string GREEN = "Green,Grün";
        public static readonly string LILA = "Lila,Lila";
        public static readonly string BLAU = "Blau,Blau";
        public static readonly string CYAN = "Cyan,Cyan";
        public static readonly string GELB = "Magenta,Magenta";
        public static readonly string ORANGE = "Orange,Orange";

        public static readonly List<string> Farben = new List<string>();
        static ColorButton()
        {
            Farben.Add(ColorButton.RED);
            Farben.Add(ColorButton.GREEN);
            Farben.Add(ColorButton.GELB);
            Farben.Add(ColorButton.LILA);
            Farben.Add(ColorButton.BLAU);
            Farben.Add(ColorButton.CYAN);
            Farben.Add(ColorButton.ORANGE);
        }

        private SolidColorBrush mainColor = new SolidColorBrush(Color.FromArgb(0xFF,0xFF,0,0));
        private SolidColorBrush mainColorPointOver = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0, 0));
        private SolidColorBrush mainColorPointPressed = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0, 0));
        private SolidColorBrush mainColorDisable = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0, 0));

        public SolidColorBrush MainColor { get => mainColor; set => mainColor = value; }
        public SolidColorBrush MainColorPointOver { get => mainColorPointOver; set => mainColorPointOver = value; }
        public SolidColorBrush MainColorPointPressed { get => mainColorPointPressed; set => mainColorPointPressed = value; }
        public SolidColorBrush MainColorDisable { get => mainColorDisable; set => mainColorDisable = value; }

        public ColorButton(string farbe)
        {
            farbe = farbe.Split(',')[0];
            FontSize = 120;
            Style = (Style)new ResourceDictionary { Source = new Uri("ms-appx:///Pages/ResourceDictionaries/MainButton.xaml") }["MainButtonStyleColor"];
            MainColor = (SolidColorBrush)App.ResourceDict["MainButton" + farbe];
            MainColorPointOver = (SolidColorBrush)App.ResourceDict["Main" + farbe + "Transparent1"];
            MainColorPointPressed = (SolidColorBrush)App.ResourceDict["Main" + farbe + "Transparent2"];
            MainColorDisable = (SolidColorBrush)App.ResourceDict["Main" + farbe + "Disable"];
            BorderBrush = MainColor;
            Background = MainColor;
        }

    }


}
