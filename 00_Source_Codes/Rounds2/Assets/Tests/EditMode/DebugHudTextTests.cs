using NUnit.Framework;
using Rounds2.UI;

namespace Rounds2.Tests.EditMode
{
    public sealed class DebugHudTextTests
    {
        [Test]
        public void FormatShowsServerAndClientState()
        {
            string text = DebugHudText.Format(serverStarted: true, clientStarted: false);

            Assert.AreEqual("Server: True\nClient: False", text);
        }
    }
}
