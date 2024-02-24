using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace PortableDriver
{
    public partial class PreDownload : UserControl
    {
        public string[] urlList {  get; set; }
        static bool continueDlnd;
        public string ext;
        public PreDownload()
        {
            InitializeComponent();
        }
        public event EventHandler InteractionComplete;

        private async void PreDownload_Load(object sender, EventArgs e)
        {
            await Task.Run(() => {
                progressBar1.BeginInvoke((Action)(() => progressBar1.Maximum = urlList.Length + 1));
                int pointer = 1;
            if (Directory.Exists("PreDownloaded"))
            {
                Directory.Delete("PreDownloaded", true);
            }
            Directory.CreateDirectory("PreDownloaded");
            foreach (var item in urlList)
            {
                try
                {
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(item, $"PreDownloaded\\driver{pointer}{fileExt(item)}");
                    } 
                } catch
                {
                    Console.WriteLine("Skipping");
                }
                finally
                {
                    pointer++;
                    progressBar1.BeginInvoke((Action)(() => progressBar1.Value = pointer));
                    label1.BeginInvoke((Action)(() => label1.Text = $"Downloading {pointer}/{urlList.Length + 1}"));  
                }
            }
                this.Invoke((Action)(() =>
                {
                    InteractionComplete.Invoke(this, EventArgs.Empty);
                    this.Hide();
                }));
            });
        }
        private string fileExt(string inp)
        {
            try
            {
                if (inp == null)
                {
                    return "unsupported"; 
                }

                if (inp.Contains(".exe"))
                {
                    return ".exe";
                }
                else if (inp.Contains(".zip"))
                {
                    return ".zip";
                }
            }
            catch (NullReferenceException)
            {
                return "unsupported";
            }

            return "unsupported";
        }


    }
}
