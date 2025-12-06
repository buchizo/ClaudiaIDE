using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace ClaudiaIDE.Forms
{
   public class AboutImageForm : Form
   {
      public AboutImageForm(ImageProvider imageProvider)
      {
         InitializeComponent();

         StartPosition = FormStartPosition.CenterScreen;

         BitmapSource bmp = imageProvider.GetBitmap();

         if (bmp != null)
         {
             txtUri.Text = imageProvider.GetCurrentImageUri();
             txtDimensions.Text = $@"{bmp.PixelWidth} x {bmp.PixelHeight}";
             txtResolution.Text = $@"{bmp.DpiX} x {bmp.DpiY}";

             pic.SizeMode = PictureBoxSizeMode.Zoom;
             pic.Image = ConvertBitmapImageToBitmap(bmp);

             PropertyInfo prop = typeof(BitmapImage).GetProperty("UriSource",
                 BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
             if (prop == null || (prop.GetValue(bmp) as Uri)?.Scheme != "file" || !File.Exists(txtUri.Text) )
             {
                 lblFileSize.Visible = txtFileSize.Visible = false;
                 btnExplore.Visible = false;
             }
             else
             {
                 FileInfo fi = new FileInfo(txtUri.Text);
                 txtFileSize.Text = fi.Length.ToString("N0");
             }
         }
         else
         {
             List<Control> controls = new List<Control>
             {
                 lblUri, txtUri,
                 lblDimensions, txtDimensions,
                 lblResolution, txtResolution,
                 pic
             };
             foreach (Control control in controls)
             {
                 control.Enabled = false;
             }

             lblFileSize.Visible = txtFileSize.Visible = false;
             btnExplore.Visible = false;
         }

         btnOK.Focus();
      }

      private Button btnOK;
      private Label lblUri;
      private TextBox txtUri;
      private Label lblDimensions;
      private TextBox txtDimensions;
      private TextBox txtResolution;
      private Label lblResolution;
      private TextBox txtFileSize;
      private Label lblFileSize;
      private Button btnExplore;
      private PictureBox pic;


      private static Bitmap ConvertBitmapImageToBitmap(BitmapSource source)
      {
          using (MemoryStream memoryStream = new MemoryStream())
          {
              BitmapEncoder encoder = new BmpBitmapEncoder();
              encoder.Frames.Add(BitmapFrame.Create(source));
              encoder.Save(memoryStream);
              return new Bitmap(memoryStream);
          }
      }

      private void txt_Enter(object sender, EventArgs e)
      {
         ((TextBox)sender).SelectAll();
      }

      private void btnOK_Click(object sender, EventArgs e)
      {
         Close();
      }

      private void btnExplore_Click(object sender, EventArgs e)
      {
          try
          {
              string path = txtUri.Text;
              string args;

              // determine if the path is a file path or a directory path
              FileAttributes attr = File.GetAttributes(path);

              if (FileAttributes.Directory == (attr & FileAttributes.Directory))
                  // make sure to navigate into the directory
                  args = "/e,/root,/select,\"" + path + "\"";
              else
                  // make sure to select the file
                  args = "/e,/select,\"" + path + "\"";

              ProcessStartInfo psi = new ProcessStartInfo(Environment.ExpandEnvironmentVariables("%WINDIR%\\Explorer.exe"))
              {
                  UseShellExecute = true,
                  Arguments = args,
              };
              Process.Start(psi);
          }
          catch
          {
              MessageBox.Show("Unable to open Windows File Explorer to the specified path.");
          }
      }

      private void InitializeComponent()
      {
         this.btnOK = new System.Windows.Forms.Button();
         this.lblUri = new System.Windows.Forms.Label();
         this.txtUri = new System.Windows.Forms.TextBox();
         this.lblDimensions = new System.Windows.Forms.Label();
         this.txtDimensions = new System.Windows.Forms.TextBox();
         this.txtResolution = new System.Windows.Forms.TextBox();
         this.lblResolution = new System.Windows.Forms.Label();
         this.txtFileSize = new System.Windows.Forms.TextBox();
         this.lblFileSize = new System.Windows.Forms.Label();
         this.btnExplore = new System.Windows.Forms.Button();
         this.pic = new System.Windows.Forms.PictureBox();
         ((System.ComponentModel.ISupportInitialize)(this.pic)).BeginInit();
         this.SuspendLayout();
         // 
         // btnOK
         // 
         this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.btnOK.Location = new System.Drawing.Point(13, 322);
         this.btnOK.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
         this.btnOK.Name = "btnOK";
         this.btnOK.Size = new System.Drawing.Size(88, 27);
         this.btnOK.TabIndex = 0;
         this.btnOK.Text = "OK";
         this.btnOK.UseVisualStyleBackColor = true;
         this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
         // 
         // lblUri
         // 
         this.lblUri.AutoSize = true;
         this.lblUri.Location = new System.Drawing.Point(12, 15);
         this.lblUri.Name = "lblUri";
         this.lblUri.Size = new System.Drawing.Size(28, 15);
         this.lblUri.TabIndex = 2;
         this.lblUri.Text = "&URI:";
         // 
         // txtUri
         // 
         this.txtUri.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
         this.txtUri.BackColor = System.Drawing.SystemColors.Control;
         this.txtUri.BorderStyle = System.Windows.Forms.BorderStyle.None;
         this.txtUri.Location = new System.Drawing.Point(130, 14);
         this.txtUri.Name = "txtUri";
         this.txtUri.ReadOnly = true;
         this.txtUri.Size = new System.Drawing.Size(592, 16);
         this.txtUri.TabIndex = 3;
         this.txtUri.Text = "#";
         this.txtUri.Enter += new System.EventHandler(this.txt_Enter);
         // 
         // lblDimensions
         // 
         this.lblDimensions.AutoSize = true;
         this.lblDimensions.Location = new System.Drawing.Point(12, 44);
         this.lblDimensions.Name = "lblDimensions";
         this.lblDimensions.Size = new System.Drawing.Size(112, 15);
         this.lblDimensions.TabIndex = 4;
         this.lblDimensions.Text = "&Dimensions (pixels):";
         // 
         // txtDimensions
         // 
         this.txtDimensions.BackColor = System.Drawing.SystemColors.Control;
         this.txtDimensions.BorderStyle = System.Windows.Forms.BorderStyle.None;
         this.txtDimensions.Font = new System.Drawing.Font("Segoe UI", 9F);
         this.txtDimensions.Location = new System.Drawing.Point(130, 44);
         this.txtDimensions.Name = "txtDimensions";
         this.txtDimensions.ReadOnly = true;
         this.txtDimensions.Size = new System.Drawing.Size(150, 16);
         this.txtDimensions.TabIndex = 5;
         this.txtDimensions.Text = "#";
         this.txtDimensions.Enter += new System.EventHandler(this.txt_Enter);
         // 
         // txtResolution
         // 
         this.txtResolution.BackColor = System.Drawing.SystemColors.Control;
         this.txtResolution.BorderStyle = System.Windows.Forms.BorderStyle.None;
         this.txtResolution.Font = new System.Drawing.Font("Segoe UI", 9F);
         this.txtResolution.Location = new System.Drawing.Point(130, 73);
         this.txtResolution.Name = "txtResolution";
         this.txtResolution.ReadOnly = true;
         this.txtResolution.Size = new System.Drawing.Size(150, 16);
         this.txtResolution.TabIndex = 7;
         this.txtResolution.Text = "#";
         this.txtResolution.Enter += new System.EventHandler(this.txt_Enter);
         // 
         // lblResolution
         // 
         this.lblResolution.AutoSize = true;
         this.lblResolution.Location = new System.Drawing.Point(12, 73);
         this.lblResolution.Name = "lblResolution";
         this.lblResolution.Size = new System.Drawing.Size(95, 15);
         this.lblResolution.TabIndex = 6;
         this.lblResolution.Text = "&Resolution (DPI):";
         // 
         // txtFileSize
         // 
         this.txtFileSize.BackColor = System.Drawing.SystemColors.Control;
         this.txtFileSize.BorderStyle = System.Windows.Forms.BorderStyle.None;
         this.txtFileSize.Font = new System.Drawing.Font("Segoe UI", 9F);
         this.txtFileSize.Location = new System.Drawing.Point(130, 102);
         this.txtFileSize.Name = "txtFileSize";
         this.txtFileSize.ReadOnly = true;
         this.txtFileSize.Size = new System.Drawing.Size(150, 16);
         this.txtFileSize.TabIndex = 9;
         this.txtFileSize.Text = "#";
         // 
         // lblFileSize
         // 
         this.lblFileSize.AutoSize = true;
         this.lblFileSize.Location = new System.Drawing.Point(12, 102);
         this.lblFileSize.Name = "lblFileSize";
         this.lblFileSize.Size = new System.Drawing.Size(87, 15);
         this.lblFileSize.TabIndex = 8;
         this.lblFileSize.Text = "&File Size (bytes)";
         // 
         // btnExplore
         // 
         this.btnExplore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.btnExplore.Location = new System.Drawing.Point(15, 289);
         this.btnExplore.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
         this.btnExplore.Name = "btnExplore";
         this.btnExplore.Size = new System.Drawing.Size(88, 27);
         this.btnExplore.TabIndex = 1;
         this.btnExplore.Text = "E&xplore";
         this.btnExplore.UseVisualStyleBackColor = true;
         this.btnExplore.Click += new System.EventHandler(this.btnExplore_Click);
         // 
         // pic
         // 
         this.pic.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
         this.pic.BackColor = System.Drawing.Color.White;
         this.pic.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
         this.pic.Location = new System.Drawing.Point(286, 41);
         this.pic.Name = "pic";
         this.pic.Size = new System.Drawing.Size(436, 308);
         this.pic.TabIndex = 12;
         this.pic.TabStop = false;
         // 
         // frmAboutImage
         // 
         this.AcceptButton = this.btnOK;
         this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.btnOK;
         this.ClientSize = new System.Drawing.Size(734, 361);
         this.Controls.Add(this.pic);
         this.Controls.Add(this.btnExplore);
         this.Controls.Add(this.txtFileSize);
         this.Controls.Add(this.lblFileSize);
         this.Controls.Add(this.txtResolution);
         this.Controls.Add(this.lblResolution);
         this.Controls.Add(this.txtDimensions);
         this.Controls.Add(this.lblDimensions);
         this.Controls.Add(this.txtUri);
         this.Controls.Add(this.lblUri);
         this.Controls.Add(this.btnOK);
         this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.MinimumSize = new System.Drawing.Size(750, 400);
         this.Name = "frmAboutImage";
         this.ShowIcon = false;
         this.ShowInTaskbar = false;
         this.Text = "About Image";
         ((System.ComponentModel.ISupportInitialize)(this.pic)).EndInit();
         this.ResumeLayout(false);
         this.PerformLayout();
      }
   }
}
