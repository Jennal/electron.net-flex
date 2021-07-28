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
            var invokeManager = new InvokeTaskManager();
            var task = invokeManager.Invoke<int>(new Pack(){Id = 1});
            invokeManager.Result(new Pack
            {
                Id = 1,
                Type = PackType.InvokeResult,
                Content = "100"
            });
            task.Wait();
            Assert.AreEqual(100, task.Result);
        }
    }
}