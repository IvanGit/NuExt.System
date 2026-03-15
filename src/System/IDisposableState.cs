namespace System;

/// <summary>Provides access to an object's disposal state.</summary>
public interface IDisposableState
{
    /// <summary>Gets a value indicating whether the object is being disposed.</summary>
    bool IsDisposing { get; }

    /// <summary>Gets a value indicating whether the object has been disposed.</summary>
    bool IsDisposed { get; }
}
