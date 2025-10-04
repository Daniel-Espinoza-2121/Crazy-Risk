using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrazyRisk.Core
{
    public class Mapa
    {
        public Lista<Territorios> Territorios { get; set; }
        public Dictionary<string, int> BonusContinentes { get; set; }
        public int ContadorIntercambios { get; set; }// usa serie de Fibonacci a partir de 2

        public void InicializarBonusContinentes()
        {
            BonusContinentes = new Dictionary<string, int>
            {
                { "Asia", 7},
            { "América del Norte", 3},
            { "Europa", 5},
                {"Africa", 3 },
                {"América del Sur", 2 },
                {"Oceania", 2 }

            };
        }
        
        
        public Mapa(){
            Territorios = new Lista<Territorios>();
            ContadorIntercambios = 0;
            InicializarBonusContinentes();
           

            }

        

        public Territorios ObtenerTerritorioPorNombre(string nombre)
        {
            foreach (var territorio in Territorios.ObtenerTodos())
            {
                if (territorio.Name.Equals(nombre, StringComparison.OrdinalIgnoreCase))
                    return territorio;
            }
            return null;
        }

        public int CalcularTropasIntercambio(int n)
        {
            if (n == 1) return 2;
            if (n == 2) return 3;
            int a = 2, b = 3;
            for (int i = 3; i <= n; i++)
            {
                int temp = a + b;
                a = b;
                b = temp;
            }
            return b;
        }

        
    }
}
