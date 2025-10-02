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
            menu_incio.Left = (this.ClientSize.Width - menu_incio.Width) / 2;
            menu_incio.Top = (this.ClientSize.Height - menu_incio.Height) / 2;
        }

        private void boton_host_Click(object sender, EventArgs e)
        {
            menu_incio.Visible = false;
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
