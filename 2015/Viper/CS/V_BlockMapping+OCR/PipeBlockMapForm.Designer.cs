namespace Revit.SDK.Samples.UIAPI.CS
{
    partial class PipeBlockMapForm
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
            this.Pipetype = new System.Windows.Forms.ComboBox();
            this.TopLevel = new System.Windows.Forms.ComboBox();
            this.BottomLevel = new System.Windows.Forms.ComboBox();
            this.TopOffset = new System.Windows.Forms.TextBox();
            this.BottomOffset = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.okbutton = new System.Windows.Forms.Button();
            this.cancelbutton = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // Pipetype
            // 
            this.Pipetype.FormattingEnabled = true;
            this.Pipetype.Location = new System.Drawing.Point(28, 45);
            this.Pipetype.Name = "Pipetype";
            this.Pipetype.Size = new System.Drawing.Size(165, 21);
            this.Pipetype.TabIndex = 0;
            this.Pipetype.TextChanged += new System.EventHandler(this.Pipetype_TextChanged);
            // 
            // TopLevel
            // 
            this.TopLevel.FormattingEnabled = true;
            this.TopLevel.Location = new System.Drawing.Point(27, 155);
            this.TopLevel.Name = "TopLevel";
            this.TopLevel.Size = new System.Drawing.Size(166, 21);
            this.TopLevel.TabIndex = 1;
            this.TopLevel.TextChanged += new System.EventHandler(this.TopLevel_TextChanged);
            // 
            // BottomLevel
            // 
            this.BottomLevel.FormattingEnabled = true;
            this.BottomLevel.Location = new System.Drawing.Point(27, 265);
            this.BottomLevel.Name = "BottomLevel";
            this.BottomLevel.Size = new System.Drawing.Size(166, 21);
            this.BottomLevel.TabIndex = 2;
            this.BottomLevel.TextChanged += new System.EventHandler(this.BottomLevel_TextChanged);
            // 
            // TopOffset
            // 
            this.TopOffset.Location = new System.Drawing.Point(28, 204);
            this.TopOffset.Name = "TopOffset";
            this.TopOffset.Size = new System.Drawing.Size(165, 20);
            this.TopOffset.TabIndex = 3;
            this.TopOffset.TextChanged += new System.EventHandler(this.TopOffset_TextChanged);
            // 
            // BottomOffset
            // 
            this.BottomOffset.Location = new System.Drawing.Point(26, 310);
            this.BottomOffset.Name = "BottomOffset";
            this.BottomOffset.Size = new System.Drawing.Size(167, 20);
            this.BottomOffset.TabIndex = 4;
            this.BottomOffset.TextChanged += new System.EventHandler(this.BottomOffset_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(25, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "PipeType";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 139);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Top Level";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(25, 188);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(86, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Top Level Offset";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(22, 249);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(69, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Bottom Level";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(25, 294);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(100, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Bottom Level Offset";
            // 
            // okbutton
            // 
            this.okbutton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okbutton.Location = new System.Drawing.Point(31, 386);
            this.okbutton.Name = "okbutton";
            this.okbutton.Size = new System.Drawing.Size(79, 23);
            this.okbutton.TabIndex = 11;
            this.okbutton.Text = "OK";
            this.okbutton.UseVisualStyleBackColor = true;
            // 
            // cancelbutton
            // 
            this.cancelbutton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelbutton.Location = new System.Drawing.Point(146, 386);
            this.cancelbutton.Name = "cancelbutton";
            this.cancelbutton.Size = new System.Drawing.Size(79, 23);
            this.cancelbutton.TabIndex = 12;
            this.cancelbutton.Text = "Cancel";
            this.cancelbutton.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(27, 69);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(68, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "Pipediameter";
            this.label6.Click += new System.EventHandler(this.label6_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(28, 85);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(167, 20);
            this.textBox1.TabIndex = 13;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // PipeBlockMapForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 431);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.cancelbutton);
            this.Controls.Add(this.okbutton);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.BottomOffset);
            this.Controls.Add(this.TopOffset);
            this.Controls.Add(this.BottomLevel);
            this.Controls.Add(this.TopLevel);
            this.Controls.Add(this.Pipetype);
            this.Name = "PipeBlockMapForm";
            this.Text = "PipeBlockMapForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox Pipetype;
        private System.Windows.Forms.ComboBox TopLevel;
        private System.Windows.Forms.ComboBox BottomLevel;
        private System.Windows.Forms.TextBox TopOffset;
        private System.Windows.Forms.TextBox BottomOffset;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button okbutton;
        private System.Windows.Forms.Button cancelbutton;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox1;
    }
}