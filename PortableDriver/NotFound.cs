﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace PortableDriver
{
    public partial class NotFound : UserControl
    {
        public string manu { get; set; }
        public string info { get; set; }
        public NotFound()
        {
            InitializeComponent();
        }

        private void NotFound_Load(object sender, EventArgs e)
        { 
            string[] supportManu = { "Exit program","ASUS", "MSI" , "VirtualBox" , "VMWare", "QEMU"};
            comboBox1.Items.AddRange(supportManu);
            comboBox1.SelectedItem = comboBox1.Items[1];
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
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
            } else if (comboBox1.SelectedIndex == 1)
            {
                richTextBox1.Clear();
                richTextBox1.AppendText("For ASUS systems, you must only provide the model number. For example, if your PC is ASUS TUF Dash F15 FX517ZC you will only type FX517ZC.");
            } else if (comboBox1.SelectedIndex == 2)
            {
                richTextBox1.Clear();
                richTextBox1.AppendText("For MSI systems, you must only provide the device. For example, if your PC is named MSI GE62VR-7RF, you input that you don't need a model number.");
            }
            
        }
        public event EventHandler InteractionComplete;
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(comboBox1.SelectedItem.ToString()))
                {
                    string bef = comboBox1.SelectedItem.ToString();
                    if (bef == "VirtualBox")
                    {
                        manu = "innotek";
                    }
                    else if (string.IsNullOrEmpty(bef))
                    {
                        MessageBox.Show("Please choose an option");
                    }
                    else
                    {
                        manu = bef;
                    }
                    info = textBox1.Text;
                    InteractionComplete.Invoke(this, EventArgs.Empty);
                    this.Hide();
                }
               
            } catch
            {
                
            }
            
        }
    }
}
