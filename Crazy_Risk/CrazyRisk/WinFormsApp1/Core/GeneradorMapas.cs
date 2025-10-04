using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrazyRisk.Core
{
    public static class GeneradorMapas
    {
        private static Random random = new Random();

        public static void CrearMapaCompleto(Mapa mapa)
        {
            // Limpiar territorios existentes
            mapa.Territorios = new Lista<Territorios>();

            // Crear territorios por continente
            CrearAsia(mapa);
            CrearEuropa(mapa);
            CrearAmericaNorte(mapa);
            CrearAmericaSur(mapa);
            CrearAfrica(mapa);
            CrearOceania(mapa);

            // Configurar todas las adyacencias
            ConfigurarAdyacenciasCompletas(mapa);
        }

        private static void CrearAsia(Mapa mapa)
        {
            var territoriosAsia = new[]
            {
                new Territorios("China", "Asia", 800, 240),
                new Territorios("India", "Asia", 700, 260),
                new Territorios("Japón", "Asia", 900, 220),
                new Territorios("Mongolia", "Asia", 720, 100),
                new Territorios("Siberia", "Asia", 800, 180), //parte asia de rusia
                new Territorios("Tailandia", "Asia", 810, 330),

                new Territorios("Ural", "Asia", 670, 110),
                new Territorios("Corea", "Asia", 900, 100),
                new Territorios("Siberia#2", "Asia", 785, 135),
                new Territorios("Siberia#3", "Asia", 810, 85),
                new Territorios("Medio Oriente", "Asia", 630, 250),
                new Territorios("Kazajstán", "Asia", 670, 190)

            };

            foreach (var territorio in territoriosAsia)
            {
                mapa.Territorios.Agregar(territorio);
            }
        }

        private static void CrearEuropa(Mapa mapa)
        {
            var territoriosEuropa = new[]
            {
                new Territorios("Francia", "Europa", 500, 145),
                new Territorios("Alemania", "Europa", 500, 75),
                new Territorios("España", "Europa", 440, 200),
                new Territorios("Italia", "Europa", 520, 180),
                new Territorios("Reino Unido", "Europa", 445, 140),
                new Territorios("Escandinavia", "Europa", 420, 80),
                new Territorios("Rusia Europea", "Europa", 590, 110)

            };

            foreach (var territorio in territoriosEuropa)
            {
                mapa.Territorios.Agregar(territorio);
            }
        }

        private static void CrearAmericaNorte(Mapa mapa)
        {
            var territoriosNA = new[]
            {
                new Territorios("Estados Unidos", "América del Norte", 115, 130),
                new Territorios("Canadá", "América del Norte", 160, 80),
                new Territorios("México", "América del Norte", 110, 280),
                new Territorios("Alaska", "América del Norte", 70, 75),
                new Territorios("Groenlandia", "América del Norte", 370, 60),
                new Territorios("Maine", "América del Norte", 240, 150),
                new Territorios("California", "América del Norte", 120, 190),
                new Territorios("Kansas", "América del Norte", 185, 130),
                new Territorios("Texas", "América del Norte", 180, 230)
            };

            foreach (var territorio in territoriosNA)
            {
                mapa.Territorios.Agregar(territorio);
            }
        }

        private static void CrearAmericaSur(Mapa mapa)
        {
            var territoriosSA = new[]
            {
                new Territorios("Brasil", "América del Sur", 300, 450),
                new Territorios("Argentina", "América del Sur", 240, 540),
                new Territorios("Perú", "América del Sur", 240, 490),
                new Territorios("Venezuela", "América del Sur", 210, 380),
                
            };

            foreach (var territorio in territoriosSA)
            {
                mapa.Territorios.Agregar(territorio);
            }
        }

        private static void CrearAfrica(Mapa mapa)
        {
            var territoriosAfrica = new[]
            {
                new Territorios("Egipto", "África", 520, 270),
                new Territorios("Sudáfrica", "África", 520, 480),
                new Territorios("Nigeria", "África", 570, 370),
                new Territorios("Congo", "África", 520, 390),
                new Territorios("Madagascar", "África", 620, 480),
                new Territorios("Marruecos", "África", 480, 340)
            };

            foreach (var territorio in territoriosAfrica)
            {
                mapa.Territorios.Agregar(territorio);
            }
        }

        private static void CrearOceania(Mapa mapa)
        {
            var territoriosOceania = new[]
            {
                new Territorios("Australia", "Oceanía", 857, 520),
                new Territorios("Nueva Zelanda", "Oceanía", 930, 550),
                new Territorios("Filipinas", "Oceanía", 930, 435),
                new Territorios("Indonesia", "Oceanía", 840, 415)
            };

            foreach (var territorio in territoriosOceania)
            {
                mapa.Territorios.Agregar(territorio);
            }
        }

        private static void ConfigurarAdyacenciasCompletas(Mapa mapa)
        {
            var territorios = mapa.Territorios.ObtenerTodos();
            var diccionarioTerritorios = new Dictionary<string, Territorios>();

            foreach (var territorio in territorios)
            {
                diccionarioTerritorios[territorio.Name] = territorio;
            }

            // Configurar adyacencias específicas (ejemplos principales)
            ConfigurarAdyacencia(diccionarioTerritorios, "China", "India");
            ConfigurarAdyacencia(diccionarioTerritorios, "China", "Mongolia");
            ConfigurarAdyacencia(diccionarioTerritorios, "China", "Siberia");
            ConfigurarAdyacencia(diccionarioTerritorios, "China", "Ural");
            ConfigurarAdyacencia(diccionarioTerritorios, "India", "Tailandia");
            ConfigurarAdyacencia(diccionarioTerritorios, "Corea", "Japón");
            ConfigurarAdyacencia(diccionarioTerritorios, "Corea", "Siberia#3");
            ConfigurarAdyacencia(diccionarioTerritorios, "Corea", "Siberia#2");
            ConfigurarAdyacencia(diccionarioTerritorios, "Corea", "Siberia");
            ConfigurarAdyacencia(diccionarioTerritorios, "Tailandia", "China");
            ConfigurarAdyacencia(diccionarioTerritorios, "Medio Oriente", "India");
            ConfigurarAdyacencia(diccionarioTerritorios, "Medio Oriente", "Egipto"); // interestatal
            ConfigurarAdyacencia(diccionarioTerritorios, "Medio Oriente", "Nigeria"); // interestatal
            ConfigurarAdyacencia(diccionarioTerritorios, "Kazajstán", "Ural");
            ConfigurarAdyacencia(diccionarioTerritorios, "Kazajstán", "China");
            ConfigurarAdyacencia(diccionarioTerritorios, "Kazajstán", "India");
            ConfigurarAdyacencia(diccionarioTerritorios, "Kazajstán", "Medio Oriente");
            ConfigurarAdyacencia(diccionarioTerritorios, "Siberia#2", "Mongolia");
            ConfigurarAdyacencia(diccionarioTerritorios, "Siberia#2", "Siberia#3");
            ConfigurarAdyacencia(diccionarioTerritorios, "Siberia#2", "Siberia");
            ConfigurarAdyacencia(diccionarioTerritorios, "Mongolia", "Siberia#3");
            ConfigurarAdyacencia(diccionarioTerritorios, "Siberia", "Japón");
            ConfigurarAdyacencia(diccionarioTerritorios, "Ural", "Mongolia");

            //europa
            ConfigurarAdyacencia(diccionarioTerritorios, "Francia", "España");
            ConfigurarAdyacencia(diccionarioTerritorios, "Francia", "Alemania");
            ConfigurarAdyacencia(diccionarioTerritorios, "Francia", "Italia");
            ConfigurarAdyacencia(diccionarioTerritorios, "Francia", "Rusia Europea");
            ConfigurarAdyacencia(diccionarioTerritorios, "Alemania", "Reino Unido");
            ConfigurarAdyacencia(diccionarioTerritorios, "Alemania", "Escandinavia"); 
            ConfigurarAdyacencia(diccionarioTerritorios, "Reino Unido", "Francia");
            ConfigurarAdyacencia(diccionarioTerritorios, "Reino Unido", "España");
            ConfigurarAdyacencia(diccionarioTerritorios, "Reino Unido", "Escandinavia");
            ConfigurarAdyacencia(diccionarioTerritorios, "Rusia Europea", "Alemania");
            ConfigurarAdyacencia(diccionarioTerritorios, "Rusia Europea", "Ural");//interestatal
            ConfigurarAdyacencia(diccionarioTerritorios, "Rusia Europea", "Italia");
            ConfigurarAdyacencia(diccionarioTerritorios, "Rusia Europea", "Medio Oriente");//interestatal
            ConfigurarAdyacencia(diccionarioTerritorios, "Italia", "España");

            //america norte
            ConfigurarAdyacencia(diccionarioTerritorios, "Estados Unidos", "Canadá");
            ConfigurarAdyacencia(diccionarioTerritorios, "Estados Unidos", "Kansas");
            ConfigurarAdyacencia(diccionarioTerritorios, "Canadá", "Alaska");
            ConfigurarAdyacencia(diccionarioTerritorios, "Canadá", "Groenlandia");
            ConfigurarAdyacencia(diccionarioTerritorios, "Canadá", "Kansas");
            ConfigurarAdyacencia(diccionarioTerritorios, "California", "México");
            ConfigurarAdyacencia(diccionarioTerritorios, "California", "Estados Unidos");
            ConfigurarAdyacencia(diccionarioTerritorios, "California", "Texas");
            ConfigurarAdyacencia(diccionarioTerritorios, "Texas", "Kansas");
            ConfigurarAdyacencia(diccionarioTerritorios, "Texas", "México");
            ConfigurarAdyacencia(diccionarioTerritorios, "Maine", "Kansas");
            ConfigurarAdyacencia(diccionarioTerritorios, "Texas", "Maine");
            ConfigurarAdyacencia(diccionarioTerritorios, "Kansas", "Groenlandia");
            ConfigurarAdyacencia(diccionarioTerritorios, "Maine", "Groenlandia");

            //america sur
            ConfigurarAdyacencia(diccionarioTerritorios, "Brasil", "Argentina");
            ConfigurarAdyacencia(diccionarioTerritorios, "Brasil", "Perú");
            ConfigurarAdyacencia(diccionarioTerritorios, "Brasil", "Venezuela");
            ConfigurarAdyacencia(diccionarioTerritorios, "Venezuela", "Perú");
            ConfigurarAdyacencia(diccionarioTerritorios, "Perú", "Argentina");
            ConfigurarAdyacencia(diccionarioTerritorios, "Brasil", "Marruecos");

            //africa
            ConfigurarAdyacencia(diccionarioTerritorios, "Egipto", "Nigeria");
            ConfigurarAdyacencia(diccionarioTerritorios, "Egipto", "Marruecos");
            ConfigurarAdyacencia(diccionarioTerritorios, "Nigeria", "Congo");
            ConfigurarAdyacencia(diccionarioTerritorios, "Congo", "Sudáfrica");
            ConfigurarAdyacencia(diccionarioTerritorios, "Nigeria", "Marruecos");
            ConfigurarAdyacencia(diccionarioTerritorios, "Nigeria", "Madagascar");
            ConfigurarAdyacencia(diccionarioTerritorios, "Nigeria", "Sudáfrica");
            ConfigurarAdyacencia(diccionarioTerritorios, "Congo", "Marruecos");
            ConfigurarAdyacencia(diccionarioTerritorios, "Madagascar", "Sudáfrica");

            //oceania
            ConfigurarAdyacencia(diccionarioTerritorios, "Australia", "Nueva Zelanda");
            ConfigurarAdyacencia(diccionarioTerritorios, "Australia", "Indonesia");
            ConfigurarAdyacencia(diccionarioTerritorios, "Filipinas", "Indonesia");
            ConfigurarAdyacencia(diccionarioTerritorios, "Filipinas", "Australia");
            ConfigurarAdyacencia(diccionarioTerritorios, "Filipinas", "Nueva Zelanda");

            // Conexiones intercontinentales importantes
            ConfigurarAdyacencia(diccionarioTerritorios, "Alaska", "Corea"); // Estrecho de Bering
            ConfigurarAdyacencia(diccionarioTerritorios, "Groenlandia", "Escandinavia");
            ConfigurarAdyacencia(diccionarioTerritorios, "España", "Marruecos"); // Gibraltar
            ConfigurarAdyacencia(diccionarioTerritorios, "México", "Venezuela"); // Conexión América
            ConfigurarAdyacencia(diccionarioTerritorios, "Egipto", "Italia"); // Mediterráneo
            ConfigurarAdyacencia(diccionarioTerritorios, "Tailandia", "Indonesia"); // Sudeste asiático

            
        }

        private static void ConfigurarAdyacencia(Dictionary<string, Territorios> territorios, string nombre1, string nombre2)
        {
            if (territorios.ContainsKey(nombre1) && territorios.ContainsKey(nombre2))
            {
                territorios[nombre1].AgregarAdyacente(territorios[nombre2]);
                territorios[nombre2].AgregarAdyacente(territorios[nombre1]);
            }
        }
    }
}
