namespace CipherDeck;

internal sealed class LatestOperationRunner : IDisposable
{
    private readonly object _sync = new();
    private CancellationTokenSource? _currentCancellation;
    private long _generation;
    private bool _disposed;

    public async Task<LatestOperationResult<T>> RunAsync<T>(Func<CancellationToken, T> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        CancellationTokenSource cancellation;
        CancellationTokenSource? previousCancellation;
        long generation;

        lock (_sync)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            cancellation = new CancellationTokenSource();
            previousCancellation = _currentCancellation;
            _currentCancellation = cancellation;
            generation = ++_generation;
        }

        previousCancellation?.Cancel();

        try
        {
            var value = await Task.Run(() =>
            {
                cancellation.Token.ThrowIfCancellationRequested();
                var result = operation(cancellation.Token);
                cancellation.Token.ThrowIfCancellationRequested();
                return result;
            }, cancellation.Token);

            lock (_sync)
            {
                var isCurrent = !_disposed &&
                                generation == _generation &&
                                ReferenceEquals(_currentCancellation, cancellation) &&
                                !cancellation.IsCancellationRequested;
                return new LatestOperationResult<T>(isCurrent, value);
            }
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            return new LatestOperationResult<T>(false, default!);
        }
        finally
        {
            lock (_sync)
            {
                if (ReferenceEquals(_currentCancellation, cancellation))
                    _currentCancellation = null;
            }

            cancellation.Dispose();
        }
    }

    public void Cancel()
    {
        CancellationTokenSource? cancellation;
        lock (_sync)
        {
            if (_disposed)
                return;

            _generation++;
            cancellation = _currentCancellation;
            _currentCancellation = null;
        }

        cancellation?.Cancel();
    }

    public void Dispose()
    {
        CancellationTokenSource? cancellation;
        lock (_sync)
        {
            if (_disposed)
                return;

            _disposed = true;
            _generation++;
            cancellation = _currentCancellation;
            _currentCancellation = null;
        }

        cancellation?.Cancel();
    }
}

internal readonly record struct LatestOperationResult<T>(bool IsCurrent, T Value);
