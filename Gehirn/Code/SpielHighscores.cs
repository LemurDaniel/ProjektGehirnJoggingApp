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
        private SpielHighscores(){}

        // id => Spiel
        public Dictionary<int, Spiel> spiele = new Dictionary<int, Spiel>(); 

        public class Spiel
        {
            public int id_spiel;
            public string spiel;
            public int globaleBestzeit;
            public int globalerDurschnitt;
            public string rekordhalter;

            public int bestzeit;
            public int schlechtesteZeit;
            public int durchschnitt;
            public int anzahl_gespielt;
            public Int64 akummulierteSpielzeit;


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
            Request<SpielHighscores> request = new Request<SpielHighscores>();
            await request.HttpRequestAsync(App.FUNCTION_GET_HIGH_SCORES);
            SpielHighscores.GetInstance().spiele = request.obj.spiele;
        }


        public static async Task AktualisiereHighscoresAsync(Spiel spiel)
        {
            Request<Spiel> request = new Request<Spiel>();
            request.obj = spiel;
            await request.HttpRequestAsync(App.FUNCTION_UPDATE_HIGHSCORE);

            spiel.rekordhalter =        request.obj.rekordhalter;
            spiel.globaleBestzeit =     request.obj.globaleBestzeit;
            spiel.globalerDurschnitt =  request.obj.globalerDurschnitt;
        }
    }

}
