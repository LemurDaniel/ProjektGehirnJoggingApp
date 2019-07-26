using GehirnJogging.Pages;
using GehirnJogging.Pages.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using static GehirnJogging.Code.SpielHighscores;

namespace GehirnJogging.Code.Spiele
{
    class ButtonPresser : SpielLogik
    {
        private readonly int MinButtonPesses = 50;
        private Button button;
        private Button decoy;
        private int ButtonPresses = 0;

        private Random rand = new Random();

        public ButtonPresser(Button button, Button decoy) : base(SpielHighscores.GetInstance().spiele[SpielHighscores.BUTTONPRESSER], new KopfrechnenController(), () => new Rechenaufgabe()) {
            this.button = button;
            this.decoy = decoy;
            button.PointerEntered += ButtonEntered;
            button.Click += async (sender, e) => await OnButtonPressedAsync();
            decoy.Click += async (sender, e) => await OnDecoyPressedAsync();
            decoy.PointerExited += async  (sender, e) => await SetNewButtonPositionsAsync();
            button.Content = MinButtonPesses-ButtonPresses+"";
            decoy.Content = "!!!NO!!!";

        }


        private void ButtonEntered(object sender, PointerRoutedEventArgs e)
        {
            // 10 % chance
            if (rand.Next(11) != 0) return;
            button.Visibility = Visibility.Collapsed;
            decoy.Visibility = Visibility.Visible;
        }

        private async Task OnDecoyPressedAsync()
        {
            Sounddesign.PlaySoundAsync(Sounddesign.WRONG2);
            base.AddTimePenalty();
            await SetNewButtonPositionsAsync();
        }

            private async Task OnButtonPressedAsync()
        {
            Sounddesign.PlaySoundAsync(Sounddesign.BUTTON_PUSH1);
            if (CheckWin()) return;
            await SetNewButtonPositionsAsync();
        }


        private async Task SetNewButtonPositionsAsync()
        {
            decoy.Visibility = Visibility.Collapsed;
            button.Visibility = Visibility.Collapsed;

            HorizontalAlignment ha;
            VerticalAlignment va;

            int randZ = rand.Next(3);
            if (randZ == 0) ha = HorizontalAlignment.Center;
            else if (randZ == 1) ha = HorizontalAlignment.Left;
            else ha = HorizontalAlignment.Right;

            randZ = rand.Next(3);
            if (randZ == 0) va = VerticalAlignment.Center;
            else if (randZ == 1) va = VerticalAlignment.Top;
            else va = VerticalAlignment.Bottom;

            await Task.Delay(50);

            (button.Parent as Border).HorizontalAlignment = ha;
            (button.Parent as Border).VerticalAlignment = va;

            (decoy.Parent as Border).HorizontalAlignment = ha;
            (decoy.Parent as Border).VerticalAlignment = va;

            button.Visibility = Visibility.Visible;
        }

        private bool CheckWin()
        {
            if (++ButtonPresses == MinButtonPesses)
            {
                base.EndGameAsync();
                (button.Parent as Border).HorizontalAlignment = HorizontalAlignment.Center;
                (button.Parent as Border).VerticalAlignment = VerticalAlignment.Center;
                return true;
            }
            button.Content = MinButtonPesses - ButtonPresses + "";
            return false;
        }

    }
}
