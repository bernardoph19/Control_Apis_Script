using System.Windows.Forms;

namespace CONTROL_APIS
{
    partial class Form1
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.minime = new System.Windows.Forms.Button();
            this.maxime = new System.Windows.Forms.Button();
            this.close = new System.Windows.Forms.Button();
            this.labelversion = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.listayaml = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel4.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(242, 135);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 16);
            this.label1.TabIndex = 0;
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(25)))), ((int)(((byte)(29)))));
            this.panel1.Controls.Add(this.minime);
            this.panel1.Controls.Add(this.maxime);
            this.panel1.Controls.Add(this.close);
            this.panel1.Controls.Add(this.labelversion);
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1134, 45);
            this.panel1.TabIndex = 8;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            this.panel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseDown);
            this.panel1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseMove);
            this.panel1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseUp);
            // 
            // minime
            // 
            this.minime.Dock = System.Windows.Forms.DockStyle.Right;
            this.minime.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.minime.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(25)))), ((int)(((byte)(29)))));
            this.minime.Image = global::CONTROL_APIS.Properties.Resources.mdi_minimize;
            this.minime.Location = new System.Drawing.Point(1008, 0);
            this.minime.Name = "minime";
            this.minime.Size = new System.Drawing.Size(42, 45);
            this.minime.TabIndex = 4;
            this.minime.UseVisualStyleBackColor = true;
            this.minime.Click += new System.EventHandler(this.minime_Click);
            // 
            // maxime
            // 
            this.maxime.Dock = System.Windows.Forms.DockStyle.Right;
            this.maxime.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.maxime.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(25)))), ((int)(((byte)(29)))));
            this.maxime.Image = global::CONTROL_APIS.Properties.Resources.fluent_maximize_28_regular;
            this.maxime.Location = new System.Drawing.Point(1050, 0);
            this.maxime.Name = "maxime";
            this.maxime.Size = new System.Drawing.Size(42, 45);
            this.maxime.TabIndex = 3;
            this.maxime.UseVisualStyleBackColor = true;
            this.maxime.Click += new System.EventHandler(this.maxime_Click);
            // 
            // close
            // 
            this.close.Dock = System.Windows.Forms.DockStyle.Right;
            this.close.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.close.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(25)))), ((int)(((byte)(29)))));
            this.close.Image = global::CONTROL_APIS.Properties.Resources.close;
            this.close.Location = new System.Drawing.Point(1092, 0);
            this.close.Name = "close";
            this.close.Size = new System.Drawing.Size(42, 45);
            this.close.TabIndex = 2;
            this.close.UseVisualStyleBackColor = true;
            this.close.Click += new System.EventHandler(this.close_Click);
            // 
            // labelversion
            // 
            this.labelversion.AutoSize = true;
            this.labelversion.Font = new System.Drawing.Font("Segoe UI Black", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelversion.ForeColor = System.Drawing.Color.White;
            this.labelversion.Location = new System.Drawing.Point(50, 16);
            this.labelversion.Name = "labelversion";
            this.labelversion.Size = new System.Drawing.Size(229, 15);
            this.labelversion.TabIndex = 1;
            this.labelversion.Text = "Control de APIS - INNOVATED 1.0.0.0";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::CONTROL_APIS.Properties.Resources.logo;
            this.pictureBox1.Location = new System.Drawing.Point(11, 8);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(32, 31);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.listayaml);
            this.panel4.Controls.Add(this.label4);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(0, 45);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(1134, 60);
            this.panel4.TabIndex = 11;
            // 
            // listayaml
            // 
            this.listayaml.BackColor = System.Drawing.Color.White;
            this.listayaml.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.listayaml.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listayaml.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.listayaml.Location = new System.Drawing.Point(950, 13);
            this.listayaml.Name = "listayaml";
            this.listayaml.Size = new System.Drawing.Size(172, 41);
            this.listayaml.TabIndex = 6;
            this.listayaml.Text = "Editar Lista";
            this.listayaml.UseVisualStyleBackColor = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Black;
            this.label4.Location = new System.Drawing.Point(8, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(107, 17);
            this.label4.TabIndex = 5;
            this.label4.Text = "Listado de Apis ";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.tabControl1);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 105);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(1134, 565);
            this.panel3.TabIndex = 12;
            // 
            // tabControl1
            // 
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1134, 565);
            this.tabControl1.TabIndex = 8;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1134, 670);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Control de APIS - INNOVATED 1.0.1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private Panel panel1;
        private PictureBox pictureBox1;
        private Label labelversion;
        private Button close;
        private Button minime;
        private Button maxime;
        private Panel panel4;
        private Label label4;
        private Panel panel3;
        private TabControl tabControl1;
        private Button listayaml;
    }
}

