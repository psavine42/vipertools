namespace Viper.Forms
{
    partial class BlockMapForm
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
            this.BoxCategory = new System.Windows.Forms.ComboBox();
            this.BoxFamily = new System.Windows.Forms.ComboBox();
            this.makeelements = new System.Windows.Forms.Button();
            this.cancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.BoxBaseLevel = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.BoxHostedBy = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.BoxAboveBelow = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // BoxCategory
            // 
            this.BoxCategory.FormattingEnabled = true;
            this.BoxCategory.Location = new System.Drawing.Point(43, 74);
            this.BoxCategory.Name = "BoxCategory";
            this.BoxCategory.Size = new System.Drawing.Size(224, 21);
            this.BoxCategory.TabIndex = 0;
            this.BoxCategory.TextChanged += new System.EventHandler(this.comboBox1_TextChanged);
            // 
            // BoxFamily
            // 
            this.BoxFamily.FormattingEnabled = true;
            this.BoxFamily.Location = new System.Drawing.Point(43, 123);
            this.BoxFamily.Name = "BoxFamily";
            this.BoxFamily.Size = new System.Drawing.Size(224, 21);
            this.BoxFamily.TabIndex = 1;
            this.BoxFamily.TextChanged += 
                new System.EventHandler(this.comboBox2_TextChanged);
            // 
            // makeelements
            // 
            this.makeelements.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.makeelements.Location = new System.Drawing.Point(39, 401);
            this.makeelements.Name = "makeelements";
            this.makeelements.Size = new System.Drawing.Size(109, 27);
            this.makeelements.TabIndex = 2;
            this.makeelements.Text = "Ok";
            this.makeelements.UseVisualStyleBackColor = true;
            // 
            // cancel
            // 
            this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancel.Location = new System.Drawing.Point(184, 401);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(109, 27);
            this.cancel.TabIndex = 3;
            this.cancel.Text = "Cancel";
            this.cancel.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(40, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Category";
          
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(38, 107);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Family";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(40, 192);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Base Level";
            // 
            // BoxBaseLevel
            // 
            this.BoxBaseLevel.FormattingEnabled = true;
            this.BoxBaseLevel.Location = new System.Drawing.Point(45, 208);
            this.BoxBaseLevel.Name = "BoxBaseLevel";
            this.BoxBaseLevel.Size = new System.Drawing.Size(224, 21);
            this.BoxBaseLevel.TabIndex = 6;
            this.BoxBaseLevel.TextChanged += 
                new System.EventHandler(this.BoxBaseLevel_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(40, 260);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(99, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Hosted By Nearest:";
            // 
            // BoxHostedBy
            // 
            this.BoxHostedBy.FormattingEnabled = true;
            this.BoxHostedBy.Location = new System.Drawing.Point(141, 257);
            this.BoxHostedBy.Name = "BoxHostedBy";
            this.BoxHostedBy.Size = new System.Drawing.Size(128, 21);
            this.BoxHostedBy.TabIndex = 10;
            this.BoxHostedBy.TextChanged +=
                new System.EventHandler(this.BoxHostedBy_TextChanged);

            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(40, 297);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(72, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "Above/Below";
            // 
            // BoxAboveBelow
            // 
            this.BoxAboveBelow.FormattingEnabled = true;
            this.BoxAboveBelow.Location = new System.Drawing.Point(141, 294);
            this.BoxAboveBelow.Name = "BoxAboveBelow";
            this.BoxAboveBelow.Size = new System.Drawing.Size(128, 21);
            this.BoxAboveBelow.TabIndex = 12;
            this.BoxAboveBelow.TextChanged +=
                new System.EventHandler(this.BoxAboveBelow_TextChanged);
            // 
            // BlockMapForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(346, 453);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.BoxAboveBelow);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.BoxHostedBy);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.BoxBaseLevel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.makeelements);
            this.Controls.Add(this.BoxFamily);
            this.Controls.Add(this.BoxCategory);
            this.Name = "BlockMapForm";
            this.Text = "BlockMapForm";
            this.Load += new System.EventHandler(this.BlockMapForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox BoxCategory;
        private System.Windows.Forms.ComboBox BoxFamily;
        private System.Windows.Forms.Button makeelements;
        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox BoxBaseLevel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox BoxHostedBy;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox BoxAboveBelow;
    }
}