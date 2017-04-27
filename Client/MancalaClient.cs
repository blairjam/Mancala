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
        private TcpClient tcpClient;

        private byte id;

        private Task listenerTask;
        private Task senderTask;

        private volatile byte[] sendBuffer;

        private volatile bool isConnected = false;
        private volatile bool isListening = false;
        private volatile bool isSending = false;

        private Action<byte> recievedSecondaryAction;

        public MancalaClient(Action<string> messageWriter)
        {
            this.messageWriter = messageWriter;
            tcpClient = new TcpClient();
        }

        // Attempts to connect the client to the server.
        public async void Connect()
        {
            // Continuously check for server connection until it is found.
            while (!isConnected)
            {
                try
                {
                    // Connect on separate thread.
                    await tcpClient.ConnectAsync(SERVER_ADDR, SERVER_PORT);
                    isConnected = true;
                }
                catch (Exception)
                {
                    // Wait for a separate thread to sleep and return. 
                    // This prevents the UI thread from being blocked, but inserts time between conenction attempts.
                    await Task.Run(() => Thread.Sleep(1000));
                }
            }

            // Start listening to the server once there is a connection.
            StartListenerTask();
            StartSenderTask();
        }

        // Closes the connection to the server.
        public void Disconnect()
        {
            sendBuffer = new byte[] { MancalaProtocol.DISCONNECTED };

            // Stop the listening thread.
            isListening = false;
            listenerTask.Wait();

            // Stop the sending thread.
            isSending = false;
            senderTask.Wait();

            tcpClient.Close();

            isConnected = false;
        }

        // Handle data recieved from the server.
        private void DataRecieved(byte[] data)
        {
            // Loop over data read from the stream.
            for (int i = 0; i < data.Length; i++)
            {
                // Complete a secondary action if one is available.
                // Executes the action and sets it to null because the action is completed.
                if (recievedSecondaryAction != null)
                {
                    recievedSecondaryAction(data[i]);
                    recievedSecondaryAction = null;
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
                        recievedSecondaryAction = x =>
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
                    if (sendBuffer == null)
                    {
                        Thread.Sleep(250);
                        continue;
                    }

                    stream.Write(sendBuffer, 0, sendBuffer.Length);
                    sendBuffer = null;
                }
            });
        }
    }
}
