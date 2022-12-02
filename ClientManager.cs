using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
namespace spacegame_server
{
    class ClientManager
    {
        Socket socket;
        main f;
        public ClientManager(main f, Socket socket)
        {
            this.socket = socket;
            this.f = f;
        }
        public void doClient()
        {
            f.ReceiveMessage(socket);
        }

    }
}