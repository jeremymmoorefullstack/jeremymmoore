using JeremyMoore.Prerender;
using JeremyMoore.Site.Content;

namespace JeremyMoore.Site.Tests;

public class AgentKnowledgeTests
{
    [Fact]
    public void Committed_knowledge_matches_the_profile()
    {
        var committed = File.ReadAllText(Path.Combine(RepoRoot(), "agent", "knowledge.md"));

        // If this fails, run: dotnet run --project tools/JeremyMoore.Prerender -- agent agent
        Assert.Equal(AgentKnowledge.Build(ProfileContent.Jeremy), committed);
    }

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "JeremyMoore.sln")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("Couldn't find the repo root.");
    }
}
