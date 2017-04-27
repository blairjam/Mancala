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

        private volatile bool isListening = false;

        private Action<byte> nextRecievedAction;

        public MancalaClient(Action<string> messageWriter)
        {
            this.messageWriter = messageWriter;
            tcpClient = new TcpClient();
        }

        public async void Connect()
        {
            bool connected = false;

            while (!connected)
            {
                try
                {
                    await tcpClient.ConnectAsync(SERVER_ADDR, SERVER_PORT);
                    connected = true;
                }
                catch (Exception)
                {
                    await Task.Run(() => Thread.Sleep(1000));
                }
            }

            StartListenerTask();
        }

        private void DataRecieved(byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (nextRecievedAction != null)
                {
                    nextRecievedAction(data[i]);
                    nextRecievedAction = null;
                    continue;
                }

                switch (data[i])
                {
                    case MancalaProtocol.CONNECTED:
                    {
                        messageWriter("Connected to server.");
                        break;
                    }
                    case MancalaProtocol.EXPECT_ID:
                    {
                        nextRecievedAction = x =>
                        {
                            id = x;
                            messageWriter("ID: " + id);
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

        private void StartListenerTask()
        {
            isListening = true;

            listenerTask = Task.Run(() =>
            {
                var stream = tcpClient.GetStream();

                while (isListening)
                {
                    var buf = new byte[256];
                    stream.Read(buf, 0, 256);

                    DataRecieved(buf);
                }
            });
        }
    }
}
