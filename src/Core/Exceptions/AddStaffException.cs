using System;
using System.Runtime.Serialization;

namespace Core.Exceptions
{
    public class AddStaffException : Exception
    {
        public AddStaffException()
        {
        }

        public AddStaffException(string message) : base(message)
        {
        }

        public AddStaffException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AddStaffException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
