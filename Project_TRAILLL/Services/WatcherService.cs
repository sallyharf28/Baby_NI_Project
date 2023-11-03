using System.Diagnostics;
using System.Drawing;
using System.Security.Cryptography;
using System.Collections.Generic;
using Microsoft.VisualBasic.FileIO;
using System.IO;
using System.Globalization;
using System.Text;
using System;

namespace Project_TRAILLL.Services
{
    public class WatcherService
    {
        public static string directoryToMonitor = "C:\\Users\\sally\\Desktop\\2023";
        public string parserDir = Path.Combine(directoryToMonitor, "Parser");
        public string archiveDir = Path.Combine(directoryToMonitor, "Archive");


        public void Main()
        {
            if (!Directory.Exists(directoryToMonitor))
            {
                Console.WriteLine("The specified directory does not exist.");
                Console.WriteLine("Press Enter to exit.");
                Console.ReadLine();
                return;
            }

            if (!Directory.Exists(parserDir))
                Directory.CreateDirectory(parserDir);

            if (!Directory.Exists(archiveDir))
                Directory.CreateDirectory(archiveDir);

            string[] existingTextFiles = Directory.GetFiles(directoryToMonitor, "*.txt");
            foreach (string filePath in existingTextFiles)
            {
                string fileName = Path.GetFileName(filePath);
                string parserDestinationPath = Path.Combine(parserDir, fileName);
                string archiveDestinationPath = Path.Combine(archiveDir, fileName);

                if (!File.Exists(parserDestinationPath))
                {
                    File.Copy(filePath, parserDestinationPath);
                    Console.WriteLine($"Copied {fileName} to the 'parser' folder.");
                }

                if (!File.Exists(archiveDestinationPath))
                {
                    File.Move(filePath, archiveDestinationPath);
                    Console.WriteLine($"Moved {fileName} to the 'archive' folder.");
                }
            }

            using var watcher = new FileSystemWatcher(directoryToMonitor);
            watcher.Created += OnCreated;
            watcher.EnableRaisingEvents = true;

            Console.WriteLine("File monitoring started. Press Enter to exit.");
            Console.ReadLine();
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            string fileName = Path.GetFileName(e.FullPath);
            string archiveFilePath = Path.Combine(archiveDir, fileName);

            if (!File.Exists(archiveFilePath))
            {
                File.Copy(e.FullPath, archiveFilePath);
                Console.WriteLine($"Copied {fileName} to the 'archive' folder.");

                string parserFilePath = Path.Combine(parserDir, fileName);
                File.Move(e.FullPath, parserFilePath);
                Console.WriteLine($"Copied {fileName} to the 'parser' folder.");
            }
            else
            {
                Console.WriteLine($"File {fileName} already exists in the 'archive' folder. Discarding it.");
                File.Delete(e.FullPath);
            }

        }
        //  ParserService parserService = new ParserService(@"C:\Users\User\Desktop\2023\Parser");


    }
}
