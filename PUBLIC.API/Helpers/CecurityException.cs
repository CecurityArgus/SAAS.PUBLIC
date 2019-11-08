using System;
using System.Runtime.Serialization;

namespace PUBLIC.API.Helpers
{
    public class CecurityException : Exception
    {
        private int _code;

        public int Code() { return _code; }

        public CecurityException()
        {
        }

        public CecurityException(int code, string message) : base(message)
        {
            _code = code;
        }

        public CecurityException(int code, string message, Exception innerException) : base(message, innerException)
        {
            _code = code;
        }

        protected CecurityException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}