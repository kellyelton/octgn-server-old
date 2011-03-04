using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Collections.Generic;


namespace Skylabs.Networking
{
    public abstract class SocketClient
    {
        private Socket sock; 
        private IPEndPoint ipEnd;
        private Boolean boolEnd = false;
        private Boolean boolConnected = false;
        private String strDisconnectReason = "";
        private int intLastPing = 0;
        protected Thread oThread;
        protected String strHost;
        protected Int32 intPort;

        public Boolean Connected
        {
            get { return boolConnected; }
        }

        private enum SocketReadState
        {
            WaitingForStart,Reading,Ended, inHeader, inArgument
        }
        /// <summary>
        /// A class used as the base of messages sent and recieved.
        /// It has variable, Empty, that if it is true, this class is treated as 
        /// being empty. If there is no Header information, the class is considered
        /// Empty, so make sure it has a header before you send anything. 
        /// </summary>
        public  class SocketMessage
        {
            public String Header
            {
                get
                {
                    if (_Header != null && !_Header.Trim().Equals(""))
                        return _Header;
                    else
                        return "";
                }
                set
                {
                    if (value != null && !value.Trim().Equals(""))
                    {
                        _Header = value;
                    }
                    else
                        _Header = "";
                }
            }
            public Boolean Empty
            {
                get
                {
                    if (_Header != null && !_Header.Trim().Equals(""))
                        return false;
                    else
                        return true;
                }
            }
            public List<String> Arguments;
            private String _Header;
            public SocketMessage()
            {
                _Header = "";
                Arguments = new List<string>(0);
            }
            public SocketMessage(String header)
            {
                _Header = "";
                Header = header;
                Arguments = new List<string>(0);
            }
            /// <summary>
            /// Takes the message data and formats it into a string
            /// for transmitting.
            /// </summary>
            /// <returns>String for transmitting</returns>
            public virtual String getMessage()
            {
                String ret = "";
                if (Empty)
                    return ret;

                ret += (char)2;
                ret += _Header;
                if (Arguments.Count != 0)
                {
                    ret += (char)3;
                    for(int i=0;i<Arguments.Count;i++)
                    {
                        ret += Arguments[i];
                        if (i != Arguments.Count - 1)
                            ret += (char)4;
                    }
                }
                ret += (char)5;
                return ret;

            }
        }
        
        /// <summary>
        /// A version of SocketMessage that is used for pinging. It requires no header or arguments.
        /// </summary>
        public class PingMessage : SocketMessage
        {
            public override String getMessage()
            {
                String ret = "";
                ret += (char)1;
                return ret;
            }
        }
        public class EndMessage : SocketMessage
        {
            public override String getMessage()
            {
                String ret = "";
                ret += (char)6;
                return ret;
            }
        }

