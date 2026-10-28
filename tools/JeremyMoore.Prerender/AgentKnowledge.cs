using System.Text;
using JeremyMoore.Site.Models;

namespace JeremyMoore.Prerender;

/// <summary>
/// The voice agent's knowledge base, written from the same <see cref="Profile"/> the site renders,
/// so the agent can't say anything the site doesn't. Hand-written answers live in agent/notes.md.
/// </summary>
public static class AgentKnowledge
{
    public static string Build(Profile profile)
    {
        var text = new StringBuilder();

        text.AppendLine($"# {profile.Name}");
        text.AppendLine();
        text.AppendLine($"{profile.Title}. Based in {profile.Location}.");
        text.AppendLine($"Writing software professionally since {profile.CareerStart}.");
        text.AppendLine();
        text.AppendLine(profile.Summary);
        text.AppendLine();

        text.AppendLine("## Experience, newest first");
        foreach (var role in profile.ExperienceNewestFirst)
        {
            text.AppendLine();
            text.AppendLine($"### {role.Title} at {role.Company} ({role.Period})");
            text.AppendLine(role.Summary);
            foreach (var highlight in role.Highlights)
            {
                text.AppendLine($"- {highlight}");
            }

            text.AppendLine($"Stack: {string.Join(", ", role.Stack)}.");
        }

        text.AppendLine();
        text.AppendLine("## Skills");
        foreach (var group in profile.Skills)
        {
            text.AppendLine($"- {group.Name}{(group.Primary ? " (primary focus)" : "")}: {string.Join(", ", group.Items)}.");
        }

        text.AppendLine();
        text.AppendLine("## Education");
        foreach (var degree in profile.Education)
        {
            text.AppendLine($"- {degree.Title}, {degree.Field}, {degree.School}, {degree.Year}.");
        }

        text.AppendLine();
        text.AppendLine("## Projects");
        foreach (var project in profile.Projects)
        {
            text.AppendLine($"- {project.Name} ({project.Language}): {project.Description}");
        }

        text.AppendLine();
        text.AppendLine("## Contact");
        foreach (var link in profile.Contact)
        {
            text.AppendLine($"- {link.Label}: {link.Display}");
        }

        return text.ToString();
    }
}
