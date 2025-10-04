using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrazyRisk.Network;



namespace CrazyRisk.Network
{
    public partial class FormConexion : Form
    {
        public NetworkManager NetworkManager { get; private set; }
        public string NombreJugador { get; private set; }
        public bool ConexionExitosa { get; private set; }

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

        public FormConexion()
        {
            NetworkManager = new NetworkManager();
            InitializeComponent();
            
            ConexionExitosa = false;
        }

        private void InitializeComponent()
        {
            this.Text = "Crazy Risk - Conexión de Red";
            this.Size = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            // Título
            Label lblTitulo = new Label
            {
                Text = "Configuración de Partida en Red",
                Location = new Point(20, 20),
                Size = new Size(450, 30),
                Font = new Font("Arial", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Radio buttons
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

            // Panel Servidor
            panelServidor = new Panel
            {
                Location = new Point(20, 110),
                Size = new Size(450, 150),
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

            Label lblPuertoServ = new Label
            {
                Text = "Puerto:",
                Location = new Point(15, 55),
                Size = new Size(100, 20),
                Font = new Font("Arial", 11)
            };

            txtPuertoServidor = new TextBox
            {
                Location = new Point(125, 53),
                Size = new Size(100, 25),
                Font = new Font("Arial", 11),
                Text = "5555"
            };

            lblIPLocal = new Label
            {
                Text = $"Tu IP: {NetworkManager.ObtenerIPLocal()}",
                Location = new Point(15, 90),
                Size = new Size(420, 50),
                Font = new Font("Arial", 9),
                ForeColor = Color.Blue,
                TextAlign = ContentAlignment.TopLeft
            };
            lblIPLocal.Text += "\n(Comparte esta IP con el otro jugador)";

            panelServidor.Controls.AddRange(new Control[] {
                lblNombreServ, txtNombreServidor,
                lblPuertoServ, txtPuertoServidor, lblIPLocal
            });

            // Panel Cliente
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

            // Estado
            lblEstado = new Label
            {
                Location = new Point(20, 275),
                Size = new Size(450, 40),
                Font = new Font("Arial", 11),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Presiona Conectar para iniciar"
            };

            // Botón conectar
            btnConectar = new Button
            {
                Text = "Crear Partida",
                Location = new Point(150, 330),
                Size = new Size(200, 50),
                Font = new Font("Arial", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnConectar.Click += BtnConectar_Click;

            // Agregar controles
            this.Controls.AddRange(new Control[] {
                lblTitulo, rbServidor, rbCliente,
                panelServidor, panelCliente,
                lblEstado, btnConectar
            });

            // Configurar eventos de red
            NetworkManager.ClienteConectado += OnClienteConectado;
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
                    
                    NombreJugador = txtNombreServidor.Text.Trim();
                    if (string.IsNullOrEmpty(NombreJugador))
                    {
                        MessageBox.Show("Ingresa tu nombre", "Error");
                        btnConectar.Enabled = true;
                        return;
                    }

                    int puerto = int.Parse(txtPuertoServidor.Text);
                    lblEstado.Text = "Esperando que el otro jugador se conecte...";
                    NetworkManager.IniciarServidor(puerto);

                    
                }
                else
                {
                    
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
                    while (!NetworkManager.Conectado && intentos < 30)
                    {
                        System.Threading.Thread.Sleep(100);
                        Application.DoEvents();
                        intentos++;
                    }

                    if (NetworkManager.Conectado)
                    {
                        lblEstado.Text = "¡Conectado! Esperando inicio del juego...";
                        lblEstado.ForeColor = Color.Green;
                        ConexionExitosa = true;
                        EnviarDatosConexion();

                        
                        System.Threading.Thread.Sleep(1000);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        throw new Exception("No se pudo conectar al servidor - timeout");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error de Conexión");
                lblEstado.Text = "Error al conectar";
                lblEstado.ForeColor = Color.Red;
                btnConectar.Enabled = true;
                NetworkManager.Desconectar();
            }
        }

        private void OnClienteConectado()
        {
            // Servidor recibió conexión
            this.Invoke((MethodInvoker)delegate
            {
                lblEstado.Text = "¡Jugador conectado! Iniciando juego...";
                lblEstado.ForeColor = Color.Green;
                ConexionExitosa = true;
                OnConexionExitosa?.Invoke(NetworkManager.EsServidor);

                System.Threading.Thread.Sleep(1000);
                this.DialogResult = DialogResult.OK;
                this.Close();
            });
        }

        private void OnErrorConexion(string mensaje)
        {
            if (!this.IsDisposed && !this.Disposing)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    lblEstado.Text = $"Error: {mensaje}";
                    lblEstado.ForeColor = Color.Red;
                    btnConectar.Enabled = true;
                });
            }
        }

        private void EnviarDatosConexion()
        {
            var datosConexion = new DatosConexion
            {
                NombreJugador = NombreJugador,
                ColorJugador = 1 // Azul para el segundo jugador
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
            NetworkManager.ErrorConexion -= OnErrorConexion;
            if (!ConexionExitosa)
            {
                NetworkManager.Desconectar();
            }
            base.OnFormClosing(e);
        }
    }
}
