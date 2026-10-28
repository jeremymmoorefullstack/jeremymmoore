using JeremyMoore.Api.Agent;
using Microsoft.Extensions.Options;

namespace JeremyMoore.Api.Tests;

public class SessionLimiterTests
{
    private readonly ManualClock _clock = new(new DateTimeOffset(2026, 10, 28, 15, 0, 0, TimeSpan.Zero));

    private SessionLimiter Limiter(int perHour = 3, int perDay = 40) =>
        new(Options.Create(new AgentOptions { SessionsPerVisitorPerHour = perHour, SessionsPerDay = perDay }), _clock);

    [Fact]
    public void A_visitor_is_capped_per_hour()
    {
        var limiter = Limiter(perHour: 2);

        Assert.True(limiter.TryAcquire("1.2.3.4"));
        Assert.True(limiter.TryAcquire("1.2.3.4"));
        Assert.False(limiter.TryAcquire("1.2.3.4"));
        Assert.True(limiter.TryAcquire("5.6.7.8"));
    }

    [Fact]
    public void A_visitor_can_start_again_after_an_hour()
    {
        var limiter = Limiter(perHour: 1);

        Assert.True(limiter.TryAcquire("1.2.3.4"));
        _clock.Advance(TimeSpan.FromMinutes(59));
        Assert.False(limiter.TryAcquire("1.2.3.4"));
        _clock.Advance(TimeSpan.FromMinutes(2));
        Assert.True(limiter.TryAcquire("1.2.3.4"));
    }

    [Fact]
    public void Everyone_shares_a_daily_cap_that_resets_at_midnight_utc()
    {
        var limiter = Limiter(perDay: 2);

        Assert.True(limiter.TryAcquire("a"));
        Assert.True(limiter.TryAcquire("b"));
        Assert.False(limiter.TryAcquire("c"));

        _clock.Advance(TimeSpan.FromHours(10));
        Assert.True(limiter.TryAcquire("c"));
    }

    private sealed class ManualClock(DateTimeOffset now) : TimeProvider
    {
        private DateTimeOffset _now = now;

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan by) => _now += by;
    }
}
