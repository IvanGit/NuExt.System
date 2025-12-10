using System.Diagnostics;

namespace NuExt.System.Tests
{
    internal class AggregateAsyncDisposableTests
    {
        private class Disp: IDisposable, IAsyncDisposable
        {
            public int Count;

            public Disp() { } 
            public void Dispose() 
            {
                Trace.WriteLine("Dispose()");
            }
            public ValueTask DisposeAsync()
            {
                Count++;
                Trace.WriteLine("DisposeAsync()");
                return default;
            }
        }

        [Test]
        public async Task AsyncSyncDisposableTest()
        {
            var disp = new Disp();
            var a = new AggregateAsyncDisposable();
            a.Add(disp);
            IDisposable d = disp;
            a.Add(d);
            await a.DisposeAsync();

            Assert.That(disp.Count, Is.EqualTo(2));
        }
    }
}
