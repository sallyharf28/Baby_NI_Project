using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Project_TRAILLL.Services
{
    public class ParserService
    {
        public readonly string parserDir;

        public ParserService(string parserDir)
        {
            this.parserDir = parserDir;
            ProcessTextFile();

        }

        public void ProcessTextFile()
        {
            string[] textFiles = Directory.GetFiles(parserDir, "*.txt");

            foreach (string filePath in textFiles)
            {
                string fileName = Path.GetFileName(filePath);
                string csvFilePath = Path.ChangeExtension(filePath, ".csv");
                string[] columnsToRemove;
                Dictionary<string, string>[] dataDictArray = ReadTxtToDict(filePath);

                //Remove rows in the Object clm
                /* dataDictArray = dataDictArray
                        .Where(row => !row.ContainsKey("Object") || row["Object"] != "Unreachable Bulk FC")
                        .ToArray();

                 //Trim the Object values
                 foreach (var row in dataDictArray)
                 {
                     if (row.ContainsKey("Object"))
                     {
                         string objectValue = row["Object"];
                         int underscoreVal = objectValue.IndexOf('_');
                         if (underscoreVal >= 0)
                         {
                             row["Object"] = objectValue.Substring(0, underscoreVal);
                         }
                     }
                 }*/

                //Adding 2 clmns
                dataDictArray = AddNewClmns(dataDictArray,fileName);

                //Adding to endoffile
                dataDictArray = AddToEndClmns(dataDictArray);
                //Discard rows where "FailureDescription" is not equal to "-"
                dataDictArray = dataDictArray
                    .Where(row => !row.ContainsKey("FailureDescription") || row["FailureDescription"] == "-")
                    .ToArray();

                if (fileName.Contains("SOEM1_TN_RADIO_LINK_POWER"))
                {
                     columnsToRemove = new string[] { "NodeName", "Position", "IdLogNum" };
                     RemovedColumnsFromData(dataDictArray, columnsToRemove);

                }
                else if (fileName.Contains("SOEM1_TN_RFInputPower"))
                {
                    columnsToRemove = new string[] { "Position", "MeanRxLevel1m","IdLogNum", "FailureDescription" };
                    RemovedColumnsFromData(dataDictArray, columnsToRemove);
                }
                
                WriteDictArrayToCsv(dataDictArray, csvFilePath);
                File.Delete(filePath);

                Console.WriteLine($"Converted {fileName} to CSV format.");
            }

        }

        /*public static void ConvertTxtToCsv(string inputFilePath, string outputFilePath)
        {
            try
            {
                using (var inputFile = new StreamReader(inputFilePath))
                using (var outputFile = new StreamWriter(outputFilePath))
                {
                    while (!inputFile.EndOfStream)
                    {
                        string line = inputFile.ReadLine();
                        string[] values = line.Split(',');
                        string csvLine = string.Join(",", values);

                        outputFile.WriteLine(csvLine);
                    }

                    Console.WriteLine($"Conversion completed. Data from {inputFilePath} is saved to {outputFilePath}.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }*/
        ///
        public  Dictionary<string, string>[] ReadTxtToDict(string filename)
        {
            string[] lines = File.ReadAllLines(filename);
            string[] headers = lines[0].Split(',');
            Dictionary<string, string>[] dataDictArray = new Dictionary<string, string>[lines.Length - 1];

            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(',');
                Dictionary<string, string> dataDict = new Dictionary<string, string>();

                for (int j = 0; j < headers.Length; j++)
                {
                    dataDict[headers[j]] = values[j];
                }

                dataDictArray[i - 1] = dataDict;
            }

            return dataDictArray;
        }
        
        ///NodeName
        
        public  void WriteDictArrayToCsv(Dictionary<string, string>[] dataDictArray, string filename)
        {
            if (dataDictArray.Length == 0)
            {
                throw new Exception("No data to write.");
            }

            string[] headers = dataDictArray[0].Keys.ToArray();

            using (StreamWriter writer = new StreamWriter(filename))
            {
                writer.WriteLine(string.Join(",", headers)); // Write the headers

                foreach (var dataDict in dataDictArray)
                {
                    string[] values = headers.Select(header => dataDict[header]).ToArray();
                    writer.WriteLine(string.Join(",", values)); // Write the data
                }
            }
        }

        public void RemovedColumnsFromData(Dictionary<string, string>[] dataDictArray, string[] columnsToRemove)
        {
            foreach(var dataDict in dataDictArray)
            {
                foreach(var column in columnsToRemove)
                {
                    if (dataDict.ContainsKey(column))
                    {
                        dataDict.Remove(column);
                    }
                }
            }
        }

        public Dictionary<string, string>[] AddNewClmns(Dictionary<string, string>[] dataDictArray, string fileName)
        {
            return dataDictArray.Select(row =>
            {
                Dictionary<string, string> newRow = new Dictionary<string, string>
        {
            { "Network_SID", CalculateHashVal(row["NeAlias"] + row["NeType"]) },
            { "DateTime_Key", ExtractDateTime(fileName)}
        };
                foreach (var kvp in row)
                {
                    newRow.Add(kvp.Key, kvp.Value);
                }
                return newRow;
            }).ToArray();
        }

        public string CalculateHashVal(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder hash = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    hash.Append(data[i].ToString("x2"));
                }
                return hash.ToString();
            }
        }

        public string ExtractDateTime(string fileName)
        {
            // Extract date and time information from the file name based on the pattern
            string pattern = @"_(\d{4})(\d{2})(\d{2})_(\d{2})(\d{2})(\d{2})";
            Match match = Regex.Match(fileName, pattern);

            if (match.Success && match.Groups.Count == 7)
            {
                int year = int.Parse(match.Groups[1].Value);
                int month = int.Parse(match.Groups[2].Value);
                int day = int.Parse(match.Groups[3].Value);
                int hour = int.Parse(match.Groups[4].Value);
                int minute = int.Parse(match.Groups[5].Value);
                int second = int.Parse(match.Groups[6].Value);

                // Create a DateTime object from the extracted values
                DateTime dateTime = new DateTime(year, month, day, hour, minute, second);

                // Format the DateTime as needed
                return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                // Handle the case where date and time information is not found in the file name
                return "UnknownDateTime";
            }
        }

        public Dictionary<string, string>[] AddToEndClmns(Dictionary<string, string>[] dataDictArray)
        {
            return dataDictArray.Select(row =>
            {
                Dictionary<string, string> newRow = new Dictionary<string, string>(row); // Create a new dictionary to preserve existing data

                if (row.ContainsKey("Object"))
                {
                    string objectValue = row["Object"];

                    int underscoreIndex = objectValue.IndexOf('_');
                    if (underscoreIndex >= 0)
                    {
                        objectValue = objectValue.Substring(0, underscoreIndex);
                    }

                    // Check if "." exists in the middle of the Object value
                    string[] parts = objectValue.Split('.');

                    if (parts.Length > 1)
                    {
                        string[] slotPortParts = parts[parts.Length - 1].Split('/');
                        if (slotPortParts.Length == 2)s
                        {
                            newRow["Link"] = $"{slotPortParts[0]}/{slotPortParts[1]}"; // Format as "Slot/Port"
                        }
                    }
                    else
                    {
                        newRow["Link"] = objectValue;
                    }
                }

                newRow["Link"] = newRow.ContainsKey("Link") ? newRow["Link"] : "UnknownLinkValue";
                newRow["TID"] = "YourTIDValue";
                newRow["FarEndTID"] = "YourFarEndTIDValue";
                newRow["Slot"] = "YourSlotValue";
                newRow["Port"] = "YourPortValue";
                return newRow;
            }).ToArray();
        }


    }
}
