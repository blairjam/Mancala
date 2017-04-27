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
        private Task gameCommunicationTask;

        private volatile Queue<MancalaMove> gameMoves;

        private volatile bool isAcceptingConnections = false;
        private volatile bool isGameRunning = false;

        private byte clientId = 0;

        public MancalaServer(Action<string> logWriter)
        {
            this.logWriter = logWriter;
            tcpListener = new TcpListener(IPAddress.Loopback, PORT);
            connectedClients = new List<ConnectedClient>();
            gameMoves = new Queue<MancalaMove>();
        }

        public void AcceptConnections()
        {
            tcpListener.Start();

            StartGameCommunicationTask();

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
                    connectedClients.Add(new ConnectedClient(connection, clientId, logWriter, AddGameMove, DisposeOfClient));
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

            isGameRunning = false;
            gameCommunicationTask.Wait();

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

                    nextPlayer.SendData(MancalaProtocol.OPPONENT_FOUND, MancalaProtocol.START_GAME);
                    newestPlayer.SendData(MancalaProtocol.OPPONENT_FOUND, MancalaProtocol.START_GAME);

                    var rand = new Random();
                    if (rand.Next() % 2 == 0)
                    {
                        nextPlayer.SendData(MancalaProtocol.PLAYER_TURN);
                        newestPlayer.SendData(MancalaProtocol.OPPONENT_TURN);
                    }
                    else
                    {
                        nextPlayer.SendData(MancalaProtocol.OPPONENT_TURN);
                        newestPlayer.SendData(MancalaProtocol.PLAYER_TURN);
                    }

                    return;
                }
            }

            newestPlayer.SendData(MancalaProtocol.WAITING_FOR_NEW_OPPONENT);
        }

        private void StartGameCommunicationTask()
        {
            isGameRunning = true;

            gameCommunicationTask = Task.Run(() =>
            {
                while (isGameRunning)
                {
                    if (gameMoves.Count == 0)
                    {
                        Thread.Sleep(250);
                        continue;
                    }

                    var nextMove = gameMoves.Dequeue();
                    var player = connectedClients.FirstOrDefault((x) => x.ClientId == nextMove.PlayerId);
                    var opponent = connectedClients.FirstOrDefault((x) => x.ClientId == nextMove.OpponentId);

                    if (player == null || opponent == null)
                    {
                        continue;
                    }

                    player.SendData(MancalaProtocol.EXPECT_MOVE, nextMove.SelectedCup ?? 0, MancalaProtocol.OPPONENT_TURN);
                    opponent.SendData(MancalaProtocol.EXPECT_MOVE, (byte)((nextMove.SelectedCup ?? 0) + 7), MancalaProtocol.PLAYER_TURN);
                }
            });
        }

        private void AddGameMove(MancalaMove move)
        {
            gameMoves.Enqueue(move);
        }

        private void DisposeOfClient(byte id)
        {
            connectedClients.RemoveAll((x) => x.ClientId == id);
        }
    }
}
