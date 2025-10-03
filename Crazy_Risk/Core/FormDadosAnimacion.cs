using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public class FormDadosAnimacion : Form
    {
        

        public event Action<int[], int[], int, int> AnimacionFinalizada;

        private int[] dadosAtacante;
        private int[] dadosDefensor;
        private System.Windows.Forms.Timer timer;
        private int frameActual = 0;
        private const int FRAMES_TOTALES = 20;
        private Random random;

        private Panel panelAtacante;
        private Panel panelDefensor;
        private Label lblAtacante;
        private Label lblDefensor;
        private Label lblResultado;

        public FormDadosAnimacion(int[] dadosAtaq, int[] dadosDef, int perdidasAtacante, int perdidasDefensor)
        {
            dadosAtacante = dadosAtaq;
            dadosDefensor = dadosDef;
            random = new Random();

            InitializeComponent(perdidasAtacante, perdidasDefensor);
            IniciarAnimacion();
        }

        private void InitializeComponent(int perdidasAtacante, int perdidasDefensor)
        {
            this.Text = "Resultado del Combate";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(40, 40, 45);

            // Título
            Label lblTitulo = new Label
            {
                Text = "¡COMBATE!",
                Location = new Point(0, 20),
                Size = new Size(600, 40),
                Font = new Font("Arial", 20, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Sección Atacante
            lblAtacante = new Label
            {
                Text = "ATACANTE",
                Location = new Point(50, 80),
                Size = new Size(200, 30),
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 100, 100),
                TextAlign = ContentAlignment.MiddleCenter
            };

            panelAtacante = new Panel
            {
                Location = new Point(50, 120),
                Size = new Size(200, 100),
                BackColor = Color.FromArgb(60, 60, 65)
            };
            panelAtacante.Paint += PanelAtacante_Paint;

            // Sección Defensor
            lblDefensor = new Label
            {
                Text = "DEFENSOR",
                Location = new Point(350, 80),
                Size = new Size(200, 30),
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 150, 255),
                TextAlign = ContentAlignment.MiddleCenter
            };

            panelDefensor = new Panel
            {
                Location = new Point(350, 120),
                Size = new Size(200, 100),
                BackColor = Color.FromArgb(60, 60, 65)
            };
            panelDefensor.Paint += PanelDefensor_Paint;

            // Resultado
            string textoResultado = $"Perdidas: Atacante -{perdidasAtacante}  |  Defensor -{perdidasDefensor}";
            lblResultado = new Label
            {
                Text = textoResultado,
                Location = new Point(50, 250),
                Size = new Size(500, 40),
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.Yellow,
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };

            // Botón cerrar
            Button btnCerrar = new Button
            {
                Text = "Continuar",
                Location = new Point(225, 310),
                Size = new Size(150, 40),
                Font = new Font("Arial", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCerrar.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                lblTitulo, lblAtacante, panelAtacante,
                lblDefensor, panelDefensor, lblResultado, btnCerrar
            });

            // Timer para animación
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 50;
            timer.Tick += Timer_Tick;
        }

        private void IniciarAnimacion()
        {
            frameActual = 0;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            frameActual++;

            if (frameActual < FRAMES_TOTALES)
            {
                // Durante la animación, mostrar valores aleatorios
                panelAtacante.Invalidate();
                panelDefensor.Invalidate();
            }
            else
            {
                // Mostrar resultado final
                timer.Stop();
                lblResultado.Visible = true;
                panelAtacante.Invalidate();
                panelDefensor.Invalidate();
                
            }
        }
        

        private void PanelAtacante_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int[] valores;
            if (frameActual < FRAMES_TOTALES)
            {
                // Mostrar valores aleatorios durante animación
                valores = new int[dadosAtacante.Length];
                for (int i = 0; i < valores.Length; i++)
                    valores[i] = random.Next(1, 7);
            }
            else
            {
                // Mostrar valores finales
                valores = dadosAtacante;
            }

            DibujarDados(g, valores, Color.FromArgb(255, 80, 80));
        }

        private void PanelDefensor_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int[] valores;
            if (frameActual < FRAMES_TOTALES)
            {
                valores = new int[dadosDefensor.Length];
                for (int i = 0; i < valores.Length; i++)
                    valores[i] = random.Next(1, 7);
            }
            else
            {
                valores = dadosDefensor;
            }

            DibujarDados(g, valores, Color.FromArgb(80, 130, 255));
        }

        private void DibujarDados(Graphics g, int[] valores, Color colorBase)
        {
            int espacioEntre = 70;
            int inicioX = (200 - (valores.Length * espacioEntre - 10)) / 2;

            for (int i = 0; i < valores.Length; i++)
            {
                int x = inicioX + i * espacioEntre;
                int y = 25;
                DibujarDado(g, x, y, valores[i], colorBase);
            }
        }

        private void DibujarDado(Graphics g, int x, int y, int valor, Color colorBase)
        {
            int tamano = 50;

            // Dibujar cubo del dado
            using (SolidBrush brush = new SolidBrush(colorBase))
            {
                g.FillRectangle(brush, x, y, tamano, tamano);
            }

            // Borde
            g.DrawRectangle(new Pen(Color.White, 2), x, y, tamano, tamano);

            // Dibujar puntos según el valor
            Color colorPunto = Color.White;
            int radio = 4;
            int centro = tamano / 2;

            switch (valor)
            {
                case 1:
                    DibujarPunto(g, x + centro, y + centro, radio, colorPunto);
                    break;
                case 2:
                    DibujarPunto(g, x + 12, y + 12, radio, colorPunto);
                    DibujarPunto(g, x + 38, y + 38, radio, colorPunto);
                    break;
                case 3:
                    DibujarPunto(g, x + 12, y + 12, radio, colorPunto);
                    DibujarPunto(g, x + centro, y + centro, radio, colorPunto);
                    DibujarPunto(g, x + 38, y + 38, radio, colorPunto);
                    break;
                case 4:
                    DibujarPunto(g, x + 12, y + 12, radio, colorPunto);
                    DibujarPunto(g, x + 38, y + 12, radio, colorPunto);
                    DibujarPunto(g, x + 12, y + 38, radio, colorPunto);
                    DibujarPunto(g, x + 38, y + 38, radio, colorPunto);
                    break;
                case 5:
                    DibujarPunto(g, x + 12, y + 12, radio, colorPunto);
                    DibujarPunto(g, x + 38, y + 12, radio, colorPunto);
                    DibujarPunto(g, x + centro, y + centro, radio, colorPunto);
                    DibujarPunto(g, x + 12, y + 38, radio, colorPunto);
                    DibujarPunto(g, x + 38, y + 38, radio, colorPunto);
                    break;
                case 6:
                    DibujarPunto(g, x + 12, y + 12, radio, colorPunto);
                    DibujarPunto(g, x + 38, y + 12, radio, colorPunto);
                    DibujarPunto(g, x + 12, y + centro, radio, colorPunto);
                    DibujarPunto(g, x + 38, y + centro, radio, colorPunto);
                    DibujarPunto(g, x + 12, y + 38, radio, colorPunto);
                    DibujarPunto(g, x + 38, y + 38, radio, colorPunto);
                    break;
            }

            // Número del dado (grande y visible)
            using (Font font = new Font("Arial", 16, FontStyle.Bold))
            {
                string texto = valor.ToString();
                SizeF textSize = g.MeasureString(texto, font);
                g.DrawString(texto, font, new SolidBrush(Color.Black),
                    x + (tamano - textSize.Width) / 2,
                    y + (tamano - textSize.Height) / 2);
            }
        }

        private void DibujarPunto(Graphics g, int x, int y, int radio, Color color)
        {
            using (SolidBrush brush = new SolidBrush(color))
            {
                g.FillEllipse(brush, x - radio, y - radio, radio * 2, radio * 2);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            timer?.Stop();
            timer?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
