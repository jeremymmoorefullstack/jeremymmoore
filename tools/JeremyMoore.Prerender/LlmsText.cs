using System.Text;
using JeremyMoore.Site.Models;

namespace JeremyMoore.Prerender;

/// <summary>
/// /llms.txt (llmstxt.org): a plain-markdown summary for AI answer engines, built from the same
/// <see cref="Profile"/> the site renders so it never claims more than the resume does.
/// </summary>
public static class LlmsText
{
    public static string Build(Profile profile, Uri site)
    {
        var text = new StringBuilder();

        text.AppendLine($"# {profile.Name}");
        text.AppendLine();
        text.AppendLine($"> {profile.Title}, based in {profile.Location}. {profile.Availability}. " +
                        $"Writing software professionally since {profile.CareerStart}.");
        text.AppendLine();
        text.AppendLine(profile.Summary);
        text.AppendLine();
        text.AppendLine($"The site at {site} has his full resume and an AI agent (text or voice) that answers questions " +
                        "from it and can pass him a message.");

        text.AppendLine();
        text.AppendLine("## Experience");
        text.AppendLine();
        foreach (var role in profile.ExperienceNewestFirst)
        {
            text.AppendLine($"- {role.Title}, {role.Company} ({role.Period}): {role.Summary}");
        }

        text.AppendLine();
        text.AppendLine("## Skills");
        text.AppendLine();
        foreach (var group in profile.Skills)
        {
            text.AppendLine($"- {group.Name}: {string.Join(", ", group.Items)}");
        }

        text.AppendLine();
        text.AppendLine("## Education");
        text.AppendLine();
        foreach (var degree in profile.Education)
        {
            text.AppendLine($"- {degree.Title}, {degree.Field}, {degree.School} ({degree.Year})");
        }

        text.AppendLine();
        text.AppendLine("## FAQ");
        foreach (var item in profile.Faq)
        {
            text.AppendLine();
            text.AppendLine($"### {item.Question}");
            text.AppendLine();
            text.AppendLine(item.Answer);
        }

        text.AppendLine();
        text.AppendLine("## Contact");
        text.AppendLine();
        text.AppendLine($"- [Website]({site})");
        foreach (var link in profile.Contact)
        {
            var href = link.Href.StartsWith("mailto:", StringComparison.Ordinal) ? link.Display : link.Href;
            text.AppendLine($"- {link.Label}: {href}");
        }

        return text.ToString();
    }
}
