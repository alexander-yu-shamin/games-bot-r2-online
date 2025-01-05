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
            label1.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(253, 25);
            label1.TabIndex = 0;
            label1.Text = "Ctrl+Shift+F12 => Start/Stop";
            // 
            // WorkingLabel
            // 
            WorkingLabel.AutoSize = true;
            WorkingLabel.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            WorkingLabel.Location = new Point(510, 9);
            WorkingLabel.Name = "WorkingLabel";
            WorkingLabel.Size = new Size(140, 25);
            WorkingLabel.TabIndex = 1;
            WorkingLabel.Text = "NOT WORKING";
            // 
            // R2Bot
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(741, 753);
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
    }
}