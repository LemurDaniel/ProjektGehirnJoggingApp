using GehirnJogging.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GehirnJogging.Code.Sounddesign;
using static GehirnJogging.Code.Spiele.SpielLogik;

namespace GehirnJogging.Code.Spiele
{
    public class WechselgeldAufgabe : SpielObjekt
    {
        public string Convert(int wert) => wert / 100 + "," + (wert % 100 < 10 ? "0" : "") + wert % 100 + "€";

        public static readonly int MIN_BETRAG_EUR = 10;
        public static readonly int MAX_BETRAG_EUR = 400;
        public static readonly int MAX_ERHALTEN = 800;

        public readonly int betrag;
        public readonly int erhaltenesGeld;
        public readonly string betragString;
        public readonly string erhaltenesGeldString;

        private Dictionary<Coins, int> loesung = new Dictionary<Coins, int>();
        private Dictionary<Coins, int> wechselgeld = new Dictionary<Coins, int>();
        public int GetWechselGeld(Coins coin) => wechselgeld[coin];
        public int GetLoesung(Coins coin) => loesung[coin];

        public WechselgeldAufgabe()
        {
            RightSound = Sounddesign.CASH_REGISTER;

            betrag = rand.Next(MIN_BETRAG_EUR * 100, MAX_BETRAG_EUR * 100 + 1);          
            erhaltenesGeld = rand.Next(betrag, Math.Max(betrag, MAX_ERHALTEN * 100) );
            erhaltenesGeld = (int)Math.Round(erhaltenesGeld / (double)Coins.EUR_10) * (int) Coins.EUR_10;

            betragString = Convert(betrag);
            erhaltenesGeldString = Convert(erhaltenesGeld);

            int temp = erhaltenesGeld  - betrag;
            Coins[] coins = (Coins[])Enum.GetValues(typeof(Coins));
            for (int i=coins.Length-1; i>=0; i--)
            {
                wechselgeld.Add(coins[i], 0);
                loesung.Add(coins[i], temp / (int)coins[i]);
                temp %= (int)coins[i];
            }
        }

        private class Move
        {
            internal Move(Coins coin, bool add)
            {
                this.coin = coin;
                this.add = add;
            }
            internal Coins coin;
            internal bool add;
        }
        private Stack<Move> moves = new Stack<Move>();
        public bool Undo()
        {
            if (Finished) return false;
            if (moves.Count != 0)
            {
                AddSubCoin(moves.Peek().coin, moves.Pop().add, false);
                return true;
            }
            return false;
        }


        public void AddSubCoin(Coins coin, bool add) => AddSubCoin(coin, add, true);
        private void AddSubCoin(Coins coin, bool add, bool SaveMove)
        {
            if (Finished)
            {
                Sounddesign.PlaySoundAsync(Sounddesign.BUTTON_PUSH1);
                return;
            }

            if (!add && wechselgeld[coin] > 0) wechselgeld[coin]--;
            else if (add) wechselgeld[coin]++;
            else
            {
                Sounddesign.PlaySoundAsync(Sounddesign.BUTTON_PUSH1);
                return;
            }


            if (coin <= Coins.EUR_2) Sounddesign.PlaySoundAsync(Sounddesign.COIN);
            else Sounddesign.PlaySoundAsync(Sounddesign.MONEY);
            // Sinnlose Moves nicht speichern
            if (moves.Count != 0 && SaveMove && moves.Peek().coin == coin && moves.Peek().add != add) return;
            moves.Push(new Move(coin, add));
        }

        public void PruefeEingabe()
        {
            foreach (Coins coin in wechselgeld.Keys)
            {
                if(wechselgeld[coin] != loesung[coin])
                {
                    Correct = false;
                    return;
                }
            }
            Correct = true;
        }

    }

    public enum Coins
    {
        CENT_1 = 1,
        CENT_2 = 2,
        CENT_5 = 5,
        CENT_10 = 10,
        CENT_20 = 20,
        CENT_50 = 50,
        EUR_1 = 100,
        EUR_2 = 200,
        EUR_5 = 500,
        EUR_10 = 1_000,
        EUR_20 = 2_000,
        EUR_50 = 5_000,
        EUR_100 = 10_000
    }
}
