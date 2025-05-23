using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
using System.Drawing;
//using System.Linq;
//using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;
//using System.Threading.Tasks;

namespace TyFlow_Installer
{
    public partial class TyFlow : Form
    {
        // string startPath = @"c:\example\start";
        string zipPath = "";// @"C:\Users\JARVIS\Downloads\tyFlow_1127.zip";
        
        //temp path where zip file will be extracted
        string extractPath = Path.GetTempPath() + "tyflowTemp";

        //bool overwriteFiles = true;
        RegistryFuncions registryFunctions = new RegistryFuncions();

        

        public TyFlow()
        {
            InitializeComponent();

            this.AllowDrop = true;
            this.DragEnter += Form1_DragEnter;
            this.DragDrop += Form1_DragDrop;


            ExtractBtn.Enabled = false;
            ExtractBtn.Visible = ExtractBtn.Enabled;
        }

        private void ExtractBtn_Click(object sender, EventArgs e)
        {
            //registryFunctions.GetMaxInstallLocations();
            // string startPath = @"c:\example\start";
            // string zipPath = @"C:\Users\JARVIS\Downloads\tyFlow_1127.zip";
            //  string extractPath = Path.GetTempPath()+"\\tyflowTemp"; 
            //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            // Encoding entryNameEncoding = Encoding.GetEncoding(850);
            //System.IO.Compression.ZipFile.CreateFromDirectory(startPath, zipPath);

            Install_Btn.Enabled = false;
            Install_Btn.Visible = Install_Btn.Enabled;

            // if previous extracted zip file directory exists than delete it
            if (Directory.Exists(extractPath))
            {
                DeleteDirectory(extractPath);
            }

            // ZipFile.ExtractToDirectory(zipPath, extractPath);

            //Extract zip file and wait till extraction is complete
            Task.Run(() => ZipFile.ExtractToDirectory(zipPath, extractPath)).Wait();
            Task.Run(() => File.Copy(extractPath+"\\tyFlow_2020.dlo", extractPath + "\\tyFlow_2021.dlo")).Wait();

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
                Console.WriteLine("extracted plugin path :"+ExtractedPluginsPaths[i]);

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

            ExtractBtn.Enabled = true;
            ExtractBtn.Visible = ExtractBtn.Enabled;
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

       
    }
}
