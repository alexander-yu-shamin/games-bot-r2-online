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
            label1.Location = new Point(14, 12);
            label1.Name = "label1";
            label1.Size = new Size(248, 32);
            label1.TabIndex = 3;
            label1.Text = "Alt+F12 => Start/Stop";
            // 
            // WorkingLabel
            // 
            WorkingLabel.AutoSize = true;
            WorkingLabel.Font = new Font("Segoe UI", 14.25F);
            WorkingLabel.Location = new Point(421, 12);
            WorkingLabel.Name = "WorkingLabel";
            WorkingLabel.Size = new Size(177, 32);
            WorkingLabel.TabIndex = 2;
            WorkingLabel.Text = "NOT WORKING";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(14, 76);
            label2.Name = "label2";
            label2.Size = new Size(88, 20);
            label2.TabIndex = 0;
            label2.Text = "Config Path:";
            // 
            // LogBox
            // 
            LogBox.Enabled = false;
            LogBox.Location = new Point(14, 178);
            LogBox.Multiline = true;
            LogBox.Name = "LogBox";
            LogBox.Size = new Size(575, 136);
            LogBox.TabIndex = 4;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(111, 68);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(478, 28);
            comboBox1.TabIndex = 5;
            // 
            // comboBox2
            // 
            comboBox2.FormattingEnabled = true;
            comboBox2.Location = new Point(173, 124);
            comboBox2.Name = "comboBox2";
            comboBox2.Size = new Size(415, 28);
            comboBox2.TabIndex = 7;
            comboBox2.SelectedIndexChanged += comboBox2_SelectedIndexChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(13, 132);
            label3.Name = "label3";
            label3.Size = new Size(0, 20);
            label3.TabIndex = 6;
            label3.Click += label3_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(19, 127);
            label4.Name = "label4";
            label4.Size = new Size(130, 20);
            label4.TabIndex = 8;
            label4.Text = "Client Config Path:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label5.Location = new Point(12, 317);
            label5.Name = "label5";
            label5.Size = new Size(145, 20);
            label5.TabIndex = 9;
            label5.Text = "Alt+F9 => Test Input";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label6.Location = new Point(220, 317);
            label6.Name = "label6";
            label6.Size = new Size(157, 20);
            label6.TabIndex = 10;
            label6.Text = "Alt+F10 => Test Client";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label7.Location = new Point(423, 317);
            label7.Name = "label7";
            label7.Size = new Size(166, 20);
            label7.TabIndex = 11;
            label7.Text = "Alt+F11 => Save Image";
            label7.Click += label7_Click;
            // 
            // R2Bot
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(601, 346);
            Controls.Add(label7);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(comboBox2);
            Controls.Add(label3);
            Controls.Add(comboBox1);
            Controls.Add(LogBox);
            Controls.Add(label2);
            Controls.Add(WorkingLabel);
            Controls.Add(label1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 4, 3, 4);
            Name = "R2Bot";
            Text = "R2Bot";
            FormClosed += R2Bot_FormClosed;
            Load += R2Bot_Load;
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
    }
}