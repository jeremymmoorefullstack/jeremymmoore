using JeremyMoore.Prerender;
using JeremyMoore.Site.Content;

// Usage: dotnet run --project tools/JeremyMoore.Prerender -- <published wwwroot>
//        dotnet run --project tools/JeremyMoore.Prerender -- agent <folder>   (writes the agent's knowledge.md)
if (args is ["agent", var agentFolder])
{
    var knowledgePath = Path.Combine(agentFolder, "knowledge.md");
    await File.WriteAllTextAsync(knowledgePath, AgentKnowledge.Build(ProfileContent.Jeremy));
    Console.WriteLine($"Wrote {knowledgePath}.");
    return 0;
}

if (args is not [var wwwroot])
{
    Console.Error.WriteLine("Usage: JeremyMoore.Prerender <published wwwroot folder> | agent <folder>");
    return 1;
}

var site = new Uri("https://jeremymmoore.com/");
var profile = ProfileContent.Jeremy;
var clock = TimeProvider.System;

var indexPath = Path.Combine(wwwroot, "index.html");
var appHtml = await StaticSite.RenderHomeAsync(profile, clock);
var html = StaticSite.Inject(await File.ReadAllTextAsync(indexPath), appHtml, StaticSite.BuildJsonLd(profile, site));
await File.WriteAllTextAsync(indexPath, html);

// The publish step precompressed the old index.html; drop those copies so nothing serves stale markup.
foreach (var stale in new[] { "index.html.br", "index.html.gz" })
{
    File.Delete(Path.Combine(wwwroot, stale));
}

var today = DateOnly.FromDateTime(clock.GetLocalNow().Date);
await File.WriteAllTextAsync(Path.Combine(wwwroot, "sitemap.xml"), StaticSite.BuildSitemap(site, today));
await File.WriteAllTextAsync(Path.Combine(wwwroot, "llms.txt"), LlmsText.Build(profile, site));

Console.WriteLine($"Prerendered {indexPath} ({html.Length:N0} chars) and wrote sitemap.xml and llms.txt.");
return 0;
