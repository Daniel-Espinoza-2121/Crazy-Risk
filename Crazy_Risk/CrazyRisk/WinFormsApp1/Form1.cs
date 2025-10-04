using CrazyRisk.Core;
using System.Linq;
using CrazyRisk.Network;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
namespace WinFormsApp1
    
{
    public partial class Form1 : Form
    {
        private NetworkManager? networkManager;
        private bool esServidor;
        private string nombreJugadorLocal = string.Empty;
        private bool modoRed = false;
        private System.Threading.Timer? debounceTimer;
        private MensajeRed? ultimoMensajePendiente;

        private EColorJugador miColorEnRed;

        private Panel panelMapa = null!;
        private Panel panelControles = null!;
        private Button btnIniciarJuego = null!;
        private Button btnDistribuir = null!;
        private Button btnAsignarTropas = null!;
        private Button btnAtacar = null!;
        private Button btnMover = null!;
        private Button btnSiguienteTurno = null!;
        private Label lblInfo = null!;
        private Label lblTarjetasInfo = null!;
        private ListBox lstTarjetas = null!;
        private ComboBox cmbTipoTrio = null!;
        private NumericUpDown nudCantidadTropas = null!;
        private ListBox lstLog = null!;
        private ComboBox cmbModoJuego = null!;
        private Button btnIntercambiarTarjetas = null!;

        private ControladorJuego? controlador;
        private Image? imagenMapa;
        private Territorios? territorioHover;
        private Territorios? territorioSeleccionado;
        private Territorios? territorioDestino;
        private const int RADIO_TERRITORIO = 15;

        



        public Form1()
        {
            // Preguntar si quiere jugar en red o local
            DialogResult resultado = MessageBox.Show(
                "¿Deseas jugar en RED con otro jugador?\n\n" +
                "Sí = Modo Red (Servidor/Cliente)\n" +
                "No = Modo Local (Solo en esta computadora)",
                "Modo de Juego",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (resultado == DialogResult.Yes)
            {
                using (FormConexion formConexion = new FormConexion())
                {
                    // Guardar referencia al modo de juego
                    int modoJuegoRed = 0;

                    formConexion.OnConexionExitosa += (esServidorConexion) =>
                    {
                        if (esServidorConexion && controlador != null)
                        {
                            EnviarEstadoCompleto();
                        }
                    };

                    if (formConexion.ShowDialog() == DialogResult.OK)
                    {
                        networkManager = formConexion.NetworkManager;
                        esServidor = networkManager.EsServidor;
                        nombreJugadorLocal = formConexion.NombreJugador;
                        modoRed = true;
                        modoJuegoRed = formConexion.ModoJuego;

                        networkManager.MensajeRecibido += OnMensajeRecibido;

                        InicializarVentana();
                        InicializarComponentes();
                        InicializarEventos();


                        AgregarLog($"Modo RED activado - {(esServidor ? "SERVIDOR" : "CLIENTE")}");
                        AgregarLog($"Tu nombre: {nombreJugadorLocal}");

                        // Preseleccionar modo en servidor
                        if (esServidor)
                        {
                            

                            // Configurar después de que InicializarComponentes() haya corrido
                            this.Shown += (s, e) =>
                            {
                                if (cmbModoJuego != null)
                                {
                                    cmbModoJuego.SelectedIndex = modoJuegoRed;
                                    cmbModoJuego.Enabled = false;
                                    AgregarLog($"Modo preseleccionado: {cmbModoJuego.Text}");
                                }
                            };
                        }
                    }
                }
            }
            else
            {
                // Modo Local
                modoRed = false;

                InicializarVentana();
                InicializarComponentes();
                InicializarEventos();

                AgregarLog("Modo LOCAL activado");
            }

        }

        



        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            
            var controlador = new ControladorJuego();
            var mapa = controlador.MapaJuego;

            
            using (var pen = new Pen(Color.Gray, 1))
            {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;

                var territorios = mapa.Territorios.ObtenerTodos();
                foreach (var territorio in territorios)
                {
                    var adyacentes = territorio.TerritoriosAdyacentes.ObtenerTodos();
                    foreach (var adyacente in adyacentes)
                    {
                        e.Graphics.DrawLine(pen,
                            new Point(territorio.PosicionX, territorio.PosicionY),
                            new Point(adyacente.PosicionX, adyacente.PosicionY));
                    }
                }
            }
        }
    

        private void InicializarVentana()
        {
            this.Text = "Crazy Risk - Test de Mapa";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);
        }

        private void InicializarComponentes()
        {
            // Panel del mapa
            panelMapa = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(1000, 700),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            // Activar doble buffer
            typeof(Panel).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic,
                null, panelMapa, new object[] { true });

