namespace InpaintWinForms
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
            this.btnOpen = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.btnInpaint = new System.Windows.Forms.Button();
            this.pbMarkup = new Zavolokas.WinFormsHelpers.MarkablePictureBox();
            this.btnOpenMarkup = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(523, 12);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(117, 23);
            this.btnOpen.TabIndex = 0;
            this.btnOpen.Text = "Open File";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.OnOpenFileClick);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.png, *.bmp) | *.jpg; *.jpeg; *.jpe; *.png; *" +
    ".bmp";
            // 
            // btnInpaint
            // 
            this.btnInpaint.Enabled = false;
            this.btnInpaint.Location = new System.Drawing.Point(523, 70);
            this.btnInpaint.Name = "btnInpaint";
            this.btnInpaint.Size = new System.Drawing.Size(117, 23);
            this.btnInpaint.TabIndex = 1;
            this.btnInpaint.Text = "Inpaint";
            this.btnInpaint.UseVisualStyleBackColor = true;
            this.btnInpaint.Click += new System.EventHandler(this.OnInpaint);
            // 
            // pbMarkup
            // 
            this.pbMarkup.BackColor = System.Drawing.Color.White;
            this.pbMarkup.BrushColor = System.Drawing.Color.Red;
            this.pbMarkup.BrushSize = 20;
            this.pbMarkup.DrawLines = false;
            this.pbMarkup.Image = null;
            this.pbMarkup.IsDrawable = true;
            this.pbMarkup.Layers = null;
            this.pbMarkup.Location = new System.Drawing.Point(0, 0);
            this.pbMarkup.Name = "pbMarkup";
            this.pbMarkup.ProtectMarkup = null;
            this.pbMarkup.RemoveMarkup = null;
            this.pbMarkup.Size = new System.Drawing.Size(517, 439);
            this.pbMarkup.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbMarkup.TabIndex = 0;
            // 
            // btnOpenMarkup
            // 
            this.btnOpenMarkup.Location = new System.Drawing.Point(523, 41);
            this.btnOpenMarkup.Name = "btnOpenMarkup";
            this.btnOpenMarkup.Size = new System.Drawing.Size(117, 23);
            this.btnOpenMarkup.TabIndex = 0;
            this.btnOpenMarkup.Text = "Open Markup";
            this.btnOpenMarkup.UseVisualStyleBackColor = true;
            this.btnOpenMarkup.Click += new System.EventHandler(this.OnOpenMarkupClick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(647, 461);
            this.Controls.Add(this.btnInpaint);
            this.Controls.Add(this.btnOpenMarkup);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.pbMarkup);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private Zavolokas.WinFormsHelpers.MarkablePictureBox pbMarkup;
        private System.Windows.Forms.Button btnInpaint;
        private System.Windows.Forms.Button btnOpenMarkup;
    }
}

