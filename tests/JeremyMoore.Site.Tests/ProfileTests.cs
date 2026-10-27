using JeremyMoore.Site.Content;
using JeremyMoore.Site.Models;

namespace JeremyMoore.Site.Tests;

public class ProfileTests
{
    private static readonly Profile Profile = ProfileContent.Jeremy;

    [Fact]
    public void Career_starts_at_earliest_role() =>
        Assert.Equal(2002, Profile.CareerStart);

    [Fact]
    public void Years_of_experience_follow_the_clock()
    {
        var clock = new FixedClock(new DateTimeOffset(2030, 6, 15, 12, 0, 0, TimeSpan.Zero));

        Assert.Equal(28, Profile.YearsExperience(clock));
    }

    [Fact]
    public void Current_role_is_listed_first()
    {
        var first = Profile.ExperienceNewestFirst.First();

        Assert.True(first.IsCurrent);
        Assert.Equal("Dolooma", first.Company);
    }

    [Theory]
    [InlineData(2018, 2024, "2018 – 2024")]
    [InlineData(2024, null, "2024 – Present")]
    public void Period_formats_open_and_closed_ranges(int start, int? end, string expected)
    {
        var role = new Role("Engineer", "Acme", start, end, "", [], []);

        Assert.Equal(expected, role.Period);
    }

    [Fact]
    public void Exactly_one_skill_group_is_primary_and_it_is_dotnet()
    {
        var primary = Assert.Single(Profile.Skills, group => group.Primary);

        Assert.Contains("C#", primary.Items);
    }
}
