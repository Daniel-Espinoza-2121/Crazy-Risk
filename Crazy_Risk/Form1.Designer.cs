namespace Crazy_Risk
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            boton_salir = new Button();
            boton_invitado = new Button();
            boton_host = new Button();
            label1 = new Label();
            menu_incio = new Panel();
            menu_incio.SuspendLayout();
            SuspendLayout();
            // 
            // boton_salir
            // 
            boton_salir.Location = new Point(49, 85);
            boton_salir.Name = "boton_salir";
            boton_salir.Size = new Size(75, 23);
            boton_salir.TabIndex = 2;
            boton_salir.Text = "Salir";
            boton_salir.UseVisualStyleBackColor = true;
            boton_salir.Click += boton_salir_Click;
            // 
            // boton_invitado
            // 
            boton_invitado.Location = new Point(21, 56);
            boton_invitado.Name = "boton_invitado";
            boton_invitado.Size = new Size(131, 23);
            boton_invitado.TabIndex = 1;
            boton_invitado.Text = "Jugar como invitado";
            boton_invitado.UseVisualStyleBackColor = true;
            boton_invitado.Click += boton_invitado_Click;
            // 
            // boton_host
            // 
            boton_host.Location = new Point(21, 27);
            boton_host.Name = "boton_host";
            boton_host.Size = new Size(131, 23);
            boton_host.TabIndex = 0;
            boton_host.Text = "Jugar como host";
            boton_host.UseVisualStyleBackColor = true;
            boton_host.Click += boton_host_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(56, 9);
            label1.Name = "label1";
            label1.Size = new Size(60, 15);
            label1.TabIndex = 1;
            label1.Text = "Crasy Risk";
            // 
            // menu_incio
            // 
            menu_incio.BackColor = Color.Transparent;
            menu_incio.Controls.Add(boton_salir);
            menu_incio.Controls.Add(label1);
            menu_incio.Controls.Add(boton_invitado);
            menu_incio.Controls.Add(boton_host);
            menu_incio.Location = new Point(531, 206);
            menu_incio.Name = "menu_incio";
            menu_incio.Size = new Size(183, 108);
            menu_incio.TabIndex = 2;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = Properties.Resources._1200px_Risk_game_map;
            ClientSize = new Size(1184, 575);
            Controls.Add(menu_incio);
            MaximumSize = new Size(1200, 614);
            MinimumSize = new Size(1200, 614);
            Name = "Form1";
            Text = "Crazy Risk";
            menu_incio.ResumeLayout(false);
            menu_incio.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Button boton_salir;
        private Button boton_invitado;
        private Button boton_host;
        private Label label1;
        private Panel menu_incio;
    }
}
