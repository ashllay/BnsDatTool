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
            this.lbDat = new System.Windows.Forms.Label();
            this.lbRfolder = new System.Windows.Forms.Label();
            this.Cb_back = new System.Windows.Forms.CheckBox();
            this.richOut = new System.Windows.Forms.RichTextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSeaarchBin = new System.Windows.Forms.Button();
            this.txbBinFile = new System.Windows.Forms.TextBox();
            this.cboxGetBinFolder = new System.Windows.Forms.CheckBox();
            this.btnDump = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.btnOutBin = new System.Windows.Forms.Button();
            this.txbBinFolder = new System.Windows.Forms.TextBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.BtnSTranslate = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.BtnSSource = new System.Windows.Forms.Button();
            this.BtnSTarget = new System.Windows.Forms.Button();
            this.TxblocalTarget = new System.Windows.Forms.TextBox();
            this.TxblocalSource = new System.Windows.Forms.TextBox();
            this.cboxtbackup = new System.Windows.Forms.CheckBox();
            this.GboxTools = new System.Windows.Forms.GroupBox();
            this.groupBox2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.GboxTools.SuspendLayout();
            this.SuspendLayout();
            // 
            // bntSearchDat
            // 
            this.bntSearchDat.Location = new System.Drawing.Point(378, 4);
            this.bntSearchDat.Name = "bntSearchDat";
            this.bntSearchDat.Size = new System.Drawing.Size(57, 23);
            this.bntSearchDat.TabIndex = 0;
            this.bntSearchDat.Text = "Search";
            this.bntSearchDat.UseVisualStyleBackColor = true;
            this.bntSearchDat.Click += new System.EventHandler(this.bntSearchDat_Click);
            // 
            // bntSearchOut
            // 
            this.bntSearchOut.Location = new System.Drawing.Point(378, 30);
            this.bntSearchOut.Name = "bntSearchOut";
            this.bntSearchOut.Size = new System.Drawing.Size(57, 23);
            this.bntSearchOut.TabIndex = 1;
            this.bntSearchOut.Text = "Search";
            this.bntSearchOut.UseVisualStyleBackColor = true;
            this.bntSearchOut.Click += new System.EventHandler(this.bnSearchOut_Click);
            // 
            // bntUnpack
            // 
            this.bntUnpack.Location = new System.Drawing.Point(378, 57);
            this.bntUnpack.Name = "bntUnpack";
            this.bntUnpack.Size = new System.Drawing.Size(57, 23);
            this.bntUnpack.TabIndex = 2;
            this.bntUnpack.Text = "Unpack";
            this.bntUnpack.UseVisualStyleBackColor = true;
            this.bntUnpack.Click += new System.EventHandler(this.BntStart_Click);
            // 
            // txbDatFile
            // 
            this.txbDatFile.Location = new System.Drawing.Point(94, 6);
            this.txbDatFile.Name = "txbDatFile";
            this.txbDatFile.Size = new System.Drawing.Size(278, 20);
            this.txbDatFile.TabIndex = 3;
            // 
            // txbRpFolder
            // 
            this.txbRpFolder.Location = new System.Drawing.Point(94, 32);
            this.txbRpFolder.Name = "txbRpFolder";
            this.txbRpFolder.Size = new System.Drawing.Size(278, 20);
            this.txbRpFolder.TabIndex = 4;
            // 
            // cB_output
            // 
            this.cB_output.AutoSize = true;
            this.cB_output.Checked = true;
            this.cB_output.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cB_output.Location = new System.Drawing.Point(11, 59);
            this.cB_output.Name = "cB_output";
            this.cB_output.Size = new System.Drawing.Size(115, 17);
            this.cB_output.TabIndex = 14;
            this.cB_output.Text = "Get Folder location";
            this.cB_output.UseVisualStyleBackColor = true;
            // 
            // btnRepack
            // 
            this.btnRepack.Location = new System.Drawing.Point(378, 84);
            this.btnRepack.Name = "btnRepack";
            this.btnRepack.Size = new System.Drawing.Size(57, 23);
            this.btnRepack.TabIndex = 15;
            this.btnRepack.Text = "Repack";
            this.btnRepack.UseVisualStyleBackColor = true;
            this.btnRepack.Click += new System.EventHandler(this.btnRepack_Click);
            // 
            // lbDat
            // 
            this.lbDat.AutoSize = true;
            this.lbDat.Location = new System.Drawing.Point(8, 9);
            this.lbDat.Name = "lbDat";
            this.lbDat.Size = new System.Drawing.Size(47, 13);
            this.lbDat.TabIndex = 17;
            this.lbDat.Text = ".dat File:";
            // 
            // lbRfolder
            // 
            this.lbRfolder.AutoSize = true;
            this.lbRfolder.Location = new System.Drawing.Point(8, 35);
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
            this.Cb_back.Location = new System.Drawing.Point(132, 59);
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
            this.richOut.Location = new System.Drawing.Point(6, 19);
            this.richOut.Name = "richOut";
            this.richOut.Size = new System.Drawing.Size(455, 90);
            this.richOut.TabIndex = 21;
            this.richOut.Text = "";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.richOut);
            this.groupBox2.Location = new System.Drawing.Point(9, 182);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(467, 115);
            this.groupBox2.TabIndex = 23;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Log:";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(6, 15);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(458, 140);
            this.tabControl1.TabIndex = 24;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.lbDat);
            this.tabPage1.Controls.Add(this.bntSearchDat);
            this.tabPage1.Controls.Add(this.txbDatFile);
            this.tabPage1.Controls.Add(this.cB_output);
            this.tabPage1.Controls.Add(this.bntUnpack);
            this.tabPage1.Controls.Add(this.Cb_back);
            this.tabPage1.Controls.Add(this.btnRepack);
            this.tabPage1.Controls.Add(this.lbRfolder);
            this.tabPage1.Controls.Add(this.bntSearchOut);
            this.tabPage1.Controls.Add(this.txbRpFolder);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(450, 114);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Dat Files";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.label1);
            this.tabPage2.Controls.Add(this.btnSeaarchBin);
            this.tabPage2.Controls.Add(this.txbBinFile);
            this.tabPage2.Controls.Add(this.cboxGetBinFolder);
            this.tabPage2.Controls.Add(this.btnDump);
            this.tabPage2.Controls.Add(this.label2);
            this.tabPage2.Controls.Add(this.btnOutBin);
            this.tabPage2.Controls.Add(this.txbBinFolder);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(450, 114);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Bin Files";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 29;
            this.label1.Text = ".bin File:";
            // 
            // btnSeaarchBin
            // 
            this.btnSeaarchBin.Location = new System.Drawing.Point(378, 4);
            this.btnSeaarchBin.Name = "btnSeaarchBin";
            this.btnSeaarchBin.Size = new System.Drawing.Size(57, 23);
            this.btnSeaarchBin.TabIndex = 21;
            this.btnSeaarchBin.Text = "Search";
            this.btnSeaarchBin.UseVisualStyleBackColor = true;
            this.btnSeaarchBin.Click += new System.EventHandler(this.btnSeaarchBin_Click);
            // 
            // txbBinFile
            // 
            this.txbBinFile.Location = new System.Drawing.Point(94, 6);
            this.txbBinFile.Name = "txbBinFile";
            this.txbBinFile.Size = new System.Drawing.Size(278, 20);
            this.txbBinFile.TabIndex = 24;
            // 
            // cboxGetBinFolder
            // 
            this.cboxGetBinFolder.AutoSize = true;
            this.cboxGetBinFolder.Checked = true;
            this.cboxGetBinFolder.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cboxGetBinFolder.Location = new System.Drawing.Point(11, 59);
            this.cboxGetBinFolder.Name = "cboxGetBinFolder";
            this.cboxGetBinFolder.Size = new System.Drawing.Size(115, 17);
            this.cboxGetBinFolder.TabIndex = 26;
            this.cboxGetBinFolder.Text = "Get Folder location";
            this.cboxGetBinFolder.UseVisualStyleBackColor = true;
            // 
            // btnDump
            // 
            this.btnDump.Location = new System.Drawing.Point(378, 57);
            this.btnDump.Name = "btnDump";
            this.btnDump.Size = new System.Drawing.Size(57, 23);
            this.btnDump.TabIndex = 23;
            this.btnDump.Text = "Dump";
            this.btnDump.UseVisualStyleBackColor = true;
            this.btnDump.Click += new System.EventHandler(this.btnDump_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 13);
            this.label2.TabIndex = 30;
            this.label2.Text = "Output Folder:";
            // 
            // btnOutBin
            // 
            this.btnOutBin.Location = new System.Drawing.Point(378, 30);
            this.btnOutBin.Name = "btnOutBin";
            this.btnOutBin.Size = new System.Drawing.Size(57, 23);
            this.btnOutBin.TabIndex = 22;
            this.btnOutBin.Text = "Search";
            this.btnOutBin.UseVisualStyleBackColor = true;
            this.btnOutBin.Click += new System.EventHandler(this.btnOutBin_Click);
            // 
            // txbBinFolder
            // 
            this.txbBinFolder.Location = new System.Drawing.Point(94, 32);
            this.txbBinFolder.Name = "txbBinFolder";
            this.txbBinFolder.Size = new System.Drawing.Size(278, 20);
            this.txbBinFolder.TabIndex = 25;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.BtnSTranslate);
            this.tabPage3.Controls.Add(this.label7);
            this.tabPage3.Controls.Add(this.label3);
            this.tabPage3.Controls.Add(this.BtnSSource);
            this.tabPage3.Controls.Add(this.BtnSTarget);
            this.tabPage3.Controls.Add(this.TxblocalTarget);
            this.tabPage3.Controls.Add(this.TxblocalSource);
            this.tabPage3.Controls.Add(this.cboxtbackup);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(450, 114);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Translate";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // BtnSTranslate
            // 
            this.BtnSTranslate.Location = new System.Drawing.Point(378, 57);
            this.BtnSTranslate.Name = "BtnSTranslate";
            this.BtnSTranslate.Size = new System.Drawing.Size(57, 23);
            this.BtnSTranslate.TabIndex = 58;
            this.BtnSTranslate.Text = "Start";
            this.BtnSTranslate.UseVisualStyleBackColor = true;
            this.BtnSTranslate.Click += new System.EventHandler(this.BtnSTranslate_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 35);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(44, 13);
            this.label7.TabIndex = 57;
            this.label7.Text = "Source:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 13);
            this.label3.TabIndex = 56;
            this.label3.Text = "Target:";
            // 
            // BtnSSource
            // 
            this.BtnSSource.Location = new System.Drawing.Point(378, 30);
            this.BtnSSource.Name = "BtnSSource";
            this.BtnSSource.Size = new System.Drawing.Size(57, 23);
            this.BtnSSource.TabIndex = 55;
            this.BtnSSource.Text = "Search";
            this.BtnSSource.UseVisualStyleBackColor = true;
            this.BtnSSource.Click += new System.EventHandler(this.BtnSSource_Click);
            // 
            // BtnSTarget
            // 
            this.BtnSTarget.Location = new System.Drawing.Point(378, 4);
            this.BtnSTarget.Name = "BtnSTarget";
            this.BtnSTarget.Size = new System.Drawing.Size(57, 23);
            this.BtnSTarget.TabIndex = 54;
            this.BtnSTarget.Text = "Search";
            this.BtnSTarget.UseVisualStyleBackColor = true;
            this.BtnSTarget.Click += new System.EventHandler(this.BtnSTarget_Click);
            // 
            // TxblocalTarget
            // 
            this.TxblocalTarget.Location = new System.Drawing.Point(56, 6);
            this.TxblocalTarget.Name = "TxblocalTarget";
            this.TxblocalTarget.Size = new System.Drawing.Size(316, 20);
            this.TxblocalTarget.TabIndex = 53;
            // 
            // TxblocalSource
            // 
            this.TxblocalSource.Location = new System.Drawing.Point(56, 32);
            this.TxblocalSource.Name = "TxblocalSource";
            this.TxblocalSource.Size = new System.Drawing.Size(316, 20);
            this.TxblocalSource.TabIndex = 52;
            // 
            // cboxtbackup
            // 
            this.cboxtbackup.AutoSize = true;
            this.cboxtbackup.Checked = true;
            this.cboxtbackup.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cboxtbackup.Location = new System.Drawing.Point(56, 58);
            this.cboxtbackup.Name = "cboxtbackup";
            this.cboxtbackup.Size = new System.Drawing.Size(95, 17);
            this.cboxtbackup.TabIndex = 33;
            this.cboxtbackup.Text = "Bakup Original";
            this.cboxtbackup.UseVisualStyleBackColor = true;
            // 
            // GboxTools
            // 
            this.GboxTools.Controls.Add(this.tabControl1);
            this.GboxTools.Location = new System.Drawing.Point(9, 12);
            this.GboxTools.Name = "GboxTools";
            this.GboxTools.Size = new System.Drawing.Size(467, 164);
            this.GboxTools.TabIndex = 25;
            this.GboxTools.TabStop = false;
            // 
            // BnsDatTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(485, 304);
            this.Controls.Add(this.GboxTools);
            this.Controls.Add(this.groupBox2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "BnsDatTool";
            this.Text = "BnsDatTool";
            this.Load += new System.EventHandler(this.BnsDatTool_Load);
            this.groupBox2.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.GboxTools.ResumeLayout(false);
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
        private System.Windows.Forms.Label lbDat;
        private System.Windows.Forms.Label lbRfolder;
        private System.Windows.Forms.CheckBox Cb_back;
        private System.Windows.Forms.RichTextBox richOut;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSeaarchBin;
        private System.Windows.Forms.TextBox txbBinFile;
        private System.Windows.Forms.CheckBox cboxGetBinFolder;
        private System.Windows.Forms.Button btnDump;
        private System.Windows.Forms.CheckBox is64Bin;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnOutBin;
        private System.Windows.Forms.TextBox txbBinFolder;
        private System.Windows.Forms.CheckBox cboxtbackup;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button BtnSSource;
        private System.Windows.Forms.Button BtnSTarget;
        private System.Windows.Forms.TextBox TxblocalTarget;
        private System.Windows.Forms.TextBox TxblocalSource;
        private System.Windows.Forms.Button BtnSTranslate;
        private System.Windows.Forms.GroupBox GboxTools;
    }
}

