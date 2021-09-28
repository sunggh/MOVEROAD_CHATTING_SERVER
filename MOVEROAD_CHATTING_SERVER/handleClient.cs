using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MOVEROAD_CHATTING_SERVER
{
    class handleClient
    {
        TcpClient clientSocket = null;
        public Dictionary<int, TcpClient> clientList = null;
        public List<Room> rooms = new List<Room>();
        public void startClient(TcpClient clientSocket, Dictionary<int, TcpClient> clientList)
        {
            this.clientSocket = clientSocket;
            this.clientList = clientList;

            Thread t_hanlder = new Thread(doChat);
            t_hanlder.IsBackground = true;
            t_hanlder.Start();
        }
        public delegate void DisconnectedHandler(TcpClient clientSocket);
        public event DisconnectedHandler OnDisconnected;
        public delegate void chatHandler(String str);
        public event chatHandler chatHandle;


        private void doChat()
        {
            NetworkStream stream = null;
            try
            {
                byte[] buffer = new byte[1024];
                string msg;
                int bytes = 0;
                int MessageCount = 0;

                while (true)
                {
                    stream = clientSocket.GetStream();
                    bytes = stream.Read(buffer, 0, buffer.Length);
                    msg = Encoding.Unicode.GetString(buffer, 0, bytes);

                    chatHandle(msg);
                }
            }
            catch (SocketException se)
            {

                if (clientSocket != null)
                {
                    if (OnDisconnected != null)
                        OnDisconnected(clientSocket);

                    clientSocket.Close();
                    stream.Close();
                }
            }
            catch (Exception ex)
            {

                if (clientSocket != null)
                {
                    if (OnDisconnected != null)
                        OnDisconnected(clientSocket);

                    clientSocket.Close();
                    stream.Close();
                }
            }

        }

    }
}
