using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrazyRisk.Core
{
    public class Tarjeta
    {
        public ETipoTarjeta Tipo { get; set; }
        public string NombreTerritorio { get; set; }
        public bool Utilizada { get; set; }
        public Tarjeta(ETipoTarjeta tipo, string nombreTerritorio)
        {
            Tipo = tipo;
            NombreTerritorio = nombreTerritorio;
            Utilizada = false;
        }
    }
}
