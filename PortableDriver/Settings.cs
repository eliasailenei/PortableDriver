using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PortableDriver
{
    public partial class Settings : UserControl
    {
        public MakeXML xML { get; set; }
        string xmlLoc, defPath;
        bool runSilent, rebootAfter;

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Select the folder where you want your XML to be saved at.";
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK && folderBrowserDialog.SelectedPath != null)
            {
                xML.setXmlLoc(folderBrowserDialog.SelectedPath);
            }
            Settings_Load(sender, e);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(textBox3.Text))
            {
                xML.setDefaultPath(textBox3.Text);
            }
            else
            {
                var mess = MessageBox.Show("This doesn't look like a real path, do you still want to use it?", "WARNING", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (mess == DialogResult.Yes)
                {
                    xML.setDefaultPath(textBox3.Text);
                } 
            }
            Settings_Load(sender, e);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (runSilent)
            {
                xML.setRunSilent(false);
            }
            else
            {
                xML.setRunSilent(true);
            }
            Settings_Load(sender, e);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (rebootAfter)
            {
                xML.setRebootAfter(false);
            }
            else
            {
                xML.setRebootAfter(true);
            }
            Settings_Load(sender, e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        public Settings()
        {
            InitializeComponent();
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            xmlLoc = xML.getXmlLoc();
            textBox4.Text = xmlLoc;
            defPath = xML.getDefaultPath();
            textBox3.Text = defPath;
            runSilent = xML.getRunSilent();
            textBox2.Text = runSilent.ToString();
            rebootAfter = xML.getRebootAfter();
            textBox1.Text = rebootAfter.ToString();
        }
    }
}
