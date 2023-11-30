namespace Retreivedata.Models
{
    public class AggregatedData
    {
        public DateTime DateTimeKey { get; set; }
        public int NetworkSID { get; set; }
        public string? NeAlias { get; set; }
        public string? NeType { get; set; }
        public float RslInputPower { get; set; }
        public float MaxRxLevel { get; set; }
        public float RslDeviation { get; set; }
    }

}
