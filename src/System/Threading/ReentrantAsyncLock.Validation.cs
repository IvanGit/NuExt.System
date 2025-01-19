#define DEBUG_VALIDATE
using System.Diagnostics;
using static System.Diagnostics.Debug;

namespace System.Threading
{
    partial class ReentrantAsyncLock
    {
        [Conditional("DEBUG")]
        private void PerformValidationAfterExecution(bool isReentrant, SemaphoreSlim? syncRoot)
        {
#if !DEBUG_VALIDATE
            return;
#endif
            WriteLineIf(!IsEntered, $"{nameof(IsEntered)}={IsEntered}");
            Assert(IsEntered, $"{nameof(IsEntered)}={IsEntered}");

            WriteLineIf(LocalId != CurrentId, $"{nameof(LocalId)}={LocalId} != {nameof(CurrentId)}={CurrentId}");
            Assert(LocalId == CurrentId, $"{nameof(LocalId)}={LocalId} != {nameof(CurrentId)}={CurrentId}");

            WriteLineIf(CurrentCount <= 0, $"{nameof(CurrentCount)}={CurrentCount}");
            Assert(CurrentCount > 0, $"{nameof(CurrentCount)}={CurrentCount}");

            Assert(isReentrant && CurrentCount > 1 || !isReentrant && CurrentCount == 1);

            WriteLineIf(!ReferenceEquals(SyncRoot, syncRoot), $"{nameof(SyncRoot)}({SyncRoot?.GetHashCode()}) != {syncRoot}({syncRoot?.GetHashCode()})");
            Assert(ReferenceEquals(SyncRoot, syncRoot), $"{nameof(SyncRoot)}({SyncRoot?.GetHashCode()}) != {syncRoot}({syncRoot?.GetHashCode()})");

            WriteLineIf(SyncRoot == null, $"{nameof(SyncRoot)} is null");
            Assert(SyncRoot != null, $"{nameof(SyncRoot)} is null");

            WriteLineIf(SyncRoot?.CurrentCount != 1, $"{nameof(SyncRoot)}.CurrentCount={SyncRoot?.CurrentCount}");
            Assert(SyncRoot?.CurrentCount == 1, $"{nameof(SyncRoot)}.CurrentCount={SyncRoot?.CurrentCount}");
        }

        [Conditional("DEBUG")]
        private void ValidateExclusiveState()
        {
#if !DEBUG_VALIDATE
            return;
#endif
            WriteLineIf(CurrentId != 0, $"{nameof(CurrentId)}={CurrentId}");
            Assert(CurrentId == 0, $"{nameof(CurrentId)}={CurrentId}");

            WriteLineIf(CurrentCount != 0, $"{nameof(CurrentCount)}={CurrentCount}");
            Assert(CurrentCount == 0, $"{nameof(CurrentCount)}={CurrentCount}");

            WriteLineIf(SyncRoot != null, $"{nameof(SyncRoot)} is not null");
            Assert(SyncRoot == null, $"{nameof(SyncRoot)} is not null");
        }

        [Conditional("DEBUG")]
        private static void ValidateExclusiveStateFinally(bool isReentrant, SemaphoreSlim? syncRoot)
        {
#if !DEBUG_VALIDATE
            return;
#endif
            WriteLineIf(isReentrant, $"{nameof(isReentrant)}={isReentrant}");
            Assert(!isReentrant, $"{nameof(isReentrant)}={isReentrant}");

            WriteLineIf(syncRoot != null, $"{nameof(syncRoot)} is not null");
            Assert(syncRoot == null, $"{nameof(syncRoot)} is not null");
        }

        [Conditional("DEBUG")]
        private void ValidateReentrantState()
        {
#if !DEBUG_VALIDATE
            return;
#endif
            WriteLineIf(!IsEntered, $"{nameof(IsEntered)}={IsEntered}");
            Assert(IsEntered, $"{nameof(IsEntered)}={IsEntered}");

            WriteLineIf(CurrentCount <= 0, $"{nameof(CurrentCount)}={CurrentCount}");
            Assert(CurrentCount > 0, $"{nameof(CurrentCount)}={CurrentCount}");

            WriteLineIf(SyncRoot == null, $"{nameof(SyncRoot)} is null");
            Assert(SyncRoot != null, $"{nameof(SyncRoot)} is null");
        }

        [Conditional("DEBUG")]
        private static void ValidateReentrantStateFinally(bool isReentrant, SemaphoreSlim? syncRoot)
        {
#if !DEBUG_VALIDATE
            return;
#endif
            WriteLineIf(!isReentrant, $"{nameof(isReentrant)}={isReentrant}");
            Assert(isReentrant, $"{nameof(isReentrant)}={isReentrant}");

            WriteLineIf(syncRoot?.CurrentCount != 0, $"{nameof(syncRoot)}.CurrentCount={syncRoot?.CurrentCount}");
            Assert(syncRoot?.CurrentCount == 0, $"{nameof(syncRoot)}.CurrentCount={syncRoot?.CurrentCount}");
        }
    }
}
