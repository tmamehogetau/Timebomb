using NUnit.Framework;
using Rounds2.Networking;

namespace Rounds2.Tests.EditMode
{
    public sealed class BootstrapLaunchOptionsTests
    {
        [Test]
        public void FromArgsDetectsServerBeforeClient()
        {
            BootstrapLaunchMode mode = BootstrapLaunchOptions.FromArgs(new[] { "-client", "-server" });

            Assert.AreEqual(BootstrapLaunchMode.Server, mode);
        }

        [Test]
        public void FromArgsDetectsClient()
        {
            BootstrapLaunchMode mode = BootstrapLaunchOptions.FromArgs(new[] { "-client" });

            Assert.AreEqual(BootstrapLaunchMode.Client, mode);
        }

        [Test]
        public void FromArgsReturnsNoneWithoutLaunchFlag()
        {
            BootstrapLaunchMode mode = BootstrapLaunchOptions.FromArgs(new[] { "-batchmode" });

            Assert.AreEqual(BootstrapLaunchMode.None, mode);
        }
    }
}