        /// <summary>
        /// Connect to a server.
        /// </summary>
        /// <param name="Host">Host name of the server</param>
        /// <param name="Port">Port of the server.</param>
        /// <returns>Boolean. True if connected, false if an error.</returns>
        public Boolean Connect(String Host, Int32 Port)
        {
            try
            {
                strHost = Host;
                intPort = Port;
                ipEnd = HostToEndpoint(Host,Port);
                sock = new Socket(ipEnd.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                sock.ReceiveTimeout = 10000;
                sock.Connect(ipEnd);
                boolConnected = true;
                intLastPing = 0;
                handleConnect(Host, Port);
                oThread = new Thread(new ThreadStart(this.run));
                oThread.Start();
                return true;
            }
            catch(Exception e)
            {
                handleError("Connect method: " + e.Message);
            }
            return false;
        }

        /// <summary>
        /// Close the connection to the server.
        /// </summary>
        /// <param name="reason">Reason for disconnecting. The reason is sent to the server.</param>
        /// <param name="sendCloseMessage">Should the socket attempt to notify the remote socket of the disconnect. Don't use if the connection has already been dropped.</param>
        public void Close(String reason, Boolean sendCloseMessage)
        {
            boolEnd = true;
            boolConnected = false;
            strDisconnectReason = reason;
            if (sendCloseMessage)
                writeMessage(new EndMessage());
            handleDisconnect(reason, strHost, intPort);
        }

        /// <summary>
        ///    Client loop. Automates pinging to make sure the connection still exists.
        ///     This should be called as a thread because it's a loop.
        ///     THIS FUNCTION STARTS THE CLIENT AFTER CONNECTION, WITHOUT IT NOTHING HAPPENS.
        /// </summary>
        public void run()
        {
            while(!boolEnd)
            {
                SocketMessage sm = readSocket();
                if (intLastPing == 10)
                    Close("Haven't recieved data in too long.", true);
                if (boolEnd)
                    break;
                if (!sm.Empty)
                    handleInput(sm);
                try
                {

                    Thread.Sleep(100);
                }
                catch (Exception ie) 
                {
                    Close("Error: " + ie.Message, false);
                }
            }
            try
            {
                sock.Close();
            }
            catch (Exception ioe) { }
            try
            {
                this.oThread.Join(1000);
            }
            catch (Exception e)
            { }
            try
            {
                this.oThread.Abort();
            }
            catch (Exception e)
            { }
        }
        /// <summary>
        /// Reads data from the socket. Handles fragmented data and data fracturing.
        /// </summary>
        /// <returns>SocketMessage with data recieved, or a SocketMessage with .Empty being true on no message.</returns>
        private SocketMessage readSocket()
        {
            String strBuff = "";
            String strTemp;
            int intTimeoutCount = 0;
            Boolean bDone = false;
            SocketReadState sr = SocketReadState.WaitingForStart;
            System.Text.Encoding enc = System.Text.Encoding.ASCII;

            byte[] bb;
            while (!bDone)
            {
                bb = new byte[256];
                try
                {
                    //sock.Receive(bb);
                    sock.Receive(bb, 256, SocketFlags.None);

                }
                catch (System.Net.Sockets.SocketException se)
                {

                    if (se.SocketErrorCode == SocketError.TimedOut)
                    {
                        if (!strBuff.Equals(""))
                        {
                            intTimeoutCount++;
                            if (intTimeoutCount == 10)
                            {
                                Close("readSocket timed out in the middle of a message", false);
                                return new SocketMessage();
                            }
                            else
                            {
                                try
                                {
                                    Thread.Sleep(100);
                                }
                                catch (Exception e)
                                {
                                    // TODO handle this exception I suppose.
                                }
                                continue;
                            }
                        }
                        else
                        {
                            writeMessage(new PingMessage());
                            return new SocketMessage();
                        }
                    }
                    else
                    {
                        Close("Socket error: " + se.ErrorCode.ToString() + " : " + se.Message, false);
                        return new SocketMessage();
                    }
                }
                catch (Exception e)
                {
                    return new SocketMessage();
                }
                try
                {
                    strTemp = enc.GetString(bb, 0, Array.IndexOf(bb, (byte)0));
                }
                catch (Exception e)
                {
                    strTemp = "";
                }
                if (strTemp.Equals(""))
                    return new SocketMessage();
                intLastPing = 0;
                foreach(char c in strTemp)
                {
                    switch (sr)
                    {
                        case SocketReadState.WaitingForStart:
                            if (c == 2)
                            {
                                sr = SocketReadState.Reading;
                                continue;
                            }
                            else if (c == 1)
                            {
                                return new SocketMessage();
                            }
                            else if (c == 6)
                            {
                                Close("Remote Host requested close.", false);
                                return new SocketMessage();
                            }
                            break;
                        case SocketReadState.Reading:
                            if (c != 5)
                                strBuff += c;
                            else
                            {
                                sr = SocketReadState.Ended;
                                continue;
                            }
                            break;
                    }
                }
                if (sr == SocketReadState.Ended)
                {
                    sr = SocketReadState.inHeader;
                    SocketMessage sm = new SocketMessage();
                    strTemp = "";
                    foreach (char c in strBuff)
                    {
                        if (sr == SocketReadState.inHeader)
                        {
                            if (c != 3)
                            {
                                strTemp += c;
                                continue;
                            }
                            else
                            {
                                sm.Header = strTemp;
                                strTemp = "";
                                sr = SocketReadState.inArgument;
                                continue;
                            }
                        }
                        else if (sr == SocketReadState.inArgument)
                        {
                            if (c != 4)
                            {
                                strTemp += c;
                                continue;
                            }
                            else
                            {
                                sm.Arguments.Add(strTemp);
                                strTemp = "";
                                continue;
                            }
                        }
                    }
                    if (!strTemp.Trim().Equals(""))
                    {
                        if (sr == SocketReadState.inHeader)
                            sm.Header = strTemp;
                        else if (sr == SocketReadState.inArgument)
                            sm.Arguments.Add(strTemp);  

                    }
                    return sm;
                }
            }
            return new SocketMessage();
        }

        /// <summary>
        /// Sends a message to the remote socket. 
        /// </summary>
        /// <param name="sm">SocketMessage to be sent.</param>
        /// <returns>true on success, false on error. Note: Success just means that the message has been sent, it doesn't verifiy it was recieved.</returns>
        public Boolean writeMessage(SocketMessage sm)
        {
            try
            {
                sock.Send(Encoding.ASCII.GetBytes(sm.getMessage()));
                return true;
            }
            catch (SocketException se)
            {
                handleError(se.SocketErrorCode + " : " + se.Message);
            }
            catch (Exception ioe)
            {
                handleError(ioe.Message);
            }
            return false;
        }

        /// <summary>
        /// Converts a string host, such as "www.google.com" or "localhost" to an IPEndPoint.
        /// </summary>
        /// <param name="hostName">Host name, such as "www.google.com" or "localhost"</param>
        /// <param name="port">Port of the server.</param>
        /// <returns>System.Net.IPEndPoint</returns>
        public IPEndPoint HostToEndpoint(String hostName, Int32 port)
        {
            IPHostEntry host = Dns.GetHostEntry(hostName);

            // Addres of the host.
            IPAddress[] addressList = host.AddressList;

            // Instantiates the endpoint and socket.
            return new IPEndPoint(addressList[addressList.Length - 1], port);
        }

        /// <summary>
        /// Called when there is an error in the SocketClient class.
        /// </summary>
        /// <param name="error">String representation of the error.</param>
        public abstract void handleError(String error);
        /// <summary>
        /// Called when the server sends data that isn't intercepted by the Socket Client class.
        /// </summary>
        /// <param name="input">Data sent from the server as a String</param>
        public abstract void handleInput(SocketMessage input);
        /// <summary>
        /// Called when the client connects to the server
        /// </summary>
        /// <param name="host">Host name of the server</param>
        /// <param name="port">Port of the server.</param>
        public abstract void handleConnect(String host, int port);
        /// <summary>
        /// Called when the connection to the server is closed for any reason.
        /// </summary>
        /// <param name="reason">String from eather the Close() method or from the server explaining why the connection was dropped.</param>
        /// <param name="host">Host name of the server</param>
        /// <param name="port">Port of the server.</param>
        public abstract void handleDisconnect(String reason, String host, int port);
    }
}
