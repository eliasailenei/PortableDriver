using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace PortableDriver
{
    public partial class GettingDrivers : UserControl
    {
        public GettingDrivers()
        {
            InitializeComponent();
        }
        public event EventHandler InteractionComplete;
        private async void GettingDrivers_Load(object sender, EventArgs e)
        {
            await Task.Run(() => {
                try
                {
                    using (var client = new WebClient())
                    {
                        client.DownloadFile("https://github.com/eliasailenei/PortableDriver/releases/download/Drivers/drivers.zip", "drive.zip");
                    }
                    ZipFile.ExtractToDirectory("drive.zip", "drivers");
                    File.Delete("drive.zip");
                }
                catch (Exception ex)
                {

                    this.Invoke((Action)(() => MessageBox.Show("Error trying to get geckodrivers. The error is " + ex.Message)));
                    this.Invoke((Action)(() =>
                    {
                        foreach (Form form in System.Windows.Forms.Application.OpenForms)
                        {
                            form.Close();
                        }
                    }));
                }
                finally
                {
                    this.Invoke((Action)(() =>
                    {
                        InteractionComplete.Invoke(this, EventArgs.Empty);
                        this.Hide();
                    }));
                }
            });
        }

    }
}
