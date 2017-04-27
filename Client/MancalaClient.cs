using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Communication;

namespace Client
{
    public class MancalaClient
    {
        private const string SERVER_ADDR = "127.0.0.1";
        private const int    SERVER_PORT = 5665;

        private Action<string> messageWriter;
        private Action<int> fillCups;
        private Action<int> cupClick;
        private TcpClient tcpClient;

        private byte id;

        private Task listenerTask;
        private Task senderTask;

        private Queue<byte> sendBuffer;

        private volatile bool isConnected = false;
        private volatile bool isListening = false;
        private volatile bool isSending = false;

        private Action<byte> recievedDataSecondaryAction;

        public MancalaClient(Action<string> messageWriter, Action<int> fillCups, Action<int> cupClick)
        {
            this.messageWriter = messageWriter;
            this.fillCups = fillCups;
            this.cupClick = cupClick;
            tcpClient = new TcpClient();
            sendBuffer = new Queue<byte>();
        }

        // Attempts to connect the client to the server.
        public void Connect()
        {
            Task.Run(() =>
            {
                while (!isConnected)
                {
                    try
                    {
                        tcpClient.Connect(SERVER_ADDR, SERVER_PORT);
                        isConnected = true;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(500);
                    }
                }

                StartListenerTask();
                StartSenderTask();
            });
        }

        // Closes the connection to the server.
        public void Disconnect()
        {
            SendData(MancalaProtocol.DISCONNECTED);

            // Stop the listening thread.
            isListening = false;
            if (listenerTask != null)
                listenerTask.Wait();

            // Stop the sending thread.
            isSending = false;
            if (senderTask != null)
                senderTask.Wait();

            tcpClient.Close();

            isConnected = false;
        }

        public void SendData(params byte[] data)
        {
            foreach (var dat in data)
            {
                sendBuffer.Enqueue(dat);
            }
        }

        public void CupClicked(byte cupLoc)
        {
            Console.WriteLine("Cup clicked: " + cupLoc);
            SendData(MancalaProtocol.EXPECT_MOVE, cupLoc);
        }

        // Handle data recieved from the server.
        private void DataRecieved(byte[] data)
        {
            // Loop over data read from the stream.
            for (int i = 0; i < data.Length; i++)
            {
                // Complete a secondary action if one is available.
                // Executes the action and sets it to null because the action is completed.
                if (recievedDataSecondaryAction != null)
                {
                    recievedDataSecondaryAction(data[i]);
                    recievedDataSecondaryAction = null;
                    continue;
                }

                // Check the byte against known protocol values.
                switch (data[i])
                {
                    case MancalaProtocol.DISCONNECTED:
                    {
                        messageWriter("Disconnected from server.");
                        isConnected = false;
                        return;
                    }
                    case MancalaProtocol.CONNECTED:
                    {
                        messageWriter("Connected to server.");
                        break;
                    }
                    case MancalaProtocol.EXPECT_ID:
                    {
                        // Create a secondary action to be completed on the next byte available.
                        recievedDataSecondaryAction = x =>
                        {
                            id = x;
                            messageWriter("ID: " + id);
                        };
                        break;
                    }
                    case MancalaProtocol.WAITING_FOR_NEW_OPPONENT:
                    {
                        messageWriter("Waiting for new opponent!");
                        break;
                    }
                    case MancalaProtocol.OPPONENT_FOUND:
                    {
                        messageWriter("Opponent found. Game beginning soon!");
                        break;
                    }
                    case MancalaProtocol.START_GAME:
                    {
                        fillCups(4);
                        break;
                    }
                    case MancalaProtocol.PLAYER_TURN:
                    {
                        messageWriter("Your turn!");
                        break;
                    }
                    case MancalaProtocol.OPPONENT_TURN:
                    {
                        messageWriter("Opponent turn!");
                        break;
                    }
                    case MancalaProtocol.EXPECT_MOVE:
                    {
                        recievedDataSecondaryAction = x =>
                        {
                            cupClick(x);
                        };
                        break;
                    }
                    default:
                    {
                        break;
                    }
                }
            }
        }

        // Creates a separate thread that listens for data sent from the server.
        private void StartListenerTask()
        {
            isListening = true;

            // Create and run new task.
            listenerTask = Task.Run(() =>
            {
                // Get reference to the data stream.
                var stream = tcpClient.GetStream();

                // Read data from the stream and process it.
                while (isConnected && isListening)
                {
                    if (!stream.DataAvailable)
                    {
                        Thread.Sleep(250);
                        continue;
                    }

                    var buf = new byte[256];
                    var res = stream.Read(buf, 0, 256);

                    DataRecieved(buf);
                }
            });
        }

        private void StartSenderTask()
        {
            isSending = true;

            senderTask = Task.Run(() =>
            {
                var stream = tcpClient.GetStream();

                while (isConnected && isSending)
                {
                    if (sendBuffer.Count <= 0)
                    {
                        Thread.Sleep(250);
                        continue;
                    }

                    var buf = sendBuffer.ToArray();
                    sendBuffer.Clear();

                    stream.Write(buf, 0, buf.Length);
                }
            });
        }
    }
}
