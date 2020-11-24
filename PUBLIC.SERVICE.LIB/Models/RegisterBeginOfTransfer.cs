using System.Collections.Generic;

namespace PUBLIC.SERVICE.LIB.Models
{
    public class RegisterBeginOfTransfer
    {
        public string SolutionName { get; set; }
        public string SolutionReference { get; set; }
        public List<FileWithFingerPrintInfo> UploadFiles { get; set; }
    }
}