            // Cargar imagen del mapa
            try
            {
                string rutaImagen = Path.Combine(Application.StartupPath, "Resources", "mapa_risk.png");
                if (File.Exists(rutaImagen))
                {
                    imagenMapa = Image.FromFile(rutaImagen);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar imagen: {ex.Message}");
            }

            // Panel de controles
            panelControles = new Panel
            {
                Location = new Point(1020, 10),
                Size = new Size(360, 750),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                
            };

            // Selector de modo de juego
            Label lblModo = new Label
            {
                Text = "Modo de Juego:",
                Location = new Point(10, 10),
                Size = new Size(150, 20),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            cmbModoJuego = new ComboBox
            {
                Location = new Point(10, 35),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbModoJuego.Items.AddRange(new string[]
            {
                "2 Jugadores",
                "2 Jugadores + Neutral",
                "3 Jugadores"
            });
            cmbModoJuego.SelectedIndex = 0;

            // Botón iniciar juego
            btnIniciarJuego = new Button
            {
                Text = "Iniciar Juego",
                Location = new Point(10, 70),
                Size = new Size(150, 35),
                BackColor = Color.Green,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            // Botón distribuir territorios
            btnDistribuir = new Button
            {
                Text = "Distribuir Territorios",
                Location = new Point(10, 115),
                Size = new Size(150, 30),
                Enabled = false
            };

            // Botón asignar tropas
            btnAsignarTropas = new Button
            {
                Text = "Modo: Asignar",
                Location = new Point(10, 155),
                Size = new Size(150, 30),
                BackColor = Color.LightBlue,
                Enabled = false
            };

            // Botón atacar
            btnAtacar = new Button
            {
                Text = "Modo: Atacar",
                Location = new Point(10, 195),
                Size = new Size(150, 30),
                BackColor = Color.LightCoral,
                Enabled = false
            };

            // Botón mover
            btnMover = new Button
            {
                Text = "Modo: Mover",
                Location = new Point(10, 235),
                Size = new Size(150, 30),
                BackColor = Color.LightGreen,
                Enabled = false
            };

            // Botón siguiente turno
            btnSiguienteTurno = new Button
            {
                Text = "Siguiente Turno",
                Location = new Point(10, 275),
                Size = new Size(150, 30),
                BackColor = Color.Orange,
                Enabled = false
            };

            

            // Label de información
            lblInfo = new Label
            {
                Location = new Point(10, 315),
                Size = new Size(330, 90),
                BorderStyle = BorderStyle.FixedSingle,
                Text = "Presiona 'Iniciar Juego' para comenzar",
                Font = new Font("Arial", 9),
                TextAlign = ContentAlignment.TopLeft
            };

            // === SECCIÓN DE TARJETAS ===
            Label lblTarjetasTitulo = new Label
            {
                Text = "Tus Tarjetas:",
                Location = new Point(10, 415),
                Size = new Size(150, 20),
                Font = new Font("Arial", 9, FontStyle.Bold)
            };

            lstTarjetas = new ListBox
            {
                Location = new Point(10, 440),
                Size = new Size(330, 80),
                Font = new Font("Arial", 10)
            };

            lblTarjetasInfo = new Label
            {
                Location = new Point(10, 525),
                Size = new Size(160, 30),
                Text = "Tarjetas: 0/5",
                Font = new Font("Arial", 10),
                TextAlign = ContentAlignment.MiddleLeft
            };

            Label lblTipoTrio = new Label
            {
                Text = "Tipo de trio:",
                Location = new Point(10, 560),
                Size = new Size(100, 20),
                Font = new Font("Arial", 10)
            };

            cmbTipoTrio = new ComboBox
            {
                Location = new Point(10, 585),
                Size = new Size(160, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Arial", 10),
                Enabled = false
            };

            btnIntercambiarTarjetas = new Button
            {
                Text = "Intercambiar",
                Location = new Point(180, 560),
                Size = new Size(160, 50),
                BackColor = Color.FromArgb(255, 152, 0),
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold),
                Enabled = false
            };

            // Control de cantidad de tropas
            Label lblCantidad = new Label
            {
                Text = "Cantidad tropas:",
                Location = new Point(10, 620),
                Size = new Size(120, 20),
                Font = new Font("Arial", 10)
            };

            nudCantidadTropas = new NumericUpDown
            {
                Location = new Point(135, 618),
                Size = new Size(70, 25),
                Minimum = 1,
                Maximum = 50,
                Value = 1
            };

            

            // Agregar controles al panel
            panelControles.Controls.AddRange(new Control[]
            {
                lblModo, cmbModoJuego, btnIniciarJuego, btnDistribuir,
                btnAsignarTropas, btnAtacar, btnMover, btnSiguienteTurno,
                lblInfo, lblTarjetasTitulo, lstTarjetas, lblTarjetasInfo,
                lblTipoTrio, cmbTipoTrio, btnIntercambiarTarjetas,
                lblCantidad, nudCantidadTropas
            });

            
            // Agregar paneles al form
            this.Controls.Add(panelMapa);
            this.Controls.Add(panelControles);

            
            Label lblLogTitle = new Label
            {
                Text = "Registro de Acciones:",
                Location = new Point(10, 720),
                Size = new Size(200, 20),
                Font = new Font("Arial", 11, FontStyle.Bold),
                BackColor = Color.White
            };

            lstLog = new ListBox
            {
                Location = new Point(10, 745),
                Size = new Size(1370, 100),  
                Font = new Font("Consolas", 8),
                BackColor = Color.White
            };

            
            this.Controls.Add(lblLogTitle);
            this.Controls.Add(lstLog);
        }

        private void InicializarEventos()
        {
            panelMapa.Paint += PanelMapa_Paint;
            panelMapa.MouseMove += PanelMapa_MouseMove;
            panelMapa.MouseClick += PanelMapa_MouseClick;

            btnIniciarJuego.Click += BtnIniciarJuego_Click;
            btnDistribuir.Click += BtnDistribuir_Click;
            btnAsignarTropas.Click += BtnAsignarTropas_Click;
            btnAtacar.Click += BtnAtacar_Click;
            btnMover.Click += BtnMover_Click;
            btnSiguienteTurno.Click += BtnSiguienteTurno_Click;
            btnIntercambiarTarjetas.Click += BtnIntercambiarTarjetas_Click;
            cmbTipoTrio.SelectedIndexChanged += CmbTipoTrio_SelectedIndexChanged;


        }




        private void BtnIniciarJuego_Click(object? sender, EventArgs e)
        {
            try
            {
                // VALIDACIÓN: Solo servidor puede iniciar
                if (modoRed && !esServidor)
                {
                    MessageBox.Show("Solo el servidor puede iniciar el juego", "Información");
                    return;
                }

                // VALIDACIÓN: Verificar clientes conectados en modo red
                if (modoRed && esServidor)
                {
                    int modoSeleccionado = cmbModoJuego.SelectedIndex;
                    int clientesEsperados = (modoSeleccionado == 2) ? 2 : 1;
                    if (modoSeleccionado == 2)  // 3 jugadores reales
                        networkManager.ClientesEsperados = 2; // 2 clientes además del servidor
                    else
                        networkManager.ClientesEsperados = 1; // solo 1 cliente
                    int clientesConectados = networkManager.NumeroClientesConectados();

                    if (clientesConectados < clientesEsperados)
                    {
                        MessageBox.Show(
                            $"Esperando jugadores...\n\nConectados: {clientesConectados}/{clientesEsperados}\n\n" +
                            $"Por favor espera a que todos se conecten.",
                            "Esperando jugadores",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        return;
                    }

                    AgregarLog($"✓ Todos los jugadores conectados ({clientesConectados}/{clientesEsperados})");
                }

                controlador = new ControladorJuego();

                bool modoTresJugadores = cmbModoJuego.SelectedIndex == 2;

                switch (cmbModoJuego.SelectedIndex)
                {
                    case 0: // 2 jugadores
                        AgregarLog("Modo: 2 Jugadores");
                        break;
                    case 1: // 2 + neutral
                        controlador.HabilitarEjercitoNeutral();
                        AgregarLog("Modo: 2 Jugadores + Ejército Neutral");
                        break;
                    case 2: // 3 jugadores
                        controlador.HabilitarTresJugadores();
                        AgregarLog("Modo: 3 Jugadores");
                        break;
                }

                // Agregar jugadores
                if (modoRed)
                {
                    if (esServidor)
                    {
                        controlador.AgregarJugador(nombreJugadorLocal, EColorJugador.Rojo);
                        miColorEnRed = EColorJugador.Rojo;
                        controlador.AgregarJugador("Jugador 2", EColorJugador.Azul);

                        if (modoTresJugadores)
                        {
                            controlador.AgregarJugador("Jugador 3", EColorJugador.Verde);
                        }
                    }
                    else
                    {
                        controlador.AgregarJugador("Jugador 1", EColorJugador.Rojo);
                        controlador.AgregarJugador(nombreJugadorLocal, EColorJugador.Azul);
                        miColorEnRed = EColorJugador.Azul;

                        if (modoTresJugadores)
                        {
                            controlador.AgregarJugador("Jugador 3", EColorJugador.Verde);
                        }
                    }
                }
                else
                {
                    // MODO LOCAL
                    controlador.AgregarJugador("Jugador 1", EColorJugador.Rojo);
                    controlador.AgregarJugador("Jugador 2", EColorJugador.Azul);

                    if (modoTresJugadores)
                    {
                        controlador.AgregarJugador("Jugador 3", EColorJugador.Verde);
                    }
                }

                btnIniciarJuego.Enabled = false;
                btnDistribuir.Enabled = true;
                cmbModoJuego.Enabled = false;

                AgregarLog("Juego inicializado");
                AgregarLog($"Total de territorios: {controlador.MapaJuego.Territorios.Contar}");

                // ENVIAR MENSAJE A TODOS LOS CLIENTES
                if (modoRed && esServidor)
                {
                    EnviarMensajeRed(TipoMensaje.IniciarJuego, new { Modo = cmbModoJuego.SelectedIndex });
                    AgregarLog("→ Mensaje IniciarJuego enviado a todos los clientes");
                }

                panelMapa.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error");
            }
        }

        private void BtnDistribuir_Click(object? sender, EventArgs e)
        {
            try
            {
                // En modo red, solo el servidor distribuye
                if (modoRed && !esServidor)
                {
                    MessageBox.Show("Solo el servidor puede distribuir territorios", "Información");
                    return;
                }
                controlador.DistribuirTerritorios();

                var jugadores = controlador.Jugadores.ObtenerTodos();
                foreach (var jugador in jugadores)
                {
                    AgregarLog($"{jugador.Nombre}: {jugador.TerritoriosControlados.Contar} territorios");
                    AgregarLog($"  Tropas para colocar: {jugador.TropasDisponibles}");
                }
                

                btnDistribuir.Enabled = false;
                btnAsignarTropas.Enabled = true;
                btnAsignarTropas.BackColor = Color.Blue; 
                                                         
                controlador.TurnoActual = 0;
                ActualizarInfo();
                AgregarLog($"Fase de colocación inicial - Turno: {controlador.ObtenerJugadorActual().Nombre}");
                AgregarLog("Cada jugador debe colocar una tropa por turno");

                // Enviar distribución por red
                
                if (modoRed && esServidor)
                {
                    System.Threading.Thread.Sleep(200);
                    EnviarEstadoCompleto();
                    AgregarLog("✅ Estado validado enviado al cliente");
                }

                panelMapa.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error");
            }
        }

        private void ResetBotones()
        {
            btnAsignarTropas.BackColor = Color.LightBlue;
            btnAtacar.BackColor = Color.LightCoral;
            btnMover.BackColor = Color.LightGreen;
        }

        private void BtnAsignarTropas_Click(object? sender, EventArgs e)
        {
            if (controlador.EstadoActual != EstadoJuego.Refuerzos &&
        controlador.EstadoActual != EstadoJuego.DistribucionInicial)
            {
                MessageBox.Show("Solo puedes asignar tropas en fase de refuerzos");
                return;
            }
            ResetBotones();
            btnAsignarTropas.BackColor = Color.Blue; // modo activo
            territorioSeleccionado = default;
            territorioDestino = default;
            AgregarLog("Modo: Asignar Tropas - Selecciona un territorio propio");
            panelMapa.Invalidate();
        }

        private void BtnAtacar_Click(object? sender, EventArgs e)
        {
            if (controlador.EstadoActual != EstadoJuego.Ataque)
            {
                MessageBox.Show("No estás en la fase de ataque");
                return;
            }
            ResetBotones();
            btnAtacar.BackColor = Color.Blue;
            territorioSeleccionado = default;
            territorioDestino = default;
            AgregarLog("Modo: Atacar - Selecciona territorio origen y destino");
            panelMapa.Invalidate();
        }

        private void BtnMover_Click(object? sender, EventArgs e)
        {
            if (controlador.EstadoActual != EstadoJuego.Planeacion) 
            { MessageBox.Show("No estás en la fase de planeación"); return; }

            ResetBotones();
            btnMover.BackColor = Color.Blue;
            territorioSeleccionado = default;
            territorioDestino = default;
            AgregarLog("Modo: Mover - Selecciona territorio origen y destino");
            panelMapa.Invalidate();
        }

        private void BtnSiguienteTurno_Click(object? sender, EventArgs e)
        {
            var jugador = controlador.ObtenerJugadorActual();
            if (modoRed)
            {
                bool esMiTurno = (jugador.Color == miColorEnRed);
                if (!esMiTurno)
                {
                    MessageBox.Show("No es su turno", "pista");
                    return;
                }
            }
            try
            {
                if (modoRed && !esServidor)
                {
                    
                    var datos = new DatosTerminarFase
                    {
                        FaseActual = controlador.EstadoActual.ToString()
                    };
                    EnviarMensajeRed(TipoMensaje.TerminarFase, datos);
                    AgregarLog($"[Red] Enviar mensaje de terminar fase: {controlador.EstadoActual}");
                    return; 
                }

                
                EjecutarTerminarFase();

                
                if (modoRed && esServidor)
                {
                    System.Threading.Thread.Sleep(100);
                    EnviarEstadoCompleto(TipoMensaje.ActualizarEstado);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error");
            }
        }

        private void BtnIntercambiarTarjetas_Click(object? sender, EventArgs e)
        {
            try
            {
                var jugadorActual = controlador.ObtenerJugadorActual();

                if (controlador.EstadoActual != EstadoJuego.Refuerzos)
                {
                    MessageBox.Show("Solo puedes intercambiar tarjetas en la fase de refuerzos", "Error");
                    return;
                }

                if (cmbTipoTrio.SelectedItem == null)
                {
                    MessageBox.Show("Selecciona un tipo de trio para intercambiar", "Error");
                    return;
                }

                TipoTrioTarjetas tipoSeleccionado = (TipoTrioTarjetas)cmbTipoTrio.SelectedItem;
                int tropasObtenidas = controlador.IntercambiarTarjetasEspecificas(jugadorActual, tipoSeleccionado);

                jugadorActual.TropasDisponibles += tropasObtenidas;

                AgregarLog($"Tarjetas intercambiadas! +{tropasObtenidas} tropas");
                MessageBox.Show($"Has obtenido {tropasObtenidas} tropas adicionales!", "Intercambio Exitoso");
                
                // Enviar por red
                if (modoRed)
                {
                    var datosIntercambio = new
                    {
                        TipoTrio = tipoSeleccionado.ToString(),
                        TropasObtenidas = tropasObtenidas
                    };
                    EnviarMensajeRed(TipoMensaje.IntercambiarTarjetas, datosIntercambio);
                }

                ActualizarInfo();
                ActualizarTarjetas();
                panelMapa.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error");
            }
        }

        private void CmbTipoTrio_SelectedIndexChanged(object? sender, EventArgs e)
        {
            btnIntercambiarTarjetas.Enabled = cmbTipoTrio.SelectedItem != null;
        }

        

        private void PanelMapa_MouseMove(object? sender, MouseEventArgs e)
        {
            if (controlador?.MapaJuego == null) return;

            Territorios actual = EncontrarTerritorio(e.X, e.Y);
            if (actual != territorioHover)
            {
                territorioHover = actual;
                panelMapa.Cursor = territorioHover != null ? Cursors.Hand : Cursors.Default;
                panelMapa.Invalidate();
            }
        }

        private void PanelMapa_MouseClick(object? sender, MouseEventArgs e)
        {
            if (controlador == null) return;

            Territorios clickeado = EncontrarTerritorio(e.X, e.Y);
            if (clickeado == null) return;

            // Determinar acción según botón activo
            if (btnAsignarTropas.BackColor == Color.Blue) // Modo seleccionado
            {
                AsignarTropa(clickeado);
            }
            else if (btnAtacar.BackColor == Color.Blue)
            {
                SeleccionarParaAtaque(clickeado);
            }
            else if (btnMover.BackColor == Color.Blue)
            {
                SeleccionarParaMovimiento(clickeado);
            }
            else
            {
                // Primer click selecciona
                territorioSeleccionado = clickeado;
                AgregarLog($"Seleccionado: {clickeado.Name}");
            }

            panelMapa.Invalidate();
        }

        private void AsignarTropa(Territorios territorio)
        {
            try
            {
                if (controlador.EstadoActual == EstadoJuego.DistribucionInicial)
                {
                    // FASE DE DISTRIBUCIÓN INICIAL
                    var jugadorActual = controlador.ObtenerJugadorActual();

                    if (modoRed)
                    {
                        if (esServidor)
                        {
                            // SERVIDOR: Solo puede actuar en su propio turno (Rojo)
                            if (jugadorActual.Color == EColorJugador.Rojo)
                            {
                                // Es el turno del servidor
                                if (territorio == null || territorio.PropietarioColor != jugadorActual.Color)
                                {
                                    AgregarLog("Selecciona un territorio rojo");
                                    return;
                                }

                                bool exito = controlador.ColocarTropaInicial(territorio);

                                if (exito)
                                {
                                    AgregarLog($"Servidor colocó tropa en {territorio.Name}");
                                    AgregarLog($"Tropas restantes: {jugadorActual.TropasDisponibles}");

                                    // Verificar si todos terminaron
                                    if (controlador.DistribucionInicialCompleta())
                                    {
                                        AgregarLog("=== DISTRIBUCIÓN INICIAL COMPLETA ===");
                                        controlador.FinalizarDistribucionInicial();

                                        btnAtacar.Enabled = false;
                                        btnMover.Enabled = false;
                                        btnSiguienteTurno.Enabled = true;
                                        btnAsignarTropas.Enabled = true;
                                        btnAsignarTropas.BackColor = Color.Blue;

                                        AgregarLog($"Comienza el juego - Turno: {controlador.ObtenerJugadorActual().Nombre}");
                                        ActualizarTarjetas();

                                        System.Threading.Thread.Sleep(200);
                                        EnviarEstadoCompleto(TipoMensaje.ActualizarEstado);
                                        AgregarLog("[RED] Estado final de distribución enviado");
                                    }
                                    else
                                    {
                                        // Siguiente turno
                                        controlador.SiguienteTurnoDistribucion();
                                        var nuevoJugador = controlador.ObtenerJugadorActual();

                                        AgregarLog($"Siguiente turno: {nuevoJugador.Nombre} ({nuevoJugador.Color})");

                                        // Si es neutral, colocar automáticamente
                                        if (nuevoJugador.EsNeutral && nuevoJugador.TropasDisponibles > 0)
                                        {
                                            AgregarLog($"Turno automático: {nuevoJugador.Nombre}");
                                            System.Threading.Thread.Sleep(300);
                                            AsignarTropa(null);
                                            return;
                                        }

                                        // Enviar estado actualizado
                                        System.Threading.Thread.Sleep(100);
                                        EnviarEstadoCompleto(TipoMensaje.ActualizarEstado);
                                    }

                                    ActualizarInfo();
                                    panelMapa.Invalidate();
                                }
                            }
                            else if (jugadorActual.Color == EColorJugador.Azul || jugadorActual.Color == EColorJugador.Verde)
                            {
                                // Es el turno del cliente, esperar su acción
                                AgregarLog($"Esperando acción del cliente (Jugador 2)...");
                            }
                            else if (jugadorActual.EsNeutral)
                            {
                                // Turno del neutral - el servidor lo maneja automáticamente
                                bool exito = controlador.ColocarTropaInicial(null);
                                if (exito)
                                {
                                    AgregarLog($"{jugadorActual.Nombre} colocó tropa automáticamente");

                                    if (controlador.DistribucionInicialCompleta())
                                    {
                                        AgregarLog("=== DISTRIBUCIÓN INICIAL COMPLETA ===");
                                        controlador.FinalizarDistribucionInicial();
                                        btnAtacar.Enabled = false;
                                        btnMover.Enabled = false;
                                        btnSiguienteTurno.Enabled = true;
                                        btnAsignarTropas.Enabled = true;
                                        ActualizarTarjetas();
                                        System.Threading.Thread.Sleep(200);
                                        EnviarEstadoCompleto(TipoMensaje.ActualizarEstado);
                                    }
                                    else
                                    {
                                        controlador.SiguienteTurnoDistribucion();
                                        System.Threading.Thread.Sleep(100);
                                        EnviarEstadoCompleto(TipoMensaje.ActualizarEstado);

                                        // Si el siguiente también es neutral, continuar
                                        var siguiente = controlador.ObtenerJugadorActual();
                                        if (siguiente.EsNeutral && siguiente.TropasDisponibles > 0)
                                        {
                                            System.Threading.Thread.Sleep(300);
                                            AsignarTropa(null);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            //Solo puede actuar en su turno 
                            if (jugadorActual.Color == EColorJugador.Azul || jugadorActual.Color == EColorJugador.Verde)
                            {
                                if (territorio == null || territorio.PropietarioColor != jugadorActual.Color)
                                {
                                    AgregarLog("Selecciona un territorio azul");
                                    return;
                                }

                                // Enviar al servidor
                                var datos = new DatosAccionTerritorio
                                {
                                    NombreTerritorio = territorio.Name,
                                    Cantidad = 1
                                };
                                EnviarMensajeRed(TipoMensaje.ColocarTropaInicial, datos);
                                AgregarLog($"Enviando: colocar tropa en {territorio.Name}");
                            }
                            else
                            {
                                AgregarLog($"No es tu turno. Turno actual: {jugadorActual.Nombre}");
                            }
                        }
                    }
                    else
                    {
                        // MODO LOCAL
                        bool exito = false;

                        if (jugadorActual.EsNeutral)
                        {
                            exito = controlador.ColocarTropaInicial(null);
                            if (exito)
                            {
                                AgregarLog($"{jugadorActual.Nombre} colocó tropa automáticamente");
                            }
                        }
                        else
                        {
                            if (territorio == null)
                            {
                                AgregarLog("Selecciona un territorio propio");
                                return;
                            }

                            if (territorio.PropietarioColor != jugadorActual.Color)
                            {
                                AgregarLog("Ese territorio no es tuyo");
                                return;
                            }

                            exito = controlador.ColocarTropaInicial(territorio);
                            if (exito)
                            {
                                AgregarLog($"{jugadorActual.Nombre} colocó tropa en {territorio.Name}");
                            }
                        }

                        if (exito)
                        {
                            if (controlador.DistribucionInicialCompleta())
                            {
                                AgregarLog("Distribución inicial completa!");
                                controlador.FinalizarDistribucionInicial();

                                btnAtacar.Enabled = true;
                                btnMover.Enabled = true;
                                btnSiguienteTurno.Enabled = true;
                                btnAsignarTropas.BackColor = Color.LightBlue;

                                AgregarLog($"Comienza el juego - Turno: {controlador.ObtenerJugadorActual().Nombre}");
                                ActualizarTarjetas();
                            }
                            else
                            {
                                controlador.SiguienteTurnoDistribucion();
                                var nuevoJugador = controlador.ObtenerJugadorActual();

                                if (nuevoJugador.EsNeutral && nuevoJugador.TropasDisponibles > 0)
                                {
                                    AgregarLog($"Turno automático: {nuevoJugador.Nombre}");
                                    System.Threading.Thread.Sleep(300);
                                    AsignarTropa(null);
                                    return;
                                }
                                else
                                {
                                    AgregarLog($"Turno para colocar: {nuevoJugador.Nombre}");
                                }
                            }

                            ActualizarInfo();
                            panelMapa.Invalidate();
                        }
                    }
                }
                else if (controlador.EstadoActual == EstadoJuego.Refuerzos)
                {
                    // FASE DE REFUERZOS (sin cambios)
                    var jugadorActual = controlador.ObtenerJugadorActual();

                    if (territorio == null || territorio.PropietarioColor != jugadorActual.Color)
                    {
                        AgregarLog("Selecciona un territorio propio");
                        return;
                    }

                    int cantidad = (int)nudCantidadTropas.Value;

                    if (modoRed)
                    {
                        if (esServidor)
                        {
                            if (controlador.AsignarTropas(territorio, cantidad))
                            {
                                AgregarLog($"Asignadas {cantidad} tropas a {territorio.Name}");

                                var datos = new DatosAccionTerritorio
                                {
                                    NombreTerritorio = territorio.Name,
                                    Cantidad = cantidad
                                };
                                EnviarMensajeRed(TipoMensaje.AsignarTropas, datos);
                                EnviarEstadoCompleto(TipoMensaje.ActualizarEstado);

                                ActualizarInfo();
                                panelMapa.Invalidate();
                            }
                        }
                        else
                        {
                            // Cliente envía al servidor
                            var datos = new DatosAccionTerritorio
                            {
                                NombreTerritorio = territorio.Name,
                                Cantidad = cantidad
                            };
                            EnviarMensajeRed(TipoMensaje.AsignarTropas, datos);
                            AgregarLog($"Enviando: Asignar {cantidad} tropas a {territorio.Name}");
                        }
                    }
                    else
                    {
                        // Modo local
                        if (controlador.AsignarTropas(territorio, cantidad))
                        {
                            AgregarLog($"Asignadas {cantidad} tropas a {territorio.Name}");
                            ActualizarInfo();
                            panelMapa.Invalidate();
                        }
                        else
                        {
                            AgregarLog("No puedes asignar esa cantidad de tropas");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AgregarLog($"Error: {ex.Message}");
            }
        }

        private void SeleccionarParaAtaque(Territorios territorio)
        {
            if (territorioSeleccionado == null)
            {
                var jugadorActual = controlador.ObtenerJugadorActual();
                if (territorio.PropietarioColor == jugadorActual.Color && territorio.CantidadTropas > 1)
                {
                    territorioSeleccionado = territorio;
                    AgregarLog($"Origen de ataque: {territorio.Name}");
                    // Ajustar límite según tropas disponibles en el territorio
                    int maxAtaque = Math.Min(3, territorio.CantidadTropas - 1);
                    nudCantidadTropas.Maximum = maxAtaque;
                    nudCantidadTropas.Value = Math.Min(nudCantidadTropas.Value, maxAtaque);
                    AgregarLog($"Puedes atacar con 1-{maxAtaque} tropas");
                }
                else
                {
                    AgregarLog("Selecciona un territorio propio con al menos 2 tropas");
                }
                
            }
            else
            {
                territorioDestino = territorio;
                EjecutarAtaque();
                territorioSeleccionado = null;
                territorioDestino = null;
                // Restaurar límite predeterminado
                nudCantidadTropas.Maximum = 3;
            }
        }

        private void SeleccionarParaMovimiento(Territorios territorio)
        {
            if (territorioSeleccionado == null)
            {
                var jugadorActual = controlador.ObtenerJugadorActual();
                if (territorio.PropietarioColor == jugadorActual.Color && territorio.CantidadTropas > 1)
                {
                    territorioSeleccionado = territorio;
                    
                    AgregarLog($"Origen de movimiento: {territorio.Name}");
                    // Ajustar límite según tropas disponibles (debe dejar al menos 1)
                    int maxMovimiento = territorio.CantidadTropas - 1;
                    nudCantidadTropas.Maximum = maxMovimiento;
                    nudCantidadTropas.Value = Math.Min(nudCantidadTropas.Value, maxMovimiento);
                    AgregarLog($"Puedes mover 1-{maxMovimiento} tropas");
                }
                else
                {
                    AgregarLog("Selecciona un territorio propio con al menos 2 tropas");
                }
                
            }
            else
            {
                territorioDestino = territorio;
                EjecutarMovimiento();
                territorioSeleccionado = null;
                territorioDestino = null;

                // Restaurar límite predeterminado
                nudCantidadTropas.Maximum = 50;
            }
        }

        private void EjecutarAtaque()
        {
            try
            {
                if (!controlador.PuedeAtacar(territorioSeleccionado, territorioDestino))
                {
                    AgregarLog("No se puede atacar ese territorio");
                    return;
                }

                int tropasAtaque = Math.Min((int)nudCantidadTropas.Value, Math.Min(3, territorioSeleccionado.CantidadTropas - 1));

                if (modoRed)
                {
                    if (esServidor)
                    {
                        // SERVIDOR: Ejecutar ataque
                        var resultado = controlador.EjecutarAtaque(
                            territorioSeleccionado,
                            territorioDestino,
                            tropasAtaque,
                            territorioDestino.CantidadTropas
                        );

                        MostrarAnimacionDados(resultado);

                        AgregarLog($"Ataque: {territorioSeleccionado.Name} → {territorioDestino.Name}");
                        AgregarLog($"Resultado: Atacante -{resultado.TropasAtacantesPerdidas}, Defensor -{resultado.TropasDefensorPerdidas}");

                        if (resultado.TerritorioConquistado)
                        {
                            AgregarLog($"¡{territorioDestino.Name} conquistado!");
                            ActualizarTarjetas();
                        }

                        // Enviar al cliente
                        var datosAtaque = new DatosAtaque
                        {
                            TerritorioOrigen = territorioSeleccionado.Name,
                            TerritorioDestino = territorioDestino.Name,
                            TropasAtacante = tropasAtaque,
                            TropasDefensor = territorioDestino.CantidadTropas,
                            DadosAtacante = resultado.DadosAtacante,
                            DadosDefensor = resultado.DadosDefensor
                        };
                        EnviarMensajeRed(TipoMensaje.Ataque, datosAtaque);

                        System.Threading.Thread.Sleep(200);
                        EnviarEstadoCompleto(TipoMensaje.ActualizarEstado);

                        ActualizarInfo();
                        panelMapa.Invalidate();

                        if (controlador.VerificarVictoria())
                        {
                            MessageBox.Show($"{controlador.ObtenerJugadorActual().Nombre} ha ganado el juego!",
                                "Victoria", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                    }
                    else
                    {
                        // CLIENTE: Enviar solicitud
                        var datosAtaque = new DatosAtaque
                        {
                            TerritorioOrigen = territorioSeleccionado.Name,
                            TerritorioDestino = territorioDestino.Name,
                            TropasAtacante = tropasAtaque,
                            TropasDefensor = territorioDestino.CantidadTropas
                        };
                        EnviarMensajeRed(TipoMensaje.Ataque, datosAtaque);
                        AgregarLog($"Enviando: Ataque con {tropasAtaque} tropas");
                    }
                }
                else
                {
                    var resultado = controlador.EjecutarAtaque(
                            territorioSeleccionado, territorioDestino,
                            tropasAtaque, territorioDestino.CantidadTropas);

                    MostrarAnimacionDados(resultado);

                    AgregarLog($"Ataque: {territorioSeleccionado.Name} → {territorioDestino.Name}");
                    AgregarLog($"Resultado: Atacante -{resultado.TropasAtacantesPerdidas}, Defensor -{resultado.TropasDefensorPerdidas}");

                    if (resultado.TerritorioConquistado)
                    {
                        AgregarLog($"¡{territorioDestino.Name} conquistado!");
                        ActualizarTarjetas(); // Actualizar porque se ganó una tarjeta
                    }
                    ActualizarInfo();
                    panelMapa.Invalidate();
                    // Verificar victoria
                    if (controlador.VerificarVictoria())
                    {
                        MessageBox.Show($"{controlador.ObtenerJugadorActual().Nombre} ha ganado el juego!",
                            "Victoria", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
            }





            catch (Exception ex)
            {
                AgregarLog($"Error: {ex.Message}");
            }
            
        }

        private void MostrarAnimacionDados(ResultadoCombate resultado)
        {
            if (resultado.DadosAtacante != null && resultado.DadosDefensor != null)
            {
                using (var formDados = new FormDadosAnimacion(
                    resultado.DadosAtacante,
                    resultado.DadosDefensor,
                    resultado.TropasAtacantesPerdidas,
                    resultado.TropasDefensorPerdidas))
                {
                    formDados.ShowDialog();
                }
            }
        }

        private void EjecutarMovimiento()
        {
            try
            {
                int cantidad = (int)nudCantidadTropas.Value;

                if (modoRed)
                {
                    if (esServidor)
                    {
                        // SERVIDOR: Ejecutar movimiento
                        if (controlador.EjecutarMovimientoPlaneacion(territorioSeleccionado, territorioDestino, cantidad))
                        {
                            AgregarLog($"Movidas {cantidad} tropas: {territorioSeleccionado.Name} -> {territorioDestino.Name}");

                            var datosMovimiento = new DatosMovimiento
                            {
                                TerritorioOrigen = territorioSeleccionado.Name,
                                TerritorioDestino = territorioDestino.Name,
                                Cantidad = cantidad
                            };
                            EnviarMensajeRed(TipoMensaje.Movimiento, datosMovimiento);

                            System.Threading.Thread.Sleep(100);
                            EnviarEstadoCompleto(TipoMensaje.ActualizarEstado);

                            ActualizarInfo();
                            panelMapa.Invalidate();
                        }
                        else
                        {
                            AgregarLog("No puedes mover más de una vez por turno");
                        }
                    }
                    else
                    {
                        // CLIENTE: Enviar solicitud
                        var datosMovimiento = new DatosMovimiento
                        {
                            TerritorioOrigen = territorioSeleccionado.Name,
                            TerritorioDestino = territorioDestino.Name,
                            Cantidad = cantidad
                        };
                        EnviarMensajeRed(TipoMensaje.Movimiento, datosMovimiento);
                        AgregarLog($"Enviando: Mover {cantidad} tropas");
                    }
                }


                else
                {
                    if (controlador.EjecutarMovimientoPlaneacion(territorioSeleccionado, territorioDestino, cantidad))
                    {
                        AgregarLog($"Movidas {cantidad} tropas: {territorioSeleccionado.Name} -> {territorioDestino.Name}");

                        // Enviar por red
                        if (modoRed)
                        {
                            var datosMovimiento = new DatosMovimiento
                            {
                                TerritorioOrigen = territorioSeleccionado.Name,
                                TerritorioDestino = territorioDestino.Name,
                                Cantidad = cantidad
                            };
                            EnviarMensajeRed(TipoMensaje.Movimiento, datosMovimiento);
                        }

                        ActualizarInfo();
                        panelMapa.Invalidate();
                    }
                    else
                    {
                        AgregarLog("No puedes mover más de una vez por turno");
                    }
                }
                
            }
            catch (Exception ex)
            {
                AgregarLog($"Error: {ex.Message}");
            }
            
        }
        private void EjecutarTerminarFase()
        {
            var jugador = controlador.ObtenerJugadorActual();

            switch (controlador.EstadoActual)
            {
                case EstadoJuego.Refuerzos:
                    if (jugador.TropasDisponibles > 0)
                    {
                        MessageBox.Show("Debe asignar todos tropas disponible");
                        return;
                    }
                    controlador.TerminarFaseRefuerzos();
                    AgregarLog("Finalizar fase refuerzos，entra fase ataque");
                    ResetBotones();
                    btnAtacar.Enabled = true;
                    btnAsignarTropas.Enabled = false;
                    btnAtacar.BackColor = Color.Blue; 
                    btnMover.Enabled = false;
                    break;

                case EstadoJuego.Ataque:
                    controlador.TerminarFaseAtaque();
                    AgregarLog("Finalizar fase ataque，entra fase de planeacion");
                    ResetBotones();
                    btnMover.Enabled = true;
                    btnAtacar.Enabled = false;
                    btnMover.BackColor = Color.Blue;
                    break;

                case EstadoJuego.Planeacion:
                    controlador.TerminarTurno();

                    
                    var nuevoJugador = controlador.ObtenerJugadorActual();
                    int intentos = 0;
                    while (nuevoJugador.TerritoriosControlados.Contar == 0 && intentos < 10)
                    {
                        AgregarLog($"{nuevoJugador.Nombre} ya se perdido，salta...");
                        controlador.SiguienteTurno();
                        nuevoJugador = controlador.ObtenerJugadorActual();
                        intentos++;
                    }

                    // Turno de neutra
                    if (nuevoJugador.EsNeutral && nuevoJugador.TerritoriosControlados.Contar > 0)
                    {
                        AgregarLog($"Turno de {nuevoJugador.Nombre} （automaticamente）");
                        System.Threading.Thread.Sleep(500);

                        // Neutra termine su turno en forma automaticamente
                        if (controlador.EstadoActual == EstadoJuego.Refuerzos)
                        {
                            controlador.ColocarTropasNeutralAleatoriamente();
                            controlador.TerminarFaseRefuerzos();
                            controlador.TerminarFaseAtaque();
                            controlador.TerminarTurno();
                            nuevoJugador = controlador.ObtenerJugadorActual();
                        }

                        AgregarLog($"{nuevoJugador.Nombre} terminal turno");
                    }

                    AgregarLog($"Finalizar turno。ahora es turno de {nuevoJugador.Nombre}");
                    
                    ResetBotones();
                    btnAsignarTropas.Enabled = true;
                    btnAsignarTropas.BackColor = Color.Blue;
                    btnAtacar.Enabled = false;
                    btnMover.Enabled = false;
                    break;
            }

            ActualizarInfo();
            ActualizarTarjetas();
            ActualizarControlesPorFase();
            panelMapa.Invalidate();
        }

        private void PanelMapa_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Dibujar imagen de fondo
            if (imagenMapa != null)
            {
                g.DrawImage(imagenMapa, 0, 0, panelMapa.Width, panelMapa.Height);
            }

            // Dibujar conexiones entre territorios
            if (controlador?.MapaJuego != null)
            {
                var territorios = controlador.MapaJuego.Territorios.ObtenerTodos();

                foreach (var territorio in territorios)
                {
                    foreach (var adyacente in territorio.TerritoriosAdyacentes.ObtenerTodos())
                    {
                        // Evitar dibujar dos veces la misma línea
                        if (territorio.Name.CompareTo(adyacente.Name) < 0)
                        {
                            Point p1 = new Point(territorio.PosicionX, territorio.PosicionY);
                            Point p2 = new Point(adyacente.PosicionX, adyacente.PosicionY);

                            if (territorio.PropietarioColor == adyacente.PropietarioColor)
                            {
                                // Misma propiedad → gris
                                using (Pen pen = new Pen(Color.Gray, 2))
                                {
                                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                                    g.DrawLine(pen, p1, p2);
                                }
                                }
                            else
                            {
                                // Diferentes jugadores → mitad y mitad
                                Point medio = new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);

                                using (Pen pen1 = new Pen(ObtenerColor(territorio.PropietarioColor), 2))
                                using (Pen pen2 = new Pen(ObtenerColor(adyacente.PropietarioColor), 2))
                                {
                                    pen1.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                                    pen2.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

                                    g.DrawLine(pen1, p1, medio);
                                    g.DrawLine(pen2, medio, p2);
                                }
                            }
                        }
                    }
                }

                // Dibujar territorios encima de las líneas
                foreach (var territorio in territorios)
                {
                    DibujarTerritorio(g, territorio);
                }
            }

            
        }

        private void DibujarTerritorio(Graphics g, Territorios territorio)
        {
            bool hover = territorio == territorioHover;
            bool seleccionado = territorio == territorioSeleccionado;
            bool destino = territorio == territorioDestino;

            int x = territorio.PosicionX;
            int y = territorio.PosicionY;
            int radio = 15;

            // Efecto hover
            if (hover)
            {
                using (Pen pen = new Pen(Color.FromArgb(200, Color.Yellow), 6))
                {
                    g.DrawEllipse(pen, x - radio - 3, y - radio - 3, (radio + 3) * 2, (radio + 3) * 2);
                }
            }

            // Círculo del territorio
            Color color = ObtenerColor(territorio.PropietarioColor);
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(180, color)))
            {
                g.FillEllipse(brush, x - radio, y - radio, radio * 2, radio * 2);
            }

            // Borde
            Color colorBorde = seleccionado ? Color.Yellow : destino ? Color.Orange : Color.Black;
            int grosor = seleccionado || destino ? 4 : 2;
            using (Pen pen = new Pen(colorBorde, grosor))
            {
                g.DrawEllipse(pen, x - radio, y - radio, radio * 2, radio * 2);
            }

            // Número de tropas
            using (Font font = new Font("Arial", 11, FontStyle.Bold))
            {
                string texto = territorio.CantidadTropas.ToString();
                SizeF tamaño = g.MeasureString(texto, font);
                g.DrawString(texto, font, Brushes.White,
                    x - tamaño.Width / 2, y - tamaño.Height / 2);
            }

            // Nombre (solo en hover)
            if (hover || seleccionado)
            {
                using (Font font = new Font("Arial", 9, FontStyle.Bold))
                {
                    SizeF tamaño = g.MeasureString(territorio.Name, font);
                    using (SolidBrush brush = new SolidBrush(Color.FromArgb(220, Color.Black)))
                    {
                        g.FillRectangle(brush, x - tamaño.Width / 2 - 4,
                            y - radio - tamaño.Height - 8,
                            tamaño.Width + 8, tamaño.Height + 4);
                    }
                    g.DrawString(territorio.Name, font, Brushes.White,
                        x - tamaño.Width / 2, y - radio - tamaño.Height - 6);
                }
            }
        }

        private Color ObtenerColor(EColorJugador color)
        {
            switch (color)
            {
                case EColorJugador.Rojo: return Color.Red;
                case EColorJugador.Azul: return Color.Blue;
                case EColorJugador.Verde: return Color.Green;
                case EColorJugador.Amarillo: return Color.Yellow;
                case EColorJugador.Morado: return Color.Purple;
                case EColorJugador.Neutral: return Color.Gray;
                default: return Color.LightGray;
            }
        }

        private Territorios EncontrarTerritorio(int x, int y)
        {
            if (controlador?.MapaJuego == null) return null;

            foreach (var territorio in controlador.MapaJuego.Territorios.ObtenerTodos())
            {
                double distancia = Math.Sqrt(
                    Math.Pow(x - territorio.PosicionX, 2) +
                    Math.Pow(y - territorio.PosicionY, 2));

                if (distancia <= RADIO_TERRITORIO)
                    return territorio;
            }
            return null;
        }

        private void ActualizarInfo()
        {
            if (controlador == null) return;

            var jugadorActual = controlador.ObtenerJugadorActual();
            lblInfo.Text = $"Turno: {jugadorActual.Nombre}\n" +
                          $"Color: {jugadorActual.Color}\n" +
                          $"Estado: {controlador.EstadoActual}\n" +
                          $"Tropas disponibles: {jugadorActual.TropasDisponibles}\n" +
                          $"Territorios: {jugadorActual.TerritoriosControlados.Contar}";
            ActualizarTarjetas();
            ActualizarLimiteCantidadTropas();

            // Verificar control de turnos en modo red
            if (modoRed)
            {
                ActualizarControlesPorTurno();
            }
        }

        // Actualizar límites del selector de cantidad según fase del juego
        private void ActualizarLimiteCantidadTropas()
        {
            if (controlador == null || nudCantidadTropas == null) return;

            switch (controlador.EstadoActual)
            {
                case EstadoJuego.DistribucionInicial:
                    // Solo puede colocar 1 tropa a la vez
                    nudCantidadTropas.Minimum = 1;
                    nudCantidadTropas.Maximum = 1;
                    nudCantidadTropas.Value = 1;
                    nudCantidadTropas.Enabled = false;
                    break;

                case EstadoJuego.Refuerzos:
                    // Puede asignar hasta las tropas disponibles
                    var jugadorActual = controlador.ObtenerJugadorActual();
                    nudCantidadTropas.Minimum = 1;
                    nudCantidadTropas.Maximum = Math.Max(1, jugadorActual.TropasDisponibles);
                    nudCantidadTropas.Value = 1;
                    nudCantidadTropas.Enabled = jugadorActual.TropasDisponibles > 0;
                    break;

                case EstadoJuego.Ataque:
                    // Máximo 3 dados para atacar
                    nudCantidadTropas.Minimum = 1;
                    nudCantidadTropas.Maximum = 3;
                    nudCantidadTropas.Value = Math.Min(3, nudCantidadTropas.Value);
                    nudCantidadTropas.Enabled = true;
                    break;

                case EstadoJuego.Planeacion:
                    // Dependerá del territorio seleccionado (actualizamos al seleccionar)
                    nudCantidadTropas.Minimum = 1;
                    nudCantidadTropas.Maximum = 50;
                    nudCantidadTropas.Value = 1;
                    nudCantidadTropas.Enabled = true;
                    break;

                default:
                    nudCantidadTropas.Enabled = false;
                    break;
            }
        }

        private void ActualizarTarjetas()
        {
            if (controlador == null) return;

            var jugadorActual = controlador.ObtenerJugadorActual();

            // Actualizar lista de tarjetas
            lstTarjetas.Items.Clear();
            var tarjetas = jugadorActual.Tarjetas.ObtenerTodos();
            foreach (var tarjeta in tarjetas)
            {
                string icono = tarjeta.Tipo switch
                {
                    ETipoTarjeta.Infanteria => "[I]",
                    ETipoTarjeta.Caballeria => "[C]",
                    ETipoTarjeta.Artilleria => "[A]",
                    _ => "[?]"
                };
                lstTarjetas.Items.Add($"{icono} {tarjeta.Tipo} - {tarjeta.NombreTerritorio}");
            }

            // Actualizar contador
            lblTarjetasInfo.Text = $"Tarjetas: {tarjetas.Length}/5";
            if (tarjetas.Length >= 6)
            {
                lblTarjetasInfo.ForeColor = Color.Red;
                lblTarjetasInfo.Text = $"Tarjetas: {tarjetas.Length}/5\nDEBES INTERCAMBIAR!";
            }
            else
            {
                lblTarjetasInfo.ForeColor = Color.Black;
            }

            // Actualizar combo de tríos disponibles
            cmbTipoTrio.Items.Clear();
            var triosDisponibles = jugadorActual.ObtenerTriosDisponibles();

            foreach (var trio in triosDisponibles)
            {
                cmbTipoTrio.Items.Add(trio);
            }

            // Habilitar/deshabilitar controles de intercambio
            bool puedeIntercambiar = triosDisponibles.Count > 0 &&
                                     controlador.EstadoActual == EstadoJuego.Refuerzos;

            cmbTipoTrio.Enabled = puedeIntercambiar;
            btnIntercambiarTarjetas.Enabled = puedeIntercambiar && cmbTipoTrio.SelectedItem != null;

            if (puedeIntercambiar && cmbTipoTrio.Items.Count > 0)
            {
                cmbTipoTrio.SelectedIndex = 0;
            }

            // Forzar intercambio si tiene 6 tarjetas
            if (tarjetas.Length >= 6 && controlador.EstadoActual == EstadoJuego.Refuerzos)
            {
                if (triosDisponibles.Count > 0)
                {
                    MessageBox.Show("Tienes 6 tarjetas! Debes intercambiar un trio antes de continuar.",
                        "Intercambio Obligatorio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
        private void AgregarLog(string mensaje)
        {
            if (lstLog != null && lstLog.IsHandleCreated)
            {
                lstLog.Items.Add($"[{DateTime.Now:HH:mm:ss}] {mensaje}");
                lstLog.TopIndex = lstLog.Items.Count - 1;
            }
            else
            {
                // Si el log aún no está inicializado, guardar en consola
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {mensaje}");
            }
        }

        // MÉTODOS DE RED

        private void OnMensajeRecibido(MensajeRed mensaje)
        {
            // Asegurar que estamos en el hilo de UI
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => OnMensajeRecibido(mensaje)));
                return;
            }
            ultimoMensajePendiente = mensaje;
            if (mensaje.Tipo == TipoMensaje.ActualizarEstado && !esServidor)
            {
                debounceTimer?.Dispose();
                debounceTimer = new System.Threading.Timer(
                    _ => {
                        if (this.InvokeRequired)
                        {
                            this.Invoke(new Action(() => ProcesarMensajeRed(ultimoMensajePendiente)));
                        }
                        else
                        {
                            ProcesarMensajeRed(ultimoMensajePendiente);
                        }
                    },
                    null,
                    200, 
                    System.Threading.Timeout.Infinite
                );
            }
            else
            {
                ProcesarMensajeRed(mensaje);
            }
        }

        private void ProcesarMensajeRed(MensajeRed mensaje)
        {
            try
            {
                AgregarLog($"[RED] Recibido: {mensaje.Tipo}");

                switch (mensaje.Tipo)
                {
                    case TipoMensaje.ConexionJugador:
                        var datosConexion = JsonConvert.DeserializeObject<DatosConexion>(mensaje.Datos);
                        AgregarLog($"Jugador conectado: {datosConexion.NombreJugador}");
                        break;

                    case TipoMensaje.IniciarJuego:
                        if (!esServidor && controlador == null)
                        {
                            var datos = JsonConvert.DeserializeObject<dynamic>(mensaje.Datos);
                            int modo = datos.Modo;

                            AgregarLog($"← Recibido IniciarJuego con modo: {modo}");

                            controlador = new ControladorJuego();

                            // Configurar según modo
                            bool modoTresJugadores = (modo == 2);

                            if (modoTresJugadores)
                            {
                                controlador.HabilitarTresJugadores();
                            }
                            else if (modo == 1)
                            {
                                controlador.HabilitarEjercitoNeutral();
                            }

                            
                            controlador.AgregarJugador("Jugador 1", EColorJugador.Rojo);
                            controlador.AgregarJugador(nombreJugadorLocal, EColorJugador.Azul);
                            

                            if (modoTresJugadores)
                            {
                                controlador.AgregarJugador("Jugador 3", EColorJugador.Verde);
                                if (nombreJugadorLocal == "Jugador 2")
                                {
                                    miColorEnRed = EColorJugador.Azul;
                                }
                                else
                                {
                                    miColorEnRed = EColorJugador.Verde; // Segundo cliente
                                }
                            }
                            else { miColorEnRed = EColorJugador.Azul; }

                            AgregarLog($"Juego inicializado - Mi color: {miColorEnRed}");

                            btnIniciarJuego.Enabled = false;
                            cmbModoJuego.Enabled = false;
                            cmbModoJuego.SelectedIndex = modo;
                            btnDistribuir.Enabled = false; // Cliente no distribuye

                            ActualizarInfo();
                            panelMapa.Invalidate();
                        }
                        break;

                    case TipoMensaje.DistribuirTerritorios:
                        
                        if (controlador == null)
                        {
                            AgregarLog("Error: Controlador no inicializado al recibir distribución");
                            return;
                        }

                        var estadoDistribucion = JsonConvert.DeserializeObject<DatosEstadoJuego>(mensaje.Datos);
                        SincronizarEstadoJuego(estadoDistribucion);

                        btnDistribuir.Enabled = false;
                        btnAsignarTropas.Enabled = true;
                        btnAsignarTropas.BackColor = Color.Blue;
                        break;

                    case TipoMensaje.ColocarTropaInicial:
                        if (controlador?.MapaJuego == null)
                        {
                            AgregarLog("Error: MapaJuego no disponible");
                            return;
                        }
                        var datosColocar = JsonConvert.DeserializeObject<DatosAccionTerritorio>(mensaje.Datos);
                        RecibirColocarTropa(datosColocar);
                        break;

                    case TipoMensaje.AsignarTropas:
                        if (controlador?.MapaJuego == null) return;
                        var datosAsignar = JsonConvert.DeserializeObject<DatosAccionTerritorio>(mensaje.Datos);
                        RecibirAsignarTropas(datosAsignar);
                        break;

                    case TipoMensaje.Ataque:
                        if (controlador?.MapaJuego == null) return;
                        var datosAtaque = JsonConvert.DeserializeObject<DatosAtaque>(mensaje.Datos);
                        RecibirAtaque(datosAtaque);
                        break;

                    case TipoMensaje.Movimiento:
                        if (controlador?.MapaJuego == null) return;
                        var datosMovimiento = JsonConvert.DeserializeObject<DatosMovimiento>(mensaje.Datos);
                        RecibirMovimiento(datosMovimiento);
                        break;

                    case TipoMensaje.TerminarFase:
                        if (controlador == null) return;
                        if (esServidor)
                        {
                            
                            RecibirTerminarFase(mensaje.Datos);
                        }
                        else
                        {
                            
                            AgregarLog("[Red] Esperando estado sincronica...");
                        }
                        break;

                    case TipoMensaje.IntercambiarTarjetas:
                        AgregarLog("El otro jugador intercambió tarjetas");
                        if (controlador != null) ActualizarInfo();
                        break;

                    case TipoMensaje.ActualizarEstado:
                        if (controlador == null) return;
                        AgregarLog($"[RED] Recibiendo ActualizarEstado");
                        var estadoJuego = JsonConvert.DeserializeObject<DatosEstadoJuego>(mensaje.Datos);

                        AgregarLog($"[RED] Estado en mensaje: {estadoJuego.EstadoActual}");
                        AgregarLog($"[RED] Turno en mensaje: {estadoJuego.TurnoActual}");
                        SincronizarEstadoJuego(estadoJuego);
                        if (!esServidor)
                        {
                            

                            ActualizarControlesPorFase();
                            ActualizarInfo();
                            ActualizarTarjetas();
                            panelMapa?.Invalidate();
                        }
                        break;
                        ;
                }
                panelMapa?.Invalidate();

            }
            catch (Exception ex)
            {
                AgregarLog($"Error procesando mensaje: {ex.Message}");
                AgregarLog($"Stack: {ex.StackTrace}");
            }
        }


        private void EnviarEstadoCompleto(TipoMensaje tipo = TipoMensaje.ActualizarEstado)
        {
            if (!modoRed || !esServidor || controlador == null) return;

            try
            {
                
                var jugadorActual = controlador.ObtenerJugadorActual();

                if (controlador.EstadoActual == EstadoJuego.DistribucionInicial)
                {
                    
                    if (jugadorActual.TerritoriosControlados.Contar == 0)
                    {
                        AgregarLog("[Error] Ese jugador no tiene territorio");
                        return;
                    }

                    
                    bool hayTropasPendientes = false;
                    foreach (var j in controlador.Jugadores.ObtenerTodos())
                    {
                        if (j.TropasDisponibles > 0)
                        {
                            hayTropasPendientes = true;
                            break;
                        }
                    }

                    if (!hayTropasPendientes)
                    {
                        AgregarLog("[Error] no tiene tropas disponible pero sigue en fase de distribuir inicial");
                    }
                }
                var estadoJuego = new DatosEstadoJuego
                {
                    EstadoActual = controlador.EstadoActual.ToString(),
                    TurnoActual = controlador.TurnoActual,
                    MapaSerializado = SerializarEstadoMapa()
                };               
                EnviarMensajeRed(tipo, estadoJuego);
                AgregarLog($"[RED] Estado completo enviado - Tipo: {tipo}");
                AgregarLog($"[RED] Turno: {controlador.TurnoActual} ({jugadorActual.Nombre})");
                AgregarLog($"[RED] Fase: {controlador.EstadoActual}");
                
            }
            catch (Exception ex)
            {
                AgregarLog($"Error enviando estado: {ex.Message}");
                
            }
        }

        private void EnviarMensajeRed(TipoMensaje tipo, object datos)
        {
            if (!modoRed || networkManager == null || !networkManager.Conectado)
                return;

            var mensaje = new MensajeRed
            {
                Tipo = tipo,
                Datos = JsonConvert.SerializeObject(datos),
                JugadorId = nombreJugadorLocal
            };

            networkManager.EnviarMensaje(mensaje);
        }
        private void RecibirColocarTropa(DatosAccionTerritorio datos)
        {
            AgregarLog($"[DEBUG] RecibirColocarTropa - Servidor: {esServidor}, Territorio: {datos.NombreTerritorio}");
            if (!esServidor)
            {
                AgregarLog($"[Red] Esperando confirma de servidor...");
                return;
            }

            var territorio = controlador.MapaJuego.ObtenerTerritorioPorNombre(datos.NombreTerritorio);
            if (territorio == null)
            {
                AgregarLog($"[Red] Territorio no encontrado: {datos.NombreTerritorio}");
                return;
            }



            var jugadorPropietario = controlador.ObtenerJugadorPorColor(territorio.PropietarioColor);
            if (jugadorPropietario == null)
            {
                AgregarLog($"[Red] Jugador no encontrado para color: {territorio.PropietarioColor}");
                return;
            }
            if (jugadorPropietario.TropasDisponibles <= 0)
            {
                AgregarLog($"[Red] {jugadorPropietario.Nombre} ya no tiene tropas para colocar");
                return;
            }
            var jugadorActual = controlador.ObtenerJugadorActual();
            if (jugadorActual.Color != jugadorPropietario.Color)
            {
                AgregarLog($"[Red] No es el turno de {jugadorPropietario.Nombre}");
                if (jugadorPropietario.Color == EColorJugador.Azul || jugadorPropietario.Color == EColorJugador.Verde)
                {

                    var todosJugadores = controlador.Jugadores.ObtenerTodos();
                    for (int i = 0; i < todosJugadores.Length; i++)
                    {
                        if (todosJugadores[i].Color == jugadorPropietario.Color)
                        {
                            controlador.TurnoActual = i;
                            AgregarLog($"[FIX] Turno ajustado a {jugadorPropietario.Nombre}");
                            jugadorActual = controlador.ObtenerJugadorActual();
                            break;
                        }
                    }

                }
                else { return; }
            }

            bool exito = controlador.ColocarTropaInicial(territorio);
            if (!exito)
            {
                AgregarLog($"[Red] fallar colocar tropa");
                return;
            }

            AgregarLog($"Cliente colocar tropa en  {datos.NombreTerritorio} ");

            

            if (controlador.DistribucionInicialCompleta()) 
            {
                AgregarLog("=== Terminar fase de distribuicion inicial ===");
                controlador.FinalizarDistribucionInicial();
                controlador.EstadoActual = EstadoJuego.Refuerzos;

                var primerJugador = controlador.ObtenerJugadorActual();
                int refuerzos = controlador.CalcularRefuerzosParaJugador(primerJugador);
                primerJugador.TropasDisponibles = refuerzos;

                btnAtacar.Enabled = false;
                btnMover.Enabled = false;
                btnSiguienteTurno.Enabled = true;
                btnAsignarTropas.Enabled = true;
                btnAsignarTropas.BackColor = Color.Blue;

                ActualizarTarjetas();
                ActualizarInfo();
            }
            else
            {
                
                controlador.SiguienteTurnoDistribucion();
                var nuevoJugador = controlador.ObtenerJugadorActual();
                AgregarLog($"Siguiente turno: {nuevoJugador.Nombre} ({nuevoJugador.Color})");

                
                if (nuevoJugador.EsNeutral && nuevoJugador.TropasDisponibles > 0)
                {
                    AgregarLog($"{nuevoJugador.Nombre} colocando automáticamente...");
                    bool exitoNeutral = controlador.ColocarTropaInicial(null);
                    if (exitoNeutral)
                    {
                        AgregarLog($"{nuevoJugador.Nombre} colocó tropa automáticamente");


                        // Verificar si la distribución está completa
                        if (controlador.DistribucionInicialCompleta())
                        {
                            AgregarLog("=== DISTRIBUCIÓN INICIAL COMPLETA ===");
                            controlador.FinalizarDistribucionInicial();
                            controlador.EstadoActual = EstadoJuego.Refuerzos;

                            // Asegurar que TurnoActual esté en 0 (primer jugador)
                            controlador.TurnoActual = 0;

                            var primerJugador = controlador.ObtenerJugadorActual();
                            int refuerzos = controlador.CalcularRefuerzosParaJugador(primerJugador);
                            primerJugador.TropasDisponibles = refuerzos;

                            btnAtacar.Enabled = false;
                            btnMover.Enabled = false;
                            btnSiguienteTurno.Enabled = true;
                            btnAsignarTropas.Enabled = true;
                            btnAsignarTropas.BackColor = Color.Blue;

                            ActualizarTarjetas();
                            ActualizarInfo();
                        }
                        else
                        {
                            // Solo avanzar turno una vez
                            controlador.SiguienteTurnoDistribucion();

                            var siguiente = controlador.ObtenerJugadorActual();

                            // Si el siguiente también es neutral, colocar automáticamente
                            if (siguiente.EsNeutral && siguiente.TropasDisponibles > 0)
                            {
                                System.Threading.Thread.Sleep(300);
                                bool exitoSiguiente = controlador.ColocarTropaInicial(null);
                                if (exitoSiguiente)
                                {
                                    AgregarLog($"{siguiente.Nombre} colocó tropa automáticamente");

                                    // Verificar nuevamente si terminó
                                    if (controlador.DistribucionInicialCompleta())
                                    {
                                        // Llamar recursivamente a RecibirColocarTropa para finalizar
                                        System.Threading.Thread.Sleep(100);
                                        RecibirColocarTropa(new DatosAccionTerritorio { NombreTerritorio = "", Cantidad = 0 });
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    
                    AgregarLog($"Esperando acción de {nuevoJugador.Nombre}");
                }
            }
        
            



            System.Threading.Thread.Sleep(200);
            EnviarEstadoCompleto(TipoMensaje.ActualizarEstado);
            AgregarLog($"[RED] Estado enviado - Turno: {controlador.ObtenerJugadorActual().Nombre}");
            ActualizarInfo();
            panelMapa.Invalidate();

        }
        

        

        private void RecibirAsignarTropas(DatosAccionTerritorio datos)
        {
            var territorio = controlador.MapaJuego.ObtenerTerritorioPorNombre(datos.NombreTerritorio);
            if (territorio != null)
            {
                controlador.AsignarTropas(territorio, datos.Cantidad);
                if (esServidor) 
                {
                    EnviarEstadoCompleto(TipoMensaje.ActualizarEstado);
                }
            }
            // Ambos actualizan visualmente
            AgregarLog($"{datos.Cantidad} tropas asignadas a {datos.NombreTerritorio}");
            ActualizarInfo();
            panelMapa.Invalidate();
        }

        private void RecibirAtaque(DatosAtaque datos)
        {
            var origen = controlador.MapaJuego.ObtenerTerritorioPorNombre(datos.TerritorioOrigen);
            var destino = controlador.MapaJuego.ObtenerTerritorioPorNombre(datos.TerritorioDestino);

            if (origen != null && destino != null)
            {
                if (esServidor)
                {
                    // SERVIDOR: Ejecutar el ataque real
                    var resultado = controlador.EjecutarAtaque(
                        origen,
                        destino,
                        datos.TropasAtacante,
                        datos.TropasDefensor
                    );

                    AgregarLog($"Ataque procesado: {datos.TerritorioOrigen} → {datos.TerritorioDestino}");
                    AgregarLog($"Resultado: Atacante -{resultado.TropasAtacantesPerdidas}, Defensor -{resultado.TropasDefensorPerdidas}");

                    if (resultado.TerritorioConquistado)
                    {
                        AgregarLog($"{datos.TerritorioDestino} fue conquistado!");
                    }

                    // Enviar resultado completo al cliente
                    var datosResultado = new DatosAtaque
                    {
                        TerritorioOrigen = datos.TerritorioOrigen,
                        TerritorioDestino = datos.TerritorioDestino,
                        TropasAtacante = datos.TropasAtacante,
                        TropasDefensor = datos.TropasDefensor,
                        DadosAtacante = resultado.DadosAtacante,
                        DadosDefensor = resultado.DadosDefensor
                    };
                    EnviarMensajeRed(TipoMensaje.Ataque, datosResultado);

                    System.Threading.Thread.Sleep(100);
                    EnviarEstadoCompleto(TipoMensaje.ActualizarEstado);

                    // Mostrar animación en servidor
                    MostrarAnimacionDados(resultado);

                    ActualizarInfo();
                    ActualizarTarjetas();
                    panelMapa.Invalidate();

                    // Verificar victoria
                    if (controlador.VerificarVictoria())
                    {
                        MessageBox.Show($"{controlador.ObtenerJugadorActual().Nombre} ha ganado el juego!",
                            "Victoria", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
                else
                {
                    // CLIENTE: Solo mostrar resultado
                    AgregarLog($"Ataque recibido: {datos.TerritorioOrigen} → {datos.TerritorioDestino}");

                    // Si tiene datos de dados, mostrar animación
                    if (datos.DadosAtacante != null && datos.DadosDefensor != null)
                    {
                        var resultado = new ResultadoCombate
                        {
                            DadosAtacante = datos.DadosAtacante,
                            DadosDefensor = datos.DadosDefensor,
                            TerritorioConquistado = destino.CantidadTropas <= 0
                        };
                        MostrarAnimacionDados(resultado);
                    }

                    ActualizarInfo();
                    ActualizarTarjetas();
                    panelMapa.Invalidate();
                }
            }
        }

        private void RecibirMovimiento(DatosMovimiento datos)
        {
            var origen = controlador.MapaJuego.ObtenerTerritorioPorNombre(datos.TerritorioOrigen);
            var destino = controlador.MapaJuego.ObtenerTerritorioPorNombre(datos.TerritorioDestino);

            if (origen != null && destino != null)
            {
                if (esServidor)
                {
                    // SERVIDOR: Ejecutar el movimiento real
                    bool exito = controlador.EjecutarMovimientoPlaneacion(origen, destino, datos.Cantidad);

                    if (exito)
                    {
                        AgregarLog($"Movimiento procesado: {datos.Cantidad} tropas de {datos.TerritorioOrigen} a {datos.TerritorioDestino}");

                        // Reenviar al cliente
                        EnviarMensajeRed(TipoMensaje.Movimiento, datos);

                        System.Threading.Thread.Sleep(100);
                        EnviarEstadoCompleto(TipoMensaje.ActualizarEstado);

                        ActualizarInfo();
                        panelMapa.Invalidate();
                    }
                    else
                    {
                        AgregarLog("Movimiento no válido o ya movió en este turno");
                    }
                }
                else
                {
                    // CLIENTE: Solo actualizar visual (ya viene sincronizado)
                    AgregarLog($"Movimiento recibido: {datos.Cantidad} tropas de {datos.TerritorioOrigen} a {datos.TerritorioDestino}");

                    ActualizarInfo();
                    panelMapa.Invalidate();
                }
            }
        }

        private void RecibirTerminarFase(string datosJson)
        {
            if (esServidor)
            {
                
                var datos = JsonConvert.DeserializeObject<DatosTerminarFase>(datosJson);
                var jugadorActual = controlador.ObtenerJugadorActual();

                
                if (jugadorActual.Color != EColorJugador.Azul &&
                    jugadorActual.Color != EColorJugador.Verde)
                {
                    AgregarLog($"[Red] Denegar solicitud de terminacion - no es turno de cliente");
                    return;
                }

                AgregarLog($"[Red] Recibi una solicitud de terminacion del cliente: {datos.FaseActual}");

               
                EjecutarTerminarFase();

                
                System.Threading.Thread.Sleep(100);
                EnviarEstadoCompleto(TipoMensaje.ActualizarEstado);
            }
            else
            {
                
                AgregarLog($"[Red] Fase ya se actualizo");
            }
        }

        private void SincronizarEstadoJuego(DatosEstadoJuego estado)
        {
            if (controlador?.MapaJuego == null)
            {
                AgregarLog("Error: No se puede sincronizar - controlador no inicializado");
                return;
            }

            try
            {
                if (string.IsNullOrEmpty(estado.MapaSerializado) || estado.MapaSerializado == "{}")
                {
                    AgregarLog("[ERROR] Estado serializado vacío o inválido");
                    return;
                }
                EstadoJuego estadoAnterior = controlador.EstadoActual;
                int turnoAnterior = controlador.TurnoActual;


                dynamic juegoData;
                try
                {
                    juegoData = JsonConvert.DeserializeObject<dynamic>(estado.MapaSerializado);
                }
                catch (JsonReaderException jex)
                {
                    AgregarLog($"[ERROR] JSON inválido: {jex.Message}");
                    AgregarLog($"[DEBUG] JSON recibido: {estado.MapaSerializado}");
                    return;
                }

                
                if (juegoData.Territorios == null)
                {
                    AgregarLog("[ERROR] Datos de territorios faltantes en JSON");
                    return;
                }

                
                controlador.TurnoActual = estado.TurnoActual;
                if (Enum.TryParse<EstadoJuego>(estado.EstadoActual, out EstadoJuego nuevoEstado))
                {
                    controlador.EstadoActual = nuevoEstado;
                }

                
                foreach (var jugador in controlador.Jugadores.ObtenerTodos())
                {
                    jugador.TerritoriosControlados = new Lista<Territorios>();
                }

                
                foreach (var tData in juegoData.Territorios)
                {
                    try
                    {
                        string nombre = tData.Nombre?.ToString();
                        if (string.IsNullOrEmpty(nombre))
                        {
                            AgregarLog("[WARN] Territorio sin nombre en datos de sincronización");
                            continue;
                        }

                        var territorio = controlador.MapaJuego.ObtenerTerritorioPorNombre(nombre);
                        if (territorio != null)
                        {
                            
                            if (tData.Color != null && tData.Tropas != null)
                            {
                                EColorJugador colorNuevo = (EColorJugador)(int)tData.Color;
                                territorio.PropietarioColor = colorNuevo;
                                territorio.CantidadTropas = (int)tData.Tropas;

                                var jugadorPropietario = controlador.ObtenerJugadorPorColor(colorNuevo);
                                if (jugadorPropietario != null)
                                {
                                    jugadorPropietario.TerritoriosControlados.Agregar(territorio);
                                }
                            }
                        }
                        else
                        {
                            AgregarLog($"[WARN] Territorio no encontrado en mapa: {nombre}");
                        }
                    }
                    catch (Exception ex)
                    {
                        AgregarLog($"[ERROR] Error sincronizando territorio: {ex.Message}");
                    }
                }


                if (juegoData.Jugadores != null)
                {
                    foreach (var jData in juegoData.Jugadores)
                    {
                        try
                        {
                            string nombreJugador = jData.Nombre?.ToString();
                            if (string.IsNullOrEmpty(nombreJugador))
                            {
                                AgregarLog("[WARN] Jugador sin nombre en datos de sincronización");
                                continue;
                            }

                            var jugador = controlador.Jugadores.ObtenerTodos()
                                .FirstOrDefault(j => j.Nombre == nombreJugador);
                            if (jugador != null && jData.TropasDisponibles != null)
                            {
                                jugador.TropasDisponibles = (int)jData.TropasDisponibles;
                            }
                        }
                        catch (Exception ex)
                        {
                            AgregarLog($"[ERROR] Error sincronizando jugador: {ex.Message}");
                        }
                    }
                }

                
                if (estadoAnterior != controlador.EstadoActual || turnoAnterior != controlador.TurnoActual)
                {
                    var jugadorActual = controlador.ObtenerJugadorActual();

                    if (estadoAnterior == EstadoJuego.DistribucionInicial &&
                        controlador.EstadoActual == EstadoJuego.Refuerzos)
                    {
                        AgregarLog("=== Finalizar fase de distribuir inicial，empezar juego ===");
                        btnAsignarTropas.Enabled = true;
                        btnAsignarTropas.BackColor = Color.Blue;
                        btnAtacar.Enabled = false;
                        btnMover.Enabled = false;
                        btnSiguienteTurno.Enabled = true;
                    }
                    else if (turnoAnterior != controlador.TurnoActual)
                    {
                        AgregarLog($"=== Cambiar turno：{jugadorActual.Nombre} ===");
                    }
                    else
                    {
                        AgregarLog($"=== Cambiar fase：{controlador.EstadoActual} ===");
                    }
                }

                
                ActualizarControlesPorFase();

                if (modoRed)
                {
                    ActualizarControlesPorTurno();
                }

                ActualizarInfo();
                ActualizarTarjetas();
                panelMapa.Invalidate();

                AgregarLog($"[Sincronica] Estado: {controlador.EstadoActual}, Turno: {controlador.ObtenerJugadorActual().Nombre}");
            }
            catch (Exception ex)
            {
                AgregarLog($"Sincronica error: {ex.Message}");
            }
        }

        // Serializar el estado del mapa para enviar por red
        private string SerializarEstadoMapa()
        {
            try
            {
                var jugadorActual = controlador.ObtenerJugadorActual();

                // Reconstruir las listas de territorios para cada jugador
                // para asegurar que estén actualizadas
                var jugadoresData = new List<object>();

                foreach (var jugador in controlador.Jugadores.ObtenerTodos())
                {
                    // Contar territorios actuales basándose en el mapa
                    int territoriosReales = 0;
                    var nombresTerritorios = new List<string>();

                    foreach (var territorio in controlador.MapaJuego.Territorios.ObtenerTodos())
                    {
                        if (territorio.PropietarioColor == jugador.Color)
                        {
                            territoriosReales++;
                            nombresTerritorios.Add(territorio.Name);
                        }
                    }

                    jugadoresData.Add(new
                    {
                        Nombre = jugador.Nombre,
                        Color = (int)jugador.Color,
                        TropasDisponibles = jugador.TropasDisponibles,
                        TerritoriosCount = territoriosReales,
                        Territorios = nombresTerritorios.ToArray(),
                        EsNeutral = jugador.EsNeutral
                    });
                }

                var estadoCompleto = new
                {
                    // Datos de territorios
                    Territorios = controlador.MapaJuego.Territorios.ObtenerTodos().Select(t => new
                    {
                        Nombre = t.Name,
                        Color = (int)t.PropietarioColor,
                        Tropas = t.CantidadTropas,
                        Continente = t.Continente,
                        PosicionX = t.PosicionX,
                        PosicionY = t.PosicionY
                    }).ToArray(),

                    // Datos de jugadores
                    Jugadores = jugadoresData.ToArray(),

                    // Estado del juego
                    TurnoActual = controlador.TurnoActual,
                    Estado = controlador.EstadoActual.ToString(),
                    JugadorActual = jugadorActual?.Nombre ?? "N/A",

                    // Información adicional para validación
                    TotalTerritorios = controlador.MapaJuego.Territorios.Contar,
                    ContadorIntercambios = controlador.MapaJuego.ContadorIntercambios,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                string json = JsonConvert.SerializeObject(estadoCompleto, Formatting.None);

                // Log para depuración
                AgregarLog($"[SERIALIZE] Estado serializado - Tamaño: {json.Length} bytes");
                AgregarLog($"[SERIALIZE] Total territorios: {controlador.MapaJuego.Territorios.Contar}");

                return json;
            }
            catch (Exception ex)
            {
                AgregarLog($"Error serializando mapa: {ex.Message}");
                AgregarLog($"Stack: {ex.StackTrace}");
                return "{}";
            }
        }

        // Bloquear controles cuando no es tu turno (modo red)
        private void ActualizarControlesPorTurno()
        {
            if (!modoRed || controlador == null) return;

            var jugadorActual = controlador.ObtenerJugadorActual();

            // Determinar si es mi turno comparando por COLOR
            bool esMiTurno = (jugadorActual.Color == miColorEnRed);

            // Habilitar/deshabilitar controles según el turno
            bool puedeAsignar = esMiTurno &&
                                (controlador.EstadoActual == EstadoJuego.Refuerzos ||
                                 controlador.EstadoActual == EstadoJuego.DistribucionInicial);
            bool puedeAtacar = esMiTurno && controlador.EstadoActual == EstadoJuego.Ataque;
            bool puedeMover = esMiTurno && controlador.EstadoActual == EstadoJuego.Planeacion;

            if (btnAsignarTropas != null)
            {
                btnAsignarTropas.Enabled = puedeAsignar;
                if (!esMiTurno)
                    btnAsignarTropas.BackColor = Color.LightBlue;
            }

            if (btnAtacar != null)
            {
                btnAtacar.Enabled = puedeAtacar;
                if (!esMiTurno)
                    btnAtacar.BackColor = Color.LightCoral;
            }

            if (btnMover != null)
            {
                btnMover.Enabled = puedeMover;
                if (!esMiTurno)
                    btnMover.BackColor = Color.LightGreen;
            }

            if (btnSiguienteTurno != null)
                btnSiguienteTurno.Enabled = esMiTurno;

            if (btnIntercambiarTarjetas != null)
                btnIntercambiarTarjetas.Enabled = esMiTurno && btnIntercambiarTarjetas.Enabled;

            // Feedback visual
            if (lblInfo != null)
            {
                if (!esMiTurno)
                {
                    lblInfo.BackColor = Color.FromArgb(255, 230, 230);
                    lblInfo.Text += "\n\n[Esperando turno...]";
                }
                else
                {
                    lblInfo.BackColor = Color.FromArgb(230, 255, 230);
                    lblInfo.Text += "\n\n[¡ES TU TURNO!]";
                }
            }

            // Control de interacción con el mapa
            if (!esMiTurno)
            {
                panelMapa.Enabled = false;
                panelMapa.BackColor = Color.FromArgb(220, 220, 220);
            }
            else
            {
                panelMapa.Enabled = true;
                panelMapa.BackColor = Color.White;
            }
        }

        private void ActualizarControlesPorFase()
        {
            if (controlador == null) return;
            ResetBotones();
            switch (controlador.EstadoActual)
            {
                case EstadoJuego.DistribucionInicial:
                    btnAsignarTropas.Enabled = true;
                    btnAsignarTropas.BackColor = Color.Blue;
                    btnAtacar.Enabled = false;
                    btnMover.Enabled = false;
                    btnSiguienteTurno.Enabled = false;
                    break;

                case EstadoJuego.Refuerzos:
                    btnAsignarTropas.Enabled = true;
                    btnAtacar.Enabled = false;
                    btnMover.Enabled = false;
                    btnSiguienteTurno.Enabled = true;
                    
                    btnAsignarTropas.BackColor = Color.Blue;
                    break;

                case EstadoJuego.Ataque:
                    btnAsignarTropas.Enabled = false;
                    btnAtacar.Enabled = true;
                    btnMover.Enabled = false;
                    btnSiguienteTurno.Enabled = true;
                    btnAtacar.BackColor = Color.Blue;
                    break;

                case EstadoJuego.Planeacion:
                    btnAsignarTropas.Enabled = false;
                    btnAtacar.Enabled = false;
                    btnMover.Enabled = true;
                    btnSiguienteTurno.Enabled = true;
                    btnMover.BackColor = Color.Blue;
                    break;

                case EstadoJuego.Finalizado:
                    btnAsignarTropas.Enabled = false;
                    btnAtacar.Enabled = false;
                    btnMover.Enabled = false;
                    btnSiguienteTurno.Enabled = false;
                    break;
            }

            // Si es modo red, aplicar restricciones de turno
            if (modoRed)
            {
                ActualizarControlesPorTurno();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (modoRed && networkManager != null)
            {
                networkManager.Desconectar();
            }
            base.OnFormClosing(e);
        }
    }
}
    

