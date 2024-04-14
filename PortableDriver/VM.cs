using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PortableDriver
{
    public partial class VM : UserControl
    {
        public string vmtype { get; set; }
        public bool useOEM { get; set; }
        public VM()
        {
            InitializeComponent();
        }
        public event EventHandler InteractionComplete;
        private async void VM_Load(object sender, EventArgs e)
        {
            if (vmtype == "virtualbox")
            {
                List<string> addons = await add("https://download.virtualbox.org/virtualbox/", true, false);
                comboBox1.Items.AddRange(addons.ToArray());
            } else if (vmtype == "vmware")
            {
                List<string> addons = await add("https://packages.vmware.com/tools/releases/", true, false);
                comboBox1.Items.AddRange(addons.ToArray());
            }
        }
        private async Task<List<string>> add(string url, bool forCombo, bool vmware)
        {
            List<string> list = new List<string>();
            var http = new HttpClient();
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(await http.GetStringAsync(url));
            var versions = doc.DocumentNode.SelectNodes("//a[@href]");
            if (versions.Count > 0)
            {
                string raw;
                foreach (var version in versions)
                {
                    if (!vmware)
                    {
                        raw = version.InnerText;
                    }
                    else
                    {
                        raw = version.Attributes["href"].Value;
                    }
                    if (raw.Contains("/") && forCombo)
                    {
                        list.Add(raw.Replace("/", string.Empty));
                    }
                    else if (!forCombo)
                    {
                        list.Add(raw.Replace("/", string.Empty));

                    }
                }
            }
            return list;
        }

        private async void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button2.Enabled = false;
            string selectedItem = comboBox1.SelectedItem.ToString();

            selectedItem = selectedItem.Replace(".", "");

            float res;
            if (float.TryParse(selectedItem, out res))
            {
                if (vmtype == "virtualbox")
                {
                    string url = "https://download.virtualbox.org/virtualbox/" + comboBox1.SelectedItem.ToString();
                    List<string> list = await add(url, false, false);
                    foreach (var item in list)
                    {
                        if (item.ToString().Contains("VBoxGuestAdditions"))
                        {
                            globalVariables.VMUrl = url + "/" + item.ToString();
                            this.Hide();
                            InteractionComplete.Invoke(this, EventArgs.Empty);
                        }
                    }
                }
                else if (vmtype == "vmware")
                {
                    string url = "https://packages.vmware.com/tools/releases/" + comboBox1.SelectedItem.ToString() + "/windows";
                    try
                    {
                        List<string> list = await add(url, false, true);
                        foreach (var item in list)
                        {
                            if (!item.ToString().Contains("arm") && !item.ToString().Contains("Parent Directory") && item.ToString().Contains("iso"))
                            {
                                string items = item.ToString().Replace("windowsVM", "VM");
                                globalVariables.VMUrl = url + "/" + items;
                                this.Hide();
                                InteractionComplete.Invoke(this, EventArgs.Empty);
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        button2.Enabled = true;
                        MessageBox.Show("Sorry, you can't use that! Error: " + ex.Message);
                    }
                }
            }
            else
            {
                button2.Enabled = true;
                MessageBox.Show("Sorry, you can't use that!");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            useOEM = true;
            this.Hide();
            InteractionComplete.Invoke(this, EventArgs.Empty);
        }
    }
}
