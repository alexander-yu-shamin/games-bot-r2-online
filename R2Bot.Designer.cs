namespace R2Bot
{
    partial class R2Bot
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(R2Bot));
            trayIcon = new NotifyIcon(components);
            label1 = new Label();
            WorkingLabel = new Label();
            label2 = new Label();
            LogBox = new TextBox();
            comboBox1 = new ComboBox();
            comboBox2 = new ComboBox();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            label7 = new Label();
            groupBox1 = new GroupBox();
            comboBox3 = new ComboBox();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // trayIcon
            // 
            trayIcon.Icon = (Icon)resources.GetObject("trayIcon.Icon");
            trayIcon.Text = "R2Bot";
            trayIcon.Visible = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 14.25F);
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(195, 25);
            label1.TabIndex = 3;
            label1.Text = "Alt+F12 => Start/Stop";
            // 
            // WorkingLabel
            // 
            WorkingLabel.AutoSize = true;
            WorkingLabel.Font = new Font("Segoe UI", 14.25F);
            WorkingLabel.Location = new Point(368, 9);
            WorkingLabel.Name = "WorkingLabel";
            WorkingLabel.Size = new Size(140, 25);
            WorkingLabel.TabIndex = 2;
            WorkingLabel.Text = "NOT WORKING";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 57);
            label2.Name = "label2";
            label2.Size = new Size(73, 15);
            label2.TabIndex = 0;
            label2.Text = "Config Path:";
            // 
            // LogBox
            // 
            LogBox.Enabled = false;
            LogBox.Location = new Point(12, 134);
            LogBox.Margin = new Padding(3, 2, 3, 2);
            LogBox.Multiline = true;
            LogBox.Name = "LogBox";
            LogBox.Size = new Size(504, 103);
            LogBox.TabIndex = 4;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(97, 51);
            comboBox1.Margin = new Padding(3, 2, 3, 2);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(419, 23);
            comboBox1.TabIndex = 5;
            // 
            // comboBox2
            // 
            comboBox2.FormattingEnabled = true;
            comboBox2.Location = new Point(151, 93);
            comboBox2.Margin = new Padding(3, 2, 3, 2);
            comboBox2.Name = "comboBox2";
            comboBox2.Size = new Size(364, 23);
            comboBox2.TabIndex = 7;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(11, 99);
            label3.Name = "label3";
            label3.Size = new Size(0, 15);
            label3.TabIndex = 6;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(17, 95);
            label4.Name = "label4";
            label4.Size = new Size(107, 15);
            label4.TabIndex = 8;
            label4.Text = "Client Config Path:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label5.Location = new Point(311, 19);
            label5.Name = "label5";
            label5.Size = new Size(161, 15);
            label5.TabIndex = 9;
            label5.Text = "Alt+F9 => Setup client colors";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label6.Location = new Point(5, 29);
            label6.Name = "label6";
            label6.Size = new Size(124, 15);
            label6.TabIndex = 10;
            label6.Text = "Alt+F10 => Test Client";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label7.Location = new Point(5, 57);
            label7.Name = "label7";
            label7.Size = new Size(130, 15);
            label7.TabIndex = 11;
            label7.Text = "Alt+F11 => Save Image";
            label7.Click += label7_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(comboBox3);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(label6);
            groupBox1.Controls.Add(label7);
            groupBox1.Location = new Point(12, 254);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(504, 85);
            groupBox1.TabIndex = 12;
            groupBox1.TabStop = false;
            groupBox1.Text = "Setup client";
            // 
            // comboBox3
            // 
            comboBox3.FormattingEnabled = true;
            comboBox3.Location = new Point(294, 49);
            comboBox3.Margin = new Padding(3, 2, 3, 2);
            comboBox3.Name = "comboBox3";
            comboBox3.Size = new Size(178, 23);
            comboBox3.TabIndex = 13;
            // 
            // R2Bot
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(529, 345);
            Controls.Add(groupBox1);
            Controls.Add(label4);
            Controls.Add(comboBox2);
            Controls.Add(label3);
            Controls.Add(comboBox1);
            Controls.Add(LogBox);
            Controls.Add(label2);
            Controls.Add(WorkingLabel);
            Controls.Add(label1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "R2Bot";
            Text = "R2Bot";
            FormClosed += R2Bot_FormClosed;
            Load += R2Bot_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private NotifyIcon trayIcon;
        private Label label1;
        private Label WorkingLabel;
        private Label label2;
        private TextBox LogBox;
        private ComboBox comboBox1;
        private ComboBox comboBox2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;
        private GroupBox groupBox1;
        private ComboBox comboBox3;
    }
}