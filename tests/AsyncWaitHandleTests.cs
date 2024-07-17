namespace NuExt.System.Tests
{
    internal class AsyncWaitHandleTests
    {
        [Test]
        public async Task WaitOneTestAsync()
        {
            using var manualResetEvent = new ManualResetEvent(false);
            using var asyncWaitHandle = new AsyncWaitHandle(manualResetEvent);

            _= Task.Run(async () =>
            {
                await Task.Delay(1000); // Simulate long-running operation
                manualResetEvent.Set(); // Signal the event upon completion
            });

            bool result = await asyncWaitHandle.WaitOneAsync(TimeSpan.FromSeconds(10));
            Assert.That(result, Is.True);

            Assert.Pass();
        }

        [Test]
        public async Task TimeoutTestAsync()
        {
            using var manualResetEvent = new ManualResetEvent(false);
            using var asyncWaitHandle = new AsyncWaitHandle(manualResetEvent);

            bool result = await asyncWaitHandle.WaitOneAsync(1000);
            Assert.That(result, Is.False);

            Assert.Pass();
        }

        [Test]
        public async Task CancellationTestAsync()
        {
            using var manualResetEvent = new ManualResetEvent(false);
            using var asyncWaitHandle = new AsyncWaitHandle(manualResetEvent);
            using var cts = new CancellationTokenSource();

            _ = Task.Run(async () =>
            {
                await Task.Delay(1000); // Wait for 1 second
                cts.Cancel(); // Cancel the token
            });

            try
            {
                bool result = await asyncWaitHandle.WaitOneAsync(TimeSpan.FromSeconds(10), cts.Token);
                Assert.Fail("The operation was expected to be cancelled.");
            }
            catch (OperationCanceledException)
            {
                // The operation was cancelled as expected
                Assert.Pass("The operation was cancelled as expected.");
            }
        }
    }
}
