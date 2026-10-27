using Bunit;
using JeremyMoore.Site.Components;
using JeremyMoore.Site.Content;
using JeremyMoore.Site.Models;
using JeremyMoore.Site.Pages;
using Microsoft.Extensions.DependencyInjection;

namespace JeremyMoore.Site.Tests;

public class ComponentTests : BunitContext
{
    public ComponentTests()
    {
        Services.AddSingleton<TimeProvider>(new FixedClock(new DateTimeOffset(2026, 6, 1, 12, 0, 0, TimeSpan.Zero)));
        Services.AddSingleton(ProfileContent.Jeremy);
    }

    [Fact]
    public void Hero_renders_name_and_computed_years()
    {
        var hero = Render<Hero>();

        hero.Find("h1").MarkupMatches("<h1 class=\"hero-name\">Jeremy M. Moore</h1>");
        Assert.Contains("Years  = <span class=\"tk-num\">24</span>", hero.Markup);
    }

    [Fact]
    public void Experience_list_marks_only_the_current_role()
    {
        var list = Render<ExperienceList>(p => p.Add(c => c.Roles, ProfileContent.Jeremy.ExperienceNewestFirst));

        var current = Assert.Single(list.FindAll("li.role.is-current"));
        Assert.Contains("Present", current.TextContent);
    }

    [Fact]
    public void Contact_links_open_external_sites_in_new_tab_but_not_mailto()
    {
        var contact = Render<ContactLinks>(p => p.Add(c => c.Links, ProfileContent.Jeremy.Contact));

        var anchors = contact.FindAll("a");
        Assert.Null(anchors.Single(a => a.GetAttribute("href")!.StartsWith("mailto:")).GetAttribute("target"));
        Assert.All(anchors.Where(a => a.GetAttribute("href")!.StartsWith("http")),
            a => Assert.Equal("noopener", a.GetAttribute("rel")));
    }

    [Fact]
    public void Home_renders_every_section_in_order()
    {
        var home = Render<Home>();

        var ids = home.FindAll("h2.section-label").Select(h => h.Id);
        Assert.Equal(
            ["section-about", "section-experience", "section-skills", "section-education", "section-projects", "section-contact"],
            ids);
    }
}
