using System;
using System.Threading.Tasks;

namespace PortableDriver
{
    class CLI
    {
        Drivers drivers = new Drivers();
        public async void Main()
        {
            string[] deviceInfo = await drivers.Driver();
            HelloScreen(deviceInfo);
        }

        private async void HelloScreen(string[] deviceInfo)
        {
            Console.WriteLine("Welcome to PortableDriver");
            Console.WriteLine("");
            string[] alsoDisplay = { "Manufactor: ", "Product Name: ", "Product BIOS Name: ", "Serial Number: " };
            int point = 0;
            foreach (string info in deviceInfo)
            {
                Console.WriteLine(alsoDisplay[point] + info);
                point++;
            }
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("We are currently scanning for drivers, please wait...");
            Console.WriteLine(); 
            Console.ReadLine();

        }
    }
}
