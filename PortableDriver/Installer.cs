using System;
using System.IO.Compression;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Net.NetworkInformation;
using System.Security.Policy;

namespace PortableDriver
{
    public partial class Installer : Form
    {
        public string xmlPath { get; set; }
        static bool continueDlnd ,silent, reboot, isPre;
        static string downloc,ext,defPath;
        List<Tuple<int, string, string, string>> found = new List<Tuple<int, string, string, string>>();
        List<string> downlFile = new List<string>(); 
        XmlDocument xmlDoc = new XmlDocument();

        public Installer(string accXml)
        {
            InitializeComponent();
            if (Directory.Exists(accXml))
            {
                xmlPath = accXml;
            } else
            {
                MessageBox.Show("Invalid script. Try again!");
                this.Close();
            }
            
        }
        public Installer()
        {
            InitializeComponent();
        }
        private async void Installer_Load(object sender, EventArgs e)
        {
            string[] placehold = { "n/a" };
            if (Directory.Exists("PreDownloaded"))
            {
                isPre = true;
                downloc = Path.Combine(Environment.CurrentDirectory, "PreDownloaded");
                findDriver();
                installDrivers();
              Directory.Delete(downloc, true);
                Application.Restart();
                return;
            }

            if (internetStatus())
            {
                await loadXML();
                pictureBox1.Hide();
                label1.Visible = false;
                showAll(true);
                await startDown(xmlPath + "\\DriverSetup\\", false, placehold);
                foreach (var item in found)
                {
                    if (item.Item2 == "Setup")
                    {
                        string app = item.Item3;
                        string args = item.Item4;
                        niniteInstall(app, args);
                        break;
                    }
                }
            }
            else
            {
                label1.Text = "No internet, please try again";
                showAll(false);
            }

            this.Close();
        }

