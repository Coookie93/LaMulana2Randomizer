namespace LM2Randomiser
{
    partial class LM2Randomiser
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.GenerateButton = new System.Windows.Forms.Button();
            this.ModButton = new System.Windows.Forms.Button();
            this.GrailCheck = new System.Windows.Forms.CheckBox();
            this.ScannerCheck = new System.Windows.Forms.CheckBox();
            this.ToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.OutputText = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // GenerateButton
            // 
            this.GenerateButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GenerateButton.Location = new System.Drawing.Point(12, 111);
            this.GenerateButton.Name = "GenerateButton";
            this.GenerateButton.Size = new System.Drawing.Size(261, 33);
            this.GenerateButton.TabIndex = 0;
            this.GenerateButton.Text = "Generate Seed";
            this.GenerateButton.UseVisualStyleBackColor = true;
            this.GenerateButton.Click += new System.EventHandler(this.GenerateButton_Click);
            // 
            // ModButton
            // 
            this.ModButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ModButton.Location = new System.Drawing.Point(12, 12);
            this.ModButton.Name = "ModButton";
            this.ModButton.Size = new System.Drawing.Size(261, 33);
            this.ModButton.TabIndex = 1;
            this.ModButton.Text = "Mod DLL";
            this.ModButton.UseVisualStyleBackColor = true;
            this.ModButton.Click += new System.EventHandler(this.ModButton_Click);
            this.ModButton.MouseHover += new System.EventHandler(this.ModButton_MouseHover);
            // 
            // GrailCheck
            // 
            this.GrailCheck.AutoSize = true;
            this.GrailCheck.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GrailCheck.Location = new System.Drawing.Point(12, 51);
            this.GrailCheck.Name = "GrailCheck";
            this.GrailCheck.Size = new System.Drawing.Size(164, 21);
            this.GrailCheck.TabIndex = 2;
            this.GrailCheck.Text = "Randomise Holy Grail";
            this.GrailCheck.UseVisualStyleBackColor = true;
            this.GrailCheck.CheckedChanged += new System.EventHandler(this.GrailCheck_CheckedChanged);
            // 
            // ScannerCheck
            // 
            this.ScannerCheck.AutoSize = true;
            this.ScannerCheck.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ScannerCheck.Location = new System.Drawing.Point(12, 81);
            this.ScannerCheck.Name = "ScannerCheck";
            this.ScannerCheck.Size = new System.Drawing.Size(193, 21);
            this.ScannerCheck.TabIndex = 3;
            this.ScannerCheck.Text = "Randomise Hand Scanner";
            this.ScannerCheck.UseVisualStyleBackColor = true;
            this.ScannerCheck.CheckedChanged += new System.EventHandler(this.ScannerCheck_CheckedChanged);
            // 
            // OutputText
            // 
            this.OutputText.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OutputText.Location = new System.Drawing.Point(12, 152);
            this.OutputText.Multiline = true;
            this.OutputText.Name = "OutputText";
            this.OutputText.ReadOnly = true;
            this.OutputText.Size = new System.Drawing.Size(260, 125);
            this.OutputText.TabIndex = 4;
            // 
            // LM2Randomiser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(285, 289);
            this.Controls.Add(this.OutputText);
            this.Controls.Add(this.ScannerCheck);
            this.Controls.Add(this.GrailCheck);
            this.Controls.Add(this.ModButton);
            this.Controls.Add(this.GenerateButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "LM2Randomiser";
            this.Text = "LM2Randomiser";
            this.Load += new System.EventHandler(this.LM2Randomiser_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button GenerateButton;
        private System.Windows.Forms.Button ModButton;
        private System.Windows.Forms.CheckBox GrailCheck;
        private System.Windows.Forms.CheckBox ScannerCheck;
        private System.Windows.Forms.ToolTip ToolTip;
        private System.Windows.Forms.TextBox OutputText;
    }
}

