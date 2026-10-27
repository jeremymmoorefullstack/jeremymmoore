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
| `tests/JeremyMoore.Site.Tests/` | Model and component tests |

To change what the site says, edit `ProfileContent.cs`. Components only read from the injected `Profile`.

## Deploy

Every push to `main` runs the tests, publishes, and deploys to Azure Static Web Apps (`.github/workflows/deploy.yml`).
Pull requests get a preview URL. The workflow needs the repo secret `AZURE_STATIC_WEB_APPS_API_TOKEN`.
