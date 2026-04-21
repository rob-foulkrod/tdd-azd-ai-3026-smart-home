using Azure.AI.Projects;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using SmartHome.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry().UseAzureMonitor();

builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var projectEndpoint = builder.Configuration["PROJECT_ENDPOINT"]
    ?? throw new InvalidOperationException("PROJECT_ENDPOINT not configured");

var projectClient = new AIProjectClient(new Uri(projectEndpoint), new DefaultAzureCredential());
builder.Services.AddSingleton(projectClient);
builder.Services.AddSingleton<SmartHomeToolService>();
builder.Services.AddSingleton<AgentService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.MapRazorPages();

try
{
    var agentService = app.Services.GetRequiredService<AgentService>();
    await agentService.EnsureAgentAsync();
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "Agent initialization failed at startup — will retry on first request");
}

app.Run();
