namespace Rounds2.Config
{
    public static class NetworkTuning
    {
        public const ushort TickRate = 60;
        public const float FixedDeltaTime = 1f / TickRate;
        public const int TransformSendIntervalTicks = 1;
        public const int TransformInterpolationTicks = 1;
        public const int TransformExtrapolationTicks = 1;
        public const int OwnerInterpolationTicks = 1;
        public const int SpectatorInterpolationTicks = 1;
    }
}
