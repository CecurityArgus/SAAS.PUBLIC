using System.Collections.Generic;

namespace PUBLIC.CONTROLLER.LIB.DTOs
{
    public class DtoUpload
    {
        public class TransferObject : RegisterBeginOfTransfer
        {
            public string SubscriptionId { get; set; }
        }
        public class RegisterBeginOfTransfer
        {
            public string SolutionName { get; set; }
            public string SolutionReference { get; set; }
            public List<FileWithFingerPrintInfo> UploadFiles { get; set; }
        }
        public class TransferRegistered
        {
            public string TransferId { get; set; }
        }
        public class FileWithFingerPrintInfo
        {
            public string FileName { get; set; }
            public long FileSize { get; set; }

            public string FingerPrint { get; set; }

            public string FingerPrintAlgorithm { get; set; }
        }
    }
}
