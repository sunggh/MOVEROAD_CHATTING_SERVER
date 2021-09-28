using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MOVEROAD_CHATTING_SERVER
{

    public partial class SERVER_FORM : Form
    {
        private static List<TcpListener> list = new List<TcpListener>();
        private static TcpListener server = null;
        private static TcpClient clientSocket = null;
        public static Dictionary<int, TcpClient> clientList = new Dictionary<int, TcpClient>(); // 유저id tcp
        public static Dictionary<int, int> room = new Dictionary<int, int>();
        public Dictionary<int, Room> rooms = new Dictionary<int, Room>();
        int Count = 0;
        public SERVER_FORM()
        {
            InitializeComponent();
            Thread t = new Thread(InitSocket);
            t.IsBackground = true;
            t.Start();
        }
        private void InitSocket()
        {

            server = new TcpListener(IPAddress.Any, 80);
            clientSocket = default(TcpClient);
            server.Start();
            DisplayText("연결 완료");
            while (true)
            {
                try
                {
                    clientSocket = server.AcceptTcpClient();
                    NetworkStream stream = clientSocket.GetStream();
                    byte[] buffer = new byte[1024];
                    int bytes = stream.Read(buffer, 0, buffer.Length);
                    string[] str = Encoding.Unicode.GetString(buffer, 0, bytes).Split(new string[] { "|" }, StringSplitOptions.None);
                    string user_name;
                    if (str.Length < 2) continue;
                    DisplayText(Encoding.Unicode.GetString(buffer, 0, bytes));
                    DisplayText(" 접속 완료 - 유저번호 : " + str[1]);
                    Count++;
                    if (clientList.ContainsKey(int.Parse(str[1])))
                    {
                        clientList.Remove(int.Parse(str[1]));
                    }
                    clientList.Add(int.Parse(str[1]), clientSocket);
                    handleClient h_client = new handleClient();
                    h_client.chatHandle += new handleClient.chatHandler(chatHandle);
                    h_client.OnDisconnected += new handleClient.DisconnectedHandler(h_client_OnDisconnected);
                    h_client.startClient(clientSocket, clientList);
                }
                catch (SocketException se)
                {
                    DisplayText(se.Message);
                    break;
                }
                catch (Exception ex)
                {
                    DisplayText(ex.Message);
                    DisplayText(ex.StackTrace);
                    DisplayText(ex.ToString());
                    break;
                }
            }

            clientSocket.Close();
            server.Stop();
        }
        private static TcpClient getTcpClient(int userid)
        {
            return clientList[userid];
        }
        private void DisplayText(string text)
        {
            if (textBox1.InvokeRequired)
            {
                textBox1.BeginInvoke(new MethodInvoker(delegate
                {
                    textBox1.AppendText(text + Environment.NewLine);
                }));
            }
            else
                textBox1.AppendText(text + Environment.NewLine);
        }

        int room_count = 1;
        private void chatHandle(String str)
        {
            DisplayText("recv :" + str);
            string[] split_str = str.Split(new string[] { "|" }, StringSplitOptions.None);
            int opcode = int.Parse(split_str[0]);
            int room_id;
            int user_id;
            int to_id;
            string msg;

            switch (opcode)
            {
                case 1: // 접속 (1|유저아이디)
                    user_id = int.Parse(split_str[1]);
                    msg = "1|" + user_id;
                    SendMessageAll(msg, user_id);
                    msg = "0|" + (Count - 1);
                    if (Count - 1 != 0) msg += "|";
                    int i = 0;
                    foreach (var client in clientList)
                    {
                        if (client.Key == user_id) continue;
                        msg += client.Key;
                        i++;
                        if (i == Count - 1) break;
                        msg += "|";
                    }
                    SendMessage(msg, user_id);
                    break;
                case 2: // 채팅방 생성  
                    user_id = int.Parse(split_str[1]);
                    to_id = int.Parse(split_str[2]);
                    Room room = new Room(room_count, user_id, to_id);
                    foreach (var r in rooms)
                    {
                        if (r.Value.userid == user_id && r.Value.toid == to_id || r.Value.userid == to_id && r.Value.toid == user_id)
                        {
                            msg = "3|" + room.index + "|" + room.toid;
                            SendMessage(msg, room.userid);
                            return;
                        }

                    }
                    // if (rooms.Contains(new Room(rooms.Count - 1, to_id, user_id)))
                    // break; //중복 생성 방지
                    rooms.Add(room_count, room);
                    room_count++;
                    msg = "3|" + room.index + "|" + room.toid;
                    SendMessage(msg, room.userid);
                    msg = "3|" + room.index + "|" + room.userid;
                    SendMessage(msg, room.toid);
                    break;
                case 3: // 메세지 보낼때 (3 |방번호| 상대방_id) 
                    room_id = int.Parse(split_str[1]);
                    to_id = int.Parse(split_str[2]);
                    string txt = split_str[3];
                    int who = rooms[room_id].userid;
                    if (to_id == who) who = rooms[room_id].toid;
                    msg = "4|" + room_id + "|" + who + "|" + txt;
                    SendMessage(msg, to_id);
                    break;
            }
        }
        public void SendMessage(string message, int userid)
        {
            TcpClient client = clientList[userid];
            NetworkStream stream = client.GetStream();
            byte[] buffer = null;
            buffer = Encoding.Unicode.GetBytes(message);
            stream.Write(buffer, 0, buffer.Length);
            stream.Flush();
            DisplayText("send (" + userid + ") : " + message);
        }
        public void SendMessageAll(string message, int userid)
        {
            NetworkStream stream;
            foreach (var client in clientList)
            {
                if (client.Key == userid) continue;
                stream = client.Value.GetStream();
                byte[] buffer = null;
                buffer = Encoding.Unicode.GetBytes(message);
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();
            }
            DisplayText("All send : " + message);
        }
        void h_client_OnDisconnected(TcpClient clientSocket)
        {
            int key = -1;
            foreach (var client in clientList)
            {
                if (client.Value == clientSocket)
                {
                    key = client.Key;
                    break;
                }
            }
            var delRoom = new List<int>();
            foreach (var r in rooms)
            {
                if (r.Value.userid == key || r.Value.toid == key)
                    delRoom.Add(r.Key);
            }
            foreach (var r in delRoom)
            {
                DisplayText("방 삭제 " + r);
                rooms.Remove(r);
            }
            if (clientList.ContainsKey(key))
                clientList.Remove(key);
            else
                return;
            Count--;
            SendMessageAll("2|" + key, key);
        }


    }
}
