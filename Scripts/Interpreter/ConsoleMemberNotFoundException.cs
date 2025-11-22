using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Terasievert.AbonConsole.Interpreter
{
    public class ConsoleMemberNotFoundException : KeyNotFoundException
    {
        public ConsoleMemberNotFoundException()
        {
        }

        public ConsoleMemberNotFoundException(string message) : base(message)
        {
        }

        public ConsoleMemberNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ConsoleMemberNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
