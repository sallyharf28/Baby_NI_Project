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
using Serilog;

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
                parserDir = Path.Combine(directoryToMonitor, "Output");
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

                // Check for existing files in the monitored folder
                string[] existingFiles = Directory.GetFiles(directoryToMonitor);
                foreach (string filePath in existingFiles)
                {
                    string fileName = Path.GetFileName(filePath);

                    if (FileExistsInVertica(fileName, File.GetLastWriteTime(filePath)))
                    {
                        // File with the same name already exists in Vertica
                        Console.WriteLine($"File {fileName} already exists in the database.");

                        if (!IsFileParsed(fileName))
                        {
                            // File exists in the database but is not parsed, move it to the output directory
                            MoveFileToParserDir(filePath, fileName);
                        }
                        else
                        {
                            // File is already parsed, update the reparsed column to true
                            UpdateParsedStatus(fileName, true);
                            Console.WriteLine($"File {fileName} is already parsed.");
                        }
                    }
                    else
                    {
                        // File does not exist in Vertica, add it to the database
                        AddFileToVertica(fileName);
                        MoveFileToParserDir(filePath, fileName);
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

                        //REEXECUTION// IF SET TRUE => WILL reexecute the file else delete it
                        //SET FALSE SO IT WONT PARSE IT AGAIN
                        shouldParse = false;
                    }
                    else
                    {
                        Console.WriteLine($"The modification date of {fileName} is the same. File not parsed and discarded.");
                        File.Delete(e.FullPath);
                    }

                    if (shouldParse)
                    {                    
                        MoveFileToParserDir(e.FullPath, fileName);
                        _parserService.parserDir = @"C:\Users\User\Desktop\2023\Output";
                        _parserService.ProcessTextFile();
                        UpdateParsedStatus(fileName, true);
                    }
                    else
                    {
                        Console.WriteLine($"File {fileName} not parsed and discarded.");
                        File.Delete(e.FullPath);
                    }
                }
                else
                {           
                    AddFileToVertica(fileName);
                    MoveFileToParserDir(e.FullPath, fileName);
                    _parserService.parserDir = @"C:\Users\User\Desktop\2023\Output";
                    _parserService.ProcessTextFile();
                }
            }

            private bool IsFileParsed(string fileName)
            {
                try
                {
                    using (var connection = new VerticaConnection(connectionString))
                    {
                        connection.Open();

                        using (var command = connection.CreateCommand())
                        {                        
                            command.CommandText = $"SELECT Reparsed FROM FileMetadata WHERE FileName = '{fileName}';";

                            command.CommandType = CommandType.Text;

                            object result = command.ExecuteScalar();

                            if (result != null && result != DBNull.Value)
                            {
                                return Convert.ToBoolean(result);
                            }

                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error checking if file {fileName} is parsed: {ex.Message}");
                    return false;
                }
            }

            private void UpdateParsedStatus(string fileName, bool parsedStatus)
            {
                try
                {
                    using (var connection = new VerticaConnection(connectionString))
                    {
                        connection.Open();

                        using (var command = connection.CreateCommand())
                        {
                            
                            command.CommandText = $"UPDATE FileMetadata SET Reparsed = {parsedStatus} WHERE FileName = '{fileName}';";

                            command.CommandType = CommandType.Text;
                            command.ExecuteNonQuery();

                            Console.WriteLine($"Parsed status for file {fileName} updated to {parsedStatus}.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating parsed status for file {fileName}: {ex.Message}");
                }
            }

            private void MoveFileToParserDir(string filePath, string fileName)
            {
                try
                {
                    string outputFilePath = Path.Combine(parserDir, fileName);

                    if (File.Exists(filePath))
                    {
                        File.Move(filePath, outputFilePath);
                       
                        Console.WriteLine($"File {fileName} moved to the output directory.");
                        _parserService.parserDir = @"C:\Users\User\Desktop\2023\Output";
                        _parserService.ProcessTextFile();
                    }
                    else
                    {
                        Console.WriteLine($"Error: File {fileName} does not exist at path {filePath}.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error moving file {fileName} to the output directory: {ex.Message}");
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
                    return false;
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
                            
                            command.CommandText = $"SELECT LastModifiedDate FROM FileMetadata WHERE FileName = '{fileName}';";

                            command.CommandType = CommandType.Text;

                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {                           
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
                    string filePath = Path.Combine(directoryToMonitor, fileName); 

                    if (File.Exists(filePath))
                    {
                        DateTime modificationDate = File.GetLastWriteTime(filePath);

                        using (var connection = new VerticaConnection(connectionString))
                        {
                            connection.Open();

                            using (var command = connection.CreateCommand())
                            {
                                
                                command.CommandText = $"INSERT INTO FileMetadata (FileName, LastModifiedDate, Reparsed) VALUES ('{fileName}', '{modificationDate:yyyy-MM-dd HH:mm:ss}', false);";

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
        }
    }





