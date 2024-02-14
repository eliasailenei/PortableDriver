using PortableDriver.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PortableDriver
{
    public partial class Form1 : Form
    {
        Drivers drivers = new Drivers();
        MakeXML xML = new MakeXML();
        string seletetedURL;
        string[] deviceInfo;
        bool init;
        List<Tuple<string, string, string>> input;
        private List<string> downUrl = new List<string>();
        public bool isCLI { get; set; }

        public Form1()
        {
            InitializeComponent();
           
        }
        private async void Form1_Load(object sender, EventArgs e)
        {
            
        }
        private async void load()
        {
            if (deviceInfo[0].ToLower().Contains("asus"))
            {
                Console.WriteLine("ASUS detected!");
                await asusLoad();
            }
            else if (deviceInfo[0].ToLower().Contains("micro-star") || deviceInfo[0].ToLower().Contains("micro star") || deviceInfo[0].ToLower().Contains("msi"))
            {
                MSI msi = new MSI();
                msi.itemModel(null);
                string pls = await msi.getUrl();
                if (pls == "not-found!")
                {
                    ModelSelect settings = new ModelSelect();
                    settings.inp = await msi.getData("https://www.msi.com/search/" + msi.getModel(), "//a[contains(@class, 'link')]");
                    this.Controls.Add(settings);
                    settings.BringToFront();
                    settings.Location = new Point((this.ClientSize.Width - settings.Width) / 2,
                                          (this.ClientSize.Height - settings.Height) / 2);
                    settings.Visible = true;
                    await Task.Run(() => { while (settings.Visible) { } });
                    await MSILoad(await msi.getDatas(globalVariables.MSIUrl));
                }
                else
                {
                    await MSILoad(await msi.getDatas(pls));
                }
            }
            else
            {
                MessageBox.Show("Program does not support your manufacture yet. Request it in the GitHub!");
            }
        }
       
        private async Task asusLoad()
        {
            Asus asus = new Asus();
            input = await asus.scrapeURL();
            Dictionary<string, string> latestVersions = new Dictionary<string, string>();
            foreach (Tuple<string, string, string> item in input)
            {
                string title = noDash(item.Item1);
                string href = item.Item3;
                if (!latestVersions.ContainsKey(title) || CompareVersions(href, latestVersions[title]) > 0)
                {
                    latestVersions[title] = href;
                }
            }
            List<Tuple<string, string, string>> filteredInput = new List<Tuple<string, string, string>>();
            foreach (var item in input)
            {
                string title = noDash(item.Item1);
                if (latestVersions.ContainsKey(title) && item.Item3 == latestVersions[title])
                {
                    filteredInput.Add(item);
                }
            }
            input = filteredInput;
            checkedListBox1.Items.Clear(); 
            int point = 0;
            foreach (var entry in latestVersions)
            {
                checkedListBox1.Items.Add($"{entry.Key}, V{entry.Value}");
                checkedListBox1.SetItemChecked(point, true);
                point++;
            }

            init = true;
        }

        private async Task MSILoad(List<Tuple<string, string, string>> inp)
        {
            await Task.Run(() => {
                input = inp;
                int point=0;
                foreach (var entry in input)
                {
                    checkedListBox1.Invoke(new Action(() => checkedListBox1.Items.Add($"{entry.Item1}, V{entry.Item3}")));
                    checkedListBox1.Invoke(new Action(() => checkedListBox1.SetItemChecked(point, true)));
                    point++;
                }
                init = true;
            });
        }

        private int CompareVersions(string version1, string version2)
        {
            try
            {
                string[] parts1 = version1.Split('.');
                string[] parts2 = version2.Split();

                for (int i = 0; i < Math.Max(parts1.Length, parts2.Length); i++)
                {
                    int part1 = i < parts1.Length ? int.Parse(parts1[i]) : 0;
                    int part2 = i < parts2.Length ? int.Parse(parts2[i]) : 0;

                    if (part1 != part2)
                    {
                        return part1.CompareTo(part2);
                    }
                }

                return 0; 
            } catch { return 0; 
            }
        }
        private string noDash(string input)
        {
            string first = input.Replace("-", "");
            return first.Replace("DOWNLOAD", "");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            checkedListBox1.Items.Clear(); 
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, false); 
           }
            load();
           
        }

        

        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            int index = e.Index;
            bool isChecked = e.NewValue == CheckState.Checked;

            if (isChecked)
            {
                if (downUrl.Count <= index)
                {
                    downUrl.Add(input[index].Item2);
                }
                else
                {
                    downUrl[index] = input[index].Item2;
                }
            }
            else
            {
                if (downUrl.Count > index)
                {
                    downUrl[index] = string.Empty;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int point =0 ;
            foreach (string url in downUrl)
            {
                if (!string.IsNullOrEmpty(url))
                {
                    xML.addToXML(point, "Driver", url, "/SILENT /NORESTART");
                    point++;
                }
            }
            xML.addToXML(point + 1, "Setup", "C:\\Windows\\Setup\\setup.exe", "-noargs-");
            xML.compileScript();
            Installer inst = new Installer();   
            inst.xmlPath = xML.getXmlLoc();
            inst.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Settings settings = new Settings();
            settings.xML = xML;
            this.Controls.Add(settings);
            settings.BringToFront();
            settings.Location = new Point((this.ClientSize.Width - settings.Width) / 2,
                                  (this.ClientSize.Height - settings.Height) / 2);
            settings.Visible = true;
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            propLoad();
        }
        private async void propLoad()
        {
            richTextBox2.AppendText("Please wait, we are loading your drivers now.");
            if (isCLI)
            {
                CLI cLI = new CLI();
                cLI.Main();
            }
            deviceInfo = await drivers.Driver();
            string[] alsoDisplay = { "Manufacture:", "Product Name:", "Model:", "Serial Number:" };
            int point = 0;
            foreach (string info in deviceInfo)
            {
                richTextBox1.AppendText($"{alsoDisplay[point]} {info}\n");
                point++;
            }
            load();
            richTextBox2.Clear();
            richTextBox2.AppendText("When you are ready, click the button below. This will create a script and install your programs.");
        }
    }
}
