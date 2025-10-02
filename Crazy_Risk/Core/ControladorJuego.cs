using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrazyRisk.Core
{
    public class ControladorJuego
    {
        public Mapa MapaJuego { get; private set; }
        public Lista<Jugador> Jugadores { get; private set; }
        public EstadoJuego EstadoActual { get; set; }
        public int TurnoActual { get; set; }
        public MotorCombate MotorCombate { get; private set; }
        private Random random;

        public bool ModoTresJugadores { get; private set; }
        public int MaximoJugadores { get; private set; }

        private bool movioEnEstaPlaneacion = false;
        public bool TieneEjercitoNeutral { get; private set; }


        public ControladorJuego()
        {
            MapaJuego = new Mapa();
            Jugadores = new Lista<Jugador>();
            EstadoActual = EstadoJuego.Preparacion;
            TurnoActual = 0;
            random = new Random();
            MotorCombate = new MotorCombate();
            ModoTresJugadores = false;
            MaximoJugadores = 2;
            TieneEjercitoNeutral = false;
            GeneradorMapas.CrearMapaCompleto(MapaJuego);

        }

        // Metodo de control turnos
        public void ComenzarTurno()
        {
            Jugador jugadorActual = ObtenerJugadorActual();

            // Calcular y asignar refuerzos
            IniciarFaseRefuerzos();
            EstadoActual = EstadoJuego.Refuerzos;
        }

        // Terminar estado de refuerzos, entra estado de ataque
        public void TerminarFaseRefuerzos()
        {
            Jugador jugadorActual = ObtenerJugadorActual();

            // Verificar que se hayan asignado todos los refuerzos
            if (jugadorActual.TropasDisponibles > 0)
            {
                throw new InvalidOperationException("Debes asignar todas las tropas de refuerzo antes de continuar");
            }

            EstadoActual = EstadoJuego.Ataque;//Entra fase de ataque
        }

        // Asignar tropas
        public bool AsignarTropas(Territorios territorio, int cantidad)
        {
            if (EstadoActual != EstadoJuego.Refuerzos && EstadoActual != EstadoJuego.DistribucionInicial)
            {
                throw new InvalidOperationException("Solo puedes asignar tropas en la fase de refuerzos");
            }

            Jugador jugadorActual = ObtenerJugadorActual();

            // Verificar si tiene ese territorio
            if (territorio.PropietarioColor != jugadorActual.Color)
            {
                return false;
            }

            // verificar si tiene cantidad suficiente de tropas
            if (jugadorActual.TropasDisponibles < cantidad)
            {
                return false;
            }

            territorio.CantidadTropas += cantidad;
            jugadorActual.TropasDisponibles -= cantidad;
            return true;
        }

        // Terminar estado de ataque
        public void TerminarFaseAtaque()
        {
            EstadoActual = EstadoJuego.Planeacion;
            
            movioEnEstaPlaneacion = false;
        }

        // Estado de planeacion(movimiento)
        public bool EjecutarMovimientoPlaneacion(Territorios origen, Territorios destino, int cantidad)
        {
            if (EstadoActual != EstadoJuego.Planeacion)
            {
                throw new InvalidOperationException("Solo puedes mover tropas en la fase de planeación");
            }

            if (movioEnEstaPlaneacion)
            {
                return false; // ya movió una vez
            }
            if (!PuedeMoverTropas(origen, destino, cantidad))
            {
                return false;
            }

            origen.CantidadTropas -= cantidad;
            destino.CantidadTropas += cantidad;

            movioEnEstaPlaneacion = true; // marcar que ya movió

            // Solo puede mover una vez
            return true;
        }

        // Terminar turno actual
        public void TerminarTurno()
        {
            // Verificar si tiene ganador
            if (VerificarVictoria())
            {
                return;
            }

            // A siguiente jugador
            SiguienteTurno();

            // Comenzar nuevo turno
            ComenzarTurno();
        }

        // Intercambio forzado de tarjetas(cuando tiene 6)
        public void ForzarIntercambioTarjetas(Jugador jugador)
        {
            if (jugador.Tarjetas.Contar >= 6)
            {
                if (!jugador.TieneTrioTarjetas())
                {
                    throw new InvalidOperationException("No tienes un trío válido de tarjetas para intercambiar");
                }

                int tropasExtra = IntercambiarTarjetas(jugador);
                jugador.TropasDisponibles += tropasExtra;
            }
        }

        // Opcional:Intercambiar tarjetas(cuando sea posible)
        public bool IntentarIntercambiarTarjetas(Jugador jugador)
        {
            if (EstadoActual != EstadoJuego.Refuerzos)
            {
                throw new InvalidOperationException("Solo puedes intercambiar tarjetas en la fase de refuerzos");
            }

            if (jugador.Tarjetas.Contar < 3)
            {
                return false;
            }

            if (!jugador.TieneTrioTarjetas())
            {
                return false;
            }

            int tropasExtra = IntercambiarTarjetas(jugador);
            jugador.TropasDisponibles += tropasExtra;
            return true;
        }

        public int IntercambiarTarjetasEspecificas(Jugador jugador, TipoTrioTarjetas tipoTrio)
        {
            var triosDisponibles = jugador.ObtenerTriosDisponibles(); // Usar metodo de clase Jugador

            if (!triosDisponibles.Contains(tipoTrio))
            {
                throw new InvalidOperationException("No tienes este tipo de trío disponible");
            }

            // Calcular bonus de tropas
            MapaJuego.ContadorIntercambios++;
            int tropasObtenidas = MapaJuego.CalcularTropasIntercambio(MapaJuego.ContadorIntercambios);

            jugador.RemoverTrioTarjetas(tipoTrio);

            return tropasObtenidas;
        }


        public int IntercambiarTarjetas(Jugador jugador)
        {
            var triosDisponibles = jugador.ObtenerTriosDisponibles();

            if (triosDisponibles.Count == 0) return 0;

            // De forma predeterminada se selecciona la primera combinacion disponible
            TipoTrioTarjetas primerTrio = triosDisponibles[0];

            return IntercambiarTarjetasEspecificas(jugador, primerTrio);
        }

        // Verficar si el estado actual permite una operacion
        public bool PuedeRealizarAccion(EstadoJuego estadoRequerido)
        {
            return EstadoActual == estadoRequerido;
        }

        public void HabilitarTresJugadores()
        {
            ModoTresJugadores = true;
            MaximoJugadores = 3;

            //Cambiar las tropas iniciales de todos los jugadores existentes
            Jugador[] todosJugadores = Jugadores.ObtenerTodos();
            foreach(var jugador in todosJugadores)
            {
                jugador.TropasDisponibles = 35;
            }
        }

        // Habilitar jugador neutra(cuando esta metodo de 2 jugador)
        public void HabilitarEjercitoNeutral()
        {
            if (ModoTresJugadores)
            {
                throw new InvalidOperationException("El ejército neutral solo está disponible en modo 2 jugadores");
            }

            if (Jugadores.Contar >= 2)
            {
                throw new InvalidOperationException("Debes habilitar el ejército neutral antes de que se conecten los jugadores");
            }

            TieneEjercitoNeutral = true;
            MaximoJugadores = 2; // Solo 2 jugadores humanos
        }

        // Crear tropas neutra（Llamada automatica）
        private void CrearEjercitoNeutral()
        {
            var ejercitoNeutral = new Jugador("Ejército Neutral", EColorJugador.Neutral, true);
            ejercitoNeutral.TropasDisponibles = 40; // Mismo tropas de jugador
            Jugadores.Agregar(ejercitoNeutral);
        }

        public void AgregarJugador(string nombre, EColorJugador color)
        {
            //  Verificar límite de jugadores
            if (Jugadores.Contar >= MaximoJugadores)
            {
                throw new InvalidOperationException($"Ya hay {MaximoJugadores} jugadores conectados");
            }

            var nuevoJugador = new Jugador(nombre, color);

            //  Ajustar tropas según el modo de juego
            if (ModoTresJugadores)
            {
                nuevoJugador.TropasDisponibles = 35;
            }
            else
            {
                nuevoJugador.TropasDisponibles = 40;
            }

            Jugadores.Agregar(nuevoJugador);
        }

        public bool PuedeIniciarJuego()
        {
            if (ModoTresJugadores)
            {
                return Jugadores.Contar == 3;
            }
            else
            {
                // Metodo de 2 jugador(solo 2 jugador humano)
                int jugadoresHumanos = 0;
                foreach (var jugador in Jugadores.ObtenerTodos())
                {
                    if (!jugador.EsNeutral)
                        jugadoresHumanos++;
                }
                return jugadoresHumanos == 2;
            }
        }

        public void DistribuirTerritorios()
        {
            // si hay neutra habilitados y aun no creados, se crea
            if (TieneEjercitoNeutral && Jugadores.Contar == 2)
            {
                CrearEjercitoNeutral();
            }

            if (!PuedeIniciarJuego() && !(TieneEjercitoNeutral && Jugadores.Contar == 3))
            {
                throw new InvalidOperationException("No hay suficientes jugadores para iniciar");
            }

            Territorios[] todosTerritorios = MapaJuego.Territorios.ObtenerTodos();
            Jugador[] todosJugadores = Jugadores.ObtenerTodos();

            // Verificar que tenemos 42 territorios
            if (todosTerritorios.Length != 42)
            {
                throw new InvalidOperationException($"El mapa debe tener 42 territorios, tiene {todosTerritorios.Length}");
            }

            // Mezclar territorios
            for (int i = todosTerritorios.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                Territorios temp = todosTerritorios[i];
                todosTerritorios[i] = todosTerritorios[j];
                todosTerritorios[j] = temp;
            }

            // Distribuir equitativamente
            int jugadorActual = 0;
            foreach (var territorio in todosTerritorios)
            {
                Jugador jugador = todosJugadores[jugadorActual % todosJugadores.Length];
                territorio.PropietarioColor = jugador.Color;
                territorio.CantidadTropas = 1;
                jugador.TerritoriosControlados.Agregar(territorio);
                jugador.TropasDisponibles--;

                jugadorActual++;
            }

            EstadoActual = EstadoJuego.DistribucionInicial;

            
        }

        // Asigna tropa inicial(ante de empezar turno normal)
        public bool ColocarTropaInicial(Territorios territorio = null)
        {
            if (EstadoActual != EstadoJuego.DistribucionInicial)
            {
                throw new InvalidOperationException("Solo puedes colocar tropas en la fase de distribución inicial");
            }

            Jugador jugadorActual = ObtenerJugadorActual();

            

            // Verificar si sigue teniendo tropa disponible
            if (jugadorActual.TropasDisponibles <= 0)
            {
                return false;
            }

            if (jugadorActual.EsNeutral)
            {
                // Neutral asigna tropa en forma ramdon
                var territoriosNeutrales = jugadorActual.TerritoriosControlados.ObtenerTodos();
                if (territoriosNeutrales.Length == 0) return false;
                Territorios elegido = territoriosNeutrales[random.Next(territoriosNeutrales.Length)];

                elegido.CantidadTropas++;
                jugadorActual.TropasDisponibles--;

                return true;
            }
            else
            {
                // Jugador asignar tropa en su territorio elegida
                if (territorio == null) return false;
                if (territorio.PropietarioColor != jugadorActual.Color) return false;

                territorio.CantidadTropas++;
                jugadorActual.TropasDisponibles--;
                return true;
            }          
        }

        // Verificar si todo jugador ya asignado sus tropas inicial
        public bool DistribucionInicialCompleta()
        {
            Jugador[] todosJugadores = Jugadores.ObtenerTodos();

            foreach (var jugador in todosJugadores)
            {
                if (jugador.TropasDisponibles > 0)
                    return false;
            }

            return true;
        }

        // Finalizar asignar tropa inicial, empezar turno normal
        public void FinalizarDistribucionInicial()
        {
            if (!DistribucionInicialCompleta())
            {
                throw new InvalidOperationException("Todos los jugadores deben colocar todas sus tropas antes de comenzar");
            }

            // Empezar turno de primer jugador
            TurnoActual = 0;
            ComenzarTurno();
        }
        public Jugador ObtenerJugadorPorColor(EColorJugador color)
        {
            Jugador[] todosJugadores = Jugadores.ObtenerTodos();
            foreach (var jugador in todosJugadores)
            {
                if (jugador.Color == color)
                    return jugador;
            }
            return null;
        }

        // Obtener tropas de neutra
        public Jugador ObtenerEjercitoNeutral()
        {
            Jugador[] todosJugadores = Jugadores.ObtenerTodos();
            foreach (var jugador in todosJugadores)
            {
                if (jugador.EsNeutral && jugador.Color == EColorJugador.Neutral)
                    return jugador;
            }
            return null;
        }

        // Neutra se colocar automaticamente las tropas restantes
        public void ColocarTropasNeutralAleatoriamente()
        {
            var ejercitoNeutral = ObtenerEjercitoNeutral();
            if (ejercitoNeutral == null) return;

            var territoriosNeutrales = ejercitoNeutral.TerritoriosControlados.ObtenerTodos();

            while (ejercitoNeutral.TropasDisponibles > 0)
            {
                // Seleccionar un territorios neutral en random
                int indiceAleatorio = random.Next(territoriosNeutrales.Length);
                Territorios territorio = territoriosNeutrales[indiceAleatorio];

                territorio.CantidadTropas++;
                ejercitoNeutral.TropasDisponibles--;
            }
        }

        public bool PuedeAtacar(Territorios origen, Territorios destino)
        {
            Jugador atacante = ObtenerJugadorPorColor(origen.PropietarioColor);

            // Neutral no se atacar
            if (atacante != null && atacante.EsNeutral)
                return false;

            return origen.CantidadTropas > 1 && //Al meno dos tropas en territorio origen
                   origen.EsAdyacente(destino) &&//Los dos territorios es adyacente
                   origen.PropietarioColor != destino.PropietarioColor;//Es distinto color(Diferente jugador)
        }

        public ResultadoCombate EjecutarAtaque(Territorios atacante, Territorios defensor, int tropasAtacante, int tropasDefensor)
        {
            var resultado = MotorCombate.ResolverCombate(tropasAtacante, tropasDefensor);

            //Actualizar cantidad de tropas de territorio defensa y ataque
            atacante.CantidadTropas -= resultado.TropasAtacantesPerdidas;
            defensor.CantidadTropas -= resultado.TropasDefensorPerdidas;

            //Verificar si conquista el territorio
            if (defensor.CantidadTropas <= 0)
            {
                resultado.TerritorioConquistado = true;
                ConquistarTerritorio(atacante, defensor, tropasAtacante);
            }

            return resultado;
        }

        public bool VerificarVictoria()
        {
            Jugador[] todosJugadores = Jugadores.ObtenerTodos();

            foreach (var jugador in todosJugadores)
            {
                if (!jugador.EsNeutral)
                {
                    // Contar territorios controlados
                    int territoriosControlados = jugador.TerritoriosControlados.Contar;

                    // Verificar si controla todos los territorios
                    if (territoriosControlados == MapaJuego.Territorios.Contar)
                    {
                        EstadoActual = EstadoJuego.Finalizado;
                        return true;
                    }
                }
            }

            return false;
        }

        private void ConquistarTerritorio(Territorios atacante, Territorios conquistado, int tropasMovidas)
        {
            EColorJugador colorAntiguo = conquistado.PropietarioColor;
            EColorJugador colorNuevo = atacante.PropietarioColor;

            // Cambiar propietario
            conquistado.PropietarioColor = colorNuevo;
            conquistado.CantidadTropas = tropasMovidas;
            atacante.CantidadTropas -= tropasMovidas;

            // Actualizar listas de territorios de jugadores
            Jugador[] todosJugadores = Jugadores.ObtenerTodos();

            foreach (var jugador in todosJugadores)
            {
                if (jugador.Color == colorAntiguo)
                {
                    jugador.TerritoriosControlados.Remover(conquistado);//El propietario original perdio su territorio
                }
                else if (jugador.Color == colorNuevo)
                {
                    jugador.TerritoriosControlados.Agregar(conquistado);//El nuevo propietario adquiere el territorio

                    // Dar tarjeta aleatoria
                    ETipoTarjeta tipoAleatorio = (ETipoTarjeta)random.Next(3);
                    jugador.AgregarTarjeta(new Tarjeta(tipoAleatorio, conquistado.Name));

                    //Verificar si necesario forzar intercambio tarjeta(tiene 6 en ese momento)
                    ForzarIntercambioTarjetas(jugador);
                }
            }
        }

        public int CalcularRefuerzosParaJugador(Jugador jugador)
        {
            return jugador.CalcularRefuerzos(MapaJuego.BonusContinentes, MapaJuego);
        }

        
        public void IniciarFaseRefuerzos()
        {
            Jugador jugadorActual = ObtenerJugadorActual();
            int refuerzos = CalcularRefuerzosParaJugador(jugadorActual);
            jugadorActual.TropasDisponibles += refuerzos;

            EstadoActual = EstadoJuego.Refuerzos;

            //  Neutral asignar tropas automaticamente y finaliza el turno
            if (jugadorActual.EsNeutral)
            {
                ColocarTropasNeutralAleatoriamente();
                TerminarFaseRefuerzos();
                TerminarFaseAtaque();
                TerminarTurno();
            }
        }

        public void SiguienteTurno()
        {
            Jugador[] todosJugadores = Jugadores.ObtenerTodos();
            TurnoActual = (TurnoActual + 1) % todosJugadores.Length;//A siguiente jugador
            // Cambiar fase si no esta fase de asignartropa inicial
            if (EstadoActual != EstadoJuego.DistribucionInicial)
            {
                EstadoActual = EstadoJuego.Refuerzos;
            }
        }

        // Rotacion en fase de asignar tropa inicial
        public void SiguienteTurnoDistribucion()
        {
            Jugador[] todosJugadores = Jugadores.ObtenerTodos();
            TurnoActual = (TurnoActual + 1) % todosJugadores.Length;
           
        }

        public Jugador ObtenerJugadorActual()
        {
            Jugador[] todosJugadores = Jugadores.ObtenerTodos();
            return todosJugadores[TurnoActual];
        }


        public bool PuedeMoverTropas(Territorios origen, Territorios destino, int cantidad)
        {
            return origen.CantidadTropas > cantidad &&    // Mantiene al meno 1 tropas despues de mover
                   EsRutaConectada(origen, destino) &&    // Vinculacion a traves del propio territorio
                   origen.PropietarioColor == destino.PropietarioColor; // Mismo Jugador
        }

        private bool EsRutaConectada(Territorios origen, Territorios destino)
        {
            if (origen == destino) return false;
            if (origen.PropietarioColor != destino.PropietarioColor) return false;

            // Usa BFS/DFS para encuentra
            Lista<Territorios> visitados = new Lista<Territorios>();
            Cola<Territorios> cola = new Cola<Territorios>();

            cola.Encolar(origen);
            visitados.Agregar(origen);

            while (!cola.EstaVacia)
            {
                Territorios actual = cola.Desencolar();

                foreach (var adyacente in actual.TerritoriosAdyacentes.ObtenerTodos())
                {
                    if (adyacente == destino &&
                        adyacente.PropietarioColor == origen.PropietarioColor)
                    {
                        return true;
                    }

                    if (!visitados.Contiene(adyacente) &&
                        adyacente.PropietarioColor == origen.PropietarioColor)
                    {
                        visitados.Agregar(adyacente);
                        cola.Encolar(adyacente);
                    }
                }
            }

            return false;
        }

    }
}

