using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
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


        private string BackPath = "backup\\";
        private string RepackPath;
        private string OutPath;
        private string DatfileName;
        private string FulldatPath;

        private string OutPathBin;
        private string BinfileName;
        private string FullBinPath;

        TextWriter _writer = null;

        public BackgroundWorker ebnsdat;
        public BackgroundWorker cbnsdat;

        public bool DatIs64 = false;

        public BnsDatTool()
        {
            InitializeComponent();
        }

        private void BnsDatTool_Load(object sender, EventArgs e)
        {
            _writer = new StrWriter(richOut);
            Console.SetOut(_writer);
        }

        private void bntSearchDat_Click(object sender, EventArgs e)
        {

            if (OfileDat.ShowDialog() != DialogResult.OK)
                return;
            // Check if 64bit or 32bit
            

            txbDatFile.Text = OfileDat.FileName;
            if (cB_output.Checked == true)
            {
                FulldatPath = OfileDat.FileName;
                DatfileName = OfileDat.SafeFileName;
                OutPath = FulldatPath + ".files"; //get full file path and add .files
                RepackPath = FulldatPath.Replace(DatfileName, "");//get working dir
                txbRpFolder.Text = OutPath;
            }

            if (FulldatPath.Contains("64"))
                DatIs64 = true;

            // richOut.AppendText(FulldatPath);
        }

        private void bnSearchOut_Click(object sender, EventArgs e)
        {
            if (OfolderDat.ShowDialog() != DialogResult.OK)
                return;
            txbRpFolder.Text = OfolderDat.SelectedPath;
            OutPath = OfolderDat.SelectedPath;

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
            DatIs64 = true;
            cbnsdat = new BackgroundWorker();
            cbnsdat.WorkerSupportsCancellation = true;
            cbnsdat.WorkerReportsProgress = true;
            cbnsdat.DoWork += datextract_DoWork;
            cbnsdat.RunWorkerAsync();
        }

        private void datextract_DoWork(object sender, DoWorkEventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;

            new BNSDat().Extract(FulldatPath, (number, of) =>
            {
                richOut.Text = "Extracting Files: " + number + "/" + of;
            }, DatIs64);

            richOut.AppendText("\r\nDone!");

            GC.Collect();
        }

        private void BntStart_Click(object sender, EventArgs e)
        {
            richOut.Clear();
            if (cbnsdat != null && cbnsdat.IsBusy)
            {
                cbnsdat.CancelAsync();
            }
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

        public void Compiler(string repackPath)
        {
            if (repackPath == null)
                return;
            DatIs64 = true;
            ebnsdat = new BackgroundWorker();
            ebnsdat.WorkerSupportsCancellation = true;
            ebnsdat.WorkerReportsProgress = true;
            ebnsdat.DoWork += new DoWorkEventHandler(datcompress_DoWork);
            ebnsdat.RunWorkerAsync();
        }

        private void datcompress_DoWork(object sender, DoWorkEventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;

            new BNSDat().Compress(OutPath, (number, of) =>
            {
                richOut.Text = "Compressing Files: " + number + "/" + of;
            }, DatIs64, 9);

            richOut.AppendText("\r\nDone!!");
            GC.Collect();
        }

        private void btnRepack_Click(object sender, EventArgs e)
        {
            richOut.Clear();
            if (cbnsdat != null && cbnsdat.IsBusy)
            {
                cbnsdat.CancelAsync();
            }
            if (Cb_back.Checked)
            {
                if (string.IsNullOrEmpty(DatfileName) == false)
                {
                    try
                    {
                        //get current date and time
                        string date = DateTime.Now.ToString("dd-MM-yy_"); // includes leading zeros
                        string time = DateTime.Now.ToString("hh.mm.ss"); // includes leading zeros

                        // folder location
                        var BackDir = RepackPath + BackPath;

                        // if it doesn't exist, create
                        if (!Directory.Exists(BackDir))
                            Directory.CreateDirectory(BackDir);

                        //Create a new subfolder under the current active folder
                        string newPath = Path.Combine(BackDir, date + time);

                        // Create the subfolder
                        Directory.CreateDirectory(newPath);

                        // Copy file to backup folder
                        var CurrBackPath = newPath + "\\";
                        File.Copy(RepackPath + DatfileName, CurrBackPath + DatfileName, true);
                    }
                    catch
                    {
                        MessageBox.Show(".dat file not found can't create backup.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            if (Directory.Exists(OutPath))
                Compiler(OutPath);
            else
                MessageBox.Show("Select Folder to repack.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        //bin
        private bool binIs64 = false;

        private void RunWithWorker(string caption, DoWorkEventHandler doWork)
        {
            CheckForIllegalCrossThreadCalls = false;
            ProgressBarForm progress = new ProgressBarForm();
            try
            {
                progress.Text = caption;
                BackgroundWorker backgroundWorker = new BackgroundWorker();
                backgroundWorker.WorkerReportsProgress = true;
                backgroundWorker.DoWork += doWork;
                backgroundWorker.ProgressChanged += ((o, args) => progress.progressBar.Value = args.ProgressPercentage);
                backgroundWorker.RunWorkerCompleted += ((o, args) => progress.Close());
                backgroundWorker.RunWorkerAsync();
                int num = (int)progress.ShowDialog();
            }
            finally
            {
                if (progress != null)
                    progress.Dispose();
            }
        }

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

            // richOut.AppendText(FulldatPath);
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
            RunWithWorker(btnDump.Text, ((o, args) =>
            {
                BDat bdat = new BDat();
                bdat.Dump(FullBinPath, OutPathBin, BXML_TYPE.BXML_PLAIN, binIs64);
            }));
        }

        //translation
        OpenFileDialog OfileTranslate = new OpenFileDialog();

        private string OutPathBinTranslate;//bin output path
        private string FullBinPathTranslate;//bin file
        private string FileTranslate;//translated xml file path
        private string DatFileTranslate;//local.dat file
        private string DatPathTranslate;//local.dat path only

        private bool tIs64 = false;

        private void btn_SearchtLocal_Click(object sender, EventArgs e)
        {
            if (OfileDat.ShowDialog() != DialogResult.OK)
                return;

            DatFileTranslate = OfileDat.FileName;//get local file file

            if (DatFileTranslate.Contains("64"))
                tIs64 = true;

            txb_tlocal.Text = DatFileTranslate;//set local file to textbox
            DatPathTranslate = Path.GetDirectoryName(DatFileTranslate);//get local file path only
            FullBinPathTranslate = DatFileTranslate + @".files\" + (tIs64 ? "localfile64.bin" : "localfile.bin"); //get full file path and add .files
            OutPathBinTranslate = DatPathTranslate + @"\Translation\";

            txbExportTranslate.Text = OutPathBinTranslate;
            richOut.AppendText(DatPathTranslate);
        }

        private void btn_textract_Click(object sender, EventArgs e)
        {
            RunWithWorker(btn_textract.Text, ((o, args) =>
            {
                new BNSDat().Extract(DatFileTranslate, (number, of) =>
                {
                    richOut.Text = "Extracting Files: " + number + "/" + of;
                }, tIs64);
            }));
        }

        private void btnSearchExportTranslate_Click(object sender, EventArgs e)
        {
            if (OfolderBin.ShowDialog() != DialogResult.OK)
                return;
            txbExportTranslate.Text = OfolderBin.SelectedPath;
            OutPathBin = OfolderBin.SelectedPath;
        }

        private void btnExportTranslate_Click(object sender, EventArgs e)
        {
            Directory.CreateDirectory(OutPathBinTranslate);

            string saveFolder = OutPathBinTranslate + @"translation.xml";//original translation xml
            string usedfilepath = FullBinPathTranslate;//original bin file

            RunWithWorker(btnExportTranslate.Text, ((o, args) =>
            {
                //BackgroundWorker backgroundWorker = o as BackgroundWorker;
                BDat bdat = new BDat();
                bdat.ExportTranslate(usedfilepath, saveFolder, BXML_TYPE.BXML_PLAIN, tIs64);
            }));
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
            string translatedpath = OutPathBinTranslate + @"translation.xml";//exported xml
            RunWithWorker(btnMergeTranslate.Text, ((o, args) =>
            {
                TranslateReader translateControl_Na = new TranslateReader();
                translateControl_Na.Load(FileTranslate);//translated xml

                TranslateReader translateControl_Org = new TranslateReader();
                translateControl_Org.Load(translatedpath, true);//                

                translateControl_Org.MergeTranslation(translateControl_Na, translatedpath);
            }));
        }

        private void btn_Translate_Click(object sender, EventArgs e)
        {
            string local = DatPathTranslate + @"\";//extracetd local folder
            string xml = OutPathBinTranslate + @"translation.xml";//translated xml

            RunWithWorker(btn_Translate.Text, ((o, args) =>
            {
                BDat bdat = new BDat();
                bdat.Translate(local, xml, tIs64);
            }));
        }

        private void btn_pack_Click(object sender, EventArgs e)
        {
            string usedfilepath = DatPathTranslate + @"\";

            RunWithWorker(btn_pack.Text, ((o, args) =>
            {
                //BackgroundWorker backgroundWorker = o as BackgroundWorker;

                // Copy one file to a location where there is a file.
                File.Copy(usedfilepath + (tIs64 ? "localfile64_new.bin" : "localfile_new.bin"), 
                    usedfilepath + (tIs64 ? @"local64.dat.files\localfile64.bin" : @"local.dat.files\localfile.bin"), true); // overwrite = true

                BNSDat m_bnsDat = new BNSDat();

                new BNSDat().Compress(usedfilepath + (tIs64 ? "local64.dat.files" : "local.dat.files"), (number, of) =>
                {
                    richOut.Text = "Compressing Files: " + number + "/" + of;
                }, tIs64, 9);
            }));
        }
    }
}
