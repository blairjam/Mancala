using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Communication;

namespace Server
{
    public class ConnectedClient
    {
        public byte ClientId { get; private set; }
        public byte? OpponentId { get; set; }

        private TcpClient connection;
        private Action<string> logWriter;
        private Action<MancalaMove> gameMoveUpdater;
        private Action<byte> clientDisposer;

        private Task listenerTask;
        private Task senderTask;

        private Queue<byte> sendBuffer;

        private volatile bool isConnected = false;
        private volatile bool isListening = false;
        private volatile bool isSending = false;

        private Action<byte> recievedDataSecondaryAction;

        public ConnectedClient(TcpClient connection, byte clientId, Action<string> logWriter, Action<MancalaMove> gameMoveUpdater, Action<byte> clientDisposer)
        {
            this.connection = connection;
            ClientId = clientId;
            this.logWriter = logWriter;
            this.gameMoveUpdater = gameMoveUpdater;
            this.clientDisposer = clientDisposer;

            sendBuffer = new Queue<byte>();

            isConnected = true;

            StartSenderTask();
            StartListenerTask();

            SendData(MancalaProtocol.CONNECTED, MancalaProtocol.EXPECT_ID, clientId);
        }

        public void Close()
        {
            SendData(MancalaProtocol.DISCONNECTED);

            isListening = false;
            listenerTask.Wait();

            isSending = false;
            senderTask.Wait();

            connection.Close();

            isConnected = false;
        }

        public void DataRecieved(byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (recievedDataSecondaryAction != null)
                {
                    recievedDataSecondaryAction(data[i]);
                    recievedDataSecondaryAction = null;
                    continue;
                }

                switch (data[i])
                {
                    case MancalaProtocol.DISCONNECTED:
                    {
                        logWriter("[Client " + ClientId + "]: Disconnecting.");
                        Task.Run(() =>
                        {
                            Close();
                            clientDisposer(ClientId);
                        });
                        return;
                    }
                    case MancalaProtocol.EXPECT_MOVE:
                    {
                        recievedDataSecondaryAction = x =>
                        {
                            logWriter("[Client " + ClientId + "]: Choosing cup #" + x + ". Sending to Client " + OpponentId + ".");
                            gameMoveUpdater(new MancalaMove(ClientId, OpponentId, x));
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

        public void SendData(params byte[] data)
        {
            foreach (var dat in data)
            {
                sendBuffer.Enqueue(dat);
            }
        }

        private void StartListenerTask()
        {
            isListening = true;
            listenerTask = Task.Run(() =>
            {
                var stream = connection.GetStream();

                while (isConnected && isListening)
                {
                    if (!stream.DataAvailable)
                    {
                        Thread.Sleep(250);
                        continue;
                    }

                    byte[] buf = new byte[256];
                    stream.Read(buf, 0, 256);

                    DataRecieved(buf);
                }
            });
        }

        private void StartSenderTask()
        {
            isSending = true;

            senderTask = Task.Run(() =>
            {
                var stream = connection.GetStream();

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
