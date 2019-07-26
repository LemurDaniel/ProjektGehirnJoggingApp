using GehirnJogging.Pages;
using GehirnJogging.Pages.Controller;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GehirnJogging.Code.Spiele.SpielLogik;

namespace GehirnJogging.Code.Spiele
{

    public class Rechenaufgabe : SpielObjekt
    {
        public static readonly int MAXVAL = 100;
        public static readonly int MINVAL = 1;

        public readonly string Term;
        public readonly int erg;
        private string nutzereingabe = "";
        public string Nutzereingabe { set => nutzereingabe = Finished ? nutzereingabe : value; get => nutzereingabe; }

        public Rechenaufgabe()
        {
            int op = rand.Next(0, 4);
            int z1, z2;
            do
            {
                z1 = rand.Next(MINVAL, MAXVAL);
                z2 = rand.Next(MINVAL, MAXVAL);
            } while (op == 2 && (z1 % z2 != 0 || z1 == 1 || z2 == 1) || (z1 == z2 && op != 0 && op != 3) || op == 3 && z1 + z2 > 50 || op == 1 && z1 <= z2);

            Term = z1 + GetOperator(op) + z2;
            erg = Berechne(z1, z2, op);
        }

        public void PruefeEingabe()
        {
            try { Correct = Int32.Parse(Nutzereingabe) == erg; }
            catch (FormatException) { Correct = false; }
        }




        public static int Berechne(int z1, int z2, int op)
        {
            switch (op)
            {
                case 0: return z1 + z2;
                case 1: return z1 - z2;
                case 2: return z1 / z2;
                case 3: return z1 * z2;
                default: return 0;
            }
        }

        public static string GetOperator(int i)
        {
            switch (i)
            {
                case 0: return " + ";
                case 1: return " - ";
                case 2: return " ÷ ";
                case 3: return " ⋅ ";
                default: return null;
            }
        }
    }
}
