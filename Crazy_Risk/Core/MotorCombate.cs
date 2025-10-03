using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrazyRisk.Core
{
    public class ResultadoCombate
    {
        public int TropasAtacantesPerdidas { get; set; }
        public int TropasDefensorPerdidas { get; set; }
        public bool TerritorioConquistado { get; set; }
        public int[] DadosAtacante { get; set; }
        public int[] DadosDefensor { get; set; }

        public ResultadoCombate()
        {
            TropasAtacantesPerdidas = 0;
            TropasDefensorPerdidas = 0;
            TerritorioConquistado = false;
        }
    }
    public class MotorCombate
    {
        private Random random;
        public MotorCombate()
        {
            random = new Random();
        }

        public int[] LanzarDados(int cantidad) //obtener numero de dados y ordenar mayor a menor
        {
            int[] dados = new int[cantidad];
            for (int i = 0; i < cantidad; i++)
            {
                dados[i] = random.Next(1, 7);
            }

            // Ordenar de mayor a menor
            Array.Sort(dados);
            Array.Reverse(dados);

            return dados;
        }

        //combatir los dados
        public ResultadoCombate ResolverCombate(int tropasAtacante, int tropasDefensor)
        {
            int dadosAtacante = Math.Min(3, tropasAtacante);
            int dadosDefensor = Math.Min(2, tropasDefensor);

            int[] dadosAtaq = LanzarDados(dadosAtacante);
            int[] dadosDef = LanzarDados(dadosDefensor);

            ResultadoCombate resultado = new ResultadoCombate();
            resultado.DadosAtacante = dadosAtaq;
            resultado.DadosDefensor = dadosDef;

            // Comparar dados
            int comparaciones = Math.Min(dadosAtacante, dadosDefensor);
            for (int i = 0; i < comparaciones; i++)
            {
                if (dadosAtaq[i] > dadosDef[i])
                {
                    resultado.TropasDefensorPerdidas++;
                }
                else
                {
                    resultado.TropasAtacantesPerdidas++;
                }
            }

            return resultado;
        }
    }
}

