using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using static GehirnJogging.Code.Spiele.SpielLogik;

namespace GehirnJogging.Code.Spiele
{
    class BuchstabenAufgabe : SpielObjekt
    {
        public static int MAX_SIZE = 10;
        public static string[] WORTE = {"Test"};
        public static bool read = false;
        public static string test = "Test";

        public static async Task ReadFile(){
            StorageFile file = await StorageFile.GetFileFromPathAsync(Windows.ApplicationModel.Package.Current.InstalledLocation.Path + "\\Code\\Spiele\\Worte.txt");
            IList<string> lines = await FileIO.ReadLinesAsync(file, Windows.Storage.Streams.UnicodeEncoding.Utf8);

            IList<string> temp = new List<string>();
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Length > MAX_SIZE || temp.Contains(lines[i])) continue;

                temp.Add(lines[i].Trim());
                if (lines[i].Length > test.Length)  test = lines[i];
            }

            WORTE = temp.ToArray<string>();
            read = true;
        }


        internal class State
        {
            internal State(char[] nutzereingabe, char[] vorgabe, int[] pos)
            {
                this.nutzereingabe = nutzereingabe;
                this.pos = pos;
                this.vorgabe = vorgabe;
            }
            internal char[] nutzereingabe;
            public int[] pos;
            internal char[] vorgabe;

            public bool Vergleich(State st)
            {
                for(int i=0; i<nutzereingabe.Length; i++)
                {
                    if (nutzereingabe[i] != st.nutzereingabe[i]) return false;
                    if (pos[i] != st.pos[i]) return false;
                    if (vorgabe[i] != st.vorgabe[i]) return false;
                }
                return true;
            }
        }

        public readonly string wort;

        public char[] nutzereingabe;
        public int[] pos;
        public char[] vorgabe;

        private Stack<State> moves = new Stack<State>();

        public void SaveState() {
            State st = new State((char[])nutzereingabe.Clone(), (char[])vorgabe.Clone(), (int[])pos.Clone());
            if (moves.Count==0 || !moves.Peek().Vergleich(st)) moves.Push(st);
        }
        public bool Undo()
        {
            if (Finished || moves.Count == 0) return false;
            nutzereingabe = moves.Peek().nutzereingabe;
            pos = moves.Peek().pos;
            vorgabe = moves.Pop().vorgabe;
            return true;
        }

        public void Pruefe()
        {
            for (int i = 0; i < wort.Length; i++)
            {
                if (wort[i] != nutzereingabe[i])
                {
                    Correct = false;
                    return;
                }
            }
            Correct = true;
        }

        public BuchstabenAufgabe()
        {
            wort = WORTE[rand.Next(0, WORTE.Length)];
            vorgabe = new char[wort.Length];
            nutzereingabe = new char[wort.Length];
            pos = new int[wort.Length];

            List<Char> temp = new List<char>();
            foreach (char c in wort) temp.Add(c);

            int i = 0;
            while(temp.Count > 0)
            {
                int random = rand.Next(0, temp.Count);
                vorgabe[i] = temp[random];
                temp.RemoveAt(random);

                nutzereingabe[i] = '-';
                pos[i] = -1;
                i++;
            }
        }
    }
}
