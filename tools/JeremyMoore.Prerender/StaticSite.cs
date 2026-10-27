using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using JeremyMoore.Site.Layout;
using JeremyMoore.Site.Models;
using JeremyMoore.Site.Pages;

namespace JeremyMoore.Prerender;

/// <summary>
/// Turns the published index.html into a crawlable page: the home page rendered to static HTML,
/// JSON-LD built from the same <see cref="Profile"/>, and a sitemap. Blazor replaces the
/// prerendered markup when the runtime boots, so both always come from one source.
/// </summary>
public static class StaticSite
{
    public const string AppStart = "<!-- prerender:app -->";
    public const string AppEnd = "<!-- /prerender:app -->";
    public const string JsonLdMarker = "<!-- prerender:jsonld -->";

    public static async Task<string> RenderHomeAsync(Profile profile, TimeProvider clock)
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(profile)
            .AddSingleton(clock)
            .BuildServiceProvider();

        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());

        return await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(LayoutView.Layout)] = typeof(MainLayout),
                [nameof(LayoutView.ChildContent)] = (RenderFragment)(builder =>
                {
                    builder.OpenComponent<Home>(0);
                    builder.CloseComponent();
                }),
            });

            var output = await renderer.RenderComponentAsync<LayoutView>(parameters);
            return output.ToHtmlString();
        });
    }

    public static string BuildJsonLd(Profile profile, Uri site)
    {
        var current = profile.ExperienceNewestFirst.First(role => role.IsCurrent);

        var person = new JsonObject
        {
            ["@type"] = "Person",
            ["@id"] = new Uri(site, "#person").ToString(),
            ["name"] = profile.Name,
            ["url"] = site.ToString(),
            ["image"] = new Uri(site, "img/jeremy-moore.webp").ToString(),
            ["jobTitle"] = profile.Title,
            ["description"] = profile.Summary,
            ["worksFor"] = new JsonObject { ["@type"] = "Organization", ["name"] = current.Company },
            ["address"] = new JsonObject
            {
                ["@type"] = "PostalAddress",
                ["addressLocality"] = "Santa Rosa",
                ["addressRegion"] = "CA",
                ["addressCountry"] = "US",
            },
            ["alumniOf"] = new JsonArray(profile.Education
                .Select(degree => (JsonNode)new JsonObject { ["@type"] = "CollegeOrUniversity", ["name"] = degree.School })
                .ToArray()),
            ["knowsAbout"] = new JsonArray(profile.Skills
                .SelectMany(group => group.Items)
                .Distinct()
                .Select(item => (JsonNode)item)
                .ToArray()),
            ["sameAs"] = new JsonArray(profile.Contact
                .Where(link => link.Href.StartsWith("https://", StringComparison.Ordinal))
                .Select(link => (JsonNode)link.Href)
                .ToArray()),
        };

        if (profile.Contact.FirstOrDefault(link => link.Href.StartsWith("mailto:", StringComparison.Ordinal)) is { } email)
        {
            person["email"] = email.Href;
        }

        var website = new JsonObject
        {
            ["@type"] = "WebSite",
            ["@id"] = new Uri(site, "#website").ToString(),
            ["url"] = site.ToString(),
            ["name"] = profile.Name,
            ["publisher"] = new JsonObject { ["@id"] = new Uri(site, "#person").ToString() },
        };

        var graph = new JsonObject
        {
            ["@context"] = "https://schema.org",
            ["@graph"] = new JsonArray(person, website),
        };

        // Keep "</script>" from ever closing the tag early.
        var json = graph.ToJsonString(new JsonSerializerOptions { WriteIndented = false }).Replace("</", "<\\/");
        return $"<script type=\"application/ld+json\">{json}</script>";
    }

    public static string BuildSitemap(Uri site, DateOnly lastModified) =>
        $"""
        <?xml version="1.0" encoding="UTF-8"?>
        <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
          <url>
            <loc>{site}</loc>
            <lastmod>{lastModified:yyyy-MM-dd}</lastmod>
          </url>
        </urlset>

        """;

    public static string Inject(string indexHtml, string appHtml, string jsonLd)
    {
        var start = indexHtml.IndexOf(AppStart, StringComparison.Ordinal);
        var end = indexHtml.IndexOf(AppEnd, StringComparison.Ordinal);
        if (start < 0 || end < start)
        {
            throw new InvalidOperationException($"index.html is missing the {AppStart} … {AppEnd} markers.");
        }

        if (!indexHtml.Contains(JsonLdMarker, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"index.html is missing the {JsonLdMarker} marker.");
        }

        var html = string.Concat(indexHtml.AsSpan(0, start), appHtml, indexHtml.AsSpan(end + AppEnd.Length));
        return html.Replace(JsonLdMarker, jsonLd, StringComparison.Ordinal);
    }
}
