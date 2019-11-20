using System;
using System.Runtime.Serialization;

namespace PUBLIC.CONTROLLER.Helpers
{
    public class CecurityError
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public object AdditionalInfo { get; set; }
    }
    public class CecurityException : Exception
    {
        public string Code { get; set; }
        public object AdditionalInfo { get; set; }

        public CecurityException()
        {
            Code = "PUBLIC_API_99999";
            AdditionalInfo = null;
        }

        public CecurityException(string code, string message) : base(message)
        {
            Code = code;
            AdditionalInfo = null;
        }
        public CecurityException(string code, string message, object additionalInfo) : base(message)
        {
            Code = code;
            AdditionalInfo = additionalInfo;
        }

        public CecurityException(string code, string message, object additionalInfo, Exception innerException) : base(message, innerException)
        {
            Code = code;
            AdditionalInfo = additionalInfo;
        }

        protected CecurityException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}