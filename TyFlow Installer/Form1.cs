using System;
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
        WebClient zipWebClient = null;
        //bool overwriteFiles = true;
        RegistryFuncions registryFunctions = new RegistryFuncions();

        

        public TyFlow()
        {
            InitializeComponent();
            htmlToTextPath  = extractPath + textFileName;
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


            // extract 3ds max version from each dlo's name 
            for (int i = 0; i < ExtractedPluginsPaths.Length; i++)
            {
                // remove tyFlow_ from the dlo file name
                var dlo_max_ver = ExtractedPluginsPaths[i].Split('_')[1];
                // remove extention ".dlo" from the dlo file name
                dlo_max_ver = dlo_max_ver.Split('.')[0];

                // convert remaining dlo name  i.e year number from string to int
                int trimmedDloMaxVersion = int.Parse(dlo_max_ver);
                Console.WriteLine(trimmedDloMaxVersion);

                // check each dlo if required version for that dlo is installed by checking registry entries
                bool isValidInstall = registryFunctions.maxPaths.ContainsKey(trimmedDloMaxVersion);

                // add button for visual representation of each dlo matched with required max version installed
                // green: required max version found ; Red: required max version for dlo not installed  
                AddButton(i, trimmedDloMaxVersion, isValidInstall);

                // print full path for each dlo extracted from zip file 
                Console.WriteLine("extracted plugin path :" + ExtractedPluginsPaths[i]);

                //Console.WriteLine(ExtractedPluginsPaths[i]);

                //after zip extraction disable its button , and enable button for moving updated dlos -
                // - to corresponding max plugins folder
                ExtractBtn.Enabled = false;

                // check if downloaded dlo version has required max version installed
                if (isValidInstall)
                {
                    // path to existing tyflow plugin files
                    string maxPluginPath = registryFunctions.maxPaths[trimmedDloMaxVersion] + "plugins\\tyFlow_" + trimmedDloMaxVersion + ".dlo";

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
            registryFunctions.GetMaxInstallLocations();
           
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
            Button newButton = new Button();

            this.Controls.Add(newButton);
            newButton.Text = _maxversion.ToString();
            int startPos =50;
            newButton.Location = new Point(startPos + (108 * i), 205);
            newButton.Size = new Size(75, 23);
            newButton.FlatStyle = FlatStyle.Standard;
            // newButton.BackColor = Color.GreenYellow;
            //newButton.BackColor = Color.Aqua;
            newButton.BackColor = isValidInstall ? Color.LawnGreen: Color.Red;
           
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
                Console.WriteLine(files[0]);
                zipPath = files[0];
            }
            else
            {
                // show error
            }
        }

        private void downloadBtn_Click(object sender, EventArgs e)
        {
            if (!isDownloading)
            {
                CheckLatestBuild();
                isDownloading = true;
            }
            else
            {
                CancelZipDownload();
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

        private void startDownloadZip()
        {
            
            zipWebClient = new WebClient();
            zipWebClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            zipWebClient.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
            zipWebClient.DownloadFileAsync(new Uri(downloadURL+internalVersion+".zip"),extractPath+@"\"+internalVersion+".zip");
            Console.WriteLine("URL :" +downloadURL + internalVersion);
            Console.WriteLine("download extract path: "+extractPath + @"\"+internalVersion +".zip");           

        }

        

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(((e.BytesReceived/1024)/1024).ToString());
            double totalBytes = double.Parse(((e.TotalBytesToReceive/1024)/1024).ToString());
            double percentage = bytesIn / totalBytes * 100;
            label2.Text = externalVersion + "\n\n" + "Downloaded " + bytesIn + " MB of " + totalBytes +" MB";
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
            progressBar1.Value = int.Parse(Math.Truncate(percentage).ToString());
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
            

            WebClient client = new WebClient();
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged_html);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted_html);
            Console.WriteLine(htmlToTextPath);
            client.DownloadFileAsync(new Uri("https://docs.tyflow.com/download/index.html"), htmlToTextPath);
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
            startDownloadZip();
        }

    }
}
