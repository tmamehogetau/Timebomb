namespace Rounds2.Match
{
    public static class RoundCombatGate
    {
        public static bool IsOpen { get; private set; } = true;

        public static void Open()
        {
            IsOpen = true;
        }

        public static void Close()
        {
            IsOpen = false;
        }
    }
}
