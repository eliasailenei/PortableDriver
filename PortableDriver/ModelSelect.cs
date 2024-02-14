using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PortableDriver
{
    public partial class ModelSelect : UserControl
    {
        public List<string> inp = new List<string>();

        public ModelSelect()
        {
            InitializeComponent();
        }

        private void ModelSelect_Load(object sender, EventArgs e)
        {
            foreach (string s in inp)
            {
                if (s.Contains("Laptop") || s.Contains("Desktop") || s.Contains("Motherboard")){
                    Match match = Regex.Match(s, @"/([^/]+)$");
                    if (match.Success)
                    {
                        string result = match.Groups[1].Value;
                        listBox1.Items.Add(makeNice(result));
                    }
                }
            }
            if (listBox1.Items.Count == 0)
            {
                MessageBox.Show("ERROR, it looks like the program timed out. Your IP may have been banned.");
            }
        }
        private string makeNice(string inp)
        {
            return inp.Replace("-", " ");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1) 
            {
                string toGive = inp[listBox1.SelectedIndex];
                globalVariables.MSIUrl = toGive;
                this.Visible = false;
            }
        }
    }
}
