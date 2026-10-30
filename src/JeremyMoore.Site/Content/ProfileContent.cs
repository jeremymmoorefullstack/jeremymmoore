using JeremyMoore.Site.Models;

namespace JeremyMoore.Site.Content;

/// <summary>Site copy. Edit here; every component reads from this one object.</summary>
public static class ProfileContent
{
    public static Profile Jeremy { get; } = new(
        Name: "Jeremy M. Moore",
        Title: "Senior Full-Stack Engineer · C# / .NET",
        Headline: "I engineer software built to last, from .NET APIs to AI agents.",
        Pitch: "{0} years shipping software, from Classic ASP to .NET 10. Clean, tested C#, cloud services " +
               "on Azure, and AI that does real work inside real systems.",
        Availability: "Available for contract & part-time",
        Location: "Santa Rosa, Sonoma County · SF Bay Area · Remote",
        Summary:
            "I build dependable software on .NET: APIs, services, and the web apps in front of them. " +
            "My career runs from Classic ASP to .NET 10, including six years on HIPAA-regulated " +
            "healthcare EDI. I care about clean, maintainable code, and I've worked remotely for most of my career.",
        Experience:
        [
            new Role(
                Title: "Senior Full-Stack Engineer",
                Company: "Dolooma",
                Start: 2024,
                End: null,
                Summary: "Towing and roadside-assistance platform: client and product sites, APIs, AI phone agents, and the pipelines that ship them.",
                Highlights:
                [
                    "Built an AI phone dispatcher on 800.com that answers roadside calls 24/7 and turns them into jobs through webhooks and live-lookup tools.",
                    "Automated handoffs and alerts with n8n workflows: voice-to-job, SMS and email notifications, and pipeline failure alerts.",
                    "Run a config-driven Next.js platform behind nearly twenty client and product sites.",
                    "Lead technical SEO: JSON-LD, sitemaps and IndexNow, Google Business Profile, GA4 and Search Console, plus an internal dashboard that syncs listings and builds client reports.",
                    "Write Azure Functions APIs on Node.js and MongoDB with Bicep infrastructure, and run Application Insights with alerts across every production site.",
                ],
                Stack: ["JavaScript", "Next.js", "Node.js", "Azure Functions", "MongoDB", "n8n", "800.com"]),
            new Role(
                Title: "Specialist Application Engineer",
                Company: "Waystar",
                Start: 2018,
                End: 2024,
                Summary: "Healthcare revenue-cycle software processing EDI transactions under HIPAA and HITECH.",
                Highlights:
                [
                    "Designed new .NET APIs and standalone utilities for enterprise healthcare clients, most recently on .NET 8.",
                    "Refactored legacy WebForms and MVC code into modern C# using standard design patterns.",
                    "Ran CI/CD on Azure DevOps and Octopus Deploy, and upgraded aging Windows Server 2012 environments.",
                ],
                Stack: ["C#", ".NET Framework", ".NET 8", "ASP.NET MVC", "WebForms", "SQL Server", "Azure DevOps"]),
            new Role(
                Title: "Full-Stack Developer",
                Company: "ProActive Webs",
                Start: 2009,
                End: 2018,
                Summary: "Custom web applications, CMS platforms, and hosting for small and mid-size businesses.",
                Highlights:
                [
                    "Built ASP.NET MVC apps with PayPal REST payments and social sign-in.",
                    "Moved client sites to AWS, which cut hosting costs and improved uptime.",
                ],
                Stack: ["C#", "ASP.NET MVC", "SQL Server", "MongoDB", "AWS"]),
            new Role(
                Title: "Software Engineer",
                Company: "FunctionalityPlus",
                Start: 2004,
                End: 2009,
                Summary: "Content management systems for the hospitality industry.",
                Highlights:
                [
                    "Wrote one of only two codebases IHG approved, used at Holiday Inn, Staybridge, and Candlewood.",
                ],
                Stack: ["Classic ASP", "VBScript", "SQL Server", "JavaScript"]),
            new Role(
                Title: "Contract Programmer",
                Company: "ESP Interactive Solutions",
                Start: 2002,
                End: 2015,
                Summary: "Turned static designs into data-driven ASP.NET applications with custom CMS tooling.",
                Highlights:
                [
                    "Built secure payment-gateway integrations and data layers on SQL Server.",
                ],
                Stack: ["C#", "ASP.NET", "WebForms", "SQL Server"]),
        ],
        Projects:
        [
            new Project(
                Name: "jeremymmoore.com",
                Language: "C#",
                Description: "This site. Blazor WebAssembly on .NET 10, with immutable records, DI, and bUnit tests.",
                Href: "https://github.com/jeremymmoorefullstack/jeremymmoore"),
        ],
        Skills:
        [
            new SkillGroup(
                "C# / .NET",
                ["C#", ".NET 8–10", "ASP.NET Core", "Web API", "Blazor", "Entity Framework", "MVC", "WebForms", "xUnit"],
                Primary: true),
            new SkillGroup("Data", ["SQL Server", "T-SQL", "MongoDB", "Healthcare EDI (X12)"]),
            new SkillGroup("Cloud & Delivery", ["Azure", "Azure Functions", "Bicep", "Application Insights", "Azure DevOps", "GitHub Actions", "Octopus Deploy", "AWS"]),
            new SkillGroup("JavaScript", ["React", "Next.js", "Node.js", "Vite", "Playwright"]),
            new SkillGroup("AI & Automation", ["800.com voice agents", "n8n", "MCP tools", "Webhooks", "Twilio", "Claude Code"]),
            new SkillGroup("SEO & Analytics", ["Technical SEO", "JSON-LD", "Google Business Profile", "GA4", "Search Console", "IndexNow"]),
        ],
        Education:
        [
            new Degree("Bachelor of Science", "Software Development", "Western Governors University", 2018),
        ],
        Faq:
        [
            new FaqItem(
                "Is Jeremy available for contract or part-time work?",
                "Yes. He takes contract and part-time work, up to about 30 hours a week. He's remote-first from Santa Rosa, " +
                "and on-site work is possible in Santa Rosa, Rohnert Park and Windsor. He isn't looking for full-time roles."),
            new FaqItem(
                "What does Jeremy build with C# and .NET?",
                "APIs, services and the web apps in front of them: new .NET APIs and standalone utilities, and refactors of " +
                "legacy WebForms and MVC code into modern C#. This site is Blazor WebAssembly on .NET 10."),
            new FaqItem(
                "Does Jeremy build AI voice agents and automations?",
                "Yes. At Dolooma he built an AI phone dispatcher on 800.com that answers roadside calls around the clock and " +
                "turns them into jobs through webhooks and live-lookup tools, and he automates handoffs and alerts with n8n. " +
                "The agent on this page is his too: ElevenLabs for voice, with a C# backend on Azure Functions."),
            new FaqItem(
                "Does Jeremy have healthcare or HIPAA experience?",
                "Yes. He spent six years at Waystar on healthcare revenue-cycle software that processes EDI transactions " +
                "under HIPAA and HITECH."),
            new FaqItem(
                "Which databases does Jeremy work with?",
                "Mainly SQL Server, which he has used since 2002, including T-SQL and stored procedures. He also works with MongoDB."),
            new FaqItem(
                "Where is Jeremy based?",
                "Santa Rosa, in Sonoma County, California, part of the SF Bay Area. He works remotely with teams across US time zones."),
            new FaqItem(
                "How do I get in touch with Jeremy?",
                "Email jeremy@jeremymmoore.com, or ask the AI agent at the top of this page to pass him a message."),
        ],
        Contact:
        [
            new ContactLink("Email", "jeremy@jeremymmoore.com", "mailto:jeremy@jeremymmoore.com"),
            new ContactLink("LinkedIn", "in/jeremymmoorefullstack", "https://www.linkedin.com/in/jeremymmoorefullstack/"),
            new ContactLink("GitHub", "jeremymmoorefullstack", "https://github.com/jeremymmoorefullstack"),
        ]);
}
