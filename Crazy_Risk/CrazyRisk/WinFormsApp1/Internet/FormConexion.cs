using System;
using System.Windows.Forms;
using CrazyRisk.Network;
using System.Drawing;
using System.Threading;

namespace CrazyRisk.Network
{
    public partial class FormConexion : Form
    {
        public NetworkManager NetworkManager { get; private set; }
        public string NombreJugador { get; private set; }
        public bool ConexionExitosa { get; private set; }
        public int ModoJuego { get; set; }

        public delegate void ConexionExitosaHandler(bool esServidor);
        public event ConexionExitosaHandler OnConexionExitosa;

        private RadioButton rbServidor;
        private RadioButton rbCliente;
        private Panel panelServidor;
        private Panel panelCliente;
        private TextBox txtNombreServidor;
        private TextBox txtPuertoServidor;
        private Label lblIPLocal;
        private TextBox txtNombreCliente;
        private TextBox txtIPServidor;
        private TextBox txtPuertoCliente;
        private Button btnConectar;
        private Label lblEstado;
        private ComboBox cmbModoServidor;
        private Label lblClientesConectados;

        private bool esperandoCierre = false;

        public FormConexion(int modoJuego = 0)
        {
            NetworkManager = new NetworkManager();
            ModoJuego = modoJuego;
            InitializeComponent();
            ConexionExitosa = false;
            NetworkManager.MensajeRecibido += OnMensajeRecibidoConexion;
        }

