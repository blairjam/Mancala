using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class ConnectedClient
    {
        private TcpClient connection;
        private int clientId;

        public ConnectedClient(TcpClient connection, int clientId)
        {
            this.connection = connection;
            this.clientId = clientId;
        }

        public void Close()
        {
            connection.Close();
        }
    }
}
