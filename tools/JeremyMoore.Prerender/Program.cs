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
var template = await File.ReadAllTextAsync(indexPath);

var home = StaticSite.Inject(template, await StaticSite.RenderHomeAsync(profile, clock), StaticSite.BuildJsonLd(profile, site));
await File.WriteAllTextAsync(indexPath, home);

Directory.CreateDirectory(Path.Combine(wwwroot, "work"));
foreach (var study in profile.CaseStudies)
{
    var page = StaticSite.Inject(template, await StaticSite.RenderCaseStudyAsync(profile, clock, study),
        StaticSite.BuildJsonLd(profile, site, study));
    page = StaticSite.WithMeta(page, $"{study.Title} · {profile.Name}", study.Summary, new Uri(site, study.Path));
    await File.WriteAllTextAsync(Path.Combine(wwwroot, $"{study.Path}.html"), page);
}

// The publish step precompressed the old index.html; drop those copies so nothing serves stale markup.
foreach (var stale in new[] { "index.html.br", "index.html.gz", "staticwebapp.config.json.br", "staticwebapp.config.json.gz" })
{
    File.Delete(Path.Combine(wwwroot, stale));
}

var configPath = Path.Combine(wwwroot, "staticwebapp.config.json");
await File.WriteAllTextAsync(configPath, StaticSite.WithCaseStudyRoutes(await File.ReadAllTextAsync(configPath), profile.CaseStudies));

var today = DateOnly.FromDateTime(clock.GetLocalNow().Date);
var paths = new[] { "" }.Concat(profile.CaseStudies.Select(study => study.Path));
await File.WriteAllTextAsync(Path.Combine(wwwroot, "sitemap.xml"), StaticSite.BuildSitemap(site, today, paths));
await File.WriteAllTextAsync(Path.Combine(wwwroot, "llms.txt"), LlmsText.Build(profile, site));

Console.WriteLine($"Prerendered the home page and {profile.CaseStudies.Count} case studies, and wrote sitemap.xml and llms.txt.");
return 0;
