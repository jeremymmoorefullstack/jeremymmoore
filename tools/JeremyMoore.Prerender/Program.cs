using JeremyMoore.Prerender;
using JeremyMoore.Site.Content;

// Usage: dotnet run --project tools/JeremyMoore.Prerender -- <published wwwroot>
if (args is not [var wwwroot])
{
    Console.Error.WriteLine("Usage: JeremyMoore.Prerender <published wwwroot folder>");
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

Console.WriteLine($"Prerendered {indexPath} ({html.Length:N0} chars) and wrote sitemap.xml.");
return 0;
