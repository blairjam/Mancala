using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace Client
{
    public partial class ClientWindow : Window
    {
        private const int PLAYER_CUP_0   = 0;
        private const int PLAYER_CUP_1   = 1;
        private const int PLAYER_CUP_2   = 2;
        private const int PLAYER_CUP_3   = 3;
        private const int PLAYER_CUP_4   = 4;
        private const int PLAYER_CUP_5   = 5;
        private const int PLAYER_GOAL    = 6;
        private const int OPPONENT_CUP_0 = 7;
        private const int OPPONENT_CUP_1 = 8;
        private const int OPPONENT_CUP_2 = 9;
        private const int OPPONENT_CUP_3 = 10;
        private const int OPPONENT_CUP_4 = 11;
        private const int OPPONENT_CUP_5 = 12;
        private const int OPPONENT_GOAL  = 13;

        private List<Cup> cups = new List<Cup>();

        private const int SERVER_PORT = 5665;
        private const string SERVER_ADDR = "127.0.0.1";

        private MancalaClient client;

        public ClientWindow()
        {
            InitializeComponent();

            InitializeCups();

            client = new MancalaClient();
        }

        private void InitializeCups()
        {
            cups.Add(new Cup(PlayerCup0_Image, PlayerCup0_Label));
            cups.Add(new Cup(PlayerCup1_Image, PlayerCup1_Label));
            cups.Add(new Cup(PlayerCup2_Image, PlayerCup2_Label));
            cups.Add(new Cup(PlayerCup3_Image, PlayerCup3_Label));
            cups.Add(new Cup(PlayerCup4_Image, PlayerCup4_Label));
            cups.Add(new Cup(PlayerCup5_Image, PlayerCup5_Label));
            cups.Add(new Cup(PlayerGoal_Image, PlayerGoal_Label));
            cups.Add(new Cup(OpponentCup0_Image, OpponentCup0_Label));
            cups.Add(new Cup(OpponentCup1_Image, OpponentCup1_Label));
            cups.Add(new Cup(OpponentCup2_Image, OpponentCup2_Label));
            cups.Add(new Cup(OpponentCup3_Image, OpponentCup3_Label));
            cups.Add(new Cup(OpponentCup4_Image, OpponentCup4_Label));
            cups.Add(new Cup(OpponentCup5_Image, OpponentCup5_Label));
            cups.Add(new Cup(OpponentGoal_Image, OpponentGoal_Label));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ClientMessage_Label.Content = "Connecting to server...";
            client.Connect();
        }

        private void FillAllCups(int gems)
        {
            for (int i = 0; i < cups.Count; i++)
            {
                if (i == PLAYER_GOAL || i == OPPONENT_GOAL)
                    continue;

                cups[i].Gems = gems;
            }
        }

        private void PlayerCupClick(int cupLoc)
        {
            var clickedCup = cups[cupLoc];
            var availableGems = clickedCup.Gems;
            clickedCup.Gems = 0;

            for (int i = 0, nextCup = cupLoc + 1; i < availableGems; i++, nextCup = (nextCup + 1) % cups.Count)
            {
                Application.Current.Dispatcher.Invoke(delegate 
                {
                    cups[nextCup].Gems++;
                });
            }
        }

        #region ButtonClickListeners
        private void PlCup0_Button_Click(object sender, RoutedEventArgs e)
        {
            PlayerCupClick(PLAYER_CUP_0);
        }

        private void PlCup1_Button_Click(object sender, RoutedEventArgs e)
        {
            PlayerCupClick(PLAYER_CUP_1);
        }

        private void PlCup2_Button_Click(object sender, RoutedEventArgs e)
        {
            PlayerCupClick(PLAYER_CUP_2);
        }

        private void PlCup3_Button_Click(object sender, RoutedEventArgs e)
        {
            PlayerCupClick(PLAYER_CUP_3);
        }

        private void PlCup4_Button_Click(object sender, RoutedEventArgs e)
        {
            PlayerCupClick(PLAYER_CUP_4);
        }

        private void PlCup5_Button_Click(object sender, RoutedEventArgs e)
        {
            PlayerCupClick(PLAYER_CUP_5);
        }
        #endregion

        
    }
}
