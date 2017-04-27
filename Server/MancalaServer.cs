using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Communication;

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

        private byte clientId = 0;

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
                    if (!tcpListener.Pending())
                    {
                        Thread.Sleep(250);
                        continue;
                    }

                    var connection = tcpListener.AcceptTcpClient();
                    connectedClients.Add(new ConnectedClient(connection, clientId, logWriter, DisposeOfClient));
                    logWriter("[Client " + clientId + "]: Connected.");
                    clientId++;

                    AssignOpponents();
                }
            });
        }

        public void CloseConnections()
        {
            isAcceptingConnections = false;
            connectionListenerTask.Wait();

            tcpListener.Stop();

            foreach (var client in connectedClients)
            {
                client.Close();
            }
        }

        private void AssignOpponents()
        {
            var newestPlayer = connectedClients[connectedClients.Count - 1];

            for (int i = 0; i < connectedClients.Count - 1; i++)
            {
                var nextPlayer = connectedClients[i];

                if (nextPlayer.OpponentId == null)
                {
                    nextPlayer.OpponentId = newestPlayer.ClientId;
                    newestPlayer.OpponentId = nextPlayer.ClientId;

                    nextPlayer.SendData(MancalaProtocol.OPPONENT_FOUND);
                    newestPlayer.SendData(MancalaProtocol.OPPONENT_FOUND);

                    return;
                }
            }

            newestPlayer.SendData(MancalaProtocol.WAITING_FOR_NEW_OPPONENT);
        }

        private void DisposeOfClient(byte id)
        {
            connectedClients.RemoveAll((x) => x.ClientId == id);
        }
    }
}
