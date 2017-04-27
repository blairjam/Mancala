using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class MancalaMove
    {
        public byte? PlayerId { get; private set; }

        public byte? OpponentId { get; private set; }

        public byte? SelectedCup { get; private set; }

        public MancalaMove(byte? playerId, byte? opponentId, byte? selectedCup)
        {
            PlayerId = playerId;
            OpponentId = opponentId;
            SelectedCup = selectedCup;
        }
    }
}
