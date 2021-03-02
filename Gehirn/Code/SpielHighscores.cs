using Gehirn;
using GehirnJogging.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GehirnJogging.Code.Nutzer;

namespace GehirnJogging.Code
{


    /* SPIELHIGHSCORE */
    public class SpielHighscores
    {
        public static readonly int SCHIEBEPUZZLE_LEICHT = 0;
        public static readonly int SCHIEBEPUZZLE_NORMAL = 1;
        public static readonly int SCHIEBEPUZZLE_SCHWER = 2;
        public static readonly int WORT_GLEICH_FARBE = 3;
        public static readonly int KOPFRECHNEN = 4;
        public static readonly int WECHSELGELD = 5;
        public static readonly int BUCHSTABENSALAT = 6;
        public static readonly int MEMORIZE = 7;
        public static readonly int CONCENTRATE = 8;
        public static readonly int BUTTONPRESSER = 9;

        private static SpielHighscores instance;
        public static SpielHighscores GetInstance()
        {
            if (instance == null) instance = new SpielHighscores();
            return instance;
        }
        private SpielHighscores(){
            if (App.OFFLINE_MODE)
            {
                spiele.Add(SCHIEBEPUZZLE_LEICHT, new Spiel("SCHIEBEPUZZLE_LEICHT"));
                spiele.Add(SCHIEBEPUZZLE_NORMAL, new Spiel("SCHIEBEPUZZLE_NORMAL"));
                spiele.Add(SCHIEBEPUZZLE_SCHWER, new Spiel("SCHIEBEPUZZLE_SCHWER"));
                spiele.Add(WORT_GLEICH_FARBE, new Spiel("WORT_GLEICH_FARBE"));
                spiele.Add(KOPFRECHNEN, new Spiel("KOPFRECHNEN"));
                spiele.Add(WECHSELGELD, new Spiel("WECHSELGELD"));
                spiele.Add(BUCHSTABENSALAT, new Spiel("BUCHSTABENSALAT"));
                spiele.Add(MEMORIZE, new Spiel("MEMORIZE"));
                spiele.Add(CONCENTRATE, new Spiel("CONCENTRATE"));
                spiele.Add(BUTTONPRESSER, new Spiel("BUTTONPRESSER"));
            }
        }

        // id => Spiel
        public Dictionary<int, Spiel> spiele = new Dictionary<int, Spiel>(); 

        public class Spiel
        {
            public Spiel(){ }
            public Spiel(string name)
            {
                spiel = name;
            }

            public int id_spiel = 0;
            public string spiel = "undefined";
            public int globaleBestzeit = 0;
            public int globalerDurschnitt = 0;
            public string rekordhalter = "undefined";

            public int bestzeit = 0;
            public int schlechtesteZeit = 0;
            public int durchschnitt = 0;
            public int anzahl_gespielt = 0;
            public Int64 akummulierteSpielzeit = 0;


            public Spiel GetCopy()
            {
                Spiel original = this;
                return new Spiel
                {
                    id_spiel = original.id_spiel,
                    spiel = original.spiel,
                    globaleBestzeit = original.globaleBestzeit,
                    globalerDurschnitt = original.globalerDurschnitt,
                    rekordhalter = original.rekordhalter,
                    schlechtesteZeit = original.schlechtesteZeit,
                    bestzeit = original.bestzeit,
                    durchschnitt = original.durchschnitt,
                    anzahl_gespielt = original.anzahl_gespielt,
                    akummulierteSpielzeit = original.akummulierteSpielzeit
                };
            }
        }


        public static async Task HoleHighscoresAsync()
        {
            if (App.OFFLINE_MODE) return;

            Request<SpielHighscores> request = new Request<SpielHighscores>();
            await request.HttpRequestAsync(App.FUNCTION_GET_HIGH_SCORES);
            SpielHighscores.GetInstance().spiele = request.obj.spiele;
        }


        public static async Task AktualisiereHighscoresAsync(Spiel spiel)
        {
            if (App.OFFLINE_MODE) return;

            Request<Spiel> request = new Request<Spiel>();
            request.obj = spiel;
            await request.HttpRequestAsync(App.FUNCTION_UPDATE_HIGHSCORE);

            spiel.rekordhalter =        request.obj.rekordhalter;
            spiel.globaleBestzeit =     request.obj.globaleBestzeit;
            spiel.globalerDurschnitt =  request.obj.globalerDurschnitt;
        }
    }

}
