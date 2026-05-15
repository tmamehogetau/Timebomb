namespace Rounds2.Networking
{
    public enum BootstrapLaunchMode
    {
        None,
        Server,
        Client
    }

    public static class BootstrapLaunchOptions
    {
        public static BootstrapLaunchMode FromArgs(string[] args)
        {
            bool clientRequested = false;

            foreach (string arg in args)
            {
                if (arg == "-server")
                {
                    return BootstrapLaunchMode.Server;
                }

                if (arg == "-client")
                {
                    clientRequested = true;
                }
            }

            return clientRequested ? BootstrapLaunchMode.Client : BootstrapLaunchMode.None;
        }
    }
}
