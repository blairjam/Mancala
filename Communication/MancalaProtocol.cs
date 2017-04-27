﻿namespace Communication
{
    public static class MancalaProtocol
    {
        public const byte DISCONNECTED             = 1;
        public const byte CONNECTED                = 2;
        public const byte EXPECT_ID                = 3;
        public const byte WAITING_FOR_NEW_OPPONENT = 4;
        public const byte OPPONENT_FOUND           = 5;
        public const byte EXPECT_MOVE              = 6;
    }
}
