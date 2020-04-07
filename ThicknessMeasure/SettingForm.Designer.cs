namespace ThicknessMeasure {
	partial class SettingForm {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing ) {
			if ( disposing && (components != null) ) {
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.gbPass = new System.Windows.Forms.GroupBox();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.tbCorr = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.cbTypeName = new System.Windows.Forms.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.lLv0 = new System.Windows.Forms.Label();
			this.lLv1 = new System.Windows.Forms.Label();
			this.lLv2 = new System.Windows.Forms.Label();
			this.lLv3 = new System.Windows.Forms.Label();
			this.tbThreshLv0 = new System.Windows.Forms.TextBox();
			this.tbThreshLv1 = new System.Windows.Forms.TextBox();
			this.tbThreshLv2 = new System.Windows.Forms.TextBox();
			this.tbThreshLv3 = new System.Windows.Forms.TextBox();
			this.tbThreshColorLv0 = new System.Windows.Forms.TextBox();
			this.tbThreshColorLv1 = new System.Windows.Forms.TextBox();
			this.tbThreshColorLv2 = new System.Windows.Forms.TextBox();
			this.tbThreshColorLv3 = new System.Windows.Forms.TextBox();
			this.tbThreshColorLv4 = new System.Windows.Forms.TextBox();
			this.bOk = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.gbPass.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// gbPass
			// 
			this.gbPass.Controls.Add(this.textBox1);
			this.gbPass.Location = new System.Drawing.Point(12, 12);
			this.gbPass.Name = "gbPass";
			this.gbPass.Size = new System.Drawing.Size(143, 42);
			this.gbPass.TabIndex = 0;
			this.gbPass.TabStop = false;
			this.gbPass.Text = "Password";
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(6, 18);
			this.textBox1.Name = "textBox1";
			this.textBox1.PasswordChar = '*';
			this.textBox1.Size = new System.Drawing.Size(130, 19);
			this.textBox1.TabIndex = 1;
			this.textBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.button1);
			this.panel1.Controls.Add(this.tbCorr);
			this.panel1.Controls.Add(this.label7);
			this.panel1.Controls.Add(this.label6);
			this.panel1.Controls.Add(this.cbTypeName);
			this.panel1.Controls.Add(this.label5);
			this.panel1.Controls.Add(this.lLv0);
			this.panel1.Controls.Add(this.lLv1);
			this.panel1.Controls.Add(this.lLv2);
			this.panel1.Controls.Add(this.lLv3);
			this.panel1.Controls.Add(this.tbThreshLv0);
			this.panel1.Controls.Add(this.tbThreshLv1);
			this.panel1.Controls.Add(this.tbThreshLv2);
			this.panel1.Controls.Add(this.tbThreshLv3);
			this.panel1.Controls.Add(this.tbThreshColorLv0);
			this.panel1.Controls.Add(this.tbThreshColorLv1);
			this.panel1.Controls.Add(this.tbThreshColorLv2);
			this.panel1.Controls.Add(this.tbThreshColorLv3);
			this.panel1.Controls.Add(this.tbThreshColorLv4);
			this.panel1.Controls.Add(this.bOk);
			this.panel1.Enabled = false;
			this.panel1.Location = new System.Drawing.Point(12, 60);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(252, 378);
			this.panel1.TabIndex = 1;
			this.panel1.Visible = false;
			// 
			// tbCorr
			// 
			this.tbCorr.BackColor = System.Drawing.Color.White;
			this.tbCorr.Location = new System.Drawing.Point(128, 239);
			this.tbCorr.Name = "tbCorr";
			this.tbCorr.Size = new System.Drawing.Size(69, 19);
			this.tbCorr.TabIndex = 20;
			this.tbCorr.Leave += new System.EventHandler(this.tbCorr_Leave);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Font = new System.Drawing.Font("MS UI Gothic", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.label7.Location = new System.Drawing.Point(20, 239);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(85, 19);
			this.label7.TabIndex = 19;
			this.label7.Text = "係数設定";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Font = new System.Drawing.Font("MS UI Gothic", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.label6.Location = new System.Drawing.Point(20, 3);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(123, 19);
			this.label6.TabIndex = 18;
			this.label6.Text = "母材種類選択";
			// 
			// cbTypeName
			// 
			this.cbTypeName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbTypeName.FormattingEnabled = true;
			this.cbTypeName.Location = new System.Drawing.Point(34, 25);
			this.cbTypeName.Name = "cbTypeName";
			this.cbTypeName.Size = new System.Drawing.Size(173, 20);
			this.cbTypeName.TabIndex = 17;
			this.cbTypeName.SelectedIndexChanged += new System.EventHandler(this.cbTypeName_SelectedIndexChanged);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("MS UI Gothic", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.label5.Location = new System.Drawing.Point(54, 68);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(110, 19);
			this.label5.TabIndex = 16;
			this.label5.Text = "しきい値設定";
			// 
			// lLv0
			// 
			this.lLv0.AutoSize = true;
			this.lLv0.Font = new System.Drawing.Font("MS UI Gothic", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.lLv0.Location = new System.Drawing.Point(93, 174);
			this.lLv0.Name = "lLv0";
			this.lLv0.Size = new System.Drawing.Size(34, 24);
			this.lLv0.TabIndex = 15;
			this.lLv0.Text = "＞";
			// 
			// lLv1
			// 
			this.lLv1.AutoSize = true;
			this.lLv1.Font = new System.Drawing.Font("MS UI Gothic", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.lLv1.Location = new System.Drawing.Point(93, 149);
			this.lLv1.Name = "lLv1";
			this.lLv1.Size = new System.Drawing.Size(34, 24);
			this.lLv1.TabIndex = 14;
			this.lLv1.Text = "＞";
			// 
			// lLv2
			// 
			this.lLv2.AutoSize = true;
			this.lLv2.Font = new System.Drawing.Font("MS UI Gothic", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.lLv2.Location = new System.Drawing.Point(93, 125);
			this.lLv2.Name = "lLv2";
			this.lLv2.Size = new System.Drawing.Size(34, 24);
			this.lLv2.TabIndex = 13;
			this.lLv2.Text = "＞";
			// 
			// lLv3
			// 
			this.lLv3.AutoSize = true;
			this.lLv3.Font = new System.Drawing.Font("MS UI Gothic", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.lLv3.Location = new System.Drawing.Point(93, 99);
			this.lLv3.Name = "lLv3";
			this.lLv3.Size = new System.Drawing.Size(34, 24);
			this.lLv3.TabIndex = 12;
			this.lLv3.Text = "＞";
			// 
			// tbThreshLv0
			// 
			this.tbThreshLv0.BackColor = System.Drawing.Color.White;
			this.tbThreshLv0.Location = new System.Drawing.Point(128, 178);
			this.tbThreshLv0.Name = "tbThreshLv0";
			this.tbThreshLv0.Size = new System.Drawing.Size(69, 19);
			this.tbThreshLv0.TabIndex = 11;
			this.tbThreshLv0.Tag = "0";
			// 
			// tbThreshLv1
			// 
			this.tbThreshLv1.BackColor = System.Drawing.Color.White;
			this.tbThreshLv1.Location = new System.Drawing.Point(128, 153);
			this.tbThreshLv1.Name = "tbThreshLv1";
			this.tbThreshLv1.Size = new System.Drawing.Size(69, 19);
			this.tbThreshLv1.TabIndex = 10;
			this.tbThreshLv1.Tag = "1";
			// 
			// tbThreshLv2
			// 
			this.tbThreshLv2.BackColor = System.Drawing.Color.White;
			this.tbThreshLv2.Location = new System.Drawing.Point(128, 128);
			this.tbThreshLv2.Name = "tbThreshLv2";
			this.tbThreshLv2.Size = new System.Drawing.Size(69, 19);
			this.tbThreshLv2.TabIndex = 9;
			this.tbThreshLv2.Tag = "2";
			// 
			// tbThreshLv3
			// 
			this.tbThreshLv3.BackColor = System.Drawing.Color.White;
			this.tbThreshLv3.Location = new System.Drawing.Point(128, 102);
			this.tbThreshLv3.Name = "tbThreshLv3";
			this.tbThreshLv3.Size = new System.Drawing.Size(69, 19);
			this.tbThreshLv3.TabIndex = 8;
			this.tbThreshLv3.Tag = "3";
			// 
			// tbThreshColorLv0
			// 
			this.tbThreshColorLv0.BackColor = System.Drawing.Color.Red;
			this.tbThreshColorLv0.Location = new System.Drawing.Point(24, 190);
			this.tbThreshColorLv0.Name = "tbThreshColorLv0";
			this.tbThreshColorLv0.ReadOnly = true;
			this.tbThreshColorLv0.Size = new System.Drawing.Size(69, 19);
			this.tbThreshColorLv0.TabIndex = 7;
			// 
			// tbThreshColorLv1
			// 
			this.tbThreshColorLv1.BackColor = System.Drawing.SystemColors.Control;
			this.tbThreshColorLv1.Location = new System.Drawing.Point(24, 165);
			this.tbThreshColorLv1.Name = "tbThreshColorLv1";
			this.tbThreshColorLv1.ReadOnly = true;
			this.tbThreshColorLv1.Size = new System.Drawing.Size(69, 19);
			this.tbThreshColorLv1.TabIndex = 6;
			// 
			// tbThreshColorLv2
			// 
			this.tbThreshColorLv2.BackColor = System.Drawing.Color.Yellow;
			this.tbThreshColorLv2.Location = new System.Drawing.Point(24, 140);
			this.tbThreshColorLv2.Name = "tbThreshColorLv2";
			this.tbThreshColorLv2.ReadOnly = true;
			this.tbThreshColorLv2.Size = new System.Drawing.Size(69, 19);
			this.tbThreshColorLv2.TabIndex = 5;
			// 
			// tbThreshColorLv3
			// 
			this.tbThreshColorLv3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(102)))), ((int)(((byte)(255)))));
			this.tbThreshColorLv3.Location = new System.Drawing.Point(24, 115);
			this.tbThreshColorLv3.Name = "tbThreshColorLv3";
			this.tbThreshColorLv3.ReadOnly = true;
			this.tbThreshColorLv3.Size = new System.Drawing.Size(69, 19);
			this.tbThreshColorLv3.TabIndex = 4;
			// 
			// tbThreshColorLv4
			// 
			this.tbThreshColorLv4.BackColor = System.Drawing.Color.Red;
			this.tbThreshColorLv4.Location = new System.Drawing.Point(24, 90);
			this.tbThreshColorLv4.Name = "tbThreshColorLv4";
			this.tbThreshColorLv4.ReadOnly = true;
			this.tbThreshColorLv4.Size = new System.Drawing.Size(69, 19);
			this.tbThreshColorLv4.TabIndex = 3;
			// 
			// bOk
			// 
			this.bOk.Location = new System.Drawing.Point(128, 289);
			this.bOk.Name = "bOk";
			this.bOk.Size = new System.Drawing.Size(86, 25);
			this.bOk.TabIndex = 2;
			this.bOk.Text = "保存";
			this.bOk.UseVisualStyleBackColor = true;
			this.bOk.Click += new System.EventHandler(this.bOk_Click);
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(19, 289);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(86, 25);
			this.button1.TabIndex = 21;
			this.button1.Text = "取消";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// SettingForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(276, 384);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.gbPass);
			this.Name = "SettingForm";
			this.Text = "Setting";
			this.Load += new System.EventHandler(this.SettingForm_Load);
			this.gbPass.ResumeLayout(false);
			this.gbPass.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox gbPass;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button bOk;
		private System.Windows.Forms.TextBox tbThreshLv0;
		private System.Windows.Forms.TextBox tbThreshLv1;
		private System.Windows.Forms.TextBox tbThreshLv2;
		private System.Windows.Forms.TextBox tbThreshLv3;
		private System.Windows.Forms.TextBox tbThreshColorLv0;
		private System.Windows.Forms.TextBox tbThreshColorLv1;
		private System.Windows.Forms.TextBox tbThreshColorLv2;
		private System.Windows.Forms.TextBox tbThreshColorLv3;
		private System.Windows.Forms.TextBox tbThreshColorLv4;
		private System.Windows.Forms.TextBox tbCorr;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.ComboBox cbTypeName;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label lLv0;
		private System.Windows.Forms.Label lLv1;
		private System.Windows.Forms.Label lLv2;
		private System.Windows.Forms.Label lLv3;
		private System.Windows.Forms.Button button1;
	}
}