using PortableDriver.Properties;
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Reflection.Emit;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.IO.Compression;
using System.Net;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime;

namespace PortableDriver
{
    public partial class Form1 : Form
    {
        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

        static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        static readonly IntPtr HWND_TOP = new IntPtr(0);

        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        const UInt32 SWP_NOSIZE = 0x0001;

        const UInt32 SWP_NOMOVE = 0x0002;

        const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;



        [DllImport("user32.dll")]

        [return: MarshalAs(UnmanagedType.Bool)]

        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        public bool port, init, userDetermined, selector, isVM;
        Drivers drivers = new Drivers(); // simple oop
        private Rectangle richb1, richb2, richb3,checb1, bttn1, bttn2, bttn3, bttn4, bttn5, bttn6;
        private Size form;
        MakeXML xML = new MakeXML(); // simple oop
        string seletetedURL;
        string[] deviceInfo;
        char tLetter;
        string inpt;
        List<Tuple<string, string, string>> input = new List<Tuple<string, string, string>>(); // list system
        private List<string> downUrl = new List<string>();// list system
        private List<string> downPort = new List<string>();// list system
        public Form1(bool fs)
        {
            SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
            InitializeComponent();
            richTextBox3.Visible = false;
            button4.Visible = false;
            
            if (fs)
            {
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
                this.Resize += Rsize;
                form = this.Size;
                this.BackColor = Color.Gray;
                richTextBox3.Visible = true;
                button4.Visible = true;
                port = fs;
            }
            else
            {
                this.Resize += Rsize;
                form = this.Size;
                richTextBox3.Visible = false;
                button4.Visible = false;
                port = fs;
            }
            richb1 = new Rectangle(richTextBox1.Location, richTextBox1.Size);
            richb2 = new Rectangle(richTextBox2.Location, richTextBox2.Size);
            richb3 = new Rectangle(richTextBox3.Location, richTextBox3.Size);
            checb1 = new Rectangle(checkedListBox1.Location, checkedListBox1.Size);
            bttn1 = new Rectangle(button1.Location, button1.Size);
            bttn2 = new Rectangle(button2.Location, button2.Size);
            bttn3 = new Rectangle(button3.Location, button3.Size);
            bttn4 = new Rectangle(button4.Location, button4.Size);
            bttn5 = new Rectangle(button5.Location, button5.Size);
            bttn6 = new Rectangle(button6.Location, button6.Size);  
        }
        private  void Form1_Load(object sender, EventArgs e)
        {
            // Due to UI conflict, Ive decieded to not use this method.
        }
        private async Task load(object sender, EventArgs e)
        {
            if (deviceInfo[0].ToLower().Contains("asus") || deviceInfo[0].ToLower().Contains("asustek"))
            {
                Console.WriteLine("ASUS detected!");
                await asusLoad();
            }
            else if (deviceInfo[0].ToLower().Contains("micro-star") || deviceInfo[0].ToLower().Contains("micro star") || deviceInfo[0].ToLower().Contains("msi"))
            {
                MSI msi = new MSI(); // simple oop
                if (userDetermined)
                {
                    msi.itemModel(inpt);
                } else
                {
                    msi.itemModel(null);
                }
                
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
            } else if (deviceInfo[0].ToLower().Contains("qemu"))
            {
                input.Add(Tuple.Create("qemudriver", "https://fedorapeople.org/groups/virt/virtio-win/direct-downloads/latest-virtio/virtio-win-guest-tools.exe", "unknown version"));
                checkedListBox1.Items.Add("QEMU VirtIO Driver, latest");
                checkedListBox1.SetItemChecked(0, true);
            } else if (deviceInfo[0].ToLower().Contains("innotek") || deviceInfo[0].ToLower().Contains("vmware"))
            {
                string vmclient = string.Empty;
                if (deviceInfo[0].ToLower().Contains("innotek"))
                {
                    vmclient = "virtualbox";
                } else if (deviceInfo[0].ToLower().Contains("vmware"))
                {
                    vmclient = "vmware";
                }
                VM showDiag = new VM(); // simple oop
                showDiag.vmtype = vmclient;
                showDiag.Location = new Point((this.ClientSize.Width - showDiag.Width) / 2,
                                          (this.ClientSize.Height - showDiag.Height) / 2);
                this.Controls.Add(showDiag);
                showDiag.BringToFront();
                showDiag.Show();
                showDiag.InteractionComplete += async (s, args) =>
                {
                    if (showDiag.useOEM)
                    {
                        globalVariables.results[0] = "na";
                        await load(sender,e);
                    }
                    else
                    {
                        downPort.Add(globalVariables.VMUrl);
                        isVM = true;
                        button2_Click(sender, e);
                    }
                };
            }
            else
            {
               await showNotFound(sender,e);
            }
        }
        private async Task showNotFound(object sender, EventArgs e)
        {
            
                NotFound showDiag = new NotFound(); // simple oop
                showDiag.Location = new Point((this.ClientSize.Width - showDiag.Width) / 2,
                                          (this.ClientSize.Height - showDiag.Height) / 2);
                this.Controls.Add(showDiag);
                showDiag.BringToFront();
                showDiag.Show();
                showDiag.InteractionComplete += async (s, args) =>
                {
                    userDetermined = true;
                    inpt = showDiag.info;
                    deviceInfo[0] = showDiag.manu;
                    await load(sender, e);
                };
            

            
        }
        
