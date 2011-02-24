using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Xml;

namespace Skylabs.ConsoleHelper
{
    [Serializable]
    public class ConsoleEventError : ConsoleEvent
    {
        [XmlElement("ExceptionMessage")]
        public String ExceptionMessage
        {
            get
            {
                return exception.Message;
            }
            set { }
        }
        [XmlElement("StackTrace")]
        public String StackTrace
        {
            get
            {
                return exception.StackTrace;
            }
            set { }
        }
        [XmlIgnore()]
        public Exception exception { get; set; }
        ConsoleEventError()
        {

        }
        public ConsoleEventError(String message, Exception e):base(message)
        {
            Header = "!Error: ";
            Color = ConsoleColor.Red;
            exception = e;
        }
    }
}
