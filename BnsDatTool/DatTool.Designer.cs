namespace BnsDatTool
{
    partial class BnsDatTool
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BnsDatTool));
            this.bntSearchDat = new System.Windows.Forms.Button();
            this.bntSearchOut = new System.Windows.Forms.Button();
            this.bntUnpack = new System.Windows.Forms.Button();
            this.txbDatFile = new System.Windows.Forms.TextBox();
            this.txbRpFolder = new System.Windows.Forms.TextBox();
            this.cB_output = new System.Windows.Forms.CheckBox();
            this.btnRepack = new System.Windows.Forms.Button();
            this.is64Dat = new System.Windows.Forms.CheckBox();
            this.lbDat = new System.Windows.Forms.Label();
            this.lbRfolder = new System.Windows.Forms.Label();
            this.Cb_back = new System.Windows.Forms.CheckBox();
            this.richOut = new System.Windows.Forms.RichTextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // bntSearchDat
            // 
            this.bntSearchDat.Location = new System.Drawing.Point(358, 11);
            this.bntSearchDat.Name = "bntSearchDat";
            this.bntSearchDat.Size = new System.Drawing.Size(75, 23);
            this.bntSearchDat.TabIndex = 0;
            this.bntSearchDat.Text = "Search";
            this.bntSearchDat.UseVisualStyleBackColor = true;
            this.bntSearchDat.Click += new System.EventHandler(this.bntSearchDat_Click);
            // 
            // bntSearchOut
            // 
            this.bntSearchOut.Location = new System.Drawing.Point(358, 37);
            this.bntSearchOut.Name = "bntSearchOut";
            this.bntSearchOut.Size = new System.Drawing.Size(75, 23);
            this.bntSearchOut.TabIndex = 1;
            this.bntSearchOut.Text = "Search";
            this.bntSearchOut.UseVisualStyleBackColor = true;
            this.bntSearchOut.Click += new System.EventHandler(this.bnSearchOut_Click);
            // 
            // bntUnpack
            // 
            this.bntUnpack.Location = new System.Drawing.Point(358, 66);
            this.bntUnpack.Name = "bntUnpack";
            this.bntUnpack.Size = new System.Drawing.Size(75, 23);
            this.bntUnpack.TabIndex = 2;
            this.bntUnpack.Text = "Unpack";
            this.bntUnpack.UseVisualStyleBackColor = true;
            this.bntUnpack.Click += new System.EventHandler(this.BntStart_Click);
            // 
            // txbDatFile
            // 
            this.txbDatFile.Location = new System.Drawing.Point(92, 13);
            this.txbDatFile.Name = "txbDatFile";
            this.txbDatFile.Size = new System.Drawing.Size(260, 20);
            this.txbDatFile.TabIndex = 3;
            // 
            // txbRpFolder
            // 
            this.txbRpFolder.Location = new System.Drawing.Point(92, 39);
            this.txbRpFolder.Name = "txbRpFolder";
            this.txbRpFolder.Size = new System.Drawing.Size(260, 20);
            this.txbRpFolder.TabIndex = 4;
            // 
            // cB_output
            // 
            this.cB_output.AutoSize = true;
            this.cB_output.Checked = true;
            this.cB_output.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cB_output.Location = new System.Drawing.Point(18, 70);
            this.cB_output.Name = "cB_output";
            this.cB_output.Size = new System.Drawing.Size(115, 17);
            this.cB_output.TabIndex = 14;
            this.cB_output.Text = "Get Folder location";
            this.cB_output.UseVisualStyleBackColor = true;
            // 
            // btnRepack
            // 
            this.btnRepack.Location = new System.Drawing.Point(358, 95);
            this.btnRepack.Name = "btnRepack";
            this.btnRepack.Size = new System.Drawing.Size(75, 23);
            this.btnRepack.TabIndex = 15;
            this.btnRepack.Text = "Repack";
            this.btnRepack.UseVisualStyleBackColor = true;
            this.btnRepack.Click += new System.EventHandler(this.btnRepack_Click);
            // 
            // is64Dat
            // 
            this.is64Dat.AutoSize = true;
            this.is64Dat.Location = new System.Drawing.Point(18, 93);
            this.is64Dat.Name = "is64Dat";
            this.is64Dat.Size = new System.Drawing.Size(70, 17);
            this.is64Dat.TabIndex = 16;
            this.is64Dat.Text = "64bit .dat";
            this.is64Dat.UseVisualStyleBackColor = true;
            // 
            // lbDat
            // 
            this.lbDat.AutoSize = true;
            this.lbDat.Location = new System.Drawing.Point(6, 16);
            this.lbDat.Name = "lbDat";
            this.lbDat.Size = new System.Drawing.Size(47, 13);
            this.lbDat.TabIndex = 17;
            this.lbDat.Text = ".dat File:";
            // 
            // lbRfolder
            // 
            this.lbRfolder.AutoSize = true;
            this.lbRfolder.Location = new System.Drawing.Point(6, 42);
            this.lbRfolder.Name = "lbRfolder";
            this.lbRfolder.Size = new System.Drawing.Size(80, 13);
            this.lbRfolder.TabIndex = 18;
            this.lbRfolder.Text = "Repack Folder:";
            // 
            // Cb_back
            // 
            this.Cb_back.AutoSize = true;
            this.Cb_back.Checked = true;
            this.Cb_back.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Cb_back.Location = new System.Drawing.Point(139, 70);
            this.Cb_back.Name = "Cb_back";
            this.Cb_back.Size = new System.Drawing.Size(95, 17);
            this.Cb_back.TabIndex = 20;
            this.Cb_back.Text = "Bakup Original";
            this.Cb_back.UseVisualStyleBackColor = true;
            // 
            // richOut
            // 
            this.richOut.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richOut.BackColor = System.Drawing.SystemColors.Control;
            this.richOut.HideSelection = false;
            this.richOut.Location = new System.Drawing.Point(6, 14);
            this.richOut.Name = "richOut";
            this.richOut.Size = new System.Drawing.Size(426, 56);
            this.richOut.TabIndex = 21;
            this.richOut.Text = "";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.lbDat);
            this.groupBox1.Controls.Add(this.bntSearchDat);
            this.groupBox1.Controls.Add(this.Cb_back);
            this.groupBox1.Controls.Add(this.bntSearchOut);
            this.groupBox1.Controls.Add(this.lbRfolder);
            this.groupBox1.Controls.Add(this.bntUnpack);
            this.groupBox1.Controls.Add(this.txbDatFile);
            this.groupBox1.Controls.Add(this.is64Dat);
            this.groupBox1.Controls.Add(this.txbRpFolder);
            this.groupBox1.Controls.Add(this.btnRepack);
            this.groupBox1.Controls.Add(this.cB_output);
            this.groupBox1.Location = new System.Drawing.Point(9, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(438, 126);
            this.groupBox1.TabIndex = 22;
            this.groupBox1.TabStop = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.richOut);
            this.groupBox2.Location = new System.Drawing.Point(9, 135);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(438, 76);
            this.groupBox2.TabIndex = 23;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Log:";
            // 
            // BnsDatTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(456, 218);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "BnsDatTool";
            this.Text = "BnsDatTool";
            this.Load += new System.EventHandler(this.BnsDatTool_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button bntSearchDat;
        private System.Windows.Forms.Button bntSearchOut;
        private System.Windows.Forms.Button bntUnpack;
        private System.Windows.Forms.TextBox txbDatFile;
        private System.Windows.Forms.TextBox txbRpFolder;
        private System.Windows.Forms.CheckBox cB_output;
        private System.Windows.Forms.Button btnRepack;
        private System.Windows.Forms.CheckBox is64Dat;
        private System.Windows.Forms.Label lbDat;
        private System.Windows.Forms.Label lbRfolder;
        private System.Windows.Forms.CheckBox Cb_back;
        private System.Windows.Forms.RichTextBox richOut;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
    }
}

