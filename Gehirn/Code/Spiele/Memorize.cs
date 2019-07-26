using GehirnJogging.Pages.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GehirnJogging.Code.Spiele.SpielLogik;

namespace GehirnJogging.Code.Spiele
{
    public class MemorizeAufgabe : SpielObjekt
    {
        public static readonly int GRIDSIZE = 3;
        public static readonly int MEMANZAHL = 7;

        public readonly Pos[] positionen = new Pos[MEMANZAHL];

        //0 undefined 1 richtig 2 falsch
        private int[] status = new int[MEMANZAHL];
        private int nutzereingabe = 0;
        public int Nutzereingabe { set => GibEin(value); get => nutzereingabe; }
        public int[] Status { get => status; }

        private void GibEin(int value)
        {
            if (Finished) return;
            if (value != nutzereingabe++)
            {
                status[value] = -1;
                Correct = false;
                return;
            } else status[value] = 1;
            if (nutzereingabe == MEMANZAHL) Correct = true;
            else Sounddesign.PlaySoundAsync(Sounddesign.BUTTON_PUSH1);
        }

        public MemorizeAufgabe()
        {
            HashSet<int> vergeben = new HashSet<int>();
            for(int i=0; i<positionen.Length;)
            {
                int z = rand.Next(0, GRIDSIZE * GRIDSIZE + 1);
                if (vergeben.Contains(z)) continue;
                vergeben.Add(z);
                positionen[i] = new Pos(z% GRIDSIZE, z/ GRIDSIZE);
                status[i] = 0;
                i++;
            }
        }

    }
}
