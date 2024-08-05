#define DEBUG_VALIDATE
using System.Diagnostics;

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
            Debug.WriteLineIf(!IsEntered, $"{nameof(IsEntered)}={IsEntered}");
            Debug.Assert(IsEntered, $"{nameof(IsEntered)}={IsEntered}");

            Debug.WriteLineIf(LocalId != CurrentId, $"{nameof(LocalId)}={LocalId} != {nameof(CurrentId)}={CurrentId}");
            Debug.Assert(LocalId == CurrentId, $"{nameof(LocalId)}={LocalId} != {nameof(CurrentId)}={CurrentId}");

            Debug.WriteLineIf(CurrentCount <= 0, $"{nameof(CurrentCount)}={CurrentCount}");
            Debug.Assert(CurrentCount > 0, $"{nameof(CurrentCount)}={CurrentCount}");

            Debug.Assert(isReentrant && CurrentCount > 1 || !isReentrant && CurrentCount == 1);

            Debug.WriteLineIf(!ReferenceEquals(SyncRoot, syncRoot), $"{nameof(SyncRoot)}({SyncRoot?.GetHashCode()}) != {syncRoot}({syncRoot?.GetHashCode()})");
            Debug.Assert(ReferenceEquals(SyncRoot, syncRoot), $"{nameof(SyncRoot)}({SyncRoot?.GetHashCode()}) != {syncRoot}({syncRoot?.GetHashCode()})");

            Debug.WriteLineIf(SyncRoot == null, $"{nameof(SyncRoot)} is null");
            Debug.Assert(SyncRoot != null, $"{nameof(SyncRoot)} is null");

            Debug.WriteLineIf(SyncRoot?.CurrentCount != 1, $"{nameof(SyncRoot)}.CurrentCount={SyncRoot?.CurrentCount}");
            Debug.Assert(SyncRoot?.CurrentCount == 1, $"{nameof(SyncRoot)}.CurrentCount={SyncRoot?.CurrentCount}");
        }

        [Conditional("DEBUG")]
        private void ValidateExclusiveState()
        {
#if !DEBUG_VALIDATE
            return;
#endif
            Debug.WriteLineIf(CurrentId != 0, $"{nameof(CurrentId)}={CurrentId}");
            Debug.Assert(CurrentId == 0, $"{nameof(CurrentId)}={CurrentId}");

            Debug.WriteLineIf(CurrentCount != 0, $"{nameof(CurrentCount)}={CurrentCount}");
            Debug.Assert(CurrentCount == 0, $"{nameof(CurrentCount)}={CurrentCount}");

            Debug.WriteLineIf(SyncRoot != null, $"{nameof(SyncRoot)} is not null");
            Debug.Assert(SyncRoot == null, $"{nameof(SyncRoot)} is not null");
        }

        [Conditional("DEBUG")]
        private static void ValidateExclusiveStateFinally(bool isReentrant, SemaphoreSlim? syncRoot)
        {
#if !DEBUG_VALIDATE
            return;
#endif
            Debug.WriteLineIf(isReentrant, $"{nameof(isReentrant)}={isReentrant}");
            Debug.Assert(!isReentrant, $"{nameof(isReentrant)}={isReentrant}");

            Debug.WriteLineIf(syncRoot != null, $"{nameof(syncRoot)} is not null");
            Debug.Assert(syncRoot == null, $"{nameof(syncRoot)} is not null");
        }

        [Conditional("DEBUG")]
        private void ValidateReentrantState()
        {
#if !DEBUG_VALIDATE
            return;
#endif
            Debug.WriteLineIf(!IsEntered, $"{nameof(IsEntered)}={IsEntered}");
            Debug.Assert(IsEntered, $"{nameof(IsEntered)}={IsEntered}");

            Debug.WriteLineIf(CurrentCount <= 0, $"{nameof(CurrentCount)}={CurrentCount}");
            Debug.Assert(CurrentCount > 0, $"{nameof(CurrentCount)}={CurrentCount}");

            Debug.WriteLineIf(SyncRoot == null, $"{nameof(SyncRoot)} is null");
            Debug.Assert(SyncRoot != null, $"{nameof(SyncRoot)} is null");
        }

        [Conditional("DEBUG")]
        private static void ValidateReentrantStateFinally(bool isReentrant, SemaphoreSlim? syncRoot)
        {
#if !DEBUG_VALIDATE
            return;
#endif
            Debug.WriteLineIf(!isReentrant, $"{nameof(isReentrant)}={isReentrant}");
            Debug.Assert(isReentrant, $"{nameof(isReentrant)}={isReentrant}");

            Debug.WriteLineIf(syncRoot?.CurrentCount != 0, $"{nameof(syncRoot)}.CurrentCount={syncRoot?.CurrentCount}");
            Debug.Assert(syncRoot?.CurrentCount == 0, $"{nameof(syncRoot)}.CurrentCount={syncRoot?.CurrentCount}");
        }
    }
}
