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
                multiworker = new BackgroundWorker
                {
                    WorkerReportsProgress = true
                };
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
                GboxTools.Enabled = true;
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


        public bool tIs64 = false;
        
        private string DatPathExtractTranslate;
        //Source
        string Sourcelocaldat = "";
        string SourcelocaldatPath = "";
        string SourcelocalFileName = "";
        string SourceMergeFilePath = "";
        string SourceMergeFile = "";
        string SourceBinFile = "";
        //Target
        string Targetlocaldat = "";
        string TargetlocaldatPath = "";
        string TargetlocalFileName = "";
        string TargetMergeFilePath = "";
        string TargetMergeFile = "";
        string TargetBinFile = "";
        BackgroundWorker worker;
        private void BtnSTarget_Click(object sender, EventArgs e)
        {
            if (OfileTranslate.ShowDialog() != DialogResult.OK)
                return;
            else
            {
                if (OfileTranslate.FileName.Contains("64"))
                    tIs64 = true;
                else
                    tIs64 = false;

                Targetlocaldat = OfileTranslate.FileName;//get local file
                TxblocalTarget.Text = Targetlocaldat;//set local file to textbox
                TargetlocalFileName = Path.GetFileName(Targetlocaldat);
                TargetlocaldatPath = Path.GetDirectoryName(Targetlocaldat) + @"\";//get local file path only
                TargetBinFile = Targetlocaldat + @".files\" + (tIs64 ? "localfile64.bin" : "localfile.bin"); //get full file path and add .files

                TargetMergeFilePath = TargetlocaldatPath + (tIs64 ? @"Translation64\" : @"Translation\");
                TargetMergeFile = TargetMergeFilePath + (tIs64 ? @"Translation64.xml" : @"Translation.xml");//original translation xml
            }
        }

        private void BtnSSource_Click(object sender, EventArgs e)
        {
            if (OfileTranslate.ShowDialog() != DialogResult.OK)
                return;
            else
            {
                if (OfileTranslate.FileName.Contains("64"))
                    tIs64 = true;
                else
                    tIs64 = false;

                Sourcelocaldat = OfileTranslate.FileName;//get local file
                TxblocalSource.Text = Sourcelocaldat;//set local file to textbox
                SourcelocalFileName = Path.GetFileName(Sourcelocaldat);
                SourcelocaldatPath = Path.GetDirectoryName(Sourcelocaldat) + @"\";//get local file path only
                SourceBinFile = Sourcelocaldat + @".files\" + (tIs64 ? "localfile64.bin" : "localfile.bin"); //get full file path and add .files

                SourceMergeFilePath = SourcelocaldatPath + (tIs64 ? @"Translation64\" : @"Translation\");
                SourceMergeFile = SourceMergeFilePath + (tIs64 ? @"Translation64.xml" : @"Translation.xml");//original translation xml
            }
        }

        private void BtnSTranslate_Click(object sender, EventArgs e)
        {
            GboxTools.Enabled = false;
            if(TargetlocaldatPath == SourcelocaldatPath)
            {
                MessageBox.Show("You Can't Translate Using The Same Files.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                GboxTools.Enabled = true;
                return;
            }
            if (File.Exists(Targetlocaldat))
            {
                CheckForIllegalCrossThreadCalls = false;
                worker = new BackgroundWorker
                {
                    WorkerSupportsCancellation = true
                };
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(TargetExtract_Status);
                worker.DoWork += TargetExtract;
                worker.RunWorkerAsync();
            }
            else
            {
                MessageBox.Show("Select Targe local.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        void TargetExtract(object sender, DoWorkEventArgs e)
        {
            new BNSDat().Extract(Targetlocaldat, (number, of) =>
            {
                richOut.Text = "Extracting Files: " + number + "/" + of;
            }, tIs64);
        }
        private void TargetExtract_Status(object sender, RunWorkerCompletedEventArgs e)
        {

            if (e.Cancelled == true)
            {
                // workerstatus = Status.Canceled;
            }
            else if (e.Error != null)
            {
                // workerstatus = Status.Error;
            }
            else
            {
                if (File.Exists(Sourcelocaldat))
                {
                    // workerstatus = Status.Done;
                    CheckForIllegalCrossThreadCalls = false;
                    worker = new BackgroundWorker
                    {
                        WorkerSupportsCancellation = true
                    };
                    worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(SorceExtract_Status);
                    worker.DoWork += SorceExtract;
                    worker.RunWorkerAsync();
                }
                else
                {
                    MessageBox.Show("Select Source local file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
        void SorceExtract(object sender, EventArgs e)
        {
            new BNSDat().Extract(Sourcelocaldat, (number, of) =>
            {
                richOut.Text = "Extracting Files: " + number + "/" + of;
            }, tIs64);
        }
        private void SorceExtract_Status(object sender, RunWorkerCompletedEventArgs e)
        {

            if (e.Cancelled == true)
            {
                // workerstatus = Status.Canceled;

            }
            else if (e.Error != null)
            {
                // workerstatus = Status.Error;
            }
            else
            {

                if (!Directory.Exists(TargetMergeFilePath))
                    Directory.CreateDirectory(TargetMergeFilePath);
                // workerstatus = Status.Done;
                CheckForIllegalCrossThreadCalls = false;
                worker = new BackgroundWorker
                {
                    WorkerSupportsCancellation = true
                };
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(TargetExport_Status);
                worker.DoWork += TargetExport;
                worker.RunWorkerAsync();
            }
        }
        private void TargetExport(object sender, EventArgs e)
        {
            BDat bdat = new BDat();
            bdat.ExportTranslate(TargetBinFile, TargetMergeFile, BXML_TYPE.BXML_PLAIN, tIs64);
        }

        private void TargetExport_Status(object sender, RunWorkerCompletedEventArgs e)
        {

            if (e.Cancelled == true)
            {
                // workerstatus = Status.Canceled;

            }
            else if (e.Error != null)
            {
                // workerstatus = Status.Error;
            }
            else
            {

                if (!Directory.Exists(SourceMergeFilePath))
                    Directory.CreateDirectory(SourceMergeFilePath);
                // workerstatus = Status.Done;
                CheckForIllegalCrossThreadCalls = false;
                worker = new BackgroundWorker
                {
                    WorkerSupportsCancellation = true
                };
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(SourceExport_Status);
                worker.DoWork += SourceExport;
                worker.RunWorkerAsync();
            }
        }
        void SourceExport(object sender, EventArgs e)
        {
            BDat bdat = new BDat();
            bdat.ExportTranslate(SourceBinFile, SourceMergeFile, BXML_TYPE.BXML_PLAIN, tIs64);
        }
        private void SourceExport_Status(object sender, RunWorkerCompletedEventArgs e)
        {

            if (e.Cancelled == true)
            {
                // workerstatus = Status.Canceled;

            }
            else if (e.Error != null)
            {
                // workerstatus = Status.Error;
            }
            else
            {
                // workerstatus = Status.Done;
                CheckForIllegalCrossThreadCalls = false;
                worker = new BackgroundWorker
                {
                    WorkerSupportsCancellation = true
                };
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(TranslateMerge_Status);
                worker.DoWork += TranslateMerge;
                worker.RunWorkerAsync();
            }
        }
        void TranslateMerge(object sender, EventArgs e)
        {
            Console.WriteLine("\rMerging translation...");

            TranslateReader translateControl_Na = new TranslateReader();
            translateControl_Na.Load(SourceMergeFile);//Source xml

            TranslateReader translateControl_Org = new TranslateReader();
            translateControl_Org.Load(TargetMergeFile, true);//Target xml          

            translateControl_Org.MergeTranslation(translateControl_Na, TargetMergeFile);//Target xml
        }

        private void TranslateMerge_Status(object sender, RunWorkerCompletedEventArgs e)
        {

            if (e.Cancelled == true)
            {
                // workerstatus = Status.Canceled;

            }
            else if (e.Error != null)
            {
                // workerstatus = Status.Error;
            }
            else
            {

                // workerstatus = Status.Done;
                CheckForIllegalCrossThreadCalls = false;
                worker = new BackgroundWorker
                {
                    WorkerSupportsCancellation = true
                };
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(TranslateFile_Status);
                worker.DoWork += TranslateFile;
                worker.RunWorkerAsync();

            }
        }

        void TranslateFile(object sender, EventArgs e)
        {
            BDat bdat = new BDat();
            bdat.Translate(TargetlocaldatPath, TargetMergeFile, tIs64);
        }

        private void TranslateFile_Status(object sender, RunWorkerCompletedEventArgs e)
        {

            if (e.Cancelled == true)
            {
                // workerstatus = Status.Canceled;

            }
            else if (e.Error != null)
            {
                // workerstatus = Status.Error;
            }
            else
            {
                // workerstatus = Status.Done;
                CheckForIllegalCrossThreadCalls = false;
                worker = new BackgroundWorker
                {
                    WorkerSupportsCancellation = true
                };
                //worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(SourceExport_Status);
                worker.DoWork += TranslatePackFile;
                worker.RunWorkerAsync();
            }
        }

        void TranslatePackFile(object sender, EventArgs e)
        {
            DatPathExtractTranslate = TargetlocaldatPath + (tIs64 ? @"local64.dat.files" : @"local.dat.files");
            // Copy one file to a location where there is a file.
            File.Copy(TargetlocaldatPath + (tIs64 ? "localfile64_new.bin" : "localfile_new.bin"),
                    DatPathExtractTranslate + (tIs64 ? @"\localfile64.bin" : @"\localfile.bin"), true); // overwrite = true
            if (cboxtbackup.Checked)
                BackUpManager(Targetlocaldat, TargetlocaldatPath, TargetlocalFileName, tIs64, FILEWORKER_TYPE.TRANSLATE);
            else
                CompileManager(DatPathExtractTranslate, tIs64);
        }
    }
}