        private void showAll(bool inp)
        {
            label2.Visible = inp;
            label3.Visible = inp;
            label4.Visible = inp;
            label5.Visible = inp;
            label6.Visible = inp;
            pictureBox2.Visible = inp;
            pictureBox3.Visible = inp;
            pictureBox4.Visible = inp;
            progressBar1.Visible = inp;
            button1.Visible = inp;
        }
        public bool internetStatus()
        {
            try
            {
                using (var ping = new Ping())
                {
                    var result = ping.Send("www.google.com");
                    return result.Status == IPStatus.Success;
                }
            }
            catch (PingException)
            { 
                return false;
            }
        }
        private async Task loadXML()
        {
            await Task.Run(() =>
            {
                try
                {
                    xmlDoc.Load(xmlPath + "\\autoDriver.xml");

                    XmlNodeList scriptNodes = xmlDoc.SelectNodes("//Script");

                    foreach (XmlNode scriptNode in scriptNodes)
                    {
                        int order = int.Parse(scriptNode.SelectSingleNode("Order").InnerText);
                        string type = scriptNode.SelectSingleNode("Type").InnerText;
                        string url = scriptNode.SelectSingleNode("URL").InnerText;
                        string args = scriptNode.SelectSingleNode("Args").InnerText;

                        found.Add(new Tuple<int, string, string, string>(order, type, url, args));
                    }
                } catch (Exception ex)
                {
                    MessageBox.Show(xmlPath + " is not a real path. Try again using speech marks. Here is error code:" +ex.Message);
                }
               
            });
        }
        public async Task startDown(string path, bool isPort, string[] arrinpt)
        {
            downloc = path;
            pictureBox3.Hide();
            pictureBox4.Hide();
            await Task.Run(() =>
            {
                int pointer = 0;
                if (Directory.Exists(downloc))
                {
                    Directory.Delete(downloc, true);
                }
                Directory.CreateDirectory(downloc);
                if (isPort)
                {
                    found.Clear();
                    foreach (string str in arrinpt)
                    {
                        found.Add(new Tuple<int, string, string, string>(0, "n/a", "n/a", str));

                    }
                }
                foreach (var item in found)
                {
                    string url = item.Item3;
                    string fileName = Path.GetFileName(url);
                    string fileExtension = fileExt(url);

                    if (fileExtension.Equals(".exe", StringComparison.OrdinalIgnoreCase) || fileExtension.Equals(".zip", StringComparison.OrdinalIgnoreCase))
                    {
                        ext = fileExtension;

                        using (HttpClient client = new HttpClient())
                        {
                            try
                            {
                                string encodedUrl = Uri.EscapeUriString(url);
                                HttpResponseMessage response = client.GetAsync(encodedUrl).Result;
                                Task.Run(async () => await ESDDownload(response, $"driver{pointer}{fileExtension}")).Wait();
                            }
                            catch
                            {
                                //skip download will be implemented later
                            }
                        }
                    }
                    else
                    {
                        // Handle unsupported file types
                    }
                    pointer++;
                    if (!isPort)
                    {
                        UpdateLabel($"Downloaded {pointer}/{found.Count}");
                    }
                    
                }
                if (!isPort)
                {
                    findDriver();
                    installDrivers();
                }
                
            });
        }
        private async void niniteInstall(string apps, string argss)
        {
            await Task.Run(() =>
            {
                pictureBox2.BeginInvoke(new Action(() => { pictureBox2.Visible = false; }));
                pictureBox4.BeginInvoke(new Action(() => { pictureBox4.Visible = false; }));
                pictureBox3.BeginInvoke(new Action(() => { pictureBox3.Visible = true; }));
                string folderPath = Path.GetDirectoryName(apps);
                Process.Start(apps, folderPath + "\\" +argss);
            });
        }
        private void UpdateLabel(string text)
        {
            if (label6.InvokeRequired)
            {
                label6.Invoke((MethodInvoker)(() => label6.Text = text));
            }
            else
            {
                label6.Text = text;
            }
        }
        private string fileExt(string inp)
        {
            if (inp.Contains(".exe"))
            {
                return ".exe";
            } else if (inp.Contains(".zip")) {
                return ".zip";
            }
            return "unsupported";
        }
       private async Task ESDDownload(HttpResponseMessage response, string progressFileName)
        {
            if (response.IsSuccessStatusCode)
            {
                long? totalSize = response.Content.Headers.ContentLength;
                string continMode = continueDlnd ? "Append" : "Create";
                FileMode fileMode = continMode == "Append" ? FileMode.Append : FileMode.Create;

                using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                using (FileStream fs = new FileStream(downloc + progressFileName, fileMode))
                {
                    byte[] buffer = new byte[8192];
                    int bytesRead;
                    long totalBytesRead = 0;
                    int prevPercent = -1;

                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fs.WriteAsync(buffer, 0, bytesRead);

                        totalBytesRead += bytesRead;

                        if (totalSize.HasValue)
                        {
                            int progressPercentage = (int)(((double)totalBytesRead / totalSize.Value) * 100);

                            if (progressPercentage != prevPercent)
                            {
                                progressBar1.Invoke((MethodInvoker)(() => progressBar1.Value = progressPercentage));
                                prevPercent = progressPercentage;
                            }
                        }
                    }
                }
            }
        }
        public void findDriver()
        {
            string[] allFiles = Directory.GetFiles(downloc);
            List<string> allZip = new List<string>();
            foreach (string file in allFiles)
            {
                if (file.Contains(".exe"))
                {
                    downlFile.Add(file);
                } else if (file.Contains(".zip")){
                    allZip.Add(file);
                }
            }
            ZipInstall(allZip);
        }
        private void ZipInstall(List<string> allZip)
        {
            try
            {
                if (allZip.Count > 0)
                {
                    foreach (string zipLoc in allZip)
                    {
                        try
                        {
                            string extractPath = Path.Combine(Path.GetDirectoryName(zipLoc), Path.GetFileNameWithoutExtension(zipLoc));
                            Directory.CreateDirectory(extractPath);

                            ZipFile.ExtractToDirectory(zipLoc, extractPath);
                        }
                        catch (IOException ex)
                        {
                            // Handle IOException caused by file being used by another process
                            Console.WriteLine($"Error extracting ZIP file: {ex.Message}");
                            continue; // Skip to the next iteration
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            // Handle UnauthorizedAccessException caused by lack of permission
                            Console.WriteLine($"Error extracting ZIP file: {ex.Message}");
                            continue; // Skip to the next iteration
                        }
                    }
                    ZipFileInstall(downloc, true);
                }
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        private void ZipFileInstall(string directoryPath, bool firstTime)
        {
            try
            {
                string[] executables;
                if (!firstTime)
                {
                    executables = Directory.GetFiles(directoryPath, "*.*")
                    .Where(file => file.ToLower().EndsWith(".exe") || file.ToLower().EndsWith(".bat"))
                    .ToArray();
                }
                else
                {
                    executables = null;
                }
                if (executables != null && executables.Length > 0)
                {
                    foreach (string executablePath in executables)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(executablePath);
                        if (fileName.ToLower().Contains("setup") || fileName.ToLower().Contains("install") || fileName.ToLower().Contains("autorun") || fileName.ToLower().Contains("installer") || fileName.ToLower().Contains("asussetup"))
                        {
                            downlFile.Add(executablePath);
                            return;
                        }
                    }
                }
                else
                {
                    string[] subDirectories = Directory.GetDirectories(directoryPath);
                    foreach (string subDirectory in subDirectories)
                    {
                        ZipFileInstall(subDirectory, false);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<Form> formsToClose = new List<Form>();
            foreach (Form form in Application.OpenForms)
            {
                formsToClose.Add(form);
            }
            foreach (Form form in formsToClose)
            {
                form.Close();
            }
        }

        private void installDrivers()
        {
            int point = 1;
            bool errorOccurred = false;
            foreach (string url in downlFile)
            {
                try
                {
                    label6.BeginInvoke(new Action(() => label6.Text = "Installing now"));
                    progressBar1.BeginInvoke(new Action(() =>
                    {
                        progressBar1.Minimum = 0;
                        if (isPre)
                        {
                            progressBar1.Maximum = point + 1;
                        }
                        else
                        {
                            progressBar1.Maximum = downlFile.Count;
                        }
                    }));


                    label6.BeginInvoke(new Action(() => label6.Text = $"Installing {point}/{downlFile.Count}"));
                    progressBar1.BeginInvoke(new Action(() => progressBar1.Value = point));

                    Process pro = new Process();
                    pro.StartInfo.FileName = url;
                    pro.Start();
                    pro.WaitForExit();

                    point++;

                }
                catch
                {
                    Console.WriteLine("Error is ignored");
                    errorOccurred = true;
                }
            }
            if (isPre)
            {
                progressBar1.Value = progressBar1.Minimum;
            }
            else
            {
                label6.BeginInvoke(new Action(() => label6.Text = errorOccurred ? "Installation failed" : "Installation completed"));
                progressBar1.BeginInvoke(new Action(() => progressBar1.Style = errorOccurred ? ProgressBarStyle.Continuous : ProgressBarStyle.Marquee));
            }
        }
    }
}
