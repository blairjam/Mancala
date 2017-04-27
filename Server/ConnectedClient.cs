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

        private Task listenerTask;
        private Task senderTask;

        private byte[] sendBuffer;

        private volatile bool isConnected = false;
        private volatile bool isListening = false;
        private volatile bool isSending = false;

        public ConnectedClient(TcpClient connection, byte clientId, Action<string> logWriter, Action<byte> clientDisposer)
        {
            this.connection = connection;
            ClientId = clientId;
            this.logWriter = logWriter;

            isConnected = true;

            StartSenderTask();
            StartListenerTask();

            sendBuffer = new byte[] { MancalaProtocol.CONNECTED, MancalaProtocol.EXPECT_ID, clientId };

            Task.Run(() =>
            {
                while (isConnected)
                {
                    Thread.Sleep(500);
                }

                Close();

                clientDisposer(clientId);
            });
        }

        public void Close()
        {
            sendBuffer = new byte[] { MancalaProtocol.DISCONNECTED };

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
                switch (data[i])
                {
                    case MancalaProtocol.DISCONNECTED:
                    {
                        logWriter("[Client " + ClientId + "]: Disconnecting.");
                        isConnected = false;
                        return;
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
            sendBuffer = data;
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
