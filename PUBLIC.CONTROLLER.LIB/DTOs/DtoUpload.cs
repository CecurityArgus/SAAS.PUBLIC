using System;
using System.Collections.Generic;
using System.Text;

namespace PUBLIC.CONTROLLER.LIB.DTOs
{
    public class DtoUpload
    {
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

        public class RegisterUploadedFile
        {
            public string fileName { get; set; }

            public string transferId { get; set; }

            public long fileSize { get; set; }
        }

        public class SubscriptionWithDetails
        {
            public string Id { get; set; }
            public int ReferenceType { get; set; }
            public string ReferenceId { get; set; }
            public string SolutionId { get; set; }
            public string SolutionParams { get; set; }
            public object Solution { get; set; }
        }

    }
}
