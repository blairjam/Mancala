using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class MancalaServer
    {
        private const int PORT = 5665;

        private Action<string> logWriter;

        private TcpListener tcpListener;
        private List<ConnectedClient> connectedClients;

        private Task connectionListenerTask;
        private volatile bool isAcceptingConnections;

        private int clientId = 0;

        public MancalaServer(Action<string> logWriter)
        {
            this.logWriter = logWriter;
            tcpListener = new TcpListener(IPAddress.Loopback, PORT);
            connectedClients = new List<ConnectedClient>();
        }

        public void AcceptConnections()
        {
            tcpListener.Start();

            isAcceptingConnections = true;
            connectionListenerTask = Task.Run(() =>
            {
                while (isAcceptingConnections)
                {
                    var connection = tcpListener.AcceptTcpClient();
                    connectedClients.Add(new ConnectedClient(connection, clientId));
                    logWriter("Client " + clientId + " connected.");
                    clientId++;
                }
            });
        }

        public async void CloseConnections()
        {
            isAcceptingConnections = false;
            await connectionListenerTask;

            tcpListener.Stop();

            foreach (var client in connectedClients)
            {
                client.Close();
            }
        }
    }
}
