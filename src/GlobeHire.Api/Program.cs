using System.Globalization;
using GlobeHire.Api.Errors;
using GlobeHire.Api.WorkforcePlans;
using GlobeHire.Application.Abstractions;
using GlobeHire.Application.WorkforcePlans;
using GlobeHire.Infrastructure.Identifiers;
using GlobeHire.Infrastructure.Time;
using GlobeHire.Infrastructure.WorkforcePlans;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<IIdGenerator, GuidIdGenerator>();
builder.Services.AddSingleton<InMemoryWorkforcePlanRepository>();
builder.Services.AddSingleton<IWorkforcePlanRepository>(services =>
    services.GetRequiredService<InMemoryWorkforcePlanRepository>());
builder.Services.AddSingleton(_ => new WorkforcePlanPricingOptions
{
    MonthlyServiceFee = ReadMonthlyServiceFee(builder.Configuration),
});
builder.Services.AddScoped<CreateWorkforcePlanHandler>();
builder.Services.AddScoped<GetWorkforcePlanHandler>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    app.MapOpenApi();
}

app.MapGet("/health/live", () => Results.Ok(new { status = "healthy" }))
    .WithName("Liveness")
    .ExcludeFromDescription();
app.MapWorkforcePlanEndpoints();

app.Run();

static decimal ReadMonthlyServiceFee(IConfiguration configuration)
{
    var configured = configuration[$"{WorkforcePlanPricingOptions.SectionName}:MonthlyServiceFee"];
    return decimal.TryParse(configured, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
        ? value
        : 500m;
}

public partial class Program;
