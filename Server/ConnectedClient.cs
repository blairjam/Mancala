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

        private Task listenerTask;
        private Task senderTask;

        private volatile bool isListening = false;
        private volatile bool isSending = false;

        public ConnectedClient(TcpClient connection, int clientId)
        {
            this.connection = connection;
            this.clientId = clientId;
        }

        public void Close()
        {
            connection.Close();
        }

        public void DataRecieved()
        {

        }

        private void StartListeningTask()
        {
            listenerTask = Task.Run(() =>
            {
                while (isListening)
                {
                    var stream = connection.GetStream();
                }
            });
        }
    }
}