        private async Task asusLoad()
        {
            Asus asus;
            if (userDetermined)
            {
                asus = new Asus(inpt);
            }
            else
            {
                asus = new Asus();
            }
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
            List<Tuple<string, string, string>> filteredInput = new List<Tuple<string, string, string>>(); // list system
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
        private string noDash(string input) // recusive algo
        {
            string first = input.Replace("-", "");
            return first.Replace("DOWNLOAD", "");
        }

        private async void button6_Click(object sender, EventArgs e)
        {
           await showNotFound(sender,e);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (selector)
            {
                selector = false;
                button4.Text = "Turn ON Selector";
            } else if (!selector)
            {
                selector= true;
                button4.Text = "Turn OFF Selector";
            }
        }

      

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                this.Invoke((Action)(() =>
                {
                    var openForms = System.Windows.Forms.Application.OpenForms.Cast<Form>().ToList();

                    foreach (Form form in openForms)
                    {
                        form.Close();
                    }
                }));
            }
            catch (InvalidOperationException)
            {
            }
            finally
            {
                MessageBox.Show("DONE");
            }

        }

        private void checkedListBox1_DoubleClick(object sender, EventArgs e)
        {
            
            if (checkedListBox1.SelectedItem != null)
            {
                string selectedItem = checkedListBox1.SelectedItem.ToString();

                if (port && selector)
                {
                    if (!downPort.Contains(selectedItem))
                    {
                        downPort.Add(selectedItem);
                        showText();
                        checkedListBox1.SetItemChecked(checkedListBox1.SelectedIndex, false);
                    } else if (downPort.Contains(selectedItem))
                    {
                        downPort.Remove(selectedItem);
                        showText();
                        checkedListBox1.SetItemChecked(checkedListBox1.SelectedIndex, true);
                    }
                }
                
            }
        }


