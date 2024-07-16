namespace NuExt.System.Tests
{
    public class AsyncLockTests
    {
        private readonly AsyncLock _asyncLock = new();

        [SetUp]
        public void Setup()
        {
        }

        [OneTimeTearDown]
        public void Dispose()
        {
            _asyncLock.Dispose();
        }

        [Test]
        public void BlockingTest()
        {
            var tasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() => Run()));
            }
            Run();
            Task.WaitAll(tasks.ToArray());
            Assert.Pass();
        }

        [Test]
        public async Task BlockingTestAsync()
        {
            var tasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() => RunAsync()));
            }
            await RunAsync();
            await Task.WhenAll(tasks);
            Assert.Pass();
        }

        private void Run( CancellationToken cancellationToken = default)
        {
            using (_asyncLock.Lock(cancellationToken))
            {
                Thread.Sleep(10);
            }
        }


        private async Task RunAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync(cancellationToken))
            {
                await Task.Delay(10, cancellationToken);
            }
        }
    }
}
