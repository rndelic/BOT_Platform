using System;

namespace MyFunctions.Exceptions
{

    [Serializable]
    public class WrongParamsException : Exception
    {
        public WrongParamsException() { }
        public WrongParamsException(string message) : base(message) { }
        public WrongParamsException(string message, Exception inner) : base(message, inner) { }
        protected WrongParamsException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}