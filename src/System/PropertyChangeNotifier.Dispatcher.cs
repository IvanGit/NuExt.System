using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System
{
    partial class PropertyChangeNotifier : IDispatcherObject
    {
        private sealed class State
        {
            internal readonly SynchronizationContextHelper Helper;

            internal State(SynchronizationContextHelper helper)
            {
                Helper = helper;
            }
        }

        private readonly State? _state;

        #region Properties

        /// <summary>
        /// Gets a value indicating whether a <see cref="SynchronizationContext"/> is provided.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if a <see cref="SynchronizationContext"/> is associated with this instance;
        /// otherwise, <see langword="false"/>.
        /// </value>
        [Browsable(false)]
        protected bool HasSynchronizationContext
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => SynchronizationContext is not null;
        }

        /// <summary>
        /// Gets the <see cref="SynchronizationContext"/> associated with this instance.
        /// </summary>
        /// <value>
        /// The <see cref="SynchronizationContext"/> used for marshaling operations to a specific thread or context,
        /// or <see langword="null"/> if no context was provided.
        /// </value>
        [Browsable(false)]
        public SynchronizationContext? SynchronizationContext { get; }

        /// <summary>
        /// Gets the thread on which this instance was created.
        /// <para>
        /// <b>Note:</b> This property captures the creation thread for reference, but when a 
        /// <see cref="SynchronizationContext"/> is provided, thread affinity is managed by 
        /// the context rather than by comparing threads directly.
        /// </para>
        /// </summary>
        [Browsable(false)]
        public Thread Thread { get; } = Thread.CurrentThread;

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the calling thread can safely access this instance directly.
        /// </summary>
        /// <returns>
        /// <list type="bullet">
        /// <item>
        /// <term>With thread-affine known SynchronizationContext</term>
        /// <description>
        /// Returns <see langword="true"/> when the calling thread has access to the context's associated thread.
        /// </description>
        /// </item>
        /// <item>
        /// <term>With unknown SynchronizationContext</term>
        /// <description>
        /// Returns <see langword="false"/>; use SynchronizationContext.Invoke methods for thread-safe access.
        /// </description>
        /// </item>
        /// <item>
        /// <term>Without SynchronizationContext</term>
        /// <description>
        /// Returns <see langword="true"/> if the calling thread matches the thread on which this instance was created.
        /// </description>
        /// </item>
        /// </list>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckAccess()
        {
            if (HasSynchronizationContext)
            {
                return _state!.Helper.CheckAccess();
            }

            return Thread == Thread.CurrentThread;
        }

        /// <summary>
        /// Verifies that the calling thread can safely access this instance's members directly.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the calling thread does not have access to this instance.
        /// </exception>
        /// <remarks>
        /// <para>
        /// For instances with a <see cref="SynchronizationContext"/>, access verification depends on the context type:
        /// <list type="bullet">
        /// <item>
        /// Thread-affine contexts (UI frameworks like WPF, WinForms, Avalonia) delegate verification to their own access checks.
        /// </item>
        /// <item>
        /// Unknown contexts (e.g., default <see cref="SynchronizationContext"/>) always require explicit synchronization via 
        /// Invoke(Action) or InvokeAsync(Action) methods.
        /// </item>
        /// </list>
        /// </para>
        /// <para>
        /// For instances without a <see cref="SynchronizationContext"/>, access is restricted to the thread where this instance was created.
        /// </para>
        /// <para>
        /// This method is the validation counterpart to <see cref="CheckAccess"/>, throwing an exception instead of returning a boolean.
        /// </para>
        /// </remarks>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VerifyAccess()
        {
            if (!CheckAccess())
            {
                ThrowVerifyAccess();
            }
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowVerifyAccess()
        {
            throw new InvalidOperationException(string.Format(SR.InvalidOperation_ThreadAccessError, Thread.ManagedThreadId));
        }

        #endregion
    }
}
