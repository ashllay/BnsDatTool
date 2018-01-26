using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using BnsDatTool.lib;

namespace BnsDatTool
{
    public partial class BnsDatTool : Form
    {
        OpenFileDialog OfileDat = new OpenFileDialog();
        FolderBrowserDialog OfolderDat = new FolderBrowserDialog();

        OpenFileDialog OfileBin = new OpenFileDialog();
        FolderBrowserDialog OfolderBin = new FolderBrowserDialog();
        //dat
        private string BackPath = "backup\\";
        private string RepackPath;
        private string OutPath;
        private string DatfileName;
        private string FulldatPath;
        //bin
        private string OutPathBin;
        private string BinfileName;
        private string FullBinPath;
        //--
        TextWriter _writer = null;

        public BackgroundWorker multiworker;

        public bool DatIs64 = false;

        public enum FILEWORKER_TYPE
        {
            NORMAL,
            TRANSLATE
        };

        public BnsDatTool()
        {
            InitializeComponent();
        }

        private void BnsDatTool_Load(object sender, EventArgs e)
        {
            _writer = new StrWriter(richOut);
            Console.SetOut(_writer);
        }

        private void RunWithWorker(DoWorkEventHandler doWork)
        {
            CheckForIllegalCrossThreadCalls = false;
            try
            {
                multiworker = new BackgroundWorker();
                multiworker.WorkerReportsProgress = true;
                multiworker.DoWork += doWork;
                multiworker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void bntSearchDat_Click(object sender, EventArgs e)
        {
            if (OfileDat.ShowDialog() != DialogResult.OK)
                return;
            

            txbDatFile.Text = OfileDat.FileName;
            FulldatPath = OfileDat.FileName;
            DatfileName = OfileDat.SafeFileName;

            // Check if 64bit or 32bit
            if (FulldatPath.Contains("64"))
                DatIs64 = true;
            else
                DatIs64 = false;

            if (cB_output.Checked == true)
            {
                RepackPath = Path.GetDirectoryName(FulldatPath) + @"\";//get working dir
                OutPath = FulldatPath + ".files"; //get full file path and add .files
                txbRpFolder.Text = OutPath;
            }
        }

        private void bnSearchOut_Click(object sender, EventArgs e)
        {
            if (OfolderDat.ShowDialog() != DialogResult.OK)
                return;

            txbRpFolder.Text = OfolderDat.SelectedPath;
            OutPath = OfolderDat.SelectedPath;

            if (OutPath.Contains("64"))
                DatIs64 = true;
            else
                DatIs64 = false;

            string datName = Path.GetFileName(OutPath);
            //get dat file name from folder
            DatfileName = datName.Replace(".files", "");
            //get working path
            RepackPath = OutPath.Replace(datName, "\\");
            //Console.WriteLine(DatfileName);
        }

        public void Extractor(string datFile)
        {
            if (datFile == null)
                return;
            try
            {
                RunWithWorker(((o, args) =>
                {
                    new BNSDat().Extract(FulldatPath, (number, of) =>
                    {
                        richOut.Text = "Extracting Files: " + number + "/" + of;
                    }, DatIs64);
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void BntStart_Click(object sender, EventArgs e)
        {
            // richOut.Clear();

            if (multiworker != null && multiworker.IsBusy)
                multiworker.CancelAsync();

            try
            {
                if (File.Exists(FulldatPath))
                {
                    Extractor(FulldatPath);
                }
                else
                {
                    MessageBox.Show("Select .dat file to unpack.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        private void btnRepack_Click(object sender, EventArgs e)
        {
            //richOut.Clear();

            if (multiworker != null && multiworker.IsBusy)
                multiworker.CancelAsync();

            if (Cb_back.Checked)
                BackUpManager(FulldatPath, RepackPath, DatfileName, DatIs64, FILEWORKER_TYPE.NORMAL);
            else
                CompileManager(OutPath, DatIs64);

        }

        private void BackUpManager(string filepath, string dir, string filename, bool is64, FILEWORKER_TYPE wtype)
        {
            //filepath full file path
            //dir filepath dir only
            //filename itself
            RunWithWorker(((o, args) =>
            {
                if (string.IsNullOrEmpty(filename) == false)
                {
                    try
                    {
                        //get current date and time
                        string date = DateTime.Now.ToString("dd-MM-yy_"); // includes leading zeros
                        string time = DateTime.Now.ToString("hh.mm.ss"); // includes leading zeros

                        // folder location
                        var BackDir = dir + BackPath;

                        // if it doesn't exist, create
                        if (!Directory.Exists(BackDir))
                            Directory.CreateDirectory(BackDir);

                        //Create a new subfolder under the current active folder
                        string newPath = Path.Combine(BackDir, date + time);

                        // Create the subfolder
                        Directory.CreateDirectory(newPath);

                        // Copy file to backup folder
                        var CurrBackPath = newPath + "\\";
                        File.Copy(dir + filename, CurrBackPath + filename, true);
                    }
                    catch
                    {
                        MessageBox.Show(".dat file not found can't create backup.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }));
            if (wtype == FILEWORKER_TYPE.NORMAL)
                CompileManager(OutPath, is64);
            else
                CompileManager(DatPathExtractTranslate, is64);
        }

        void CompileManager(string outdir, bool is64)
        {
            //outdir
            if (Directory.Exists(outdir))
                Compiler(outdir, is64);
            else
                MessageBox.Show("Select Folder to repack.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public void Compiler(string repackPath, bool is64)
        {
            if (repackPath == null)
                return;

            RunWithWorker(((o, args) =>
            {
                BNSDat m_bnsDat = new BNSDat();
                new BNSDat().Compress(repackPath, (number, of) =>
                {
                    richOut.Text = "Compressing Files: " + number + "/" + of;
                }, is64, 9);
            }));
        }

        //bin
        private bool binIs64 = false;

        private void btnSeaarchBin_Click(object sender, EventArgs e)
        {
            if (OfileBin.ShowDialog() != DialogResult.OK)
                return;

            txbBinFile.Text = OfileBin.FileName;
            FullBinPath = OfileBin.FileName;

            if (cboxGetBinFolder.Checked == true)
            {
                BinfileName = OfileBin.SafeFileName;
                OutPathBin = FullBinPath + ".files"; //get full file path and add .files
                txbBinFolder.Text = OutPathBin;
            }

            if (BinfileName.Contains("64"))
                binIs64 = true;
            else
                binIs64 = false;
        }

        private void btnOutBin_Click(object sender, EventArgs e)
        {
            if (OfolderBin.ShowDialog() != DialogResult.OK)
                return;
            txbBinFolder.Text = OfolderBin.SelectedPath;
            OutPathBin = OfolderBin.SelectedPath;
        }

        private void btnDump_Click(object sender, EventArgs e)
        {
            if (!File.Exists(FullBinPath))
            {
                MessageBox.Show("Select .bin file to dump.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                RunWithWorker(((o, args) =>
                {
                    BDat bdat = new BDat();
                    bdat.Dump(FullBinPath, OutPathBin, BXML_TYPE.BXML_PLAIN, binIs64);
                }));
            }
        }

        //translation
        OpenFileDialog OfileTranslate = new OpenFileDialog();

        private string OutPathBinTranslate;//bin output path
        private string FullBinPathTranslate;//bin file
        private string FileTranslate;//translated xml file path
        private string DatFileTranslate;//local.dat file
        private string DatPathTranslate;//local.dat path only
        private string DatFileNameTranslate;//local.dat file name only
        private string DatPathExtractTranslate;

        public bool tIs64 = false;

        private void btn_SearchtLocal_Click(object sender, EventArgs e)
        {
            if (OfileDat.ShowDialog() != DialogResult.OK)
                return;
            else
            {
                if (OfileDat.FileName.Contains("64"))
                    tIs64 = true;
                else
                    tIs64 = false;

                DatFileTranslate = OfileDat.FileName;//get local file
                txb_tlocal.Text = DatFileTranslate;//set local file to textbox
                DatFileNameTranslate = Path.GetFileName(DatFileTranslate);
                DatPathTranslate = Path.GetDirectoryName(DatFileTranslate) + @"\";//get local file path only
                FullBinPathTranslate = DatFileTranslate + @".files\" + (tIs64 ? "localfile64.bin" : "localfile.bin"); //get full file path and add .files

                OutPathBinTranslate = DatPathTranslate + (tIs64 ? @"Translation64\" : @"Translation\");
                txbExportTranslate.Text = OutPathBinTranslate;
            }
        }

        private void btn_textract_Click(object sender, EventArgs e)
        {
            if (File.Exists(DatFileTranslate))
            {
                RunWithWorker(((o, args) =>
                {
                    new BNSDat().Extract(DatFileTranslate, (number, of) =>
                    {
                        richOut.Text = "Extracting Files: " + number + "/" + of;
                    }, tIs64);
                }));
            }
            else
            {
                MessageBox.Show("Select local file to extract.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnExportTranslate_Click(object sender, EventArgs e)
        {
            string saveFolder = OutPathBinTranslate + (tIs64 ? @"Translation64.xml" : @"Translation.xml");//original translation xml
            string usedfilepath = FullBinPathTranslate;//original bin file

            if (!File.Exists(usedfilepath))
            {
                MessageBox.Show("bin file not found make sure you have unpacked local file and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                if (!Directory.Exists(OutPathBinTranslate))
                    Directory.CreateDirectory(OutPathBinTranslate);

                RunWithWorker(((o, args) =>
                {
                    //BackgroundWorker backgroundWorker = o as BackgroundWorker;
                    BDat bdat = new BDat();
                    bdat.ExportTranslate(usedfilepath, saveFolder, BXML_TYPE.BXML_PLAIN, tIs64);
                }));
            }
        }

        private void btnSearchTranslateFile_Click(object sender, EventArgs e)
        {
            if (OfileTranslate.ShowDialog() != DialogResult.OK)
                return;

            FileTranslate = OfileTranslate.FileName;
            txbImportTranslate.Text = FileTranslate;
        }

        private void btnMergeTranslate_Click(object sender, EventArgs e)
        {
            string translatedpath = OutPathBinTranslate + (tIs64 ? @"Translation64.xml" : @"Translation.xml");//exported xml

            if (!File.Exists(translatedpath))
            {
                MessageBox.Show("Original translation.xml file not found make sure you have unpacked local file and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (!File.Exists(FileTranslate))
            {
                MessageBox.Show("Merge translation.xml file not found make sure you have selected it and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                Console.WriteLine("\rMerging translation...");
                RunWithWorker(((o, args) =>
                {
                    TranslateReader translateControl_Na = new TranslateReader();
                    translateControl_Na.Load(FileTranslate);//translated xml

                    TranslateReader translateControl_Org = new TranslateReader();
                    translateControl_Org.Load(translatedpath, true);//                

                    translateControl_Org.MergeTranslation(translateControl_Na, translatedpath);
                }));
            }
        }

        private void btn_Translate_Click(object sender, EventArgs e)
        {
            string local = DatPathTranslate;//extracted local folder
            string xml = OutPathBinTranslate + (tIs64 ? @"Translation64.xml" : @"Translation.xml");//translated xml

            if (!File.Exists(xml))
            {
                MessageBox.Show("Original translation.xml file not found make sure you have unpacked local file and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (!Directory.Exists(local))
            {
                MessageBox.Show("Unpacked local files not found make sure you have unpacked it and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                RunWithWorker(((o, args) =>
                {
                    BDat bdat = new BDat();
                    bdat.Translate(local, xml, tIs64);
                }));
            }
        }

        private void btn_pack_Click(object sender, EventArgs e)
        {
            string usedfilepath = DatPathTranslate;
            DatPathExtractTranslate = DatPathTranslate + (tIs64 ? @"local64.dat.files" : @"local.dat.files");

            if (!Directory.Exists(usedfilepath))
                MessageBox.Show("Repack folder not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            if (!File.Exists(usedfilepath + (tIs64 ? @"localfile64_new.bin" : @"localfile_new.bin")))
                MessageBox.Show("_new.bin file not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
            {
                // Copy one file to a location where there is a file.
                File.Copy(usedfilepath + (tIs64 ? "localfile64_new.bin" : "localfile_new.bin"),
                        DatPathExtractTranslate + (tIs64 ? @"\localfile64.bin" : @"\localfile.bin"), true); // overwrite = true
                if (cboxtbackup.Checked)
                    BackUpManager(DatFileTranslate, usedfilepath, DatFileNameTranslate, tIs64, FILEWORKER_TYPE.TRANSLATE);
                else
                    CompileManager(DatPathExtractTranslate, tIs64);
            }
        }
    }
}
