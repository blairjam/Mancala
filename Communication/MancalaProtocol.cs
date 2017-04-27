namespace Communication
{
    public static class MancalaProtocol
    {
        public const byte DISCONNECTED = 0;
        public const byte CONNECTED    = 1;
        public const byte EXPECT_ID    = 2;
        public const byte EXPECT_MOVE  = 3;
    }
}
