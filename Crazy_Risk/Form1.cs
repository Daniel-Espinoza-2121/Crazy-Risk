namespace Crazy_Risk
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            men�_incio.Left = (this.ClientSize.Width - men�_incio.Width) / 2;
            men�_incio.Top = (this.ClientSize.Height - men�_incio.Height) / 2;
        }

        private void boton_host_Click(object sender, EventArgs e)
        {
            men�_incio.Visible = false;
            this.Size = new System.Drawing.Size(1200, 614);
        }

        private void boton_invitado_Click(object sender, EventArgs e)
        {

        }

        private void boton_salir_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
