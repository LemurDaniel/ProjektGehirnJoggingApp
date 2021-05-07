using GehirnJogging.Pages.Controller;
using GehirnJogging.Pages.ResourceDictionaries;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static GehirnJogging.Code.Spiele.SpielLogik;

namespace GehirnJogging.Code.Spiele
{
    public class FarbeAufgabe : SpielObjekt
    {
        public readonly ColorButton FarbButton;
        private readonly bool fGleichW;

        private bool nutzereingabe;
        public bool Nutzereingabe { set => nutzereingabe = Finished ? nutzereingabe : value; get => nutzereingabe; }

        public FarbeAufgabe()
        {
            int z1 = rand.Next(0, ColorButton.Farben.Count);
            int z2 = z1;
            if(rand.Next(0,2) == 0) while(z2==z1) z2 = rand.Next(0, ColorButton.Farben.Count);

            FarbButton = new ColorButton(ColorButton.Farben[z2]);
            FarbButton.Content = ColorButton.Farben[z1].Split(',')[1];
            fGleichW = z1 == z2;
        }
        public void PruefeEingabe() => Correct = fGleichW == nutzereingabe;
    }
}
