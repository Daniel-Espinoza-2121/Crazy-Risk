using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrazyRisk.Core
{
    public class Jugador
    {
        public string Nombre { get; set; }
        public EColorJugador Color { get; set; }
        public Lista<Tarjeta> Tarjetas { get; set; }
        public Lista<Territorios> TerritoriosControlados { get; set; }
        public int TropasDisponibles { get; set; }
        public bool EsNeutral { get; set; }

        public Jugador(string nombre, EColorJugador color, bool esNeutral = false)
        {
            Nombre = nombre;
            Color = color;
            Tarjetas = new Lista<Tarjeta>();
            TerritoriosControlados = new Lista<Territorios>();
            TropasDisponibles = 0;
            EsNeutral = esNeutral;
        }
        public void AgregarTarjeta(Tarjeta tarjeta)
        {
            if(Tarjetas.Contar < 6)
            {
                Tarjetas.Agregar(tarjeta);
            }
        }

        
        public bool TieneTrioTarjetas()
        {
            return ObtenerTriosDisponibles().Count > 0;
        }

        //Return todas las combinaciones de tipo disponibles
        public List<TipoTrioTarjetas> ObtenerTriosDisponibles()
        {
            var triosDisponibles = new List<TipoTrioTarjetas>();
            Tarjeta[] misCartas = Tarjetas.ObtenerTodos();

            if (misCartas.Length < 3) return triosDisponibles;

            // Verificar de 3 direfentes
            bool tieneInfanteria = false, tieneCaballeria = false, tieneArtilleria = false;
            foreach (var carta in misCartas)
            {
                switch (carta.Tipo)
                {
                    case ETipoTarjeta.Infanteria: tieneInfanteria = true; break;
                    case ETipoTarjeta.Caballeria: tieneCaballeria = true; break;
                    case ETipoTarjeta.Artilleria: tieneArtilleria = true; break;
                }
            }

            if (tieneInfanteria && tieneCaballeria && tieneArtilleria)
            {
                triosDisponibles.Add(TipoTrioTarjetas.Diferentes);
            }

            // Verificar de 3 tarjetas en mismo tipo
            int[] contadorTipos = new int[3];
            foreach (var carta in misCartas)
            {
                contadorTipos[(int)carta.Tipo]++;
            }

            if (contadorTipos[0] >= 3) triosDisponibles.Add(TipoTrioTarjetas.TresInfanteria);
            if (contadorTipos[1] >= 3) triosDisponibles.Add(TipoTrioTarjetas.TresCaballeria);
            if (contadorTipos[2] >= 3) triosDisponibles.Add(TipoTrioTarjetas.TresArtilleria);

            return triosDisponibles;
        }

        

        // Remover 3 tarjetas del tipo especificado
        public void RemoverTrioTarjetas(TipoTrioTarjetas tipoTrio)
        {
            Tarjeta[] todasLasCartas = Tarjetas.ObtenerTodos();
            Lista<Tarjeta> cartasARemover = new Lista<Tarjeta>();

            switch (tipoTrio)
            {
                case TipoTrioTarjetas.Diferentes:
                    // Remover uno de cada tipo
                    bool removidaInfanteria = false, removidaCaballeria = false, removidaArtilleria = false;
                    foreach (var carta in todasLasCartas)
                    {
                        if (!removidaInfanteria && carta.Tipo == ETipoTarjeta.Infanteria)
                        {
                            cartasARemover.Agregar(carta);
                            removidaInfanteria = true;
                        }
                        else if (!removidaCaballeria && carta.Tipo == ETipoTarjeta.Caballeria)
                        {
                            cartasARemover.Agregar(carta);
                            removidaCaballeria = true;
                        }
                        else if (!removidaArtilleria && carta.Tipo == ETipoTarjeta.Artilleria)
                        {
                            cartasARemover.Agregar(carta);
                            removidaArtilleria = true;
                        }

                        if (cartasARemover.Contar == 3) break;
                    }
                    break;

                case TipoTrioTarjetas.TresInfanteria:
                    RemoverTresDelTipo(ETipoTarjeta.Infanteria, cartasARemover, todasLasCartas);
                    break;

                case TipoTrioTarjetas.TresCaballeria:
                    RemoverTresDelTipo(ETipoTarjeta.Caballeria, cartasARemover, todasLasCartas);
                    break;

                case TipoTrioTarjetas.TresArtilleria:
                    RemoverTresDelTipo(ETipoTarjeta.Artilleria, cartasARemover, todasLasCartas);
                    break;
            }

            // Remover la tarjeta que se cambio
            foreach (var carta in cartasARemover.ObtenerTodos())
            {
                carta.Utilizada = true;
                Tarjetas.Remover(carta);
            }
        }

        private void RemoverTresDelTipo(ETipoTarjeta tipo, Lista<Tarjeta> cartasARemover, Tarjeta[] todasLasCartas)
        {
            int contadorRemovidas = 0;
            foreach (var carta in todasLasCartas)
            {
                if (carta.Tipo == tipo && contadorRemovidas < 3)
                {
                    cartasARemover.Agregar(carta);
                    contadorRemovidas++;
                }
            }
        }

        public int CalcularRefuerzos(Dictionary<string, int> bonusContinentes, Mapa mapa)
        {
            // Refuerzos basicos, al menos 3
            int refuerzos = Math.Max(3, TerritoriosControlados.Contar / 3);

            // Bonus sobre control todos territorios de un continente
            refuerzos += CalcularBonusContinentes(bonusContinentes, mapa);

            return refuerzos;
        }

        //Logica de calcular bonus de continentes
        private int CalcularBonusContinentes(Dictionary<string, int> bonusContinentes, Mapa mapa)
        {
            int bonusTotal = 0;

            // Calcular territorios se contro de cada continente
            Dictionary<string, int> misTerritorioPorContinente = new Dictionary<string, int>();
            foreach (var territorio in TerritoriosControlados.ObtenerTodos())
            {
                if (!misTerritorioPorContinente.ContainsKey(territorio.Continente))
                    misTerritorioPorContinente[territorio.Continente] = 0;
                misTerritorioPorContinente[territorio.Continente]++;
            }

            // Calcular total de territorios de cotinente
            Dictionary<string, int> totalTerritorioPorContinente = new Dictionary<string, int>();
            foreach (var territorio in mapa.Territorios.ObtenerTodos())
            {
                if (!totalTerritorioPorContinente.ContainsKey(territorio.Continente))
                    totalTerritorioPorContinente[territorio.Continente] = 0;
                totalTerritorioPorContinente[territorio.Continente]++;
            }

            //Bonus solo otorgaran si control completamente el continente
            foreach (var continente in misTerritorioPorContinente.Keys)
            {
                int misTerritorios = misTerritorioPorContinente[continente];
                int totalTerritorios = totalTerritorioPorContinente.GetValueOrDefault(continente, 0);

                
                if (misTerritorios == totalTerritorios && bonusContinentes.ContainsKey(continente))
                {
                    bonusTotal += bonusContinentes[continente];
                }
            }

            return bonusTotal;
        }

        //Verificar si control completamente el continente
        public bool ControlaCompletamenteContinente(string continente, Mapa mapa)
        {
            int misTerritorios = 0;
            int totalTerritorios = 0;

            foreach (var territorio in mapa.Territorios.ObtenerTodos())
            {
                if (territorio.Continente == continente)
                {
                    totalTerritorios++;
                    if (TerritoriosControlados.Contiene(territorio))
                    {
                        misTerritorios++;
                    }
                }
            }
            if (misTerritorios == totalTerritorios && totalTerritorios > 0)
            {
                Console.WriteLine($"¡{Nombre} controla completamente {continente}! (+{mapa.BonusContinentes[continente]} tropas)");
            }

            return misTerritorios > 0 && misTerritorios == totalTerritorios;
        }
    }


    }

