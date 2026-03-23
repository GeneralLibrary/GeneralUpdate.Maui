namespace GeneralUpdate.Maui.Android.Utilities;

internal sealed class SpeedCalculator
{
    private readonly Queue<(DateTimeOffset timestamp, long bytes)> _samples = new();
    private readonly TimeSpan _window = TimeSpan.FromSeconds(3);

    public void AddSample(long bytes)
    {
        var now = DateTimeOffset.UtcNow;
        _samples.Enqueue((now, bytes));

        while (_samples.Count > 0 && now - _samples.Peek().timestamp > _window)
        {
            _samples.Dequeue();
        }
    }

    public double GetBytesPerSecond()
    {
        if (_samples.Count < 2)
        {
            return 0D;
        }

        var first = _samples.Peek();
        var last = _samples.Last();
        var elapsed = (last.timestamp - first.timestamp).TotalSeconds;

        if (elapsed <= 0)
        {
            return 0D;
        }

        var bytesDelta = last.bytes - first.bytes;
        return bytesDelta <= 0 ? 0D : bytesDelta / elapsed;
    }
}
