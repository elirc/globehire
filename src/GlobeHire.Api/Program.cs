using GlobeHire.Api.Errors;
using GlobeHire.Api.HireCases;
using GlobeHire.Api.WorkforcePlans;
using GlobeHire.Application.Abstractions;
using GlobeHire.Application.HireCases;
using GlobeHire.Application.WorkforcePlans;
using GlobeHire.Infrastructure.Identifiers;
using GlobeHire.Infrastructure.HireCases;
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
builder.Services.AddSingleton<ICountryPolicyRepository, InMemoryCountryPolicyRepository>();
builder.Services.AddSingleton<IHireCaseRepository, InMemoryHireCaseRepository>();
builder.Services.AddScoped<CreateWorkforcePlanHandler>();
builder.Services.AddScoped<GetWorkforcePlanHandler>();
builder.Services.AddScoped<OpenHireCaseHandler>();
builder.Services.AddScoped<ChangeHireCaseHandler>();

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
app.MapHireCaseEndpoints();

app.Run();

public partial class Program;
