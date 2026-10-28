using Microsoft.Extensions.Options;

namespace JeremyMoore.Api.Agent;

/// <summary>
/// Caps how often voice sessions start: per visitor per hour, and in total per day.
/// It lives in memory, so it is per instance; the hard ceiling is the spend cap set in ElevenLabs.
/// </summary>
public sealed class SessionLimiter(IOptions<AgentOptions> options, TimeProvider clock)
{
    private readonly Dictionary<string, Queue<DateTimeOffset>> _visitors = [];
    private readonly Lock _gate = new();
    private DateOnly _day;
    private int _today;

    public bool TryAcquire(string visitor)
    {
        var settings = options.Value;
        var now = clock.GetUtcNow();
        var hourAgo = now.AddHours(-1);

        lock (_gate)
        {
            var day = DateOnly.FromDateTime(now.UtcDateTime);
            if (day != _day)
            {
                _day = day;
                _today = 0;
                _visitors.Clear();
            }

            if (_today >= settings.SessionsPerDay)
            {
                return false;
            }

            if (!_visitors.TryGetValue(visitor, out var starts))
            {
                _visitors[visitor] = starts = new Queue<DateTimeOffset>();
            }

            while (starts.TryPeek(out var oldest) && oldest <= hourAgo)
            {
                starts.Dequeue();
            }

            if (starts.Count >= settings.SessionsPerVisitorPerHour)
            {
                return false;
            }

            starts.Enqueue(now);
            _today++;
            return true;
        }
    }
}
