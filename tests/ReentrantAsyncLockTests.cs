#define DEBUG_WRITELINE_
using System.ComponentModel;

using ReentrantAsyncLockAlias = System.Threading.ReentrantAsyncLock;

namespace NuExt.System.Tests
{
    public class ReentrantAsyncLockTests
    {
        private ReentrantAsyncLockAlias _asyncLock = new();

        private void DoRecursion(int depth = 3, CancellationToken cancellationToken = default)
        {
#if DEBUG_WRITELINE
            Debug.WriteLine($"Executing DoRecursion depth={depth}");
#endif
            _asyncLock.Acquire(() =>
            {
                Assert.That(_asyncLock.IsEntered, Is.True);
                if (depth > 0)
                {
                    DoRecursion(depth - 1, cancellationToken);
                    _asyncLock.Acquire(() =>
                    {
                        Assert.That(_asyncLock.IsEntered, Is.True);
                    }, cancellationToken);
                }
                Thread.Sleep(10);
            }, cancellationToken);
#if DEBUG_WRITELINE
            Debug.WriteLine($"Executed DoRecursion depth={depth}");
#endif
        }


        private async Task DoRecursionAsync(int depth = 3, CancellationToken cancellationToken = default)
        {
#if DEBUG_WRITELINE
            Debug.WriteLine($"Executing DoRecursionAsync depth={depth}");
#endif
            await _asyncLock.AcquireAsync(async () =>
            {
                Assert.That(_asyncLock.IsEntered, Is.True);
                if (depth > 0)
                {
                    await DoRecursionAsync(depth - 1, cancellationToken).ConfigureAwait(false);
                }
                await Task.Delay(10, cancellationToken);
            }, cancellationToken).ConfigureAwait(false);
#if DEBUG_WRITELINE
            Debug.WriteLine($"Executed DoRecursionAsync depth={depth}");
#endif
        }

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
        public async Task AsyncConcurrentTest()
        {
            using var asyncLock = new ReentrantAsyncLockAlias();
            using var lifeTime = new LifeTime();
            lifeTime.AddBracket(() => asyncLock.PropertyChanged += OnPropertyChanged, () => asyncLock.PropertyChanged -= OnPropertyChanged);

            var raceConditionActualValue = 0;
            var raceConditionExpectedValue = 0;
            var raceConditionCurrentCount = 0;

            await asyncLock.AcquireAsync(async () =>
            {
                raceConditionCurrentCount++;
                Assert.That(raceConditionCurrentCount, Is.EqualTo(asyncLock.CurrentCount));
                Assert.That(asyncLock.CurrentCount, Is.EqualTo(1));
                Assert.That(asyncLock.SyncRoot, Is.Not.Null);
                for (var i = 0; i < 100; ++i)
                {
                    await Task.WhenAll(
                        GenerateTask(),
                        GenerateTask(),
                        GenerateTask(),
                        GenerateTask(),
                        GenerateTask()
                    );
                }
                raceConditionCurrentCount--;

            });

            Assert.That(raceConditionActualValue, Is.EqualTo(raceConditionExpectedValue));
            Assert.That(raceConditionCurrentCount, Is.EqualTo(0));
            Assert.Pass();

            async Task GenerateTask()
            {
                var parent = asyncLock.SyncRoot!;
                Assert.That(asyncLock.SyncRoot, Is.Not.Null);
                asyncLock.Acquire(() =>
                {
                    raceConditionCurrentCount++;
                    Assert.That(raceConditionCurrentCount, Is.EqualTo(asyncLock.CurrentCount));
                    Assert.That(asyncLock.CurrentCount, Is.EqualTo(2));
                    Assert.That(asyncLock.SyncRoot, Is.Not.Null);
                    Assert.That(parent.CurrentCount, Is.EqualTo(0));
                    var tasks = new List<Task>();
                    for (int i = 0; i < 10; i++)
                    {
                        tasks.Add(Task.Run(ConcurrentTask));
                    }
                    for (int i = 0; i < 100; i++)
                    {
                        new SpinWait().SpinOnce();
                        asyncLock.Acquire(() =>
                        {
                            raceConditionCurrentCount++;
                            Assert.That(raceConditionCurrentCount, Is.EqualTo(asyncLock.CurrentCount));
                            Assert.That(asyncLock.CurrentCount, Is.EqualTo(3));
                            raceConditionActualValue++;
                            Interlocked.Increment(ref raceConditionExpectedValue);
                            raceConditionCurrentCount--;
                        });
                    }
                    Task.WaitAll(tasks.ToArray());
                    raceConditionCurrentCount--;
                });
                await Task.Yield();
                Assert.That(asyncLock.SyncRoot, Is.Not.Null);
                Assert.That(asyncLock.SyncRoot, Is.EqualTo(parent));
            }

            async Task ConcurrentTask()
            {
                var parent = asyncLock.SyncRoot!;
                Assert.That(asyncLock.SyncRoot, Is.Not.Null);
                asyncLock.Acquire(() =>
                {
                    raceConditionCurrentCount++;
                    Assert.That(raceConditionCurrentCount, Is.EqualTo(asyncLock.CurrentCount));
                    Assert.That(asyncLock.CurrentCount, Is.EqualTo(3));
                    Assert.That(parent.CurrentCount, Is.EqualTo(0));
                    Assert.That(asyncLock.SyncRoot, Is.Not.Null);
                    raceConditionActualValue++;
                    Interlocked.Increment(ref raceConditionExpectedValue);
                    raceConditionCurrentCount--;
                });
                Assert.That(asyncLock.SyncRoot, Is.Not.Null);
                Assert.That(asyncLock.SyncRoot, Is.EqualTo(parent));
                await asyncLock.AcquireAsync(async () =>
                {
                    raceConditionCurrentCount++;
                    Assert.That(raceConditionCurrentCount, Is.EqualTo(asyncLock.CurrentCount));
                    Assert.That(asyncLock.CurrentCount, Is.EqualTo(3));
                    Assert.That(parent.CurrentCount, Is.EqualTo(0));
                    Assert.That(asyncLock.SyncRoot, Is.Not.Null);
                    raceConditionActualValue++;
                    Interlocked.Increment(ref raceConditionExpectedValue);
                    await Task.Yield();
                    raceConditionActualValue++;
                    Interlocked.Increment(ref raceConditionExpectedValue);
                    raceConditionCurrentCount--;
                });
            }

            void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                if (sender is not ReentrantAsyncLockAlias @lock) return;
                Assert.That(e.PropertyName, Is.Not.EqualTo(nameof(@lock.CurrentCount)));
                switch (e.PropertyName)
                {
                    case nameof(@lock.CurrentCount)://does not triggered
                        Assert.That(@lock.IsEntered, Is.True);
                        raceConditionCurrentCount = @lock.CurrentCount;
                        break;
                }
            }
        }

