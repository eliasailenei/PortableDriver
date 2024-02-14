using System;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Drawing;
using Newtonsoft.Json;
using System.Windows.Forms;
using HtmlAgilityPack;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;
using PortableDriver;
using PortableDriver.Properties;

class globalVariables
{
    public static string[] results { get; set; }
    public static string MSIUrl { get; set; }
}
class Drivers
{
    string proName;
    public async Task<string[]> Driver()
    {
        string[] commands = { "csproduct get vendor", "csproduct get name", "baseboard get product", "baseboard get serialnumber" };

         globalVariables.results = new string[commands.Length];

        for (int i = 0; i < commands.Length; i++)
        {
            globalVariables.results[i] = await Exec(commands[i]);
        }

        return globalVariables.results;
    }

    private async Task<string> Exec(string command)
    {
        using (Process pro = new Process())
        {
            pro.StartInfo.FileName = "wmic.exe";
            pro.StartInfo.Arguments = command;
            pro.StartInfo.CreateNoWindow = true;
            pro.StartInfo.UseShellExecute = false;
            pro.StartInfo.RedirectStandardOutput = true;

            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();

            StringBuilder outputBuilder = new StringBuilder();
            bool skip = false; 

            pro.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    if (!skip)
                    {
                        skip = true; 
                    }
                    else
                    {
                        outputBuilder.AppendLine(e.Data);
                        skip = false;
                        
                    }
                }
            };

            pro.Start();
            pro.BeginOutputReadLine();
             pro.WaitForExit(); 
            string output = outputBuilder.ToString().Trim();

            return output;
        }

    }
    


}
class Asus
{
    private async Task<string> HTMLResponse()
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync("https://www.asus.com/support/api/product.asmx/GetPDSupportTab?website=global&pdid=&pdhashedid=&model=" + removeDot(globalVariables.results[2]));

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return responseBody;
                }
                else
                {
                    return "error";
                }
            }
            catch (HttpRequestException e)
            {
                return "error";
            }
        }
    }
    public async Task<string> rawHTML()
    {
        return await HTMLResponse();
    }
    public async Task<string> driverURL()
    {
        var jsonObject = JsonConvert.DeserializeObject<dynamic>(await rawHTML());
        string url = FindUrlByTitle(jsonObject, "helpdesk_download");
        return url;
    }
    string FindUrlByTitle(dynamic jsonObject, string targetTitle)
    {
        foreach (var item in jsonObject.Result.Obj)
        {
            if (item.Type != null && item.Type.ToString().ToLower() == targetTitle.ToLower())
            {
                return item.Url.ToString();
            }

            foreach (var subItem in item.Items)
            {
                if (subItem.Type != null && subItem.Type.ToString().ToLower() == targetTitle.ToLower())
                {
                    return subItem.Url.ToString();
                }
            }
        }
        return null;
    }
    private string removeDot(string input)
    {
        Regex regex = new Regex(@"\.\d+");
        return regex.Replace(input, "");
    }
    public async Task<List<Tuple<string, string, string>>> scrapeURL()
    {
        List<Tuple<string, string, string>> input = new List<Tuple<string, string, string>>();
        var driverService = FirefoxDriverService.CreateDefaultService(@"drivers\geckodriver.exe");
        driverService.HideCommandPromptWindow = true;
        driverService.BrowserCommunicationPort = 26877;
        var options = new FirefoxOptions();
        options.BinaryLocation = @"drivers\FirefoxPortable\App\Firefox64\firefox.exe";
        options.AddArgument("--headless");
        using (var driver = new FirefoxDriver(driverService, options))
        {
            driver.Navigate().GoToUrl(await driverURL());
            var lastHeight = (long)((IJavaScriptExecutor)driver).ExecuteScript("return document.body.scrollHeight;");
            ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            var elements = driver.FindElements(By.XPath("//a[contains(@class, 'SolidButton')]"));

            foreach (var element in elements)
            {
                var href = element.GetAttribute("href");
                var title = element.GetAttribute("title");

                if (!string.IsNullOrEmpty(href) && !string.IsNullOrEmpty(title))
                {
                    try
                    {
                        input.Add(Tuple.Create(title, href, getVersion(href)));
                    } catch {
                        Console.WriteLine("Not a valid driver url, skipping");
                    }
                    
                }
            }
            driver.Quit();
        }
        
        return input;
    }
    public string getVersion(string url)
    {
        string pattern = @"V([\d.]+)_";
        Match match = Regex.Match(url, pattern);

        if (match.Success)
        {
            string versionString = match.Groups[1].Value;
            return versionString;
        }
        else
        {
            throw new FormatException("HREF invalid");
        }
    }

}

