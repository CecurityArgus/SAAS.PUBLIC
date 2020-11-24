using System.Collections.Generic;

namespace PUBLIC.CONTROLLER.LIB.DTOs
{
    /// <summary>
    /// 
    /// </summary>
    public class DtoUpload
    {
        /// <summary>
        /// 
        /// </summary>
        public class TransferObject : RegisterBeginOfTransfer
        {
            /// <summary>
            /// 
            /// </summary>
            public string SubscriptionId { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class RegisterBeginOfTransfer
        {
            /// <summary>
            /// 
            /// </summary>
            public string SolutionName { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string SolutionReference { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public List<FileWithFingerPrintInfo> UploadFiles { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class TransferRegistered
        {
            /// <summary>
            /// 
            /// </summary>
            public string TransferId { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class FileWithFingerPrintInfo
        {
            /// <summary>
            /// 
            /// </summary>
            public string FileName { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public long FileSize { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string FingerPrint { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string FingerPrintAlgorithm { get; set; }
        }
    }
}
