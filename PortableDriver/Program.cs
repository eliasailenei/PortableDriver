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

        [DllImport("kernel32.dll")]
       private static extern bool AllocConsole();

       [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

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
            } catch {
                Console.WriteLine("GekoDriver isn't running already.");
            }
            
            // Pass args to the constructor
            Program program = new Program(args);
            if (program.issCLI()) // Call issCLI() on an instance of Program
            {
                AllocConsole();
                Form1 form = new Form1();
                form.isCLI = true;
                form.Show();
                 FreeConsole();
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
        }

        public Program(string[] args)
        {
            this.args = args;
        }

        private bool issCLI()
        {
            return args.Length > 0 && args[0] == "-test";
        }
        
    }
    
}
