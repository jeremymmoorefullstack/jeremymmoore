using System.Text.Json;
using JeremyMoore.Prerender;
using JeremyMoore.Site.Content;
using JeremyMoore.Site.Models;

namespace JeremyMoore.Site.Tests;

public class StaticSiteTests
{
    private static readonly Profile Profile = ProfileContent.Jeremy;
    private static readonly Uri Site = new("https://jeremymmoore.com/");

    [Fact]
    public async Task Prerendered_home_has_every_section_and_the_footer()
    {
        var html = await StaticSite.RenderHomeAsync(Profile, new FixedClock(new DateTimeOffset(2030, 6, 15, 12, 0, 0, TimeSpan.Zero)));

        foreach (var id in new[] { "about", "experience", "skills", "education", "projects", "contact" })
        {
            Assert.Contains($"id=\"section-{id}\"", html);
        }

        Assert.Contains("<h1 class=\"hero-name\">Jeremy M. Moore</h1>", html);
        Assert.Contains("2030 Jeremy M. Moore</span>", html);
    }

    [Fact]
    public void Json_ld_describes_the_person_from_the_profile()
    {
        var script = StaticSite.BuildJsonLd(Profile, Site);
        var json = script["<script type=\"application/ld+json\">".Length..^"</script>".Length];

        using var doc = JsonDocument.Parse(json);
        var person = doc.RootElement.GetProperty("@graph")[0];

        Assert.Equal("Person", person.GetProperty("@type").GetString());
        Assert.Equal(Profile.Name, person.GetProperty("name").GetString());
        Assert.Equal("Dolooma", person.GetProperty("worksFor").GetProperty("name").GetString());
        Assert.Equal("mailto:jeremy@jeremymmoore.com", person.GetProperty("email").GetString());
        Assert.Contains(person.GetProperty("sameAs").EnumerateArray(), link => link.GetString()!.Contains("linkedin.com"));
        Assert.Contains(person.GetProperty("knowsAbout").EnumerateArray(), skill => skill.GetString() == "C#");
    }

    [Fact]
    public void Inject_replaces_the_boot_block_and_json_ld_marker()
    {
        var index = $"<head>{StaticSite.JsonLdMarker}</head><div id=\"app\">{StaticSite.AppStart}loading{StaticSite.AppEnd}</div>";

        var html = StaticSite.Inject(index, "<main>home</main>", "<script>ld</script>");

        Assert.Equal("<head><script>ld</script></head><div id=\"app\"><main>home</main></div>", html);
    }

    [Fact]
    public void Inject_fails_loudly_when_markers_are_missing() =>
        Assert.Throws<InvalidOperationException>(() => StaticSite.Inject("<div id=\"app\"></div>", "x", "y"));

    [Fact]
    public void Sitemap_lists_the_home_page() =>
        Assert.Contains("<loc>https://jeremymmoore.com/</loc>", StaticSite.BuildSitemap(Site, new DateOnly(2026, 10, 27)));
}
