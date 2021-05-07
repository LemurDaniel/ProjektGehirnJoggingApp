using GehirnJogging.Code.Spiele;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using static GehirnJogging.Code.Spiele.SpielLogik;

namespace GehirnJogging.Pages.Controller
{
    public interface ISpielController
    {
        SpielLogik SpielLogik();
        bool Undoable();
        int TimePenalty();
        int DurchgaengeMin();

        void ZeigeSpielobjekt(SpielObjekt objekt);
        void Undo();
    }
}