        private async void button1_Click(object sender, EventArgs e)
        {
            checkedListBox1.Items.Clear(); 
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, false); 
           }
            await load(sender,e);
            Cursor.Current = Cursors.Arrow;

        }

        private void showText()
        {
            richTextBox3.Clear();
            richTextBox3.AppendText("The following is going to be pre-installed:\n");
            foreach (string item in downPort)
            {
                richTextBox3.AppendText("- "+item + "\n");
            }
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
            if (port)
            {
                int pointer = 0;
                string[] urls = new string[downPort.Count]; 

                foreach (string item in downPort)
                {
                    if (isVM)
                    {
                        urls[pointer] = item;
                        pointer++;
                    }
                    else
                    {
                        if (deviceInfo[0].ToLower().Contains("asus"))
                        {
                            string modifiedItem = "DOWNLOAD" + item;
                            int commaIndex = modifiedItem.IndexOf(',');
                            string updatedItem = commaIndex != -1 ? modifiedItem.Substring(0, commaIndex) : modifiedItem;
                            foreach (var main in input)
                            {
                                if (main.Item1 == updatedItem)
                                {
                                    urls[pointer] = main.Item2;
                                    pointer++;
                                    break;
                                }
                            }
                        }
                        else
                        {

                            foreach (var main in input)
                            {
                                if (main.Item1 == item)
                                {
                                    urls[pointer] = main.Item2;
                                    pointer++;
                                }
                            }
                        }
                    }
                }
                PreDownload showDiag = new PreDownload(); // simple oop
                showDiag.urlList = urls;
                showDiag.Location = new Point((this.ClientSize.Width - showDiag.Width) / 2,
                                          (this.ClientSize.Height - showDiag.Height) / 2);
                this.Controls.Add(showDiag);
                showDiag.BringToFront();
                showDiag.Show();
                showDiag.InteractionComplete += (s, args) =>
                {
                    xML.xmlLoc = Directory.GetCurrentDirectory();
                    int points = 0;
                    foreach (string url in downUrl)
                    {
                        if (!string.IsNullOrEmpty(url))
                        {
                            xML.addToXML(points, "Driver", url, "/SILENT /NORESTART");
                            points++;
                        }
                    }
                   
                    xML.addToXML(points + 1, "Setup", "C:\\Windows\\Setup\\Scripts\\autorun.exe", "autorun.au3");
                    xML.compileScript(); // writing and reading a file
                    button5_Click(sender, e);
                };

            }
            else
            {
                int point = 0;
                foreach (string url in downUrl)
                {
                    if (!string.IsNullOrEmpty(url))
                    {
                        xML.addToXML(point, "Driver", url, "/SILENT /NORESTART");
                        point++;
                    }
                }
                xML.compileScript();
                Installer inst = new Installer(xML.getXmlLoc()); // simple opp
                this.Hide();
                inst.Show();
            }
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Settings settings = new Settings(); // simple oop
            settings.xML = xML;
            this.Controls.Add(settings);
            settings.BringToFront();
            settings.Location = new Point((this.ClientSize.Width - settings.Width) / 2,
                                  (this.ClientSize.Height - settings.Height) / 2);
            settings.Visible = true;
        }
        private void resizeControl(Rectangle r, Control c)
        {
            float xRatio = (float)this.ClientSize.Width / form.Width;
            float yRatio = (float)this.ClientSize.Height / form.Height;

            int newX = (int)(r.X * xRatio);
            int newY = (int)(r.Y * yRatio);

            int newWidth = (int)(r.Width * xRatio);
            int newHeight = (int)(r.Height * yRatio);

            c.Location = new Point(newX, newY);
            c.Size = new Size(newWidth, newHeight);
        }
        
        private void Rsize(object sender, EventArgs e) // recursive algo
        {
            resizeControl(bttn1, button1);
            resizeControl(bttn2, button2);
            resizeControl(bttn3, button3);
            resizeControl(bttn4, button4);
            resizeControl(bttn5, button5);
            resizeControl(checb1, checkedListBox1);
            resizeControl(richb1, richTextBox1);
            resizeControl(richb2, richTextBox2);
            resizeControl(richb3, richTextBox3);
            resizeControl(bttn6, button6);
        }
        private async void Form1_Shown(object sender, EventArgs e)
        {
            if (port)
            {
                if (File.Exists(Environment.SystemDirectory + "\\driveLetters.txt"))
                {
                    string[] letters = File.ReadAllLines(Environment.SystemDirectory + "\\driveLetters.txt");
                    if (letters.Length >= 2)
                    {
                        tLetter = letters[1][0];
                    }
                }
                else
                {
                    tLetter = 'T';
                }
            }
            if (!Directory.Exists("drivers"))
            {
                GettingDrivers showDiag = new GettingDrivers(); // simple oop
                showDiag.Location = new Point((this.ClientSize.Width - showDiag.Width) / 2,
                                          (this.ClientSize.Height - showDiag.Height) / 2);
                this.Controls.Add(showDiag);
                showDiag.BringToFront();
                showDiag.Show();
                showDiag.InteractionComplete += async (s, args) =>
                {
                  await propLoad(sender,e);
                };
            }
            else
            {
               await propLoad(sender,e);
            }
            
        }
        private async Task propLoad(object sender, EventArgs e)
        {
            richTextBox2.BeginInvoke((Action)(() => richTextBox2.AppendText("Please wait, we are loading your drivers now.")));

            deviceInfo = await drivers.Driver();

            string[] alsoDisplay = { "Manufacture:", "Product Name:", "Model:", "Serial Number:" };
            int point = 0;
            foreach (string info in deviceInfo)
            {
                int temp = point;
                richTextBox1.BeginInvoke((Action)(() => richTextBox1.AppendText($"{alsoDisplay[temp]} {info}\n")));
                point++;
            }
            await load(sender,e);

            richTextBox2.BeginInvoke((Action)(() =>
            {
                richTextBox2.Clear();
                richTextBox2.AppendText("-When you are ready, click the button below. This will create a script and install your programs.\n-Sometimes the program might show nothing, please rescan.");
                if (port) richTextBox2.AppendText("\n-You are running via PortableISO! You can double click any text to pre-download the driver. For example, if you need the WiFi driver to connect to the internet.\n \n Hint: to select your pre-downloads, turn on selector!");
                Cursor.Current = Cursors.Arrow;
            }));
        }

    }
}
