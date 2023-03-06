using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Upload;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;
using System.Windows.Forms;

namespace GoogleUploader
{
    public partial class Form1 : Form
    {
        private readonly string PathToServiceAccountKeyFile = Application.StartupPath + @""; //Change to the path of your google account key (Needs to be save locally)
        private const string ServiceAccountEmail = "";
        private readonly string UploadFileName = Application.StartupPath + @"GameBuild.Apk";
        private const string DirectoryID = ""; //Change to your google directory

        private string buildNumber = "";
        private bool stopped;

        private string buildNumberPath = Application.StartupPath + @"BuildNumber.txt"; //Used to grab the build number (Set up manually, part of the README)

        private bool silent;

        public Form1(bool silent)
        {
            InitializeComponent();

            this.silent = silent;
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            //Check to see if we want to run in the backgroud
            if(silent)
            {
                Visible = false;
                ShowInTaskbar = false;
            }

            fileInfoLabel.Text = "";

            //Tray menu
            trayIcon.Text = "Google Uploader";
            trayIcon.ContextMenuStrip = contextMenuStrip1;
            trayIcon.Visible = true;

            trayIcon.Click += TrayIcon_Click;

            FindBuildNumber();

            if (!stopped) LaunchAsyncUpload();
        }

        private void TrayIcon_Click(object? sender, EventArgs e)
        {
            Visible = true;
            ShowInTaskbar = true;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Visible = true;
            ShowInTaskbar = true;
        }


        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(stopped)
            {
                Application.Exit();
                return;
            }

            //Create prompt asking for deletion
            Prompt promptForm = new Prompt();
            DialogResult dialogResult = promptForm.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                Application.Exit();
            }
            else
            {
                Debug.WriteLine("Canceled");
                return;
            }

            promptForm.Dispose();
        }


        private void FindBuildNumber()
        {
            if (File.Exists(buildNumberPath))
            {
                string full = "";
                string[] lines = File.ReadAllLines(buildNumberPath);

                for (int i = 0; i < lines.Length; i++)
                {
                    full += lines[i];
                }

                full.Trim();

                buildNumber = full;

                fileInfoLabel.Text = buildNumber;

            }
            else
            {
                infoLabel.Text = "Could not find the build number!";
                fileInfoLabel.Text = Directory.GetParent(Application.StartupPath).FullName;

                stopped = true;
            }
        }


        //Load up the file and send it to the directory ID through google docs
        private async void LaunchAsyncUpload()
        {
            statusLabel.Text = "Status: Starting";

            if (!File.Exists(PathToServiceAccountKeyFile))
            {
                infoLabel.Text = "Could not find the key file!";
                return;
            }

            try
            {
                statusLabel.Text = "Status: Grabbing file";

                var cred = GoogleCredential.FromFile(PathToServiceAccountKeyFile).CreateScoped(Google.Apis.Drive.v3.DriveService.ScopeConstants.Drive);

                //Create the drive service
                var service = new DriveService(new Google.Apis.Services.BaseClientService.Initializer() { HttpClientInitializer = cred });

                statusLabel.Text = "Status: Created service, selecting upload";

                //Upload the file metadata
                var fileMetaData = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = $"GameBuildAPK({buildNumber}).apk", //At some point lead over to the build file and apend the build numbers
                    Parents = new List<string>() { "1mq6su5APiPAf9bxCBSFuu5sVdmVPsNmJ" },
                };

                string uploadedFileID;

                //Create the file on google drive
                await using (var fsSource = new FileStream(UploadFileName, FileMode.Open, FileAccess.Read))
                {
                    statusLabel.Text = "Status: Creating file on google drive";

                    //Set the max progress bar size
                    progressBar1.Maximum = (int)(fsSource.Length / 1000000);

                    label1.Text = $"Current Size: {(fsSource.Length / 1000000)}MB";

                    //Start the upload
                    var request = service.Files.Create(fileMetaData, fsSource, "application/vnd.android.package-archive"); request.Fields = "*";
                    request.ProgressChanged += Request_ProgressChanged;

                    statusLabel.Text = "Status: Starting upload";

                    var resaults = await request.UploadAsync(CancellationToken.None);

                    //Check to see how the upload went
                    if (resaults.Status == UploadStatus.Failed)
                    {
                        Console.WriteLine($"Failed to upload file: {resaults.Exception.Message}");
                    }
                    else if (resaults.Status == UploadStatus.Completed)
                    {
                        Console.WriteLine("Completed!");

                        infoLabel.Text = "Completed!";
                    }

                    uploadedFileID = request.ResponseBody?.Id;

                    fsSource.Close();
                }

                statusLabel.Text = "Status: DONE!";
                stopped = true;

                if(silent) Application.Exit();
            }
            catch(Exception e)
            {
                infoLabel.Text = "Some issue occurred! " + e.Message;

                stopped = true;
            }
        }


        //Check the progress when the chunks update
        private void Request_ProgressChanged(IUploadProgress obj)
        {
            Invoke(new Action(() =>
            {
                progressBar1.Value = (int)(obj.BytesSent / 1000000);
            }));
        }

        //Random
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void trayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Visible = true;
            ShowInTaskbar = true;
        }
    }
}