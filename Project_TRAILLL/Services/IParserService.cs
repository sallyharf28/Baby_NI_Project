namespace Project_TRAILLL.Services
{
    public interface IParserService
    {
        string parserDir { get; set; }
        void ProcessTextFile();
        Dictionary<string, string>[] ReadTxtToDict(string filename);
        void WriteDictArrayToCsv(Dictionary<string, string>[] dataDictArray, string filename);
        void RemovedColumnsFromData(Dictionary<string, string>[] dataDictArray, string[] columnsToRemove);
        Dictionary<string, string>[] AddNewClmns(Dictionary<string, string>[] dataDictArray, string fileName);
        string CalculateHashVal(string input);
        string ExtractDateTime(string fileName);
        Dictionary<string, string>[] AddToEndClmns(Dictionary<string, string>[] dataDictArray);
        string ExtractTIDFromObject(string objectValue);
        string ExtractFarEndTIDFromObject(string objectValue);
        void ExtractLinkFromObject(string objectValue, out string link);
        void ExtractSlotAndPort(Dictionary<string, string> row, List<Dictionary<string, string>> resultRows);
        Dictionary<string, string>[] AddToEndClmnsFile2(Dictionary<string, string>[] dataDictArray);
        void ExtractSlot(string objectValue, out string slot);
        void ExtractPort(string objectValue, out string port);
    }
}
