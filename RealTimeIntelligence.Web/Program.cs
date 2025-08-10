using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using RealTimeIntelligence.Web.Services;
using RealTimeIntelligence.Web.Hubs;
using Vision;

var builder = WebApplication.CreateBuilder(args);
string? apiKey = builder.Configuration["Api:Key"] ?? "dev-key";

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor().AddCircuitOptions(o=>
{
    o.DetailedErrors = true;
});
builder.Services.AddSignalR();

// Enregistrer nos services dans le bon ordre
builder.Services.AddSingleton<ActivityLogService>();
builder.Services.AddSingleton<IScreenCaptureService, ScreenCaptureService>();
builder.Services.AddSingleton<IOcrService, OcrService>();
builder.Services.AddSingleton<IMicrophoneService, MicrophoneService>();
builder.Services.AddSingleton<IAIAnalysisService, AIAnalysisService>();
builder.Services.AddSingleton<IRealTimeCoordinator, RealTimeCoordinator>();
builder.Services.AddScoped<IntelligenceService>();
// TEMP disable to debug early shutdown
// builder.Services.AddHostedService<ScreenCaptureHostedService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapHub<ActivityHub>("/activityhub");
app.MapHub<RealTimeHub>("/realtime");
// Simple API to change mask style at runtime
app.MapPost("/api/vision/mask", (IScreenCaptureService svc, HttpRequest req) =>
{
    if(!ValidateKey(req)) return Results.Unauthorized();
    if (!req.Query.ContainsKey("style")) return Results.BadRequest("style missing");
    var styleStr = req.Query["style"].ToString();
    if (!Enum.TryParse<MaskStyle>(styleStr, true, out var style)) return Results.BadRequest("invalid style");
    int blur = 8;
    if (req.Query.ContainsKey("blur") && int.TryParse(req.Query["blur"], out var b)) blur = b;
    svc.ConfigureMask(style, blur);
    return Results.Ok(new { applied = style.ToString(), blur });
});

app.MapPost("/api/vision/tune", (HttpRequest req, ScreenCaptureHostedService hosted) =>
{
    if(!ValidateKey(req)) return Results.Unauthorized();
    double? change = req.Query.ContainsKey("change") && double.TryParse(req.Query["change"], out var c) ? c : null;
    bool? jpeg = req.Query.ContainsKey("jpeg") && bool.TryParse(req.Query["jpeg"], out var j) ? j : null;
    int? quality = req.Query.ContainsKey("quality") && int.TryParse(req.Query["quality"], out var q) ? q : null;
    hosted.Tune(change, jpeg, quality);
    return Results.Ok(new { change, jpeg, quality });
});

bool ValidateKey(HttpRequest req)
{
    if (!req.Headers.TryGetValue("X-API-KEY", out var provided)) return false;
    return string.Equals(provided.ToString(), apiKey, StringComparison.Ordinal);
}
app.MapGet("/healthz", () => Results.Ok(new { ok = true, time = DateTime.UtcNow }));
app.MapFallbackToPage("/_Host");

app.Run();
