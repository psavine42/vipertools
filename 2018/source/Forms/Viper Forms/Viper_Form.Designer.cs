
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Viper.Forms
{
    partial class Viper_Form
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
            this.heightBox = new System.Windows.Forms.TextBox();
            this.typeSelection = new System.Windows.Forms.ComboBox();
            this.Commit = new System.Windows.Forms.Button();
            this.diameterBox = new System.Windows.Forms.TextBox();
            this.diameterLabel = new System.Windows.Forms.Label();
            this.typeSelectionLabel = new System.Windows.Forms.Label();
            this.heightLabel = new System.Windows.Forms.Label();
            this.straigthThreshLabel = new System.Windows.Forms.Label();
            this.straightThresh = new System.Windows.Forms.TextBox();
            this.elbowLabel = new System.Windows.Forms.Label();
            this.elbowThreshold = new System.Windows.Forms.TextBox();
            this.systemSelectionLabel = new System.Windows.Forms.Label();
            this.systemSelection = new System.Windows.Forms.ComboBox();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.SelectLayers = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // heightBox
            // 
            this.heightBox.Location = new System.Drawing.Point(25, 70);
            this.heightBox.Name = "heightBox";
            this.heightBox.Size = new System.Drawing.Size(144, 20);
            this.heightBox.TabIndex = 0;
            this.heightBox.Text = "10";
            this.heightBox.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // typeSelection
            // 
            this.typeSelection.FormattingEnabled = true;
            this.typeSelection.Location = new System.Drawing.Point(25, 184);
            this.typeSelection.Name = "typeSelection";
            this.typeSelection.Size = new System.Drawing.Size(144, 21);
            this.typeSelection.TabIndex = 1;
            this.typeSelection.SelectedIndexChanged += new System.EventHandler(this.typeSelected);
            // 
            // Commit
            // 
            this.Commit.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Commit.Location = new System.Drawing.Point(37, 359);
            this.Commit.Name = "Commit";
            this.Commit.Size = new System.Drawing.Size(75, 23);
            this.Commit.TabIndex = 2;
            this.Commit.Text = "Make Pipes";
            this.Commit.UseVisualStyleBackColor = true;
            // 
            // diameterBox
            // 
            this.diameterBox.Location = new System.Drawing.Point(25, 125);
            this.diameterBox.Name = "diameterBox";
            this.diameterBox.Size = new System.Drawing.Size(144, 20);
            this.diameterBox.TabIndex = 3;
            this.diameterBox.Text = "1";
            this.diameterBox.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // diameterLabel
            // 
            this.diameterLabel.AutoSize = true;
            this.diameterLabel.Location = new System.Drawing.Point(22, 105);
            this.diameterLabel.Name = "diameterLabel";
            this.diameterLabel.Size = new System.Drawing.Size(126, 13);
            this.diameterLabel.TabIndex = 4;
            this.diameterLabel.Text = "Default Diameter (inches)";
            // 
            // typeSelectionLabel
            // 
            this.typeSelectionLabel.AutoSize = true;
            this.typeSelectionLabel.Location = new System.Drawing.Point(22, 168);
            this.typeSelectionLabel.Name = "typeSelectionLabel";
            this.typeSelectionLabel.Size = new System.Drawing.Size(55, 13);
            this.typeSelectionLabel.TabIndex = 5;
            this.typeSelectionLabel.Text = "Pipe Type";
            // 
            // heightLabel
            // 
            this.heightLabel.AutoSize = true;
            this.heightLabel.Location = new System.Drawing.Point(22, 50);
            this.heightLabel.Name = "heightLabel";
            this.heightLabel.Size = new System.Drawing.Size(102, 13);
            this.heightLabel.TabIndex = 6;
            this.heightLabel.Text = "Default Height (feet)";
            // 
            // straigthThreshLabel
            // 
            this.straigthThreshLabel.AutoSize = true;
            this.straigthThreshLabel.Location = new System.Drawing.Point(196, 105);
            this.straigthThreshLabel.Name = "straigthThreshLabel";
            this.straigthThreshLabel.Size = new System.Drawing.Size(147, 13);
            this.straigthThreshLabel.TabIndex = 8;
            this.straigthThreshLabel.Text = "Threshold straight connection";
            // 
            // straightThresh
            // 
            this.straightThresh.Location = new System.Drawing.Point(199, 125);
            this.straightThresh.Name = "straightThresh";
            this.straightThresh.Size = new System.Drawing.Size(144, 20);
            this.straightThresh.TabIndex = 7;
            this.straightThresh.Text = "2";
            this.straightThresh.TextChanged += new System.EventHandler(this.textBox3_TextChanged);
            // 
            // elbowLabel
            // 
            this.elbowLabel.AutoSize = true;
            this.elbowLabel.Location = new System.Drawing.Point(196, 54);
            this.elbowLabel.Name = "elbowLabel";
            this.elbowLabel.Size = new System.Drawing.Size(86, 13);
            this.elbowLabel.TabIndex = 10;
            this.elbowLabel.Text = "Threshold Elbow";
            // 
            // elbowThreshold
            // 
            this.elbowThreshold.Location = new System.Drawing.Point(199, 70);
            this.elbowThreshold.Name = "elbowThreshold";
            this.elbowThreshold.Size = new System.Drawing.Size(144, 20);
            this.elbowThreshold.TabIndex = 9;
            this.elbowThreshold.Text = "0.5";
            this.elbowThreshold.TextChanged += new System.EventHandler(this.systemSelected);
            // 
            // systemSelectionLabel
            // 
            this.systemSelectionLabel.AutoSize = true;
            this.systemSelectionLabel.Location = new System.Drawing.Point(20, 220);
            this.systemSelectionLabel.Name = "systemSelectionLabel";
            this.systemSelectionLabel.Size = new System.Drawing.Size(92, 13);
            this.systemSelectionLabel.TabIndex = 11;
            this.systemSelectionLabel.Text = "Pipe System Type";
            // 
            // systemSelection
            // 
            this.systemSelection.FormattingEnabled = true;
            this.systemSelection.Location = new System.Drawing.Point(23, 236);
            this.systemSelection.Name = "systemSelection";
            this.systemSelection.Size = new System.Drawing.Size(320, 21);
            this.systemSelection.TabIndex = 12;
            // 
            // textBox5
            // 
            this.textBox5.Location = new System.Drawing.Point(199, 184);
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(144, 20);
            this.textBox5.TabIndex = 13;
            this.textBox5.Text = "2";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(196, 168);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(55, 13);
            this.label7.TabIndex = 14;
            this.label7.Text = "Pipe Type";
            // 
            // SelectLayers
            // 
            this.SelectLayers.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.SelectLayers.Location = new System.Drawing.Point(268, 359);
            this.SelectLayers.Name = "SelectLayers";
            this.SelectLayers.Size = new System.Drawing.Size(75, 23);
            this.SelectLayers.TabIndex = 15;
            this.SelectLayers.Text = "Pick Layers";
            this.SelectLayers.UseVisualStyleBackColor = true;
            // 
            // Viper_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(399, 409);
            this.Controls.Add(this.SelectLayers);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.textBox5);
            this.Controls.Add(this.systemSelection);
            this.Controls.Add(this.systemSelectionLabel);
            this.Controls.Add(this.elbowLabel);
            this.Controls.Add(this.elbowThreshold);
            this.Controls.Add(this.straigthThreshLabel);
            this.Controls.Add(this.straightThresh);
            this.Controls.Add(this.heightLabel);
            this.Controls.Add(this.typeSelectionLabel);
            this.Controls.Add(this.diameterLabel);
            this.Controls.Add(this.diameterBox);
            this.Controls.Add(this.Commit);
            this.Controls.Add(this.typeSelection);
            this.Controls.Add(this.heightBox);
            this.Name = "Viper_Form";
            this.Text = "Viper Pipe Maker";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox heightBox;
        private System.Windows.Forms.ComboBox typeSelection;
        private System.Windows.Forms.Button Commit;
        private System.Windows.Forms.TextBox diameterBox;
        private System.Windows.Forms.Label diameterLabel;
        private System.Windows.Forms.Label typeSelectionLabel;
        private System.Windows.Forms.Label heightLabel;
        private System.Windows.Forms.Label straigthThreshLabel;
        private System.Windows.Forms.TextBox straightThresh;
        private System.Windows.Forms.Label elbowLabel;
        private System.Windows.Forms.TextBox elbowThreshold;
        private System.Windows.Forms.Label systemSelectionLabel;
        private System.Windows.Forms.ComboBox systemSelection;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button SelectLayers;
    }
}