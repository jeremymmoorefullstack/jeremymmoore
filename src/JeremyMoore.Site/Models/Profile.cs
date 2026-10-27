namespace JeremyMoore.Site.Models;

/// <summary>Everything the site renders. One immutable object, registered as a singleton.</summary>
public sealed record Profile(
    string Name,
    string Title,
    string Location,
    string Summary,
    IReadOnlyList<Role> Experience,
    IReadOnlyList<Project> Projects,
    IReadOnlyList<SkillGroup> Skills,
    IReadOnlyList<ContactLink> Contact)
{
    public int CareerStart => Experience.Min(role => role.Start);

    public int YearsExperience(TimeProvider clock) =>
        clock.GetLocalNow().Year - CareerStart;

    public IEnumerable<Role> ExperienceNewestFirst =>
        Experience.OrderByDescending(role => role.End ?? int.MaxValue)
                  .ThenByDescending(role => role.Start);
}

public sealed record Role(
    string Title,
    string Company,
    int Start,
    int? End,
    string Summary,
    IReadOnlyList<string> Highlights,
    IReadOnlyList<string> Stack)
{
    public bool IsCurrent => End is null;

    public string Period => End is int end ? $"{Start} – {end}" : $"{Start} – Present";
}

public sealed record Project(
    string Name,
    string Language,
    string Description,
    string? Href = null);

public sealed record SkillGroup(string Name, IReadOnlyList<string> Items, bool Primary = false);

public sealed record ContactLink(string Label, string Display, string Href);
