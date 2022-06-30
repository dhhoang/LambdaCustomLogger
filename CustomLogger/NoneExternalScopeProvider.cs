using System;
using Microsoft.Extensions.Logging;

namespace CustomLogger;

public class NoneExternalScopeProvider : IExternalScopeProvider
{
    private NoneExternalScopeProvider()
    {
    }

    /// <summary>
    /// Returns a cached instance of <see cref="NoneExternalScopeProvider"/>.
    /// </summary>
    public static IExternalScopeProvider Instance { get; } = new NoneExternalScopeProvider();

    /// <inheritdoc />
    void IExternalScopeProvider.ForEachScope<TState>(Action<object?, TState> callback, TState state)
    {
    }

    /// <inheritdoc />
    IDisposable IExternalScopeProvider.Push(object? state) => NoopDisposable.Singleton;

    private class NoopDisposable : IDisposable
    {
        public static IDisposable Singleton { get; } = new NoopDisposable();

        public void Dispose()
        {
        }
    }
}
