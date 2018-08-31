using System;
using System.Runtime.Serialization;

namespace Core.Exceptions
{
    public class UpdateStaffException : Exception
    {
        public UpdateStaffException()
        {
        }

        public UpdateStaffException(string message) : base(message)
        {
        }

        public UpdateStaffException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UpdateStaffException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
