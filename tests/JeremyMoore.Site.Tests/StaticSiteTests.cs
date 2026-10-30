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

        foreach (var id in new[] { "about", "work", "experience", "skills", "education", "projects", "faq", "contact" })
        {
            Assert.Contains($"id=\"section-{id}\"", html);
        }

        Assert.Contains("<h1 class=\"hero-headline\">", html);
        Assert.Contains("id=\"ask-input\"", html);
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
    public void Sitemap_lists_the_home_page_and_every_case_study()
    {
        var sitemap = StaticSite.BuildSitemap(Site, new DateOnly(2026, 10, 30),
            new[] { "" }.Concat(Profile.CaseStudies.Select(study => study.Path)));

        Assert.Contains("<loc>https://jeremymmoore.com/</loc>", sitemap);
        Assert.Contains("<loc>https://jeremymmoore.com/work/ai-phone-dispatcher</loc>", sitemap);
        Assert.Equal(1 + Profile.CaseStudies.Count, sitemap.Split("<url>").Length - 1);
    }

    [Fact]
    public async Task Case_study_page_is_prerendered_with_its_own_meta_and_article_json_ld()
    {
        var study = Profile.CaseStudies[0];
        var template = "<head><title>Home</title><meta name=\"description\" content=\"home\" />" +
                       "<link rel=\"canonical\" href=\"https://jeremymmoore.com/\" />" +
                       "<meta property=\"og:url\" content=\"https://jeremymmoore.com/\" />" +
                       $"{StaticSite.JsonLdMarker}</head><div id=\"app\">{StaticSite.AppStart}x{StaticSite.AppEnd}</div>";

        var body = await StaticSite.RenderCaseStudyAsync(Profile, new FixedClock(new DateTimeOffset(2030, 6, 15, 12, 0, 0, TimeSpan.Zero)), study);
        var html = StaticSite.WithMeta(StaticSite.Inject(template, body, StaticSite.BuildJsonLd(Profile, Site, study)),
            "A & B", study.Summary, new Uri(Site, study.Path));

        Assert.Contains($"<h1 class=\"case-title\">{study.Title}</h1>", html);
        Assert.Contains("<title>A &amp; B</title>", html);
        Assert.Contains($"<link rel=\"canonical\" href=\"https://jeremymmoore.com/{study.Path}\"", html);
        Assert.Contains($"<meta property=\"og:url\" content=\"https://jeremymmoore.com/{study.Path}\"", html);
        Assert.Contains("\"@type\":\"Article\"", html);
        Assert.DoesNotContain("FAQPage", html);
    }

    [Fact]
    public void Case_study_routes_rewrite_clean_urls_to_the_prerendered_files()
    {
        var config = StaticSite.WithCaseStudyRoutes("{\"navigationFallback\":{\"rewrite\":\"/index.html\"}}", Profile.CaseStudies);

        using var doc = JsonDocument.Parse(config);
        var route = doc.RootElement.GetProperty("routes")[0];
        Assert.Equal("/work/ai-phone-dispatcher", route.GetProperty("route").GetString());
        Assert.Equal("/work/ai-phone-dispatcher.html", route.GetProperty("rewrite").GetString());
        Assert.True(doc.RootElement.TryGetProperty("navigationFallback", out _));
    }

    [Fact]
    public void Json_ld_includes_the_faq()
    {
        var script = StaticSite.BuildJsonLd(Profile, Site);
        var json = script["<script type=\"application/ld+json\">".Length..^"</script>".Length];

        using var doc = JsonDocument.Parse(json);
        var faq = doc.RootElement.GetProperty("@graph").EnumerateArray()
            .Single(node => node.GetProperty("@type").GetString() == "FAQPage");

        Assert.Equal(Profile.Faq.Count, faq.GetProperty("mainEntity").GetArrayLength());
    }

    [Fact]
    public void Llms_txt_summarizes_the_profile_with_faq_and_contact()
    {
        var llms = LlmsText.Build(Profile, Site);

        Assert.StartsWith("# Jeremy M. Moore", llms);
        Assert.Contains(Profile.Availability, llms);
        Assert.Contains("### Is Jeremy available for contract or part-time work?", llms);
        Assert.Contains("- Email: jeremy@jeremymmoore.com", llms);
        Assert.Contains("](https://jeremymmoore.com/work/healthcare-edi)", llms);
        Assert.DoesNotContain("mailto:", llms);
    }
}