class MSI
{
    
     public string model { get; set; }
    
    List<string> results = new List<string>();
    private string addDash(string input)
    {
        return input.Replace(" ", "-");
    }
    public async Task<string> getUrl()
    {
            string[] possibleOut = new string[3];
            possibleOut[2] = "https://www.msi.com/Laptop/" + model + "/support#driver";
            possibleOut[1] = "https://www.msi.com/Desktop/" + model + "/support#driver";
            possibleOut[0] = "https://www.msi.com/Motherboard/" + model + "/support#driver";
            foreach (string possible in possibleOut) {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(possible).Result;
                    if (response.IsSuccessStatusCode)
                    {
                    return possible; 
                    }

                }

            }
        return "not-found!";
    }
   public void itemModel(string inp)
    {
        
        if (!string.IsNullOrEmpty(inp))
        {
            model = addDash(inp);
        } else
        {
           model =  globalVariables.results[1];
        }
    }
    public string getModel()
    {
        return model;
    }
    public async Task<List<string>> getData(string url, string findpath)
    {
        
            var driverService = FirefoxDriverService.CreateDefaultService(@"drivers\geckodriver.exe");
            driverService.HideCommandPromptWindow = true;
            driverService.BrowserCommunicationPort = 26877;
            var options = new FirefoxOptions();
            options.BinaryLocation = @"drivers\FirefoxPortable\App\Firefox64\firefox.exe";
            options.AddArgument("--headless");
            using (var driver = new FirefoxDriver(driverService, options))
            {
                driver.Navigate().GoToUrl(url);
                var lastHeight = (long)((IJavaScriptExecutor)driver).ExecuteScript("return document.body.scrollHeight;");
                ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                var elements = driver.FindElements(By.XPath(findpath));

                foreach (var element in elements)
                {
                    var href = element.GetAttribute("href");
                    if (!string.IsNullOrEmpty(href))
                    {
                        try
                        {
                            results.Add(href);
                        }
                        catch
                        {
                            Console.WriteLine("Not a valid driver url, skipping");
                        }

                    }
                }
                driver.Quit();
            }
            return results;
    }
    public async Task<List<Tuple<string, string, string>>> getDatas(string url)
    {
        string accUrl = url + "/support#driver";
        List<Tuple<string, string, string>> results = new List<Tuple<string, string, string>>();
        var driverService = FirefoxDriverService.CreateDefaultService(@"drivers\geckodriver.exe");
        driverService.HideCommandPromptWindow = true;
        driverService.BrowserCommunicationPort = 26877;
        var options = new FirefoxOptions();
        options.BinaryLocation = @"drivers\FirefoxPortable\App\Firefox64\firefox.exe";
        options.AddArgument("--headless");

        using (var driver = new FirefoxDriver(driverService, options))
        {
            await Task.Run(() =>
            {
                driver.Navigate().GoToUrl(accUrl);
                var lastHeight = (long)((IJavaScriptExecutor)driver).ExecuteScript("return document.body.scrollHeight;");
                ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");

                var propButtons = driver.FindElements(By.XPath("//div[@class='badges']/button"));
                if (propButtons.Count > 0)
                {
                    foreach (var button in propButtons)
                    {
                        driver.ExecuteScript("arguments[0].click();", button);
                        var bodyHREF = driver.FindElements(By.XPath("//div[@class='content']//section[@class='spec']"));
                        foreach (var res in bodyHREF)
                        {
                            var fileUrl = res.FindElement(By.XPath(".//div[@class='download']/a")).GetAttribute("href");
                            IWebElement titleElement = driver.FindElement(By.XPath("//span[5]"));
                            string title = titleElement.Text;
                            IWebElement versionElement = driver.FindElement(By.XPath("//span[6]"));
                            string version = versionElement.Text;
                            results.Add(Tuple.Create(title, fileUrl, version));
                        }
                    }
                }
                else
                {
                    results.Add(Tuple.Create("MSI servers are down", "failed", "ery rare to happen, reopen the app to scan again"));
                }
            });
        }

        return results;
    }




}
