using JeremyMoore.Site.Models;

namespace JeremyMoore.Site.Content;

/// <summary>Site copy. Edit here; every component reads from this one object.</summary>
public static class ProfileContent
{
    public static Profile Jeremy { get; } = new(
        Name: "Jeremy M. Moore",
        Title: "Senior Full-Stack Engineer · C# / .NET",
        Location: "Santa Rosa, Sonoma County · SF Bay Area · Remote",
        Summary:
            "I build dependable software on .NET: APIs, services, and the web apps in front of them. " +
            "My career runs from Classic ASP to .NET 8, including six years on HIPAA-regulated " +
            "healthcare EDI. I care about clean, maintainable code and have worked remotely the whole way.",
        Experience:
        [
            new Role(
                Title: "Senior Full-Stack Engineer",
                Company: "Dolooma",
                Start: 2024,
                End: null,
                Summary: "Towing and roadside-assistance platform: client brand sites, APIs, and the pipelines that ship them.",
                Highlights:
                [
                    "Built a config-driven Next.js platform that runs nine client brand sites from one template.",
                    "Wrote Azure Functions APIs on Node.js and MongoDB with database-per-tenant isolation.",
                    "Own CI/CD and observability: GitHub Actions to Azure, with Application Insights across every site.",
                ],
                Stack: ["JavaScript", "React", "Next.js", "Node.js", "Azure Functions", "MongoDB"]),
            new Role(
                Title: "Specialist Application Engineer",
                Company: "Waystar",
                Start: 2018,
                End: 2024,
                Summary: "Healthcare revenue-cycle software processing EDI transactions under HIPAA and HITECH.",
                Highlights:
                [
                    "Designed new .NET 8 APIs and standalone utilities for enterprise healthcare clients.",
                    "Refactored legacy WebForms and MVC code into modern C# using standard design patterns.",
                    "Ran CI/CD on Azure DevOps and Octopus Deploy, and upgraded aging Windows Server 2012 environments.",
                ],
                Stack: ["C#", ".NET 8", "ASP.NET MVC", "WebForms", "SQL Server", "Azure DevOps"]),
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
            new SkillGroup("Cloud & Delivery", ["Azure", "Azure Functions", "Azure DevOps", "GitHub Actions", "Octopus Deploy", "AWS"]),
            new SkillGroup("JavaScript", ["React", "Next.js", "Node.js", "Vite"]),
        ],
        Education:
        [
            new Degree("Bachelor of Science", "Software Development", "Western Governors University", 2018),
        ],
        Contact:
        [
            new ContactLink("Email", "jeremy@jeremymmoore.com", "mailto:jeremy@jeremymmoore.com"),
            new ContactLink("LinkedIn", "in/jeremymmoorefullstack", "https://www.linkedin.com/in/jeremymmoorefullstack/"),
            new ContactLink("GitHub", "jeremymmoorefullstack", "https://github.com/jeremymmoorefullstack"),
        ]);
}
