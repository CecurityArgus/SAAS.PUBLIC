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
            public List<UploadFile> UploadFiles { get; set; }
        }

        public class BeginOfTransferRegistered
        {
            public string TransferId { get; set; }
        }

        public class UploadFile
        {
            public string Name { get; set; }
            public long Size { get; set; }
            public string Hash { get; set; }
        }
    }
}
