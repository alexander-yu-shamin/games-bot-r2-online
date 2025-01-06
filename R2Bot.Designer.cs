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
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(R2Bot));
            trayIcon = new NotifyIcon(components);
            label1 = new Label();
            WorkingLabel = new Label();
            textBox1 = new TextBox();
            label2 = new Label();
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
            label1.Size = new Size(253, 25);
            label1.Text = "Alt+F12 => Start/Stop";
            // 
            // WorkingLabel
            // 
            WorkingLabel.AutoSize = true;
            WorkingLabel.Font = new Font("Segoe UI", 14.25F);
            WorkingLabel.Location = new Point(368, 9);
            WorkingLabel.Name = "WorkingLabel";
            WorkingLabel.Size = new Size(140, 25);
            WorkingLabel.Text = "NOT WORKING";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(91, 54);
            textBox1.Name = "textBox1";
            textBox1.Text = ConfigPath;
            textBox1.Size = new Size(235, 23);
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 57);
            label2.Name = "label2";
            label2.Size = new Size(73, 15);
            label2.Text = "Config Path:";
            // 
            // R2Bot
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(526, 102);
            Controls.Add(label2);
            Controls.Add(textBox1);
            Controls.Add(WorkingLabel);
            Controls.Add(label1);
            Icon = (Icon)resources.GetObject("$this.Icon");
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
        private TextBox textBox1;
        private Label label2;
    }
}