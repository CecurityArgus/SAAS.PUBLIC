namespace PUBLIC.SERVICE.LIB.Models
{
    public class FileWithFingerPrintInfo
    {
        public string FileName { get; set; }
        public long FileSize { get; set; }

        public string FingerPrint { get; set; }

        public string FingerPrintAlgorithm { get; set; }
    }
}
