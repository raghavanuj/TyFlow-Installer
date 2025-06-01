using System;
using System.Collections.Generic;
using System.ComponentModel;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection.Emit;

//using System.Linq;
//using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

//using System.Threading.Tasks;

namespace TyFlow_Installer
{
    public partial class TyFlow : Form
    {
        // string startPath = @"c:\example\start";
        string zipPath = "";// @"C:\Users\JARVIS\Downloads\tyFlow_1127.zip";


        //temp path where zip file will be extracted
        string extractPath = Path.GetTempPath() + "tyflowTemp";
        string textFileName = @"\tyflow_latest.txt";
        string htmlToTextPath = "";
        string downloadURL = @"https://pro.tyflow.com/download/dlo/";

        string internalVersion = "";
        string externalVersion = "";
        bool isDownloading = false;
        bool isCheckingVersion = false;
        bool canDownload = false;
        WebClient zipWebClient = null;
        WebClient htmlWebClient = null;
        //bool overwriteFiles = true;
        RegistryFuncions registryFunctions = new RegistryFuncions();

        Dictionary<int, string> maxPaths = new Dictionary<int, string>();

        List<MaxVersions> maxVersionUI = new List<MaxVersions>();

        public TyFlow()
        {
            InitializeComponent();
            htmlToTextPath = extractPath + textFileName;
            this.AllowDrop = true;
            this.DragEnter += Form1_DragEnter;
            this.DragDrop += Form1_DragDrop;


            ExtractBtn.Enabled = false;
            ExtractBtn.Visible = ExtractBtn.Enabled;
        }

        private void ExtractBtn_Click(object sender, EventArgs e)
        {

            ExtractAndInstall();

        }


