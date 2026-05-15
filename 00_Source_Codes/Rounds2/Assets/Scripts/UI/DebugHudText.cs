namespace Rounds2.UI
{
    public static class DebugHudText
    {
        public static string Format(bool serverStarted, bool clientStarted)
        {
            return $"Server: {serverStarted}\nClient: {clientStarted}";
        }
    }
}