        [Test]
        public void RecursionTest()
        {
            var id = Task.CurrentId;
            var tasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() => DoRecursion()));
            }
            DoRecursion();
            Task.WaitAll(tasks.ToArray());
            Assert.Pass();
        }

        [Test]
        public async Task AsyncRecursionTest()
        {
            var tasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() => DoRecursionAsync()));
            }
            await DoRecursionAsync();
            await Task.WhenAll(tasks);
            Assert.Pass();
        }


        [Test]
        public async Task AsyncContinuationTest()
        {
            using var asyncLock = new ReentrantAsyncLockAlias();
            var tasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(GetTask());
            }
            await Task.Run(GetTask);
            await Task.WhenAll(tasks);
            Assert.Pass();

            Task GetTask()
            {
                Assert.That(asyncLock.SyncRoot, Is.Null);
                asyncLock.Acquire(() =>
                {
                    var parent = asyncLock.SyncRoot!;
                    Assert.That(parent, Is.Not.Null);
                    Assert.That(parent.CurrentCount, Is.EqualTo(1));
                    Assert.That(asyncLock.IsEntered, Is.True);
                    asyncLock.Acquire(() =>
                    {
                        Assert.That(parent.CurrentCount, Is.EqualTo(0));
                        Assert.That(asyncLock.IsEntered, Is.True);
                    });
                    Assert.That(asyncLock.SyncRoot, Is.EqualTo(parent));
                    var task = asyncLock.AcquireAsync(async () =>
                    {
                        Assert.That(parent.CurrentCount, Is.EqualTo(0));
                        Assert.That(asyncLock.IsEntered, Is.True);
                        asyncLock.Acquire(() =>
                        {
                            Assert.That(asyncLock.IsEntered, Is.True);
                            Assert.That(parent.CurrentCount, Is.EqualTo(0));
                            Assert.That(asyncLock.SyncRoot, Is.Not.EqualTo(parent));
                        });
                        await Task.Yield();
                    }).AsTask().ContinueWith(t =>
                    {
                        Assert.That(asyncLock.SyncRoot, Is.EqualTo(parent));
                        Assert.That(parent.CurrentCount, Is.EqualTo(1));
                        asyncLock.Acquire(() =>
                                {
                                    Assert.That(asyncLock.IsEntered, Is.True);
                                    Assert.That(parent.CurrentCount, Is.EqualTo(0));
                                    Assert.That(asyncLock.SyncRoot, Is.Not.EqualTo(parent));
                                });
                    });
                    task.Wait();
                });
                return Task.CompletedTask;
            }
        }

        [Test]
        public async Task SupportAsynchronousReentrancyTest()
        {
            using var asyncLock = new ReentrantAsyncLockAlias();
            int depth = 0;
            await Task.Run(async () =>
            {
                Assert.That(asyncLock.IsEntered, Is.False);
                Assert.That(depth, Is.EqualTo(asyncLock.CurrentCount));
                await asyncLock.AcquireAsync(async () =>
                {
                    depth++;
                    Assert.That(asyncLock.IsEntered, Is.True);
                    Assert.That(depth, Is.EqualTo(asyncLock.CurrentCount));
                    await Task.Run(async () =>
                    {
                        Assert.That(asyncLock.IsEntered, Is.True);
                        Assert.That(depth, Is.EqualTo(asyncLock.CurrentCount));
                        await asyncLock.AcquireAsync(async () =>
                        {
                            depth++;
                            Assert.That(asyncLock.IsEntered, Is.True);
                            Assert.That(depth, Is.EqualTo(asyncLock.CurrentCount));
                            await Task.Run(async () =>
                            {
                                Assert.That(asyncLock.IsEntered, Is.True);
                                Assert.That(depth, Is.EqualTo(asyncLock.CurrentCount));
                                await asyncLock.AcquireAsync(async () =>
                                {
                                    depth++;
                                    Assert.That(asyncLock.IsEntered, Is.True);
                                    Assert.That(depth, Is.EqualTo(asyncLock.CurrentCount));
                                    await Task.Run(async () =>
                                    {
                                        Assert.That(asyncLock.IsEntered, Is.True);
                                        Assert.That(depth, Is.EqualTo(asyncLock.CurrentCount));
                                        await asyncLock.AcquireAsync(async () =>
                                        {
                                            depth++;
                                            await Task.Yield();
                                            //await Task.Delay(100).ConfigureAwait(false);
                                            Assert.That(asyncLock.IsEntered, Is.True);
                                            Assert.That(depth, Is.EqualTo(asyncLock.CurrentCount));
                                        }).ConfigureAwait(false);
                                        depth--;
                                        //await Task.Delay(100).ConfigureAwait(false);
                                        Assert.That(asyncLock.IsEntered, Is.True);
                                        Assert.That(depth, Is.EqualTo(asyncLock.CurrentCount));
                                    }).ConfigureAwait(false);
                                    Assert.That(asyncLock.IsEntered, Is.True);
                                    Assert.That(depth, Is.EqualTo(asyncLock.CurrentCount));
                                }).ConfigureAwait(false);
                                depth--;
                                //await Task.Delay(100).ConfigureAwait(false);
                                Assert.That(asyncLock.IsEntered, Is.True);
                                Assert.That(depth, Is.EqualTo(asyncLock.CurrentCount));
                            }).ConfigureAwait(false);
                            Assert.That(asyncLock.IsEntered, Is.True);
                            Assert.That(depth, Is.EqualTo(asyncLock.CurrentCount));
                        }).ConfigureAwait(false);
                        depth--;
                        //await Task.Delay(100).ConfigureAwait(false);
                        Assert.That(asyncLock.IsEntered, Is.True);
                        Assert.That(depth, Is.EqualTo(asyncLock.CurrentCount));
                    }).ConfigureAwait(false);
                    Assert.That(asyncLock.IsEntered, Is.True);
                    Assert.That(depth, Is.EqualTo(asyncLock.CurrentCount));
                }).ConfigureAwait(false);
                depth--;
                //await Task.Delay(100).ConfigureAwait(false);
                Assert.That(asyncLock.IsEntered, Is.False);
                Assert.That(depth, Is.EqualTo(asyncLock.CurrentCount));
            }).ConfigureAwait(false);
            //await Task.Delay(100).ConfigureAwait(false);
            Assert.That(depth, Is.EqualTo(asyncLock.CurrentCount));
            Assert.That(depth, Is.EqualTo(0));
            Assert.Pass();
        }

        [Test]
        public async Task SerializeReentrantCode()
        {
            using var asyncLock = new ReentrantAsyncLockAlias();
            int raceConditionActualValue = 0;
            var raceConditionExpectedValue = 0;
            await asyncLock.AcquireAsync(async () =>
            {
                Assert.That(asyncLock.CurrentCount, Is.EqualTo(1));
                int currentId = asyncLock.CurrentId;
                Task GenerateTask() => Task.Run(async () =>
                {
                    int currentId1 = asyncLock.CurrentId;
                    Assert.That(currentId1, Is.EqualTo(currentId));
                    asyncLock.Acquire(() =>
                    {
                        Assert.That(asyncLock.CurrentCount, Is.EqualTo(2));
                        int currentId2 = asyncLock.CurrentId;
                        Assert.That(currentId2, Is.EqualTo(currentId1));
                        raceConditionActualValue++;
                        Interlocked.Increment(ref raceConditionExpectedValue);
                        asyncLock.Acquire(() =>
                        {
                            Assert.That(asyncLock.CurrentCount, Is.EqualTo(3));
                            int currentId3 = asyncLock.CurrentId;
                            Assert.That(currentId3, Is.EqualTo(currentId2));
                            Assert.That(currentId3, Is.EqualTo(currentId1));
                            raceConditionActualValue++;
                            Interlocked.Increment(ref raceConditionExpectedValue);
                        });
                    });
                    await asyncLock.AcquireAsync(async () =>
                    {
                        int currentId3 = asyncLock.CurrentId;
                        Assert.That(currentId3, Is.EqualTo(currentId1));
                        Assert.That(asyncLock.CurrentCount, Is.EqualTo(2));
                        // If the code in this block is running simultaneously on multiple threads then the ++ operator
                        // is a race condition. This test detects the failure mode of that race condition--namely when
                        // one increment operation overwrites/undoes the outcome of another one. If that happens then
                        // the count won't be quite right at the end of this test.
                        raceConditionActualValue++;
                        Interlocked.Increment(
                            ref raceConditionExpectedValue); // This will always correctly increment, even in the face of multiple threads
                        // Another (perhaps more reliable) way to detect racing threads is to assert that the currently
                        // executing thread is the same thread that is currently processing the WorkQueue used
                        // internally by the ReentrantAsyncLock.
                        //Assert.True(asyncLock.IsOnQueue);
                        Assert.That(asyncLock.IsEntered, Is.True);

                        var task1 = DoTask();

                        asyncLock.Acquire(() =>
                        {
                            Assert.That(asyncLock.CurrentCount, Is.EqualTo(3));
                            int currentId4 = asyncLock.CurrentId;
                            Assert.That(currentId4, Is.EqualTo(currentId3));
                            raceConditionActualValue++;
                            Interlocked.Increment(ref raceConditionExpectedValue);
                        });

                        await task1.ConfigureAwait(false);

                        var task2 = DoTask();

                        var task3 = Task.Run(DoTask);

                        asyncLock.Acquire(() =>
                        {
                            Assert.That(asyncLock.CurrentCount, Is.EqualTo(3));
                            int currentId4 = asyncLock.CurrentId;
                            Assert.That(currentId4, Is.EqualTo(currentId3));
                            raceConditionActualValue++;
                            Interlocked.Increment(ref raceConditionExpectedValue);
                        });

                        // Asynchronously go away for a bit
                        await Task.Yield();
                        //await Task.Delay(1).ConfigureAwait(false);//No Gotchas

                        await task2.ConfigureAwait(false);

                        await task3;

                        // ...then come back and do the above stuff a second time.
                        raceConditionActualValue++;
                        Interlocked.Increment(ref raceConditionExpectedValue);
                        //Assert.True(asyncLock.IsOnQueue);
                        Assert.That(asyncLock.IsEntered, Is.True);
                        Assert.That(asyncLock.CurrentId, Is.EqualTo(currentId3));
                        Assert.That(asyncLock.CurrentId, Is.Not.Zero);
                        Assert.That(asyncLock.CurrentCount, Is.EqualTo(2));

                        Task DoTask()
                        {
                            asyncLock.Acquire(() =>
                            {
                                Assert.That(asyncLock.CurrentCount, Is.EqualTo(3));
                                int currentId4 = asyncLock.CurrentId;
                                Assert.That(currentId4, Is.EqualTo(currentId3));
                                raceConditionActualValue++;
                                Interlocked.Increment(ref raceConditionExpectedValue);
                            });
                            return Task.CompletedTask;
                        }
                    });
                });
                for (var i = 0; i < 1000; ++i)
                {
                    await Task.WhenAll(
                        GenerateTask(),
                        GenerateTask(),
                        GenerateTask(),
                        GenerateTask(),
                        GenerateTask()
                    );
                }
            });
            Assert.That(raceConditionActualValue, Is.EqualTo(raceConditionExpectedValue));
            Assert.Pass();
        }


        [Test]
        public async Task GotchasTest()
        {
            using var asyncLock = new ReentrantAsyncLockAlias();
            var state = 0;
            async Task ChangeGuardedStateAsync()
            {
                await asyncLock.AcquireAsync(async () =>
                {
                    state++;
                    await Task.Yield();
                });
            }

            await asyncLock.AcquireAsync(async () =>
            {
                var task = ChangeGuardedStateAsync();
                var stateBefore = state;
                await Task.Yield();
                Assert.That(stateBefore, Is.EqualTo(state)); //this line does not throw
                await task;
            });
            Assert.Pass();
        }
    }
}