        private void InitializeComponent()
        {
            this.Text = "Crazy Risk - Conexión de Red";
            this.Size = new Size(500, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            Label lblTitulo = new Label
            {
                Text = "Configuración de Partida en Red",
                Location = new Point(20, 20),
                Size = new Size(450, 30),
                Font = new Font("Arial", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            rbServidor = new RadioButton
            {
                Text = "Crear Partida (Servidor)",
                Location = new Point(50, 70),
                Size = new Size(180, 25),
                Checked = true,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            rbServidor.CheckedChanged += RbTipo_CheckedChanged;

            rbCliente = new RadioButton
            {
                Text = "Unirse a Partida (Cliente)",
                Location = new Point(250, 70),
                Size = new Size(200, 25),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            rbCliente.CheckedChanged += RbTipo_CheckedChanged;

            panelServidor = new Panel
            {
                Location = new Point(20, 110),
                Size = new Size(450, 200),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(240, 248, 255)
            };

            Label lblNombreServ = new Label
            {
                Text = "Tu Nombre:",
                Location = new Point(15, 20),
                Size = new Size(100, 20),
                Font = new Font("Arial", 11)
            };

            txtNombreServidor = new TextBox
            {
                Location = new Point(125, 18),
                Size = new Size(300, 25),
                Font = new Font("Arial", 11),
                Text = "Jugador 1"
            };

            Label lblModoServ = new Label
            {
                Text = "Modo de juego:",
                Location = new Point(15, 55),
                Size = new Size(100, 20),
                Font = new Font("Arial", 11)
            };

            cmbModoServidor = new ComboBox
            {
                Location = new Point(125, 53),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Arial", 11)
            };
            cmbModoServidor.Items.AddRange(new string[] {
                "2 Jugadores",
                "2 Jugadores + Neutral",
                "3 Jugadores"
            });
            cmbModoServidor.SelectedIndex = ModoJuego;

            Label lblPuertoServ = new Label
            {
                Text = "Puerto:",
                Location = new Point(15, 90),
                Size = new Size(100, 20),
                Font = new Font("Arial", 11)
            };

            txtPuertoServidor = new TextBox
            {
                Location = new Point(125, 88),
                Size = new Size(100, 25),
                Font = new Font("Arial", 11),
                Text = "5555"
            };

            lblIPLocal = new Label
            {
                Text = $"Tu IP: {NetworkManager.ObtenerIPLocal()}",
                Location = new Point(15, 125),
                Size = new Size(420, 30),
                Font = new Font("Arial", 9),
                ForeColor = Color.Blue,
                TextAlign = ContentAlignment.TopLeft
            };
            lblIPLocal.Text += "\n(Comparte esta IP con los otros jugadores)";

            lblClientesConectados = new Label
            {
                Text = "Clientes conectados: 0",
                Location = new Point(15, 165),
                Size = new Size(420, 25),
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.Green
            };

            panelServidor.Controls.AddRange(new Control[] {
                lblNombreServ, txtNombreServidor,
                lblModoServ, cmbModoServidor,
                lblPuertoServ, txtPuertoServidor,
                lblIPLocal, lblClientesConectados
            });

            panelCliente = new Panel
            {
                Location = new Point(20, 110),
                Size = new Size(450, 150),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(255, 248, 240),
                Visible = false
            };

            Label lblNombreCli = new Label
            {
                Text = "Tu Nombre:",
                Location = new Point(15, 20),
                Size = new Size(100, 20),
                Font = new Font("Arial", 11)
            };

            txtNombreCliente = new TextBox
            {
                Location = new Point(125, 18),
                Size = new Size(300, 25),
                Font = new Font("Arial", 11),
                Text = "Jugador 2"
            };

            Label lblIPServ = new Label
            {
                Text = "IP del Servidor:",
                Location = new Point(15, 55),
                Size = new Size(100, 20),
                Font = new Font("Arial", 11)
            };

            txtIPServidor = new TextBox
            {
                Location = new Point(125, 53),
                Size = new Size(200, 25),
                Font = new Font("Arial", 11),
                Text = "127.0.0.1"
            };

            Label lblPuertoCli = new Label
            {
                Text = "Puerto:",
                Location = new Point(15, 90),
                Size = new Size(100, 20),
                Font = new Font("Arial", 11)
            };

            txtPuertoCliente = new TextBox
            {
                Location = new Point(125, 88),
                Size = new Size(100, 25),
                Font = new Font("Arial", 11),
                Text = "5555"
            };

            panelCliente.Controls.AddRange(new Control[] {
                lblNombreCli, txtNombreCliente,
                lblIPServ, txtIPServidor,
                lblPuertoCli, txtPuertoCliente
            });

            lblEstado = new Label
            {
                Location = new Point(20, 325),
                Size = new Size(450, 60),
                Font = new Font("Arial", 11),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Presiona Conectar para iniciar"
            };

            btnConectar = new Button
            {
                Text = "Crear Partida",
                Location = new Point(150, 400),
                Size = new Size(200, 50),
                Font = new Font("Arial", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnConectar.Click += BtnConectar_Click;

            this.Controls.AddRange(new Control[] {
                lblTitulo, rbServidor, rbCliente,
                panelServidor, panelCliente,
                lblEstado, btnConectar
            });

            NetworkManager.ClienteConectado += OnClienteConectadoRed;
            NetworkManager.TodosClientesConectados += OnTodosClientesConectados;
            NetworkManager.ErrorConexion += OnErrorConexion;
        }

        private void RbTipo_CheckedChanged(object sender, EventArgs e)
        {
            if (rbServidor.Checked)
            {
                panelServidor.Visible = true;
                panelCliente.Visible = false;
                btnConectar.Text = "Crear Partida";
                btnConectar.BackColor = Color.FromArgb(76, 175, 80);
            }
            else
            {
                panelServidor.Visible = false;
                panelCliente.Visible = true;
                btnConectar.Text = "Unirse a Partida";
                btnConectar.BackColor = Color.FromArgb(33, 150, 243);
            }
        }

        private void BtnConectar_Click(object sender, EventArgs e)
        {
            btnConectar.Enabled = false;
            lblEstado.ForeColor = Color.Blue;

            try
            {
                if (rbServidor.Checked)
                {
                    // SERVIDOR
                    NombreJugador = txtNombreServidor.Text.Trim();
                    if (string.IsNullOrEmpty(NombreJugador))
                    {
                        MessageBox.Show("Ingresa tu nombre", "Error");
                        btnConectar.Enabled = true;
                        return;
                    }

                    int puerto = int.Parse(txtPuertoServidor.Text);
                    ModoJuego = cmbModoServidor.SelectedIndex;

                    int clientesEsperados = (ModoJuego == 2) ? 2 : 1;

                    lblEstado.Text = $"Esperando {clientesEsperados} jugador(es)...\n" +
                                   $"Tu IP: {NetworkManager.ObtenerIPLocal()}";

                    NetworkManager.IniciarServidor(puerto, clientesEsperados);

                    // NO cerrar - esperar evento OnTodosClientesConectados
                }
                else
                {
                    // CLIENTE
                    NombreJugador = txtNombreCliente.Text.Trim();
                    if (string.IsNullOrEmpty(NombreJugador))
                    {
                        MessageBox.Show("Ingresa tu nombre", "Error");
                        btnConectar.Enabled = true;
                        return;
                    }

                    string ip = txtIPServidor.Text.Trim();
                    int puerto = int.Parse(txtPuertoCliente.Text);

                    lblEstado.Text = "Conectando al servidor...";

                    NetworkManager.ConectarAServidor(ip, puerto);

                    int intentos = 0;
                    while (!NetworkManager.Conectado && intentos < 50)
                    {
                        Thread.Sleep(100);
                        Application.DoEvents();
                        intentos++;
                    }

                    if (NetworkManager.Conectado)
                    {
                        lblEstado.Text = "¡Conectado!\nEsperando que todos se conecten...";
                        lblEstado.ForeColor = Color.Green;
                        ConexionExitosa = true;

                        EnviarDatosConexion();

                        // Esperar señal para cerrar
                        esperandoCierre = true;

                        // Iniciar timer para detectar cuando todos estén listos
                        System.Windows.Forms.Timer timerEspera = new System.Windows.Forms.Timer();
                        timerEspera.Interval = 500;
                        timerEspera.Tick += (s, evt) =>
                        {
                            if (!esperandoCierre || this.IsDisposed)
                            {
                                timerEspera.Stop();
                                return;
                            }

                            // Aquí el servidor debería enviar señal cuando esté listo
                            Application.DoEvents();
                        };
                        timerEspera.Start();
                    }
                    else
                    {
                        throw new Exception("Timeout - No se pudo conectar");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error");
                lblEstado.Text = "Error al conectar";
                lblEstado.ForeColor = Color.Red;
                btnConectar.Enabled = true;
                NetworkManager.Desconectar();
            }
        }

        private void OnClienteConectadoRed()
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;

            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate { OnClienteConectadoRed(); });
                return;
            }

            int conectados = NetworkManager.NumeroClientesConectados();
            int esperados = NetworkManager.ClientesEsperados;

            lblClientesConectados.Text = $"Clientes conectados: {conectados}/{esperados}";
            lblEstado.Text = $"Esperando jugadores...\n({conectados}/{esperados} conectados)";
        }

        private void OnTodosClientesConectados()
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;

            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate { OnTodosClientesConectados(); });
                return;
            }

            lblEstado.Text = "¡Todos conectados!\nIniciando juego...";
            lblEstado.ForeColor = Color.Green;
            ConexionExitosa = true;
            esperandoCierre = false;

            OnConexionExitosa?.Invoke(NetworkManager.EsServidor);

            Thread.Sleep(1000);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void OnErrorConexion(string mensaje)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;

            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate { OnErrorConexion(mensaje); });
                return;
            }

            lblEstado.Text = $"Error: {mensaje}";
            lblEstado.ForeColor = Color.Red;
            btnConectar.Enabled = true;
        }

        //
        private void OnMensajeRecibidoConexion(MensajeRed mensaje)
        {
            if (mensaje.Tipo == TipoMensaje.ConexionJugador)
            {
                try
                {
                    var datos = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(mensaje.Datos);

                    if (datos.TodosConectados != null && (bool)datos.TodosConectados)
                    {
                        // TODOS CONECTADOS - CERRAR VENTANA
                        if (this.InvokeRequired)
                        {
                            this.BeginInvoke((MethodInvoker)delegate { CerrarVentanaConexion(); });
                        }
                        else
                        {
                            CerrarVentanaConexion();
                        }
                    }
                }
                catch { }
            }
        }

        private void CerrarVentanaConexion()
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;

            esperandoCierre = false;
            ConexionExitosa = true;

            lblEstado.Text = "¡Todos conectados! Iniciando...";
            lblEstado.ForeColor = Color.Green;

            Thread.Sleep(500);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void EnviarDatosConexion()
        {
            var datosConexion = new DatosConexion
            {
                NombreJugador = NombreJugador,
                ColorJugador = 1
            };

            var mensaje = new MensajeRed
            {
                Tipo = TipoMensaje.ConexionJugador,
                Datos = Newtonsoft.Json.JsonConvert.SerializeObject(datosConexion),
                JugadorId = NombreJugador
            };

            NetworkManager.EnviarMensaje(mensaje);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            esperandoCierre = false;

            try
            {
                NetworkManager.ClienteConectado -= OnClienteConectadoRed;
                NetworkManager.TodosClientesConectados -= OnTodosClientesConectados;
                NetworkManager.ErrorConexion -= OnErrorConexion;
                NetworkManager.MensajeRecibido -= OnMensajeRecibidoConexion;
            }
            catch { }

            if (!ConexionExitosa)
            {
                NetworkManager.Desconectar();
            }

            base.OnFormClosing(e);
        }
    }
}
