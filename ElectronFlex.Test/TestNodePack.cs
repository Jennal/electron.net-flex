using NUnit.Framework;

namespace ElectronFlex.Test
{
    public class TestNodePack
    {
        [SetUp]
        public void Setup()
        {
            Config.CommandLineOptions = new CommandLineOptions
            {
                StartFromElectron = true
            };
        }

        [Test]
        public void TestCallback()
        {
            
        }
    }
}