#if !NET9_0_OR_GREATER

#pragma warning disable CS9216 // A value of type 'System.Threading.Lock' converted to a different type will use likely unintended monitor-based locking in 'lock' statement.

using System.Runtime.CompilerServices;

namespace System.Threading
{
    /// <summary>
    /// Stub for System.Threading.Lock type (available in .NET 9+).
    /// Provides compilation compatibility when using the 'lock' keyword in earlier .NET versions.
    /// </summary>
    /// <remarks>
    /// In .NET 9+, the actual System.Threading.Lock type is used.
    /// </remarks>
    public sealed class Lock
    {
        /* Lock lockObj = new Lock();
         * 
         * In .NET 9+, for a Lock instance `lockObj`
         * 
         * lock (lockObj)
         * {
         *     //code
         * }
         * 
         * compiles into
         * 
         * Lock.Scope scope = lockObj.EnterScope();
         * try
         * {
         *     //code
         * }
         * finally
         * {
         *     scope.Dispose();
         * }
        */

        /// <summary>
        /// <inheritdoc cref="Monitor.Enter(object)"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Enter()
        {
            Monitor.Enter(this);
        }

        /// <summary>
        /// Enters the lock and returns a <see cref="Scope"/> that may be disposed to exit the lock. Once the method returns,
        /// the calling thread would be the only thread that holds the lock. This method is intended to be used along with a
        /// language construct that would automatically dispose the <see cref="Scope"/>, such as with the C# <code>using</code>
        /// statement.
        /// </summary>
        /// <returns>
        /// A <see cref="Scope"/> that may be disposed to exit the lock.
        /// </returns>
        /// <remarks>
        /// If the lock cannot be entered immediately, the calling thread waits for the lock to be exited. If the lock is
        /// already held by the calling thread, the lock is entered again. The calling thread should exit the lock, such as by
        /// disposing the returned <see cref="Scope"/>, as many times as it had entered the lock to fully exit the lock and
        /// allow other threads to enter the lock.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Scope EnterScope()
        {
            bool lockTaken = false;
            Monitor.Enter(this, ref lockTaken);
            return new Scope(this, lockTaken);
        }

        /// <summary>
        /// A disposable structure that is returned by <see cref="EnterScope()"/>, which when disposed, exits the lock.
        /// </summary>
        public ref struct Scope
        {
            private Lock? _lockObj;
            private readonly bool _lockTaken;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Scope(Lock lockObj, bool lockTaken)
            {
                _lockObj = lockObj;
                _lockTaken = lockTaken;
            }

            /// <summary>
            /// Exits the lock.
            /// </summary>
            /// <remarks>
            /// If the calling thread holds the lock multiple times, such as recursively, the lock is exited only once. The
            /// calling thread should ensure that each enter is matched with an exit.
            /// </remarks>
            /// <exception cref="SynchronizationLockException">
            /// The calling thread does not hold the lock.
            /// </exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                Lock? lockObj = _lockObj;
                if (lockObj is not null)
                {
                    _lockObj = null;
                    if (_lockTaken)
                    {
                        lockObj.Exit();
                    }
                }
            }
        }

        /// <summary>
        /// <inheritdoc cref="Monitor.TryEnter(object)"/>
        /// </summary>
        /// <returns>
        /// <inheritdoc cref="Monitor.TryEnter(object)"/>
        /// </returns>
        [MethodImpl(MethodImplOptions.NoInlining)]

        public bool TryEnter() => Monitor.TryEnter(this);

        /// <summary>
        /// <inheritdoc cref="Monitor.TryEnter(object, int)"/>
        /// </summary>
        /// <returns>
        /// <inheritdoc cref="Monitor.TryEnter(object, int)"/>
        /// </returns>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait for the lock.</param>
        /// <exception cref="ArgumentOutOfRangeException">millisecondsTimeout is negative, and not equal to <see cref="Timeout.Infinite"/>.</exception>
        public bool TryEnter(int millisecondsTimeout)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(millisecondsTimeout, -1);
            return Monitor.TryEnter(this, millisecondsTimeout);
        }

        /// <summary>
        /// <inheritdoc cref="Monitor.TryEnter(object, TimeSpan)"/>
        /// </summary>
        /// <returns>
        /// <inheritdoc cref="Monitor.TryEnter(object, TimeSpan)"/>
        /// </returns>
        /// <param name="timeout">A <see cref="TimeSpan" /> representing the amount of time to wait for the lock.
        /// A value of -1 millisecond specifies an infinite wait.</param>
        /// <exception cref="ArgumentOutOfRangeException">The value of timeout in milliseconds is negative and is not equal to <see cref="Timeout.Infinite"/>
        /// (-1 millisecond), or is greater than <see cref="int.MaxValue"/>.</exception>
        public bool TryEnter(TimeSpan timeout) => Monitor.TryEnter(this, timeout);


        /// <summary>
        /// <inheritdoc cref="Monitor.Exit(object)"/>
        /// </summary>
        /// <exception cref="SynchronizationLockException">The current thread does not own the lock for the specified object.</exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Exit() => Monitor.Exit(this);

        /// <summary>
        /// <code>true</code> if the lock is held by the calling thread, <code>false</code> otherwise.
        /// </summary>
        public bool IsHeldByCurrentThread => Monitor.IsEntered(this);
    }
}
#endif
