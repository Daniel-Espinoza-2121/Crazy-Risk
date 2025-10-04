using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrazyRisk.Core
{
    public class Territorios
    {
        public string Name { get; set; }
        public string Continente { get; set; }
        public int CantidadTropas { get; set; }
        public  EColorJugador PropietarioColor { get; set; }
        public Lista<Territorios> TerritoriosAdyacentes { get; set; }
        public int PosicionX { get; set; }
        public int PosicionY { get; set; }

        public Territorios(string name, string continente, int x, int y)
        {
            Name = name;
            Continente = continente;
            CantidadTropas = 0;
            PropietarioColor = EColorJugador.Neutral;
            TerritoriosAdyacentes = new Lista<Territorios>();
            PosicionX = x;
            PosicionY = y;
        }
        public void AgregarAdyacente(Territorios territorio)
        {
            if (!TerritoriosAdyacentes.Contiene(territorio))
            {
                TerritoriosAdyacentes.Agregar(territorio);
            }
        }

        public bool EsAdyacente(Territorios territorio)
        {
            return TerritoriosAdyacentes.Contiene(territorio);
        }
    }
}
