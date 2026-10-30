using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using JeremyMoore.Site.Layout;
using JeremyMoore.Site.Models;
using JeremyMoore.Site.Pages;

namespace JeremyMoore.Prerender;

/// <summary>
/// Turns the published index.html into crawlable pages: the home page and each case study rendered to
/// static HTML with their own title and description, JSON-LD built from the same <see cref="Profile"/>,
/// a sitemap, and the routes that serve them. Blazor replaces the prerendered markup when the runtime
/// boots, so both always come from one source.
/// </summary>
public static class StaticSite
{
    public const string AppStart = "<!-- prerender:app -->";
    public const string AppEnd = "<!-- /prerender:app -->";
    public const string JsonLdMarker = "<!-- prerender:jsonld -->";

    public static Task<string> RenderHomeAsync(Profile profile, TimeProvider clock) =>
        RenderPageAsync<Home>(profile, clock, pageParameters: null);

    public static Task<string> RenderCaseStudyAsync(Profile profile, TimeProvider clock, CaseStudy study) =>
        RenderPageAsync<CaseStudyPage>(profile, clock, new Dictionary<string, object?> { [nameof(CaseStudyPage.Slug)] = study.Slug });

    private static async Task<string> RenderPageAsync<TPage>(
        Profile profile, TimeProvider clock, IReadOnlyDictionary<string, object?>? pageParameters)
        where TPage : IComponent
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(profile)
            .AddSingleton(clock)
            .AddSingleton<IJSRuntime, NoBrowser>()
            .BuildServiceProvider();

        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());

        return await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(LayoutView.Layout)] = typeof(MainLayout),
                [nameof(LayoutView.ChildContent)] = (RenderFragment)(builder =>
                {
                    builder.OpenComponent<TPage>(0);
                    foreach (var (name, value) in pageParameters ?? new Dictionary<string, object?>())
                    {
                        builder.AddComponentParameter(1, name, value);
                    }

                    builder.CloseComponent();
                }),
            });

            var output = await renderer.RenderComponentAsync<LayoutView>(parameters);
            return output.ToHtmlString();
        });
    }

    /// <summary>The home page gets the FAQ; a case study page gets an Article written by the same Person.</summary>
    public static string BuildJsonLd(Profile profile, Uri site, CaseStudy? study = null)
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

        // FAQ rich results are limited on Google now, but answer engines still read FAQPage markup.
        var faq = new JsonObject
        {
            ["@type"] = "FAQPage",
            ["@id"] = new Uri(site, "#faq").ToString(),
            ["mainEntity"] = new JsonArray(profile.Faq
                .Select(item => (JsonNode)new JsonObject
                {
                    ["@type"] = "Question",
                    ["name"] = item.Question,
                    ["acceptedAnswer"] = new JsonObject { ["@type"] = "Answer", ["text"] = item.Answer },
                })
                .ToArray()),
        };

        var page = study is null
            ? faq
            : new JsonObject
            {
                ["@type"] = "Article",
                ["@id"] = new Uri(site, $"{study.Path}#article").ToString(),
                ["url"] = new Uri(site, study.Path).ToString(),
                ["headline"] = study.Title,
                ["description"] = study.Summary,
                ["author"] = new JsonObject { ["@id"] = new Uri(site, "#person").ToString() },
                ["about"] = new JsonArray(study.Stack.Select(tech => (JsonNode)tech).ToArray()),
            };

        var graph = new JsonObject
        {
            ["@context"] = "https://schema.org",
            ["@graph"] = new JsonArray(person, website, page),
        };

        // Keep "</script>" from ever closing the tag early.
        var json = graph.ToJsonString(new JsonSerializerOptions { WriteIndented = false }).Replace("</", "<\\/");
        return $"<script type=\"application/ld+json\">{json}</script>";
    }

    /// <summary>Every page, relative to the site root ("" is the home page).</summary>
    public static string BuildSitemap(Uri site, DateOnly lastModified, IEnumerable<string> paths)
    {
        var urls = string.Concat(paths.Select(path => $"""
              <url>
                <loc>{new Uri(site, path)}</loc>
                <lastmod>{lastModified:yyyy-MM-dd}</lastmod>
              </url>

            """));

        return $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
            {urls}</urlset>

            """;
    }

    /// <summary>Gives a prerendered copy of index.html its own title, description and canonical URL.</summary>
    public static string WithMeta(string html, string title, string description, Uri url)
    {
        var encodedTitle = System.Net.WebUtility.HtmlEncode(title);
        var encodedDescription = System.Net.WebUtility.HtmlEncode(description);

        html = Regex.Replace(html, "<title>.*?</title>", $"<title>{encodedTitle}</title>");
        html = ReplaceContent(html, "name=\"description\"", encodedDescription);
        html = ReplaceContent(html, "property=\"og:title\"", encodedTitle);
        html = ReplaceContent(html, "property=\"og:description\"", encodedDescription);
        html = ReplaceContent(html, "property=\"og:url\"", url.ToString());
        html = Regex.Replace(html, "<link rel=\"canonical\" href=\"[^\"]*\"", $"<link rel=\"canonical\" href=\"{url}\"");
        return html;

        static string ReplaceContent(string html, string attribute, string value) =>
            Regex.Replace(html, $"(<meta {Regex.Escape(attribute)} content=\")[^\"]*(\")", match => match.Groups[1].Value + value + match.Groups[2].Value);
    }

    /// <summary>Clean URLs: /work/{slug} is served from the prerendered /work/{slug}.html.</summary>
    public static string WithCaseStudyRoutes(string staticWebAppConfig, IEnumerable<CaseStudy> studies)
    {
        var config = JsonNode.Parse(staticWebAppConfig)!.AsObject();
        var routes = config["routes"] as JsonArray ?? [];
        foreach (var study in studies)
        {
            routes.Add(new JsonObject { ["route"] = $"/{study.Path}", ["rewrite"] = $"/{study.Path}.html" });
        }

        config["routes"] = routes;
        return config.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
    }

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

    /// <summary>Prerendering has no browser; components only touch JS after render, which never happens here.</summary>
    private sealed class NoBrowser : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            throw new InvalidOperationException("JavaScript isn't available while prerendering.");

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) =>
            throw new InvalidOperationException("JavaScript isn't available while prerendering.");
    }
}
