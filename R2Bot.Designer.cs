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
            LogBox.Location = new Point(14, 118);
            LogBox.Name = "LogBox";
            LogBox.Size = new Size(575, 27);
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
            // R2Bot
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(601, 166);
            Controls.Add(comboBox1);
            Controls.Add(LogBox);
            Controls.Add(label2);
            Controls.Add(WorkingLabel);
            Controls.Add(label1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 4, 3, 4);
            Name = "R2Bot";
            Text = "R2Bot";
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
    }
}