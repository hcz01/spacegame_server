using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Windows.Forms;

namespace spacegame_server
{
    public partial class Form1 : Form
    {

        List<string>ClientNames = new List<string>();
        public Form1()
        {
            
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SocketServie();
        }
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static byte[] result = new byte[1024];

        public  void SocketServie()
        {
            listBox1.Items.Add(("server active"));
            string host = "127.0.0.1";//IP地址
            int port = 2000;//端口
            socket.Bind(new IPEndPoint(IPAddress.Parse(host), port));
            socket.Listen(100);//设定最多100个排队连接请求   
            Thread myThread = new Thread(ListenClientConnect);//通过多线程监听客户端连接  
            myThread.Start();
          
        }

     
        private  void ListenClientConnect()
        {
           
            while (true)
            {
                Socket clientSocket = socket.Accept();
              //  clientSocket.Send(Encoding.UTF8.GetBytes("sono il server"));
                Thread receiveThread = new Thread(ReceiveMessage);
                receiveThread.Start(clientSocket);
            }
        }

        //use this method to use control
        public void WriteTextSafe(string text)
        {
            if (listBox1.InvokeRequired)
            {
                // Call this same method but append THREAD2 to the text
                Action safeWrite = delegate { WriteTextSafe($"{text} (THREAD2)"); };
                listBox1.Invoke(safeWrite);
            }
            else
                listBox1.Items.Add(text);
        }
        private  void ReceiveMessage(object clientSocket)
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
                    string risultato=Encoding.UTF8.GetString(result, 0, receiveNumber);
                    // Console.WriteLine("il messaggio ricevuto", myClientSocket.RemoteEndPoint.ToString(), Encoding.UTF8.GetString(result, 0, receiveNumber));
                    //return the data of client
                    //check this name is validate
                    if (risultato!="" && CheckExsisteClientNames(risultato)==false)
                    {
                        sendStr = "true";
                        registClient(risultato);
                        WriteTextSafe(risultato);
                    }
                    else
                        sendStr = "false";
                    byte[] bs = Encoding.UTF8.GetBytes(sendStr);
                    myClientSocket.Send(bs, bs.Length, 0);  //send the data of client

                }
                catch (Exception ex)
                {
                   MessageBox.Show(ex.Message);
                    myClientSocket.Close();//close clientsoket
                    break;
                }
            }
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
