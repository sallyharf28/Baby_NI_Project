using System.Diagnostics;
using System.Drawing;
using System.Security.Cryptography;
using System.Collections.Generic;
using Microsoft.VisualBasic.FileIO;
using System.IO;
using System.Globalization;
using System.Text;
using System;
using System.Data.SqlTypes;
using Vertica.Data.VerticaClient;
using System.Linq;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Microsoft.Extensions.Logging;

namespace Project_TRAILLL.Services
{
    public class WatcherService : IWatcherService
    {
        private readonly IParserService _parserService;

        private readonly string connectionString;

       
        public string directoryToMonitor { get; set; }
        public string parserDir { get; set; }
        public WatcherService(IParserService parserService)
        {
            _parserService = parserService;
           
            connectionString = "Server=10.10.4.231;Database=test;User=bootcamp4;Password=bootcamp42023";
            directoryToMonitor = "C:\\Users\\User\\Desktop\\2023";
            parserDir = Path.Combine(directoryToMonitor, "Parser");

        }
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

            using var watcher = new FileSystemWatcher(directoryToMonitor);
            watcher.Created += OnCreated;
            watcher.EnableRaisingEvents = true;

            Console.WriteLine("File monitoring started. Press Enter to exit.");
           

            Console.ReadLine();
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            string fileName = Path.GetFileName(e.FullPath);
            string parserFilePath = Path.Combine(parserDir, fileName);
           
            bool shouldParse = false;
            if (FileExistsInVertica(fileName, File.GetLastWriteTime(e.FullPath)))
            {
                // File with the same name already exists in Vertica
                Console.WriteLine($"File {fileName} already exists in the database.");

                DateTime verticaModificationDate = GetModificationDateFromVertica(fileName);

                // Check if the modification date is different
                if (verticaModificationDate != File.GetLastWriteTime(e.FullPath))
                {
                    Console.WriteLine($"The modification date of {fileName} is different.");
                    // Ask the user if they want to parse the file
                    shouldParse = true;
           
                }
                else
                {
                    Console.WriteLine($"The modification date of {fileName} is the same. File not parsed and discarded.");
                    File.Delete(e.FullPath);
                }

                if (shouldParse)
                {
                    // Your parsing logic goes here
                    File.Copy(e.FullPath, parserFilePath);
                    _parserService.parserDir = @"C:\Users\User\Desktop\2023\Parser";
                    _parserService.ProcessTextFile();
                    //UpdateVerticaModificationDate(fileName, File.GetLastWriteTime(e.FullPath));
                }
                else
                {
                    Console.WriteLine($"File {fileName} not parsed and discarded.");
                    File.Delete(e.FullPath);
                }
            }
            else
            {
                // Your parsing logic goes here
                AddFileToVertica(fileName);
                File.Move(e.FullPath, parserFilePath);
                _parserService.parserDir = @"C:\Users\User\Desktop\2023\Parser";
                _parserService.ProcessTextFile();
            }   

        }
     
        private bool FileExistsInVertica(string fileName, DateTime modificationDate)
        {
            try
            {
                using (var connection = new VerticaConnection(connectionString))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        // Assuming you have a table named FileMetadata in your Vertica database
                        command.CommandText = $"SELECT COUNT(*) FROM FileMetadata WHERE FileName = '{fileName}' AND LastModifiedDate = '{modificationDate:yyyy-MM-dd HH:mm:ss}';";

                        command.CommandType = CommandType.Text;

                        int count = Convert.ToInt32(command.ExecuteScalar());

                        // If count is greater than 0, the file exists in Vertica
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking if file {fileName} exists in Vertica: {ex.Message}");
                return false; // Return false in case of an error
            }

        }

        private DateTime GetModificationDateFromVertica(string fileName)
        {
            DateTime modificationDate = DateTime.MinValue;

            try
            {
                using (var connection = new VerticaConnection(connectionString))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        // Assuming you have a table named FileMetadata in your Vertica database
                        command.CommandText = $"SELECT LastModifiedDate FROM FileMetadata WHERE FileName = '{fileName}';";

                        command.CommandType = CommandType.Text;

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Assuming the LastModifiedDate column is of type TIMESTAMP in Vertica
                                modificationDate = reader.GetDateTime(0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving modification date from Vertica for file {fileName}: {ex.Message}");
            }

            return modificationDate;
        }

        private void AddFileToVertica(string fileName)
        {
            try
            {
                string filePath = Path.Combine(directoryToMonitor, fileName); // Assuming the file is in the monitored directory

                if (File.Exists(filePath))
                {
                    DateTime modificationDate = File.GetLastWriteTime(filePath);

                    using (var connection = new VerticaConnection(connectionString))
                    {
                        connection.Open();

                        using (var command = connection.CreateCommand())
                        {
                            // Assuming you have a table named FileMetadata in your Vertica database
                            command.CommandText = $"INSERT INTO FileMetadata (FileName, LastModifiedDate) VALUES ('{fileName}', '{modificationDate:yyyy-MM-dd HH:mm:ss}');";

                            command.CommandType = CommandType.Text;
                            command.ExecuteNonQuery();

                            Console.WriteLine($"File {fileName} information added to the database.");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Error: File {fileName} does not exist at path {filePath}.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding file {fileName} information to Vertica: {ex.Message}");
            }
        }

        private void UpdateVerticaModificationDate(string fileName, DateTime modificationDate)
        {
            try
            {
                using (var connection = new VerticaConnection(connectionString))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        // Assuming you have a table named FileMetadata in your Vertica database
                        command.CommandText = $"UPDATE FileMetadata SET LastModifiedDate = '{modificationDate:yyyy-MM-dd HH:mm:ss}' WHERE FileName = '{fileName}';";

                        command.CommandType = CommandType.Text;
                        command.ExecuteNonQuery();

                        Console.WriteLine($"Modification date updated for file {fileName} in Vertica.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating modification date for file {fileName} in Vertica: {ex.Message}");
            }
        }
    }
}
        
