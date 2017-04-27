using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Server
{
    public partial class ServerWindow : Window
    {
        private MancalaServer server;

        delegate void LogUpdater(string msg);

        public ServerWindow()
        {
            InitializeComponent();

            server = new MancalaServer(x => AppendLineToLog(x));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            server.AcceptConnections();
        }

        private void AppendLineToLog(string message)
        {
            LogUpdater updater = x => { ServerLog_TextBox.AppendText(x + Environment.NewLine); };
            ServerLog_TextBox.Dispatcher.BeginInvoke(updater, message);
        }
    }
}
