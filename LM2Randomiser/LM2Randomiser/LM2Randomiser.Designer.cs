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
            this.GenerateButton = new System.Windows.Forms.Button();
            this.GrailCheck = new System.Windows.Forms.CheckBox();
            this.ScannerCheck = new System.Windows.Forms.CheckBox();
            this.OutputText = new System.Windows.Forms.TextBox();
            this.SeedLabel = new System.Windows.Forms.Label();
            this.SeedInput = new System.Windows.Forms.TextBox();
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
            // GrailCheck
            // 
            this.GrailCheck.AutoSize = true;
            this.GrailCheck.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GrailCheck.Location = new System.Drawing.Point(12, 8);
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
            this.ScannerCheck.Location = new System.Drawing.Point(12, 38);
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
            this.OutputText.Location = new System.Drawing.Point(12, 150);
            this.OutputText.Multiline = true;
            this.OutputText.Name = "OutputText";
            this.OutputText.ReadOnly = true;
            this.OutputText.Size = new System.Drawing.Size(261, 125);
            this.OutputText.TabIndex = 4;
            // 
            // SeedLabel
            // 
            this.SeedLabel.AutoSize = true;
            this.SeedLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SeedLabel.Location = new System.Drawing.Point(9, 62);
            this.SeedLabel.Name = "SeedLabel";
            this.SeedLabel.Size = new System.Drawing.Size(41, 17);
            this.SeedLabel.TabIndex = 5;
            this.SeedLabel.Text = "Seed";
            // 
            // SeedInput
            // 
            this.SeedInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SeedInput.Location = new System.Drawing.Point(12, 82);
            this.SeedInput.Name = "SeedInput";
            this.SeedInput.Size = new System.Drawing.Size(261, 23);
            this.SeedInput.TabIndex = 6;
            // 
            // LM2Randomiser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(285, 285);
            this.Controls.Add(this.SeedInput);
            this.Controls.Add(this.SeedLabel);
            this.Controls.Add(this.OutputText);
            this.Controls.Add(this.ScannerCheck);
            this.Controls.Add(this.GrailCheck);
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
        private System.Windows.Forms.CheckBox GrailCheck;
        private System.Windows.Forms.CheckBox ScannerCheck;
        private System.Windows.Forms.TextBox OutputText;
        private System.Windows.Forms.Label SeedLabel;
        private System.Windows.Forms.TextBox SeedInput;
    }
}

