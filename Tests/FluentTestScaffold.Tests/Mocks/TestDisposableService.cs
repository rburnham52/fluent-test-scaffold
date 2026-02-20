using System;

namespace FluentTestScaffold.Tests.Mocks;

public class TestDisposableService : IDisposable
{
    public bool WasDisposed { get; private set; }

    public void Dispose()
    {
        WasDisposed = true;
    }
}
