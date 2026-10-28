using JeremyMoore.Api.Agent;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.Services.Configure<AgentOptions>(builder.Configuration.GetSection(AgentOptions.Section));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<SessionLimiter>();
builder.Services.AddHttpClient<ElevenLabsClient>(http => http.BaseAddress = new Uri("https://api.elevenlabs.io/"));
builder.Services.AddHttpClient<IMessageSender, ResendMessageSender>(http => http.BaseAddress = new Uri("https://api.resend.com/"));

builder.Build().Run();
