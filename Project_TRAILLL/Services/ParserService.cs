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


                //Adding 2 clmns
                dataDictArray = AddNewClmns(dataDictArray, fileName);


                //Discard rows where "FailureDescription" is not equal to "-"
                dataDictArray = dataDictArray
                    .Where(row => !row.ContainsKey("FailureDescription") || row["FailureDescription"] == "-")
                    .ToArray();

                if (fileName.Contains("SOEM1_TN_RADIO_LINK_POWER"))
                {
                    //Adding to endoffile
                    dataDictArray = AddToEndClmns(dataDictArray);

                    columnsToRemove = new string[] { "NodeName", "Position", "IdLogNum" };
                    RemovedColumnsFromData(dataDictArray, columnsToRemove);

                }
                else if (fileName.Contains("SOEM1_TN_RFInputPower"))
                {
                    dataDictArray = AddToEndClmnsFile2(dataDictArray);

                    columnsToRemove = new string[] { "Position", "MeanRxLevel1m", "IdLogNum", "FailureDescription" };
                    RemovedColumnsFromData(dataDictArray, columnsToRemove);

                    dataDictArray = dataDictArray
                    .Where(row => !row.ContainsKey("FarEndTID") || row["FarEndTID"] != "----")
                    .ToArray();
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
        public Dictionary<string, string>[] ReadTxtToDict(string filename)
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

       

        public void WriteDictArrayToCsv(Dictionary<string, string>[] dataDictArray, string filename)
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
            foreach (var dataDict in dataDictArray)
            {
                foreach (var column in columnsToRemove)
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

                    ExtractLinkFromObject(objectValue, out string linkValue);
                    newRow["Link"] = linkValue;


                    newRow["TID"] = ExtractTIDFromObject(objectValue);
                    newRow["FarEndTID"] = ExtractFarEndTIDFromObject(objectValue);

                    ExtractSlotAndPortFromObject(objectValue, out string slotValue, out string portValue);

                    newRow["Slot"] = slotValue;
                    newRow["Port"] = portValue;
                    
                }

                newRow["Link"] = newRow.ContainsKey("Link") ? newRow["Link"] : "UnknownLinkValue";
                newRow["TID"] = newRow.ContainsKey("TID") ? newRow["TID"] : "UnknownTIDValue";
                newRow["FarEndTID"] = newRow.ContainsKey("FarEndTID") ? newRow["FarEndTID"] : "UnknownFarEndTIDValue"; ;
                newRow["Slot"] = newRow.ContainsKey("Slot") ? newRow["Slot"] : "UnknownSlotValue";
                newRow["Port"] = newRow.ContainsKey("Port") ? newRow["Port"] : "UnknownPortValue"; 
                return newRow;

            }).ToArray();
        }
        public string ExtractTIDFromObject(string objectValue)
        {
            // Extract the value between the first "_" and last "_"
            int firstUnderscoreIndex = objectValue.IndexOf("__");
            int lastUnderscoreIndex = objectValue.LastIndexOf("__");

            if (firstUnderscoreIndex >= 0 && lastUnderscoreIndex >= 0 && firstUnderscoreIndex < lastUnderscoreIndex)
            {
                return objectValue.Substring(firstUnderscoreIndex + 2, lastUnderscoreIndex - firstUnderscoreIndex - 2);
            }

            return "UnknownTIDValue";
        }

        public string ExtractFarEndTIDFromObject(string objectValue)
        {
            int lastDoubleUnderscoreIndex = objectValue.LastIndexOf("__");

            if (lastDoubleUnderscoreIndex >= 0)
            {
                return objectValue.Substring(lastDoubleUnderscoreIndex + 2);
            }

            return "UnknownFarEndTIDValue";
        }

        public void ExtractLinkFromObject(string objectValue, out string link)
        {
            link = "UnknownLinkValue";

            int underscoreIndex = objectValue.IndexOf('_');
            if (underscoreIndex >= 0)
            {
                objectValue = objectValue.Substring(0, underscoreIndex);
            }

            // Check if "." exists in the middle of the Object value
            int dotIndex = objectValue.IndexOf(".");
            if (dotIndex >= 0)
            {
                while (objectValue.StartsWith("1/"))
                {
                    objectValue = objectValue.Substring(2);
                }


                while (objectValue.EndsWith("/1"))
                {
                    objectValue = objectValue.Substring(0, objectValue.Length - 2);
                }
                objectValue = objectValue.Replace(".", "/");
                link = objectValue;
            }
            else
            {
                while (objectValue.StartsWith("1/"))
                {
                    objectValue = objectValue.Substring(2);
                }
                link = objectValue;
            }
        }

        public void ExtractSlotAndPortFromObject(string objectValue, out string slot, out string port)
        {
            slot = "UnknownSlotVal";
            port = "UnknownPortVal";

            int firstUnderscoreIndex = objectValue.IndexOf("__");

            if (firstUnderscoreIndex >= 0)
            {
                string slotPortPart = objectValue.Substring(0, firstUnderscoreIndex);
                string[] slotPortArray = slotPortPart.Split('+');

                if (slotPortArray.Length > 1)
                {
                    // Handle Case 1: Multiple slots
                    foreach (var slotPort in slotPortArray)
                    {
                        string[] slotPortPair = slotPort.Split('/');
                        if (slotPortPair.Length == 2)
                        {
                            slot = slotPortPair[0];
                            port = slotPortPair[1];

                        }
                    }
                }
                else
                {
                    // Handle Case 2: Single slot
                    string[] slotPortPair = slotPortPart.Split('/');
                    
                    if (slotPortPair.Length >= 1)
                    {
                        slot = slotPortPair[1];
                        port = "1";
                    }
                }
            }
        }

        public Dictionary<string, string>[] AddToEndClmnsFile2(Dictionary<string, string>[] dataDictArray)
        {
            return dataDictArray.Select(row =>
            {
                Dictionary<string, string> newRow = new Dictionary<string, string>(row); // Create a new dictionary to preserve existing data
                if (row.ContainsKey("Object"))
                {
                    string objectValue = row["Object"];

                    ExtractSlot(objectValue, out string slotValue);
                    newRow["Slot"] = slotValue;

                    ExtractPort(objectValue, out string portValue);
                    newRow["Port"]= portValue;
                }

                newRow["Slot"] = newRow.ContainsKey("Slot") ? newRow["Slot"] : "UnknownSlotValue";
                newRow["Port"] = newRow.ContainsKey("Port") ? newRow["Port"] : "UnknownPortValue";
                return newRow;

            }).ToArray();

        }

        public static void ExtractSlot(string objectValue, out string slot)
        {
            slot = "UnknownSlotValue";
            
            // Split the Object value by "+" and take the first part
            string[] objectParts = objectValue.Split('+');
            if (objectParts.Length > 0)
            {
                // Replace ".1" with "+"
                string slotPart = objectParts[0].Replace(".1", "+");

                if (slotPart.EndsWith("/1"))
                {
                    slotPart = slotPart.Substring(0, slotPart.Length - 2);
                }

                slot = slotPart;
            }
        }
        public static void ExtractPort(string objectValue, out string port)
        {
            port = "UnknownPortValue";

            int lastDotIndex = objectValue.LastIndexOf('.');
            if (lastDotIndex >= 0)
            {
                port = objectValue.Substring(lastDotIndex + 1);

                if (port.EndsWith("/1"))
                {
                    port = port.Substring(0, port.Length - 2);
                }
            }
        }
    }
}