        private async Task ExtractAndInstall(bool removeDirexctory = true)
        {


            Install_Btn.Enabled = false;
            Install_Btn.Visible = Install_Btn.Enabled;

            // if previous extracted zip file directory exists than delete it
            if (Directory.Exists(extractPath) && removeDirexctory)
            {
                DeleteDirectory(extractPath);
            }

            // ZipFile.ExtractToDirectory(zipPath, extractPath);



            //Extract zip file and wait till extraction is complete
            label2.Text = "Extracting zip file...";
            Task.Run(() => ZipFile.ExtractToDirectory(zipPath, extractPath)).Wait();
            label2.Text = "Zip file extracted successfully";
            /*
            using (var archive = ZipFile.OpenRead(zipPath))
            {
                var count = archive.Entries.Count(x => !string.IsNullOrWhiteSpace(x.Name));

                foreach (var entry in archive.Entries)
                {
                    await Task.Run(() => { ZipFile.};
                }
            }
            */

            //make copy of 2020 version and rename it as 2021 version , as 2021 dlo is same as provided for 2020
            if (File.Exists(extractPath + "\\tyFlow_2020.dlo"))
            {
                Task.Run(() => File.Copy(extractPath + "\\tyFlow_2020.dlo", extractPath + "\\tyFlow_2021.dlo")).Wait();
            }

            // get paths for all .dlo files extracted from zip file as an array
            var ExtractedPluginsPaths = Directory.GetFiles(extractPath, "*.dlo", SearchOption.AllDirectories);
            UpdateMaxVersionsUI(ExtractedPluginsPaths);

            // extract 3ds max version from each dlo's name 
            for (int i = 0; i < ExtractedPluginsPaths.Length; i++)
            {
                Console.WriteLine("Extracted Plugin Path: " + ExtractedPluginsPaths[i]);
                // remove tyFlow_ from the dlo file name
                var dlo_max_ver = ExtractedPluginsPaths[i].Split('_')[1];
                // remove extention ".dlo" from the dlo file name
                dlo_max_ver = dlo_max_ver.Split('.')[0];

                // convert remaining dlo name  i.e year number from string to int
                int trimmedDloMaxVersion = int.Parse(dlo_max_ver);
                //Console.WriteLine(trimmedDloMaxVersion);

                // check each dlo if required version for that dlo is installed by checking registry entries
                //bool isValidInstall = registryFunctions.maxPaths.ContainsKey(trimmedDloMaxVersion);

                bool isValidInstall = maxPaths.ContainsKey(trimmedDloMaxVersion);

              
                ExtractBtn.Enabled = false;

                // check if downloaded dlo version has required max version installed
                if (isValidInstall)
                {
                    // path to existing tyflow plugin files

                    string maxPluginPath = maxPaths[trimmedDloMaxVersion] + "plugins";//\\tyFlow_";// + trimmedDloMaxVersion + ".dlo";
                    if (!Directory.Exists(maxPluginPath))
                    {
                        Directory.CreateDirectory(maxPluginPath);
                    }
                    maxPluginPath = Path.Combine(maxPluginPath, "tyFlow_"+trimmedDloMaxVersion + ".dlo");
                    
                    
                    foreach (var mv in maxVersionUI)
                    {
                        if(mv.maxVersion == trimmedDloMaxVersion)
                        {
                            maxPluginPath = mv.textBox.Text + "plugins\\";
                            if (!Directory.Exists(maxPluginPath))
                            {
                                Directory.CreateDirectory(maxPluginPath);
                            }
                            string extendedPath = "tyFlow_"+trimmedDloMaxVersion + ".dlo";
                            maxPluginPath = Path.Combine(maxPluginPath, extendedPath);
                        }

                       
                       
                    }
                    Console.WriteLine("final path "+ maxPluginPath);
                    // string maxPluginPath = maxVersionUI[maxPaths[trimmedDloMaxVersion].IndexOf()] + "plugins\\tyFlow_" + trimmedDloMaxVersion + ".dlo";
                    // delete existing tyflow plugin from max plugin directory
                    Task.Run(() => File.Delete(maxPluginPath)).Wait();
                    Console.WriteLine("deleting: " + maxPluginPath);

                    // move updated dlo file from extracted zip folder to max plugin directory
                    Task.Run(() => File.Move(extractPath + "\\tyFlow_" + trimmedDloMaxVersion + ".dlo", maxPluginPath)).Wait();
                    Console.WriteLine("Moving from : " + extractPath + "\\tyFlow_" + trimmedDloMaxVersion + ".dlo TO " + maxPluginPath);
                    label2.Text = "Plugin Installed successfully";

                }
                // File.Delete(registryFunctions.maxPaths[trimmedDloMaxVersion]);
                // System.IO.File.Delete(pluginsPaths[i]);

                // enablezip file browse button again
                Install_Btn.Enabled = true;
                Install_Btn.Visible = Install_Btn.Enabled;

                // disable file update button after files are updated
                ExtractBtn.Enabled = false;
                ExtractBtn.Visible = ExtractBtn.Enabled;
            }
        }

        /// <summary>
        /// deletes the existing temp directory so that each time newly updated files are extracted to that location
        /// </summary>
        /// <param name="directoryPath"> path to  check for existing extracted tyflow plugin zip files</param>
        public static void DeleteDirectory(string directoryPath)
        {
            try
            {
                Directory.Delete(directoryPath, true);
            }
            catch (Exception e)
            {
                // Handle exceptions, such as directory being in use or read-only
            }
        }

        /// <summary>
        /// Get all max versions installed along with their install location as soon as application loads
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            maxPaths = registryFunctions.GetMaxInstallLocations();
            CheckLatestVersionOnline();
            AddMaxInstallsUI();


        }

        void AddMaxInstallsUI()
        {
            maxVersionUI = new List<MaxVersions>();
            for (int i = 0; i < maxPaths.Keys.Count; i++)
            {
                AddButton(i, maxPaths.Keys.ElementAt(i), true);
                //maxVersionButtons.Add((Button)this.Controls[i + 3]); // +3 because first 3 controls are not buttons
            }
        }

