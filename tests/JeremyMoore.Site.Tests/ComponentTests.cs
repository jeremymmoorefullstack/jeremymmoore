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
        JSInterop.SetupModule("./js/agent.js").Setup<bool>("available").SetResult(false);
    }

    [Fact]
    public void Hero_renders_headline_pitch_and_computed_years()
    {
        var hero = Render<Hero>();

        Assert.Equal(ProfileContent.Jeremy.Headline, hero.Find("h1.hero-headline").TextContent);
        Assert.StartsWith("24 years shipping software, from Classic ASP to .NET 10.", hero.Find(".hero-pitch").TextContent);
        Assert.NotNull(hero.Find("img.hero-photo"));
        Assert.Contains("Years  = <span class=\"tk-num\">24</span>", hero.Markup);
        Assert.Equal("#ask", hero.Find("a.btn-primary").GetAttribute("href"));
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
            ["section-about", "section-experience", "section-skills", "section-education", "section-projects", "section-faq", "section-contact"],
            ids);
    }

    [Fact]
    public void Ask_panel_is_disabled_until_the_api_says_the_agent_is_available()
    {
        var offline = Render<AskMyAi>();
        Assert.True(offline.Find("#ask-input").HasAttribute("disabled"));
        Assert.Contains("offline", offline.Find(".ask-note").TextContent);

        JSInterop.SetupModule("./js/agent.js").Setup<bool>("available").SetResult(true);
        var live = Render<AskMyAi>();
        Assert.False(live.Find("#ask-input").HasAttribute("disabled"));
        Assert.Equal(4, live.FindAll(".ask-chips button:not([disabled])").Count);
    }

    [Fact]
    public void Starting_a_call_shows_an_unmissable_connecting_banner()
    {
        var module = JSInterop.SetupModule("./js/agent.js");
        module.Setup<bool>("available").SetResult(true);
        module.Setup<string>("startVoice", _ => true); // never completes: the call stays "connecting"

        var panel = Render<AskMyAi>();
        panel.Find("button.ask-mic").Click();

        Assert.Contains("Connecting your call", panel.Find(".ask-call.is-connecting").TextContent);
        Assert.Contains("Connecting", panel.Find("button.ask-mic").TextContent);
    }
}
