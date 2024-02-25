using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PortableDriver
{
    class Program
    {
        private string[] args;
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = "/c taskkill /f /im geckodriver.exe";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.Start();
                process.WaitForExit();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = "/c taskkill /f /im firefox.exe";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.Start();
                process.WaitForExit();

            } catch {
                Console.WriteLine("GekoDriver isn't running already.");
            }
            try
            {
                if (args[0] == "--test")
                {
                    Application.SetCompatibleTextRenderingDefault(false); 
                    Form1 form = new Form1(true);
                    Application.EnableVisualStyles();
                    Application.Run(form);
                }
                else if (args[0] == "--installer")
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new Installer(args[1]));
                }
                else
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new Form1(false));
                }
            } catch {
                
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new Form1(false));
                
            }
            
        }

        public Program(string[] args)
        {
            this.args = args;
        }
        
    }
    
}