        /// <summary>
        /// Visual indicator to show which max versions got updated tyflow version
        /// Red : no , Green : Yes
        /// </summary>
        /// <param name="i"></param>
        /// <param name="_maxversion"></param>
        /// <param name="isValidInstall"></param>
        private void AddButton(int i, int _maxversion, bool isValidInstall)
        {
            // create new button for each max version found in registry and add it to the form
            Button newButton = new Button();


            this.Controls.Add(newButton);
            newButton.Text = _maxversion.ToString();
            int startPos = 190;
            //newButton.Location = new Point(startPos + (108 * i), 205);
            newButton.Location = new Point(startPos, 175 + (i * 25));
            newButton.Size = new Size(75, 23);
            newButton.FlatStyle = FlatStyle.Standard;
            // newButton.BackColor = Color.GreenYellow;
            //newButton.BackColor = Color.Aqua;
            newButton.BackColor = isValidInstall ? Color.LawnGreen : Color.Red;

            ////////////////////////////////////////////////////////////////
            TextBox tb = new TextBox();
            //int tbWidth = tb.Width;
            this.Controls.Add(tb);
            tb.Location = new Point(startPos + 75, newButton.Top +2);
            tb.Size = new Size(250, 23);
            //tb.FlatStyle = FlatStyle.Standard;
            if (isValidInstall)
            {
                tb.Text = registryFunctions.maxPaths[_maxversion];
            }


            Button browseBtn = new Button();
            this.Controls.Add(browseBtn);
            browseBtn.Location = new Point(startPos + 75 + tb.Width, newButton.Top);
            browseBtn.Size = newButton.Size;
            browseBtn.Text = "browse";
            browseBtn.FlatStyle = FlatStyle.Standard;           
            browseBtn.BackColor = Color.White;
           // browseBtn.Name = "browseBtn_" + _maxversion.ToString();
            browseBtn.Name = "browseBtn_" + i;
            browseBtn.Click += OnBrowseBtn;

            MaxVersions mv = new MaxVersions(newButton, tb, _maxversion,browseBtn);
            maxVersionUI.Add(mv);
        }

