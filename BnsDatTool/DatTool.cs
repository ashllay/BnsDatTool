using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public static byte[] LkAES;
        public static byte[] LkXor;
        public BackgroundWorker multiworker;

        public bool DatIs64 = false;

        public BnsDatTool()
        {
            InitializeComponent();
        }
        private void BnsDatTool_Load(object sender, EventArgs e)
        {
            tabPage3.Enabled = false;
            _writer = new StrWriter(richOut);
            Console.SetOut(_writer);
            FillCbox(CboxDatReg);
            FillCbox(CboxManualTranslateRegion);
            FillCbox(CboxRegTarget);
            FillCbox(CboxRegSource);
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

        void FillCbox(ComboBox Cbox)
        {
            Dictionary<string, int> dict = new Dictionary<string, int>()
            {
                {"bns_obt_kr_2014#",0},
                {"ja#n_2@020_compl",1},
                {"DatV3",2},
                {"DatV3_xmlV4",3},
                {"PlayBNS",4},
                {"Tiberium",5},
                {"CBT-2013",6},
                {"NEO",7}
            };

            Cbox.DataSource = new BindingSource(dict, null);
            Cbox.DisplayMember = "Key";
            Cbox.ValueMember = "Value";
        }
        private void bntSearchDat_Click(object sender, EventArgs e)
        {
            //string result = Encoding.UTF8.GetString(LkAES);
            //Console.WriteLine(result);
            //byte[] bytes = Encoding.ASCII.GetBytes("bns_fgt_cb_2010!");
            //string hex = BitConverter.ToString(LkXor);
            OfileDat.Filter = "dat file(*.dat)|*.dat";
            if (OfileDat.ShowDialog() != DialogResult.OK)
                return;

            txbDatFile.Text = OfileDat.FileName;
            FulldatPath = OfileDat.FileName;
            DatfileName = OfileDat.SafeFileName;

            // Check if 64bit or 32bit
            //DatIs64 = FulldatPath.Contains("64");
            if (Cbox64Bit.Checked)
                DatIs64 = true;
            if (cB_output.Checked != true) return;
            RepackPath = Path.GetDirectoryName(FulldatPath) + @"\";//get working dir
            OutPath = FulldatPath + ".files"; //get full file path and add .files
            txbRpFolder.Text = OutPath;
        }

        private void bnSearchOut_Click(object sender, EventArgs e)
        {
            if (OfolderDat.ShowDialog() != DialogResult.OK)
                return;

            txbRpFolder.Text = OfolderDat.SelectedPath;
            OutPath = OfolderDat.SelectedPath;

            //DatIs64 = OutPath.Contains("64");
            if (Cbox64Bit.Checked)
                DatIs64 = true;
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
                    new BNSDat().Extract(datFile, (number, of) =>
                    {
                        richOut.Text = "Extracting Files: " + number + "/" + of;
                    }, DatIs64);
                    GboxTools.Enabled = true;
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        void SelectAES(int iIndex)
        {
            switch (iIndex)
            {
                case 0:
                    LkAES = new byte[] { 98, 110, 115, 95, 111, 98, 116, 95, 107, 114, 95, 50, 48, 49, 52, 35 };
                    break;
                case 1://106,97,35,110,95,50,64,48,50,48,95,99,111,109,112,108
                    LkAES = new byte[] { 106, 97, 35, 110, 95, 50, 64, 48, 50, 48, 95, 99, 111, 109, 112, 108 };
                    break;
                case 2:
                    LkAES = new byte[] { 23, 81, 170, 213, 30, 54, 74, 27, 254, 96, 116, 231, 208, 133, 7, 104 };
                    BNSDat.newversion = true;
                    break;
                case 3:
                    LkAES = new byte[] { 23, 81, 170, 213, 30, 54, 74, 27, 254, 96, 116, 231, 208, 133, 7, 104 };
                    LkXor = new byte[] { 240, 200, 186, 170, 18, 31, 130, 159, 172, 24, 84, 33, 138, 58 };
                    BNSDat.newversion = true;
                    break;
                case 4:
                    LkAES = new byte[] { 42, 80, 108, 97, 121, 66, 78, 83, 40, 99, 41, 50, 48, 49, 52, 42 };
                    break;
                case 5:
                    LkAES = new byte[] { 46, 46, 46, 46, 46, 33, 46, 44, 46, 46, 44, 46, 63, 46, 46, 124 };
                    break;
                case 6://bns_fgt_cb_2010!
                    LkAES = new byte[] { 98, 110, 115, 95, 102, 103, 116, 95, 99, 98, 95, 50, 48, 49, 48, 33 };
                    break;
                case 7://BNSR#XOR@Encrypt$Key
                    LkAES = new byte[] { 66, 78, 83, 82, 35, 88, 79, 82, 64, 69, 110, 99, 114, 121, 112, 116, 36, 75, 101, 121 };
                    break;
            }

            if (iIndex != 3)
            {
                LkXor = new byte[] { 164, 159, 216, 179, 246, 142, 57, 194, 45, 224, 97, 117, 92, 75, 26, 7 };
                string hex = BitConverter.ToString(LkAES);

                //string result = Encoding.UTF8.GetString(LkAES);
                //Console.WriteLine(result);
                //byte[] bytes = Encoding.ASCII.GetBytes("bns_fgt_cb_2010!");
                //string hex = BitConverter.ToString(LkAES);
                //Console.WriteLine(hex);
                //byte[] bytes = Encoding.Unicode.GetBytes("儗햪㘞ᭊ惾藐标");

            }


            if (multiworker != null && multiworker.IsBusy)
                multiworker.CancelAsync();
        }

        private void BntStart_Click(object sender, EventArgs e)
        {
            if (Cbox64Bit.Checked)
                DatIs64 = true;

            if (CboxDatReg.SelectedItem == null)
            {
                MessageBox.Show("Select game region!.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            SelectAES(CboxDatReg.SelectedIndex);
            //string result = Encoding.UTF8.GetString(LkAES);
            // Console.WriteLine(result);
            //byte[] bytes = Encoding.ASCII.GetBytes("bns_fgt_cb_2010!");
            //string hex = BitConverter.ToString(LkAES);
            //richOut.Text = hex;
            try
            {
                if (File.Exists(FulldatPath))
                {
                    Extractor(FulldatPath);
                    GboxTools.Enabled = false;
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
            if (CboxDatReg.SelectedIndex == -1)
            {
                MessageBox.Show("Select game region!.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            SelectAES(CboxDatReg.SelectedIndex);

            //if (CboxDatReg.SelectedIndex == 2||CboxDatReg.SelectedIndex == 3)
            //{
            //    MessageBox.Show("You can't repack V3 files.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}
            if (multiworker != null && multiworker.IsBusy)
                multiworker.CancelAsync();
            GboxTools.Enabled = false;
            if (Cb_back.Checked)
                BackUpManager(FulldatPath, RepackPath, DatfileName, DatIs64, 0);
            else
                CompileManager(OutPath, DatIs64);

        }

        private void BackUpManager(string filepath, string dir, string filename, bool is64, int wtype)
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
            if (wtype == 0)
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
                //BNSDat m_bnsDat = new BNSDat();
                new BNSDat().Compress(repackPath, (number, of) =>
                {
                    richOut.Text = "Compressing Files: " + number + "/" + of;
                }, is64, 9);
                GboxTools.Enabled = true;
            }));
        }

        //bin
        private bool binIs64 = false;
        string BinDir;
        private void btnSeaarchBin_Click(object sender, EventArgs e)
        {
            OfileBin.Filter = "bin file(*.bin)|*.bin";
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
            BinDir = FullBinPath.Replace(OfileBin.SafeFileName, "");
            binIs64 = BinfileName.Contains("64");
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

        private void BntPack_Click(object sender, EventArgs e)
        {
            BDat Bnsdat = new BDat();
            // using (var file = new FileStream(@"C:\Temp\bnsbin\datafile.bin", FileMode.Open))
            // {
            //     BinaryReader br = new BinaryReader(file);
            //     bd.Load(br, BXML_TYPE.BXML_PLAIN, false);
            // }

            //var modify = bd._content.Lists.ToList().First(x => x.ID == 111);
            //var fields = modify.Collection.Loose.Fields;
            //for (int i = 0; i < fields.Length; i++)
            //{
            //    if (fields[i] != null)
            //    {
            //        //Clear Achivement Data
            //        for (int j = 67; j < 88; j++)
            //        {
            //            fields[i].Data[j] = 0;
            //        }
            //    }
            //}

            //using (var file = new FileStream(@"C:\Temp\bnsbin\data.bin", FileMode.Create, FileAccess.Write))
            //{
            //    BinaryWriter bw = new BinaryWriter(file);
            //    bd.Save(bw, BXML_TYPE.BXML_PLAIN, false);
            //}

            //Console.Write("\rTranslating: Content...");
            // string bin = is64 ? @"local64.dat.files\localfile64.bin" : @"local.dat.files\localfile.bin";
            FileStream fs = new FileStream(FullBinPath, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);
            Bnsdat.Load(br, BXML_TYPE.BXML_BINARY, false);


            //var modify = Bnsdat._content.Lists.ToList().First(x => x.ID == 111);
            //var fields = modify.Collection.Loose.Fields;
            //for (int i = 0; i < fields.Length; i++)
            //{
            //    if (fields[i] != null)
            //    {
            //        //Clear Achivement Data
            //        for (int j = 67; j < 88; j++)
            //        {
            //            fields[i].Data[j] = 0;
            //        }
            //    }
            //}

            br.Close();

            //BinaryWriter bw = new BinaryWriter(File.Open(BinDir+@"\datafile_new.bin", FileMode.Create));

            //Bnsdat.Save(bw, BXML_TYPE.BXML_BINARY, false);
            //bw.Close();
            Console.WriteLine("\rDone!!");
        }
        //translation
        OpenFileDialog OfileTranslate = new OpenFileDialog();


        public bool tIs64 = false;
        public bool isDatafile = false;

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
            OfileTranslate.Filter = "dat file(*.dat)|*.dat";
            if (OfileTranslate.ShowDialog() != DialogResult.OK)
                return;
            tIs64 = OfileTranslate.FileName.Contains("64");

            Targetlocaldat = OfileTranslate.FileName;//get local file
            TxblocalTarget.Text = Targetlocaldat;//set local file to textbox
            TargetlocalFileName = Path.GetFileName(Targetlocaldat);
            TargetlocaldatPath = Path.GetDirectoryName(Targetlocaldat) + @"\";//get local file path only
            TargetBinFile = Targetlocaldat + @".files\" + (tIs64 ? "localfile64.bin" : "localfile.bin"); //get full file path and add .files

            TargetMergeFilePath = TargetlocaldatPath + (tIs64 ? @"Translation64\" : @"Translation\");
            TargetMergeFile = TargetMergeFilePath + (tIs64 ? @"Translation64.xml" : @"Translation.xml");//original translation xml
        }

        private void BtnSSource_Click(object sender, EventArgs e)
        {
            OfileTranslate.Filter = "dat file(*.dat)|*.dat";
            if (OfileTranslate.ShowDialog() != DialogResult.OK)
                return;

            tIs64 = OfileTranslate.FileName.Contains("64");

            Sourcelocaldat = OfileTranslate.FileName;//get local file
            TxblocalSource.Text = Sourcelocaldat;//set local file to textbox
            SourcelocalFileName = Path.GetFileName(Sourcelocaldat);
            SourcelocaldatPath = Path.GetDirectoryName(Sourcelocaldat) + @"\";//get local file path only
            SourceBinFile = Sourcelocaldat + @".files\" + (tIs64 ? "localfile64.bin" : "localfile.bin"); //get full file path and add .files

            SourceMergeFilePath = SourcelocaldatPath + (tIs64 ? @"Translation64\" : @"Translation\");
            SourceMergeFile = SourceMergeFilePath + (tIs64 ? @"Translation64.xml" : @"Translation.xml");//original translation xml

        }

        private void BtnSTranslate_Click(object sender, EventArgs e)
        {

            GboxTools.Enabled = false;
            if (string.IsNullOrWhiteSpace(TargetlocaldatPath) || string.IsNullOrWhiteSpace(SourcelocaldatPath))
            {
                MessageBox.Show("You need to select source and target files to translate.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                GboxTools.Enabled = true;
                return;
            }
            if (TargetlocaldatPath == SourcelocaldatPath)
            {
                MessageBox.Show("You Can't Translate Using The Same Files.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                GboxTools.Enabled = true;
                return;
            }
            if (CboxRegTarget.SelectedItem == null || CboxRegSource.SelectedItem == null)
            {
                MessageBox.Show("You need to select game region(source and target).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                worker.RunWorkerCompleted += TargetExtract_Status;
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
            SelectAES(CboxRegTarget.SelectedIndex);
            new BNSDat().Extract(Targetlocaldat, (number, of) =>
            {
                richOut.Text = "Extracting Files: " + number + "/" + of;
            }, tIs64);
        }
        private void TargetExtract_Status(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            { // workerstatus = Status.Canceled;
            }
            else if (e.Error != null)
            {// workerstatus = Status.Error;
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
            SelectAES(CboxRegSource.SelectedIndex);
            new BNSDat().Extract(Sourcelocaldat, (number, of) =>
            {
                richOut.Text = "Extracting Files: " + number + "/" + of;
            }, tIs64);
        }
        private void SorceExtract_Status(object sender, RunWorkerCompletedEventArgs e)
        {

            if (e.Cancelled)
            { // workerstatus = Status.Canceled;
            }
            else if (e.Error != null)
            {// workerstatus = Status.Error;
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
                worker.RunWorkerCompleted += TargetExport_Status;
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

            if (e.Cancelled)
            {// workerstatus = Status.Canceled;
            }
            else if (e.Error != null)
            {// workerstatus = Status.Error;
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
                worker.RunWorkerCompleted += SourceExport_Status;
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

            if (e.Cancelled)
            {// workerstatus = Status.Canceled;
            }
            else if (e.Error != null)
            { // workerstatus = Status.Error;
            }
            else
            {// workerstatus = Status.Done;
                CheckForIllegalCrossThreadCalls = false;
                worker = new BackgroundWorker
                {
                    WorkerSupportsCancellation = true
                };
                worker.RunWorkerCompleted += TranslateMerge_Status;
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

            if (e.Cancelled)
            { // workerstatus = Status.Canceled;
            }
            else if (e.Error != null)
            {// workerstatus = Status.Error;
            }
            else
            {
                // workerstatus = Status.Done;
                CheckForIllegalCrossThreadCalls = false;
                worker = new BackgroundWorker
                {
                    WorkerSupportsCancellation = true
                };
                worker.RunWorkerCompleted += TranslateFile_Status;
                worker.DoWork += TranslateFile;
                worker.RunWorkerAsync();
            }
        }

        void TranslateFile(object sender, EventArgs e)
        {
            BDat bdat = new BDat();
            bdat.Translate(TargetlocaldatPath, TargetMergeFile, isDatafile, tIs64);
        }

        private void TranslateFile_Status(object sender, RunWorkerCompletedEventArgs e)
        {

            if (e.Cancelled == true)
            {// workerstatus = Status.Canceled;
            }
            else if (e.Error != null)
            { // workerstatus = Status.Error;
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
            SelectAES(CboxRegTarget.SelectedIndex);

            DatPathExtractTranslate = TargetlocaldatPath + (tIs64 ? @"local64.dat.files" : @"local.dat.files");
            // Copy one file to a location where there is a file.
            File.Copy(TargetlocaldatPath + (tIs64 ? "localfile64_new.bin" : "localfile_new.bin"),
                    DatPathExtractTranslate + (tIs64 ? @"\localfile64.bin" : @"\localfile.bin"), true); // overwrite = true
            if (Cb_back.Checked)
                BackUpManager(Targetlocaldat, TargetlocaldatPath, TargetlocalFileName, tIs64, 1);
            else
                CompileManager(DatPathExtractTranslate, tIs64);
        }

        OpenFileDialog EncDecFile = new OpenFileDialog();
        FolderBrowserDialog EncDecFolder = new FolderBrowserDialog();
        string stEncDec;
        private void BtnEncDecSearch_Click(object sender, EventArgs e)
        {
            if (RBFolder.Checked)
            {
                if (EncDecFolder.ShowDialog() != DialogResult.OK)
                    return;
                TxbEncDec.Text = EncDecFolder.SelectedPath;
            }
            if (RBFile.Checked)
            {
                if (EncDecFile.ShowDialog() != DialogResult.OK)
                    return;
                TxbEncDec.Text = EncDecFile.FileName;
            }
        }
        private void RBFolder_CheckedChanged(object sender, EventArgs e)
        {
            labelMode.Text = "Folder:";
        }

        private void RBFile_CheckedChanged(object sender, EventArgs e)
        {
            labelMode.Text = "File:";
        }

        //public 
        private void BtnDecode_Click(object sender, EventArgs e)
        {
            LkXor = new byte[] { 164, 159, 216, 179, 246, 142, 57, 194, 45, 224, 97, 117, 92, 75, 26, 7 };
            string hex = BitConverter.ToString(LkXor);
            Console.WriteLine(hex);
            stEncDec = TxbEncDec.Text;
            if (RBFolder.Checked)
            {
                GboxTools.Enabled = false;

                DirectoryInfo d = new DirectoryInfo(stEncDec);

                // FileInfo[] allfiles = Directory.GetFiles(stEncDec, "*.xml*", SearchOption.AllDirectories);
                //int totalfiles = int.Parse(allfiles.Length.ToString());
                FileInfo[] allfiles = d.GetFiles("*.xml*", SearchOption.AllDirectories);
                RunWithWorker(((o, args) =>
                {
                    Console.Write("\rDecrypting!");
                    //Console.Write("\rDecrypting");
                    Parallel.ForEach(allfiles, file =>
                    {
                        string ExtractFilePath = file.FullName;
                        MemoryStream memoryStream = new MemoryStream();
                        FileStream fileStream = new FileStream(ExtractFilePath, FileMode.Open);
                        byte[] array = new byte[fileStream.Length];
                        fileStream.Read(array, 0, array.Length);
                        fileStream.Close();
                        MemoryStream memoryStream2 = new MemoryStream(array);
                        BXML bxml = new BXML(LkXor);
                        new BNSDat().Convert(memoryStream2, bxml.DetectType(memoryStream2), memoryStream, BXML_TYPE.BXML_PLAIN);
                        memoryStream2.Close();
                        File.WriteAllBytes(ExtractFilePath, memoryStream.ToArray());
                        memoryStream.Close();
                    });
                    Console.Write("Done!\r");
                    GboxTools.Enabled = true;

                }));
            }
            if (RBFile.Checked)
            {
                RunWithWorker(((o, args) =>
                {
                    Console.Write("\rDecrypting: {0}\n", stEncDec);
                    MemoryStream memoryStream = new MemoryStream();
                    FileStream fileStream = new FileStream(stEncDec, FileMode.Open);
                    byte[] array = new byte[fileStream.Length];
                    fileStream.Read(array, 0, array.Length);
                    fileStream.Close();
                    MemoryStream memoryStream2 = new MemoryStream(array);
                    BXML bxml = new BXML(LkXor);
                    new BNSDat().Convert(memoryStream2, bxml.DetectType(memoryStream2), memoryStream, BXML_TYPE.BXML_PLAIN);
                    memoryStream2.Close();
                    File.WriteAllBytes(stEncDec, memoryStream.ToArray());
                    memoryStream.Close();
                    Console.Write("Done!\r");
                }));

            }
        }

        private void BtnEncode_Click(object sender, EventArgs e)
        {
            LkXor = new byte[] { 164, 159, 216, 179, 246, 142, 57, 194, 45, 224, 97, 117, 92, 75, 26, 7 };
            if (RBFolder.Checked)
            {
                string[] allfiles = Directory.GetFiles(stEncDec, "*.xml*", SearchOption.AllDirectories);
                GboxTools.Enabled = false;
                RunWithWorker(((o, args) =>
                {
                    foreach (var file in allfiles)
                    {
                        Console.Write("Encrypting: {0}\n", file);
                        FileStream fileStream = new FileStream(file, FileMode.Open);
                        MemoryStream memoryStream = new MemoryStream();
                        BXML bxml = new BXML(LkXor);
                        new BNSDat().Convert(fileStream, bxml.DetectType(fileStream), memoryStream, BXML_TYPE.BXML_BINARY);
                        fileStream.Close();
                        File.WriteAllBytes(file, memoryStream.ToArray());
                        memoryStream.Close();
                        Console.Write("Done!");
                    }
                    GboxTools.Enabled = true;
                }));
            }
            if (RBFile.Checked)
            {
                Console.Write("Encrypting: {0}\n", stEncDec);
                FileStream fileStream = new FileStream(stEncDec, FileMode.Open);
                MemoryStream memoryStream = new MemoryStream();
                BXML bxml = new BXML(LkXor);
                new BNSDat().Convert(fileStream, bxml.DetectType(fileStream), memoryStream, BXML_TYPE.BXML_BINARY);
                fileStream.Close();
                File.WriteAllBytes(stEncDec, memoryStream.ToArray());
                memoryStream.Close();
                Console.Write("Done!");
            }
        }
        //Manual Translation
        OpenFileDialog TranslationOfileDat = new OpenFileDialog();
        FolderBrowserDialog TranslationOfolderDat = new FolderBrowserDialog();

        OpenFileDialog TranslationOSourceFile = new OpenFileDialog();
        OpenFileDialog TranslationOMergeFile = new OpenFileDialog();

        //
        private string TranslationDatPath;//path of local.dat
        private string TranslationlocaldatName;//file name only
        private string TranslationlocalOutPath;//path where the files will be extracted
        private string TranslationBinFile;//that's it

        private string TranslationExportFile;//translation.xml file that will be generated
        private string TranslationFilePath;//path where translation.xml will be saved
        private string TranslateRepackPath;//
        private string TranslationSourceFile;//file from where the translation will be read
        private string TranslationTargetFile;//file that will be translated
        //
        string TranslationBinFileTranslated;

        void CheckDat()
        {
            if (CheckBoxTranslateXmldat.Checked)
            {
                isDatafile = true;
                TranslationBinFile = TranslationlocalOutPath + @"\" + (DatIs64 ? "datafile64.bin" : "datafile.bin"); //get full file path and add .files
                TranslationBinFileTranslated = TranslateRepackPath + (DatIs64 ? "datafile64_new.bin" : "datafile_new.bin");
            }
            else
            {
                TranslationBinFile = TranslationlocalOutPath + @"\" + (DatIs64 ? "localfile64.bin" : "localfile.bin"); //get full file path and add .files
                TranslationBinFileTranslated = TranslateRepackPath + (DatIs64 ? "localfile64_new.bin" : "localfile_new.bin");
            }
        }
        private void BtnSearchLocal_Click(object sender, EventArgs e)
        {
            if (TranslationOfileDat.ShowDialog() != DialogResult.OK)
                return;

            TxbLocalFile.Text = TranslationOfileDat.FileName;
            TranslationDatPath = TranslationOfileDat.FileName;
            TranslationlocaldatName = TranslationOfileDat.SafeFileName;

            // Check if 64bit or 32bit
            //DatIs64 = TranslationDatPath.Contains("64");
            if (Cbox64Bit.Checked)
                DatIs64 = true;

            TranslateRepackPath = Path.GetDirectoryName(TranslationDatPath) + @"\";//get working dir
            TranslationlocalOutPath = TranslationDatPath + ".files"; //get full file path and add .files
            TxbOut.Text = TranslationlocalOutPath;

            TranslationFilePath = TranslateRepackPath + (DatIs64 ? "Translation64" : "Translation");
            TranslationExportFile = TranslationFilePath + (DatIs64 ? @"\Translation64.xml" : @"\Translation.xml");//original translation xml
        }

        private void BtnUnpack_Click(object sender, EventArgs e)
        {
            if (CboxManualTranslateRegion.SelectedItem == null)
            {
                MessageBox.Show("Select game region!.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CheckDat();
            SelectAES(CboxManualTranslateRegion.SelectedIndex);

            if (multiworker != null && multiworker.IsBusy)
                multiworker.CancelAsync();
            try
            {
                if (File.Exists(TranslationDatPath))
                {
                    Extractor(TranslationDatPath);
                    GboxTools.Enabled = false;
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

        private void BtnSearchOutput_Click(object sender, EventArgs e)
        {
            if (TranslationOfolderDat.ShowDialog() != DialogResult.OK)
                return;

            TxbOut.Text = TranslationOfolderDat.SelectedPath;
            TranslationlocalOutPath = TranslationOfolderDat.SelectedPath;

            //DatIs64 = TranslationlocalOutPath.Contains("64");
            if (Cbox64Bit.Checked)
                DatIs64 = true;
            string datName = Path.GetFileName(TranslationlocalOutPath);
            //get dat file name from folder
            TranslationlocaldatName = datName.Replace(".files", "");
            //get working path
            TranslateRepackPath = TranslationlocalOutPath.Replace(datName, "\\");
            //Console.WriteLine(DatfileName);
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (multiworker != null && multiworker.IsBusy)
                multiworker.CancelAsync();
            CheckDat();
            if (string.IsNullOrWhiteSpace(TranslationFilePath))
                TxbOut.Text = TranslationlocalOutPath;

            Directory.CreateDirectory(TranslationFilePath);
            try
            {
                if (File.Exists(TranslationBinFile))
                {
                    GboxTools.Enabled = false;
                    RunWithWorker((o, args) =>
                    {
                        BDat bdat = new BDat();
                        bdat.ExportTranslate(TranslationBinFile, TranslationExportFile, BXML_TYPE.BXML_PLAIN, DatIs64);
                        GboxTools.Enabled = true;
                    });
                }
                else
                {
                    MessageBox.Show(".bin not exist please extract first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void BtnSourceSearch_Click(object sender, EventArgs e)
        {
            if (TranslationOSourceFile.ShowDialog() != DialogResult.OK)
                return;

            TxbSourceTranslate.Text = TranslationOSourceFile.FileName;
            TranslationSourceFile = TranslationOSourceFile.FileName;
            //DatfileName = OSourceFile.SafeFileName;

            // Check if 64bit or 32bit
            //DatIs64 = TranslationSourceFile.Contains("64");
            if (Cbox64Bit.Checked)
                DatIs64 = true;
        }

        private void BtnSearchMerge_Click(object sender, EventArgs e)
        {
            if (TranslationOMergeFile.ShowDialog() != DialogResult.OK)
                return;

            TxbMergeFile.Text = TranslationOMergeFile.FileName;
            TranslationTargetFile = TranslationOMergeFile.FileName;
            //DatfileName = OSourceFile.SafeFileName;

            // Check if 64bit or 32bit
            //DatIs64 = TranslationTargetFile.Contains("64");
            if (Cbox64Bit.Checked)
                DatIs64 = true;
        }

        private void BtnMerge_Click(object sender, EventArgs e)
        {
            if (multiworker != null && multiworker.IsBusy)
                multiworker.CancelAsync();

            CheckDat();
            try
            {
                if (!string.IsNullOrWhiteSpace(TranslationSourceFile))
                {
                    Console.WriteLine("\rMerging translation...");
                    GboxTools.Enabled = false;
                    RunWithWorker(((o, args) =>
                    {
                        TranslateReader translateControl_Na = new TranslateReader();
                        translateControl_Na.Load(TranslationSourceFile);//Source xml
                        TranslateReader translateControl_Org = new TranslateReader();
                        translateControl_Org.Load(TranslationTargetFile, true);//Target xml          
                        translateControl_Org.MergeTranslation(translateControl_Na, TranslationTargetFile);//Target xml
                        GboxTools.Enabled = true;
                    }));
                }
                else
                {
                    MessageBox.Show("Translation files not exist please select source and merge files first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void BtnTranslate_Click(object sender, EventArgs e)
        {
            if (multiworker != null && multiworker.IsBusy)
                multiworker.CancelAsync();
            CheckDat();
            try
            {
                if (File.Exists(TranslationTargetFile))
                {
                    GboxTools.Enabled = false;
                    RunWithWorker(((o, args) =>
                    {
                        BDat bdat = new BDat();
                        bdat.Translate(TranslateRepackPath, TranslationTargetFile, isDatafile, DatIs64);
                        GboxTools.Enabled = true;
                    }));
                }
                else
                {
                    MessageBox.Show("Translation files not exist please select source and merge files first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void BtnRepackTranslation_Click(object sender, EventArgs e)
        {
            SelectAES(CboxManualTranslateRegion.SelectedIndex);
            CheckDat();
            //if (CboxManualTranslateRegion.SelectedIndex == 2||CboxManualTranslateRegion.SelectedIndex == 3)
            //{
            //    MessageBox.Show("You can't repack V3 files.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}

            if (CboxManualTranslateRegion.SelectedIndex == -1)
            {
                MessageBox.Show("Select game region!.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (File.Exists(TranslationBinFileTranslated))
            {
                // Copy one file to a location where there is a file.
                File.Copy(TranslationBinFileTranslated, TranslationBinFile, true); // overwrite = true
            }
            else
            {
                MessageBox.Show("Translation files not exist please select source and merge files first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (Cb_back.Checked)
                TranslateBackUpManager(TranslationDatPath, TranslateRepackPath, TranslationlocaldatName, DatIs64);
            else
                CompileManager(TranslationlocalOutPath, DatIs64);
        }
        private void TranslateBackUpManager(string filepath, string dir, string filename, bool is64)
        {
            //filepath full file path
            //dir filepath dir only
            //filename itself
            if (multiworker != null && multiworker.IsBusy)
                multiworker.CancelAsync();

            RunWithWorker(((o, args) =>
            {
                if (string.IsNullOrEmpty(filename) == false)
                {
                    try
                    {
                        GboxTools.Enabled = false;
                        //get current date and time
                        string date = DateTime.Now.ToString("dd-MM-yy_"); // includes leading zeros
                        string time = DateTime.Now.ToString("hh.mm.ss"); // includes leading zeros

                        // folder location
                        var BackDir = dir + "Backup\\";

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
                        GboxTools.Enabled = true;
                    }
                }
            }));
            CompileManager(TranslationlocalOutPath, is64);
        }
    }
}
