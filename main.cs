using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Windows.Forms;

namespace spacegame_server
{
    public partial class main : Form
    {
        // poinf of game 
        PointF p = new PointF(125, 100);
        PointF p2 = new PointF(377, 100);

        List<string> WaitClients = new List<string>();
        List<Socket> listSoket = new List<Socket>();

        List<string> ClientNames = new List<string>();
        List<step> steps = new List<step>();
        public main()
        {

            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SocketServie();
            button1.Enabled = false;
        }
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static byte[] result = new byte[1024];

        public void SocketServie()
        {
            listBox1.Items.Add(("server active"));

            string host = "127.0.0.1";//ip
            int port = 2000;//port
            socket.Bind(new IPEndPoint(IPAddress.Parse(host), port));
            socket.Listen(100);//max 100  
            Thread myThread = new Thread(ListenClientConnect);
            myThread.Start();


        }

        private void ListenClientConnect()
        {

            while (true)
            {
                WriteTextSafe("waiting for a client...");
                Socket clientSocket = socket.Accept();
                ClientManager cm = new ClientManager(this, clientSocket);
                Thread receiveThread = new Thread(cm.doClient);
                receiveThread.Start();
            }
        }

        //use this method to use control
        public void WriteTextSafe(string text)
        {
            if (listBox1.InvokeRequired)
            {
                // Call this same method but append THREAD2 to the text
                Action safeWrite = delegate { WriteTextSafe($"{text}"); };
                listBox1.Invoke(safeWrite);
            }
            else
                listBox1.Items.Add(text);
        }
        public void ReceiveMessage(object clientSocket)
        {
            string sendStr;
            Socket myClientSocket = (Socket)clientSocket;
            while (true)
            {
                try
                {
                    //use SocketClient for receive the result
                    int receiveNumber = myClientSocket.Receive(result);
                    if (receiveNumber == 0)
                        return;
                    string risultato = Encoding.UTF8.GetString(result, 0, receiveNumber);
                    // Console.WriteLine("il messaggio ricevuto", myClientSocket.RemoteEndPoint.ToString(), Encoding.UTF8.GetString(result, 0, receiveNumber));
                    //return the data of client
                    //check this name is validate
                    sendStr = checkTypeMsg(risultato, myClientSocket);
                    //checke need open to game mode 
                    if (sendStr == "2;true")
                        vsPc(true, myClientSocket);
                    else if (sendStr == "4;true")
                        vsClient(true, myClientSocket);

                    byte[] bs = Encoding.UTF8.GetBytes(sendStr);
                    myClientSocket.Send(bs, bs.Length, 0);  //send the data of client
                }
                catch (Exception ex)
                {
                    myClientSocket.Close();//close clientsoket
                    break;
                }
            }
        }
        private void SentMsg(string sentstr, Socket clientSocket)
        {
            string sendMessage = sentstr;//the strign send for server
            int i = clientSocket.Send(Encoding.UTF8.GetBytes(sendMessage));

        }

        private string checkTypeMsg(string msg, Socket clientSocket)
        {
            if (msg != "" && msg != null)
            {

                string[] str = msg.Split(';');// [0] type  [1] context
                int type = Int32.Parse(str[0]);
                string context = str[1];

                switch (type)
                {
                    // regist name
                    case 0:
                        if (context != "" && context != " " && context != null && CheckExsisteClientNames(context) == false)
                        {
                            registClient(context);
                            WriteTextSafe(context);
                            return buildMsg(0, "true");
                        }
                        break;
                    //vs client
                    case 1:

                        break;
                    //vs computer
                    case 2:
                        //check does ready for start game
                        if (context == "ready")
                            return buildMsg(2, "true");
                        else
                            return buildMsg(2, "false");
                        break;
                    //callback position of client vs pc
                    case 3:
                        int x = Int32.Parse(str[1]);
                        int y = Int32.Parse(str[2]);
                        TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                        step s = new step(Convert.ToInt64(ts.TotalSeconds), p.X, p.Y);
                        steps.Add(s);
                        break;
                    //client vs client
                    case 4:
                        if (listSoket.Contains(clientSocket) != true)
                        {
                            WaitClients.Add(context);
                            listSoket.Add(clientSocket);
                            if (listSoket.Count > 1)
                                return buildMsg(4, "true");
                            else
                                return buildMsg(4, "false");
                        }

                        break;
                    case 5:
                        break;
                    default:
                        break;
                }
            }
            return buildMsg(0, "false");

        }

        private void vsPc(bool ready, object clientSocket)
        {
            //start the game if client is ready
            string sendStr;
            Socket myClientSocket = (Socket)clientSocket;
            byte[] bs;
            Random r;
            int count = 0;
            while (ready == true)
            {
                System.Threading.Thread.Sleep(500);
                count++;
                r = new Random();
                sendStr = buildMsg(2, r.Next(4).ToString());
                if (count == 20)
                    break;

                bs = Encoding.UTF8.GetBytes(sendStr);
                myClientSocket.Send(bs, bs.Length, 0);  //send the data of client
            }
        }

        private void vsClient(bool ready, object clientSocket)
        {
            Socket socket1 = GetSocket(clientSocket);
            Socket socket2 = (Socket)clientSocket;
            if (socket1 == null)
                return;
            SentMsg(buildMsg(4, "ready"), socket1);
            SentMsg(buildMsg(4, "ready"), socket2);
            //start game




        }
        private Socket GetSocket(object clientSocket)
        {

            foreach (Socket item in listSoket)
            {
                if (item != (Socket)clientSocket)
                {
                    listSoket.Remove(item);
                    listSoket.Remove((Socket)clientSocket);
                    SentMsg(buildMsg(4, "true"), item);
                    return item;
                }
            }
            return null;
        }
        private string buildMsg(int type, string msg)
        {
            return type + ";" + msg;
        }

        private void registClient(string name)
        {
            ClientNames.Add(name);
        }
        private bool CheckExsisteClientNames(string name)
        {
            if (ClientNames.Contains(name))
                return true;
            return false;
        }

    }
}
