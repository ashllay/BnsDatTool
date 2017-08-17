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
        OpenFileDialog OfileTranslate = new OpenFileDialog();

        private string BackPath = "backup\\";
        private string RepackPath;
        private string OutPath;
        private string DatfileName;
        private string FulldatPath;

        private string OutPathBin;
        private string BinfileName;
        private string FullBinPath;

        private string OutPathBinTranslate;
        private string BinfileNameTranslate;
        private string FullBinPathTranslate;
        private string FileTranslate;

        TextWriter _writer = null;

        public BackgroundWorker ebnsdat;
        public BackgroundWorker cbnsdat;

        public bool Datis64;

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

            txbDatFile.Text = OfileDat.FileName;
            if (cB_output.Checked == true)
            {
                FulldatPath = OfileDat.FileName;
                DatfileName = OfileDat.SafeFileName;
                OutPath = FulldatPath + ".files"; //get full file path and add .files
                RepackPath = FulldatPath.Replace(DatfileName, "");//get working dir
                txbRpFolder.Text = OutPath;
            }
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
            Datis64 = is64Dat.Checked == true;
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
            }, Datis64);

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
            Datis64 = is64Dat.Checked == true;
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
            }, Datis64, 9);

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
        private void RunWithWorker(string caption, DoWorkEventHandler doWork)
        {
            //ProgressBarForm progress = new ProgressBarForm();
            try
            {
                //pTimer.Start();
                //progress.Text = caption;
                CheckForIllegalCrossThreadCalls = false;
                BackgroundWorker backgroundWorker = new BackgroundWorker();
                backgroundWorker.WorkerReportsProgress = true;
                backgroundWorker.DoWork += doWork;
               // backgroundWorker.ProgressChanged += ((o, args) => progressBar.Value = args.ProgressPercentage);
               // backgroundWorker.RunWorkerCompleted += ((o, args) => pTimer.Stop());
                backgroundWorker.RunWorkerAsync();
                //int num = (int)progress.ShowDialog();
            }
            finally
            {
                //pTimer.Stop();
                //  if (progress != null)
                //      progress.Dispose();
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
                bdat.Dump(FullBinPath, OutPathBin, BXML_TYPE.BXML_PLAIN, is64Bin.Checked);
            }));
        }

        //translation
        private void btnSearchBinTranslate_Click(object sender, EventArgs e)
        {
            if (OfileBin.ShowDialog() != DialogResult.OK)
                return;

            txbBinTranslate.Text = OfileBin.FileName;
            FullBinPathTranslate = OfileBin.FileName;

            if (cboxGetBinFolderTranslate.Checked == true)
            {
                BinfileNameTranslate = OfileBin.SafeFileName;
                OutPathBinTranslate = FullBinPathTranslate + ".files"; //get full file path and add .files
                txbExportTranslate.Text = OutPathBinTranslate;
            }
        }

        private void btnSearchExportTranslate_Click(object sender, EventArgs e)
        {
            if (OfolderBin.ShowDialog() != DialogResult.OK)
                return;
            txbExportTranslate.Text = OfolderBin.SelectedPath;
            OutPathBin = OfolderBin.SelectedPath;
        }

        private void btnSearchTranslateFile_Click(object sender, EventArgs e)
        {
            if (OfileTranslate.ShowDialog() != DialogResult.OK)
                return;

            txbImportTranslate.Text = OfileTranslate.FileName;
            FileTranslate = OfileTranslate.FileName;
        }

        private void btnExportTranslate_Click(object sender, EventArgs e)
        {
            Directory.CreateDirectory(OutPathBinTranslate);
            RunWithWorker(btnExportTranslate.Text, ((o, args) =>
            {
                BDat bdat = new BDat();
                bdat.ExportTranslate(FullBinPathTranslate, OutPathBinTranslate + @"\translation.xml", BXML_TYPE.BXML_PLAIN, cboxIs64Translate.Checked);
            }));
        }

        private void btnMergeTranslate_Click(object sender, EventArgs e)
        {
            //string translatedpath = AppDomain.CurrentDomain.BaseDirectory + @"datas\" + GetXmlRegion();
            RunWithWorker(btnMergeTranslate.Text, ((o, args) =>
            {
                TranslateReader translateControl_Na = new TranslateReader();
                translateControl_Na.Load(FileTranslate);

                TranslateReader translateControl_Org = new TranslateReader();
                //translateControl_Org.Load(translatedpath, true);//                

               // translateControl_Org.MergeTranslation(translateControl_Na, translatedpath);
            }));            
        }
    }
}
