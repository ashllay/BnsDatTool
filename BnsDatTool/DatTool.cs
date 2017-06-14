using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace BnsDatTool
{
    public partial class BnsDatTool : Form
    {
        OpenFileDialog OfileDat = new OpenFileDialog();
        FolderBrowserDialog OfolderDat = new FolderBrowserDialog();
        private string BackPath = "backup\\";
        private string RepackPath;
        private string OutPath;
        private string DatfileName;
        private string FulldatPath;
        //TextWriter _writer = null;

        public BackgroundWorker ebnsdat;
        public BackgroundWorker cbnsdat;

        public bool Datis64;

        public BnsDatTool()
        {
            InitializeComponent();
        }

        private void BnsDatTool_Load(object sender, EventArgs e)
        {
            // _writer = new StreamWriter(richOut);
            // Console.SetOut(_writer);
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
    }
}
