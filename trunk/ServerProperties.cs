using System;
using System.Xml;
using System.Collections.Generic;
namespace Skylabs.oserver
{
	public class ServerProperties
	{
		public XmlDocument Properties{get;set;}
		public ServerProperties ()
		{
			Properties = new XmlDocument();
			Properties.LoadXml("ServerOptions.xml");
		}
		
	}
}

