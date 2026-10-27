# jeremymmoore.com

Personal site for Jeremy M. Moore, built in C# with **Blazor WebAssembly on .NET 10**.

## Develop

```bash
dotnet watch --project src/JeremyMoore.Site   # http://localhost:5039, hot reload
dotnet test                                   # xUnit + bUnit
```

## Layout

| Path | What |
|------|------|
| `src/JeremyMoore.Site/Content/ProfileContent.cs` | All site copy, as one immutable `Profile` record |
| `src/JeremyMoore.Site/Models/Profile.cs` | Content model (`Profile`, `Role`, `SkillGroup`, …) |
| `src/JeremyMoore.Site/Components/` | Page sections (`Hero`, `ExperienceList`, …) |
| `src/JeremyMoore.Site/wwwroot/css/app.css` | Styles; light/dark tokens at the top |
| `tools/JeremyMoore.Prerender/` | Build step: renders the home page to static HTML, adds JSON-LD and `sitemap.xml` |
| `tests/JeremyMoore.Site.Tests/` | Model, component and prerender tests |

To change what the site says, edit `ProfileContent.cs`. Components only read from the injected `Profile`.

## Deploy

Every push to `main` runs the tests, publishes, prerenders, and deploys to Azure Static Web Apps (`.github/workflows/deploy.yml`).
Prerendering means crawlers and link previews get the full page as HTML; Blazor takes over when the runtime loads.
To try it locally: `dotnet publish src/JeremyMoore.Site -c Release -o publish && dotnet run --project tools/JeremyMoore.Prerender -- publish/wwwroot`.
Pull requests get a preview URL. The workflow needs the repo secret `AZURE_STATIC_WEB_APPS_API_TOKEN`.