        void OnBrowseBtn(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;
            int index = int.Parse( clickedButton.Name.Split('_')[1]);
            Console.WriteLine();
            OpenFileDialog folderBrowser = new OpenFileDialog();
            folderBrowser.ValidateNames = false;
            folderBrowser.CheckFileExists = false;
            folderBrowser.CheckPathExists = true;
            //folderBrowser.DefaultExt = ".zip";
            //openFileDialog1.Filter = "Zip files (tyFlow_*.zip)|tyFlow_*.zip";
            //DialogResult result = folderBrowser.ShowDialog(); // Show the dialog.

            folderBrowser.FileName = "Max root Path.";
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                string folderPath = Path.GetDirectoryName(folderBrowser.FileName) +@"\";
                maxVersionUI[index].textBox.Text = folderPath;
                // ...
            }
        }

        private void UpdateMaxVersionsUI(string[] _dloPaths)
        {
            List<int> maxDloVersions = new List<int>() ;
            foreach (string dloVer in _dloPaths)
            {
                var dlo_max_ver = dloVer.Split('_')[1];
                dlo_max_ver = dlo_max_ver.Split('.')[0];
                int trimmedDloMaxVersion = int.Parse(dlo_max_ver);
                maxDloVersions.Add(trimmedDloMaxVersion);
            }
                for (int i = 0; i < maxPaths.Keys.Count; i++)
            {

                var currKey = maxPaths.Keys.ElementAt(i);
                Console.WriteLine("max paths found " + maxPaths.Keys.Count);// + maxDloVersions.Contains(currKey));
                bool _isValidInstall = maxDloVersions.Contains(currKey);
                Console.WriteLine(currKey + " ______________ " + _isValidInstall);
                maxVersionUI[i].button.BackColor = _isValidInstall ? Color.LawnGreen : Color.DimGray;
                maxVersionUI[i].textBox.Enabled = _isValidInstall;
                maxVersionUI[i].button.Enabled = _isValidInstall;
                maxVersionUI[i].textBox.Text = _isValidInstall ? maxVersionUI[i].textBox.Text : "This TyFlow version is not supported for " + maxVersionUI[i].maxVersion;
            
                
            }
        }

        /// <summary>
        /// browse zip file downloaded from tyflow website, restrict file browser to .zip file only
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Install_Btn_Click(object sender, EventArgs e)
        {
            CancelZipDownload();

            ExtractBtn.Enabled = false;
            ExtractBtn.Visible = ExtractBtn.Enabled;

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.DefaultExt = ".zip";
            openFileDialog1.Filter = "Zip files (tyFlow_*.zip)|tyFlow_*.zip";
            DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.

            if (result == DialogResult.OK) // Test result.
            {
                zipPath = openFileDialog1.FileName;

                

            }
            else
            {                 // user cancelled the file selection
                return;
            }



            Install_Btn.Enabled = false;
            Install_Btn.Visible = Install_Btn.Enabled;


            // ExtractBtn.Enabled = true;
            // ExtractBtn.Visible = ExtractBtn.Enabled;
            ExtractAndInstall();
        }


        /// <summary>
        /// detects file dragged in on the app , currently dosent work due to admin prevlages required by app 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }


        /// <summary>
        /// gets path from zip file dragged on to app area. not working due to windows restriction
        /// for apps running as admin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Form1_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 1)
            {
                // do what you want
               // Console.WriteLine(files[0]);
                zipPath = files[0];
            }
            else
            {
                // show error
            }
        }

        private void CheckLatestVersionOnline()
        {
            
            if (!isCheckingVersion)
            {
                if (!CheckForInternetConnection("https://docs.tyflow.com/download/index.html"))
                {
                   // label2.Text = "Unable to connect to internet";
                    latestVersion.Text = "Unable to connect to internet. [click again to retry]"; 
                    CancelVersionChecking();
                    return;
                }
                CheckLatestBuild();
                isCheckingVersion = true;
                //isDownloading = true;
            }
            else
            {
                CancelVersionChecking();
                return;
            }
            //CheckLatestBuild();
        }

        private void downloadBtn_Click(object sender, EventArgs e)
        {
            if (isDownloading)
            {
                //CheckLatestBuild();
                //isDownloading = true;
                //isDownloading = true;
                CancelZipDownload();
                return;
            }

            if (canDownload)
            {
                if (!CheckForInternetConnection("https://docs.tyflow.com/download/index.html"))
                {
                    // label2.Text = "Unable to connect to internet";
                    latestVersion.Text = "Unable to connect to internet. [click again to retry]";
                    CancelVersionChecking();
                    return;
                }
                startDownloadZip();
                return;
            }





            //startDownload();
        }

        public void CancelZipDownload()
        {
            if (isDownloading)
            {
                zipWebClient.CancelAsync();
                label2.Text = "Download Aborted";
                progressBar1.Value = 0;
                zipWebClient.DownloadFileCompleted -= new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                isDownloading = false;
            }
        }

        public void CancelVersionChecking(bool isaborted = true)
        {
            if (isCheckingVersion)
            {
                htmlWebClient.CancelAsync();
                if (isaborted)
                {   
                    label2.Text = "Version Check Aborted";
                }
                else
                {
                    label2.Text = "Latest Version Info Downloaded";
                }
                
                    
                progressBar1.Value = 0;
                progressBar2.Value = 0;
                htmlWebClient.DownloadFileCompleted -= new AsyncCompletedEventHandler(client_DownloadFileCompleted_html);
                isCheckingVersion = false;
            }
        }



        private void startDownloadZip()
        {

            zipWebClient = new WebClient();
            zipWebClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            zipWebClient.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
            zipWebClient.DownloadFileAsync(new Uri(downloadURL + internalVersion + ".zip"), extractPath + @"\" + internalVersion + ".zip");
            Console.WriteLine("URL :" + downloadURL + internalVersion);
            Console.WriteLine("download extract path: " + extractPath + @"\" + internalVersion + ".zip");
            isDownloading = true;
            isCheckingVersion = false;

        }



        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(((e.BytesReceived / 1024) / 1024).ToString());
            double totalBytes = double.Parse(((e.TotalBytesToReceive / 1024) / 1024).ToString());
            double percentage = bytesIn / totalBytes * 100;
            label2.Text = externalVersion + " " + "Downloaded " + bytesIn + " MB of " + totalBytes + " MB";
            // label2.Text =  externalVersion + "\n" +  "Downloaded " + e.BytesReceived + " of " + e.TotalBytesToReceive ;
            progressBar1.Value = int.Parse(Math.Truncate(percentage).ToString());
        }

        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            label2.Text = "Completed";
            zipPath = extractPath + @"\" + internalVersion + ".zip";
            Console.WriteLine(zipPath);
            ExtractAndInstall(false);

        }

        void client_DownloadProgressChanged_html(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            label2.Text = "Latest Version Downloaded Info " + e.BytesReceived + " of " + e.TotalBytesToReceive;
            progressBar2.Value = int.Parse(Math.Truncate(percentage).ToString());
        }

        void client_DownloadFileCompleted_html(object sender, AsyncCompletedEventArgs e)
        {
            label2.Text = "Version Info Downloaded ";
            ProcessTextFile();

        }
        private void CheckLatestBuild()
        {
            

            if (Directory.Exists(extractPath))
            {
                DeleteDirectory(extractPath);
            }


            // Directory.Delete(extractPath);
            Directory.CreateDirectory(extractPath);


            htmlWebClient = new WebClient();
            htmlWebClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged_html);
            htmlWebClient.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted_html);
            
            Console.WriteLine(htmlToTextPath);
            htmlWebClient.DownloadFileAsync(new Uri("https://docs.tyflow.com/download/index.html"), htmlToTextPath);
        }


        public static bool CheckForInternetConnection(string _urlToCheck)
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead(_urlToCheck)) // Or any reliable URL
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        public void ProcessTextFile()
        {
            string lineWithLatestVersion = "";
            //string internalVersion = "";
            //string externalVersion = "";
            Console.WriteLine("Reading Text File For Version");
            foreach (string line in File.ReadLines(htmlToTextPath))
            {
                if (line.Contains("<option class=\"download-dropdown-item\" value=") & line.Contains("tyFlow_"))
                {
                    Console.WriteLine(line);
                    lineWithLatestVersion = line;
                    break;
                }
            }

            internalVersion = lineWithLatestVersion.Split('"')[3];
            externalVersion = lineWithLatestVersion.Split('>')[1];
            externalVersion = externalVersion.Split('<')[0];
            Console.WriteLine(internalVersion);
            Console.WriteLine(externalVersion);
            latestVersion.Text = "Latest Version: " + externalVersion;
            latestVersion.Visible = true;
            canDownload = true;
            CancelVersionChecking(false);
            // startDownloadZip();
        }

        private void progressBar2_Click(object sender, EventArgs e)
        {

        }

        private void latestVersion_Click(object sender, EventArgs e)
        {
            CheckLatestVersionOnline();
        }

        struct MaxVersions
        {
            public Button button;
            public TextBox textBox;
            public int maxVersion;
            public Button browseBtn;

            public MaxVersions(Button b, TextBox tb, int mv, Button browsebtn)
            {
                button = b;
                textBox = tb;
                maxVersion = mv;
                browseBtn = browsebtn;
            }
        }
    }
}
