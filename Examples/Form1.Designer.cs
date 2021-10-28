﻿
namespace Examples
{
    partial class Form1
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
            this.pnlPadding = new System.Windows.Forms.Panel();
            this.grpDpiSettings = new AcrylicUI.Controls.AcrylicGroupBox();
            this.txtAutoScaleFactor = new AcrylicUI.Controls.AcrylicTextBox();
            this.lblAutoScaleFactor = new System.Windows.Forms.Label();
            this.txtWinVer = new AcrylicUI.Controls.AcrylicTextBox();
            this.lblWinVer = new System.Windows.Forms.Label();
            this.txtDpi = new AcrylicUI.Controls.AcrylicTextBox();
            this.lblDpi = new System.Windows.Forms.Label();
            this.btnUpdateDpi = new AcrylicUI.Controls.AcrylicButton();
            this.pnlPadding.SuspendLayout();
            this.grpDpiSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlPadding
            // 
            this.pnlPadding.BackColor = System.Drawing.Color.Transparent;
            this.pnlPadding.Controls.Add(this.grpDpiSettings);
            this.pnlPadding.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPadding.Location = new System.Drawing.Point(0, 0);
            this.pnlPadding.Name = "pnlPadding";
            this.pnlPadding.Padding = new System.Windows.Forms.Padding(24);
            this.pnlPadding.Size = new System.Drawing.Size(527, 283);
            this.pnlPadding.TabIndex = 8;
            // 
            // grpDpiSettings
            // 
            this.grpDpiSettings.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.grpDpiSettings.BackColor = System.Drawing.Color.Transparent;
            this.grpDpiSettings.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(69)))), ((int)(((byte)(69)))));
            this.grpDpiSettings.Controls.Add(this.txtAutoScaleFactor);
            this.grpDpiSettings.Controls.Add(this.lblAutoScaleFactor);
            this.grpDpiSettings.Controls.Add(this.txtWinVer);
            this.grpDpiSettings.Controls.Add(this.lblWinVer);
            this.grpDpiSettings.Controls.Add(this.txtDpi);
            this.grpDpiSettings.Controls.Add(this.lblDpi);
            this.grpDpiSettings.Controls.Add(this.btnUpdateDpi);
            this.grpDpiSettings.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.grpDpiSettings.Location = new System.Drawing.Point(24, 24);
            this.grpDpiSettings.Name = "grpDpiSettings";
            this.grpDpiSettings.Size = new System.Drawing.Size(479, 235);
            this.grpDpiSettings.TabIndex = 8;
            this.grpDpiSettings.TabStop = false;
            this.grpDpiSettings.Text = "Dpi Settings";
            // 
            // txtAutoScaleFactor
            // 
            this.txtAutoScaleFactor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(43)))), ((int)(((byte)(43)))));
            this.txtAutoScaleFactor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtAutoScaleFactor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.txtAutoScaleFactor.Location = new System.Drawing.Point(170, 130);
            this.txtAutoScaleFactor.Name = "txtAutoScaleFactor";
            this.txtAutoScaleFactor.Size = new System.Drawing.Size(275, 23);
            this.txtAutoScaleFactor.TabIndex = 13;
            this.txtAutoScaleFactor.Text = "0";
            // 
            // lblAutoScaleFactor
            // 
            this.lblAutoScaleFactor.AutoSize = true;
            this.lblAutoScaleFactor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.lblAutoScaleFactor.Location = new System.Drawing.Point(41, 133);
            this.lblAutoScaleFactor.Name = "lblAutoScaleFactor";
            this.lblAutoScaleFactor.Size = new System.Drawing.Size(105, 15);
            this.lblAutoScaleFactor.TabIndex = 12;
            this.lblAutoScaleFactor.Text = "Auto Scale Factor :";
            // 
            // txtWinVer
            // 
            this.txtWinVer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(43)))), ((int)(((byte)(43)))));
            this.txtWinVer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtWinVer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.txtWinVer.Location = new System.Drawing.Point(170, 94);
            this.txtWinVer.Name = "txtWinVer";
            this.txtWinVer.Size = new System.Drawing.Size(275, 23);
            this.txtWinVer.TabIndex = 11;
            this.txtWinVer.Text = "0";
            // 
            // lblWinVer
            // 
            this.lblWinVer.AutoSize = true;
            this.lblWinVer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.lblWinVer.Location = new System.Drawing.Point(41, 97);
            this.lblWinVer.Name = "lblWinVer";
            this.lblWinVer.Size = new System.Drawing.Size(50, 15);
            this.lblWinVer.TabIndex = 10;
            this.lblWinVer.Text = "WinVer :";
            // 
            // txtDpi
            // 
            this.txtDpi.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(43)))), ((int)(((byte)(43)))));
            this.txtDpi.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDpi.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.txtDpi.Location = new System.Drawing.Point(170, 58);
            this.txtDpi.Name = "txtDpi";
            this.txtDpi.Size = new System.Drawing.Size(136, 23);
            this.txtDpi.TabIndex = 9;
            this.txtDpi.Text = "0";
            // 
            // lblDpi
            // 
            this.lblDpi.AutoSize = true;
            this.lblDpi.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.lblDpi.Location = new System.Drawing.Point(41, 61);
            this.lblDpi.Name = "lblDpi";
            this.lblDpi.Size = new System.Drawing.Size(72, 15);
            this.lblDpi.TabIndex = 8;
            this.lblDpi.Text = "System Dpi :";
            // 
            // btnUpdateDpi
            // 
            this.btnUpdateDpi.Default = false;
            this.btnUpdateDpi.ImagePadding = 6;
            this.btnUpdateDpi.Location = new System.Drawing.Point(312, 52);
            this.btnUpdateDpi.Name = "btnUpdateDpi";
            this.btnUpdateDpi.Padding = new System.Windows.Forms.Padding(6);
            this.btnUpdateDpi.Size = new System.Drawing.Size(133, 32);
            this.btnUpdateDpi.TabIndex = 7;
            this.btnUpdateDpi.Text = "Update";
            this.btnUpdateDpi.UseVisualStyleBackColor = false;
            this.btnUpdateDpi.Click += new System.EventHandler(this.BtnUpdateDpi_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(527, 283);
            this.Controls.Add(this.pnlPadding);
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.pnlPadding.ResumeLayout(false);
            this.grpDpiSettings.ResumeLayout(false);
            this.grpDpiSettings.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlPadding;
        private AcrylicUI.Controls.AcrylicGroupBox grpDpiSettings;
        
        private System.Windows.Forms.Label lblAutoScaleFactor;
        private AcrylicUI.Controls.AcrylicTextBox txtAutoScaleFactor;
        private AcrylicUI.Controls.AcrylicTextBox txtWinVer;
        private AcrylicUI.Controls.AcrylicTextBox txtDpi;
        private System.Windows.Forms.Label lblWinVer;
        
        private System.Windows.Forms.Label lblDpi;
        private AcrylicUI.Controls.AcrylicButton btnUpdateDpi;
    }
}