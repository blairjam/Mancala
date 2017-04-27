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
        private TcpClient connection;
        private int clientId;

        private Task listenerTask;
        private Task senderTask;

        private volatile byte[] sendBuffer;

        private volatile bool isListening = false;
        private volatile bool isSending = false;

        public ConnectedClient(TcpClient connection, byte clientId)
        {
            this.connection = connection;
            this.clientId = clientId;

            StartSenderTask();
            sendBuffer = new byte[]{ MancalaProtocol.CONNECTED, MancalaProtocol.EXPECT_ID, clientId };
        }

        public async void Close()
        {
            isListening = false;
            isSending = false;

            await listenerTask;
            await senderTask;

            connection.Close();
        }

        public void DataRecieved()
        {

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
                while (isListening)
                {
                    byte[] buf = new byte[1024];
                    var stream = connection.GetStream();

                    //stream.R
                }
            });
        }

        private void StartSenderTask()
        {
            isSending = true;

            senderTask = Task.Run(() =>
            {
                var stream = connection.GetStream();
                
                while (isSending)
                {
                    if (sendBuffer == null)
                    {
                        Thread.Sleep(250);
                        Console.WriteLine("Server Send null buffer.");
                        continue;
                    }

                    Console.WriteLine("Server send buffer data.");

                    stream.Write(sendBuffer, 0, sendBuffer.Length);

                    sendBuffer = null;
                }
            });
        }
    }
}
