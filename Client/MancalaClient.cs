using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    public class MancalaClient
    {
        private const string SERVER_ADDR = "127.0.0.1";
        private const int    SERVER_PORT = 5665;

        private TcpClient tcpClient;

        public MancalaClient()
        {
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
        }
    }
}
