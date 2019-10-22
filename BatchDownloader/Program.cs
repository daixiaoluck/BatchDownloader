using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Net.Http;

namespace BatchDownloader
{
    class Program
    {
        static private HttpClient _HttpClient
        {
            get
            {
                return new HttpClient();
            }
        }
        static private DirectoryInfo _ApplicationPathDirectoryInfo
        {
            get
            {
                return new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent;
            }
        }
        static private DirectoryInfo _DownloadedDirectoryInfo
        {
            get
            {
                string downloadedPath = Path.Combine(_ApplicationPathDirectoryInfo.FullName, "Downloaded");
                return new DirectoryInfo(downloadedPath);
            }
        }
        static void Main(string[] args)
        {
            var result = ParseText();
            Console.ReadLine();
        }

        static async Task DownloadAsync(string url, string imageFolderPath)
        {
            Regex regex = new Regex(@"\.\w+");
            string randomValue = Path.GetRandomFileName();
            string eliminateQuestionMark = Regex.Replace(url, @"\?\w+", "");
            MatchCollection matches = regex.Matches(eliminateQuestionMark);
            string fileFormat = matches[matches.Count - 1].Value;
            string filePath = Path.Combine(imageFolderPath, randomValue + fileFormat);
            HttpResponseMessage httpResponseMessage = await _HttpClient.GetAsync(url);
            if(httpResponseMessage.IsSuccessStatusCode)
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    await httpResponseMessage.Content.CopyToAsync(fileStream);
                    Console.WriteLine($"The file from {url} has just been downloaded.");
                }
            }
            else
            {
                Console.WriteLine($"Can not download the medium from {url}.");
            }
        }

        static async Task ParseText()
        {
            List<string> txtFileNames = new List<string>();
            string applicationPath = _ApplicationPathDirectoryInfo.FullName;
            string[] files = Directory.GetFiles(applicationPath);
            string pattern = @".+\.txt";
            foreach (var file in files)
            {
                if (Regex.IsMatch(file, pattern))
                {
                    txtFileNames.Add(file);
                }
            }
            foreach (var txtFileName in txtFileNames)
            {
                string eliminateFileFormat = Path.GetFileNameWithoutExtension(txtFileName);
                string imageFolderPath = Path.Combine(_DownloadedDirectoryInfo.FullName, eliminateFileFormat);
                if (!Directory.Exists(imageFolderPath))
                {
                    Directory.CreateDirectory(imageFolderPath);
                }
                using (StreamReader streamReader = new StreamReader(Path.Combine(applicationPath, txtFileName)))
                {
                    Console.WriteLine("The download starts.");
                    string line;
                    while (true)
                    {
                        line = streamReader.ReadLine();
                        if (line == null)
                        {
                            break;
                        }
                        //使用下载的方法
                        await DownloadAsync(line, imageFolderPath);
                    }
                    Console.WriteLine("The download completes.");
                }
            }
        }
    }
}