using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Runtime.Serialization.Formatters;

namespace Skylabs.ConsoleHelper
{
    public class ConsoleEventLog
    {
        public delegate void EventEventDelegate(ConsoleEvent e);
        public static event EventEventDelegate eAddEvent = null;
        public static List<ConsoleEvent> Events { get { return _Events; } set { _Events = value;} }
        private static List<ConsoleEvent> _Events = new List<ConsoleEvent>();
        public static void addEvent(ConsoleEvent con)
        {
            Events.Add(con);
            if (eAddEvent != null)
            {
                if (eAddEvent.GetInvocationList().Length != 0)
                    eAddEvent.Invoke(con);
            }
        }
        public static void SerializeEvents(string filename)
        {
            using (Stream stream = File.Open(filename, FileMode.Create, FileAccess.ReadWrite))
            {
                XmlSerializer xs;
                XmlTextWriter xmlTextWriter = new XmlTextWriter(stream, Encoding.ASCII);
                xmlTextWriter.Formatting = Formatting.Indented;
                xmlTextWriter.Indentation = 4;
                List<Type> cTypes = new List<Type>();
                foreach (ConsoleEvent c in ConsoleEventLog.Events)
                {
                    bool foundit = false;
                    foreach(Type t in cTypes)
                    {
                        if (c.GetType() == t)
                        {
                            foundit = true;
                            break;
                        }
                    }
                    if (!foundit)
                        cTypes.Add(c.GetType());
                }
                xs = new XmlSerializer(typeof(ConsoleEvent),cTypes.ToArray());
                //xs.Serialize(xmlTextWriter, events);
                foreach (ConsoleEvent c in ConsoleEventLog.Events)
                {
                    xs = new XmlSerializer(c.GetType());
                    xs.Serialize(xmlTextWriter, c);
                }

            }
        }
    }
}
