using GehirnJogging.Pages.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GehirnJogging.Code.Spiele.SpielLogik;

namespace GehirnJogging.Code.Spiele
{
    public class ConcentrateAufgabe : SpielObjekt
    {
        private static readonly string[] zeichenMuster = { "oO0,-/,6", "aAbB,-,6", "0123456789,-|.,3" };
        public static readonly int KETTEN_LAENGE = 60;
        public static readonly int KETTEN_ANZAHL = 6;

        private int gruppenLaenge = 3;
        private string trennzeichen;
        private string gruppenzeichen;

        public readonly int pos;
        public readonly string zeichenkette;
        public readonly string[] zeichenketten = new string[KETTEN_ANZAHL];

        private int nutzereingabe;
        public int Nutzereingabe { set => nutzereingabe = Finished ? nutzereingabe : value; get => nutzereingabe; }

        public ConcentrateAufgabe()
        {
            string[] muster = zeichenMuster[rand.Next(0, zeichenMuster.Length)].Split(',');
            try { gruppenLaenge = int.Parse(muster[2]); } catch { }
            trennzeichen = muster[1];
            gruppenzeichen = muster[0];

            pos = rand.Next(0, KETTEN_ANZAHL);
            zeichenkette = GenerateRandomString();
            for (int i = 0; i < zeichenketten.Length; i++) zeichenketten[i] = i == pos ? zeichenkette : GenerateDecoy();
        }

        private string GenerateRandomString()
        {
            StringBuilder b = new StringBuilder();
            for (int i = 0; i < KETTEN_LAENGE; i++)
            {
                if (i % gruppenLaenge != 0 || i == 0) b.Append(gruppenzeichen[rand.Next(0, gruppenzeichen.Length)]);
                else b.Append(trennzeichen[rand.Next(0, trennzeichen.Length)]);
            }
            return b.ToString();
        }
        private string GenerateDecoy()
        {
            StringBuilder b = new StringBuilder();
            for (int i = 0; i < KETTEN_LAENGE; i++)
            {
                if (rand.Next(0, 6) == 0)
                {
                    if (i % gruppenLaenge != 0 || i == 0) b.Append(gruppenzeichen[rand.Next(0, gruppenzeichen.Length)]);
                    else b.Append(trennzeichen[rand.Next(0, trennzeichen.Length)]);
                }
                else b.Append(zeichenkette[i]);
            }
            if (b.ToString().Equals(zeichenkette)) return GenerateDecoy();
            else return b.ToString();
        }

        public void PruefeEingabe() => Correct = nutzereingabe == pos;
    }
}
