using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octgn.Lobby
{
    public class HostedGame
    {
        public String strName { get; set; }
        private String[] strHost;
        private int intPort;
        private String strGUID;
        public String strGameName { get; set; }
        private String strGameVersion;
        private String strPassword;
        public int intUGameNum { get; set; }
        private Boolean bIsAvailable;
        public String strHostName { get; set; }

        public String getName()
        {
            return strName;
        }

        public String[] getStrHost()
        {
            return strHost;
        }

        public int getIntPort()
        {
            return intPort;
        }

        public String getStrGUID()
        {
            return strGUID;
        }

        public String getStrGameName()
        {
            return strGameName;
        }

        public String getStrGameVersion()
        {
            return strGameVersion;
        }

        public String getStrPassword()
        {
            return strPassword;
        }

        public int getIntUGameNum()
        {
            return intUGameNum;
        }

        public Boolean isbIsAvailable()
        {
            return bIsAvailable;
        }

        public HostedGame()
        {
            bIsAvailable = false;
        }
        public HostedGame(String strName, String strGUID, String strGameName,
                String strGameVersion, String strPassword, String[] strHost, int intPort)
        {
            this.strName = strName;
            this.strGUID = strGUID;
            this.strGameName = strGameName;
            this.strGameVersion = strGameVersion;
            this.strPassword = strPassword;
            this.strHost = strHost;
            this.intPort = intPort;
            bIsAvailable = false;
        }
    }
}
