using GlobeHire.Api.Errors;
using GlobeHire.Api.Employments;
using GlobeHire.Api.EmploymentLifecycle;
using GlobeHire.Api.Contractors;
using GlobeHire.Api.Compliance;
using GlobeHire.Api.BulkData;
using GlobeHire.Api.HireCases;
using GlobeHire.Api.Identity;
using GlobeHire.Api.Integrations;
using GlobeHire.Api.Offers;
using GlobeHire.Api.Payroll;
using GlobeHire.Api.PeopleOperations;
using GlobeHire.Api.WorkforcePlans;
using GlobeHire.Application.Abstractions;
using GlobeHire.Application.Employments;
using GlobeHire.Application.HireCases;
using GlobeHire.Application.Offers;
using GlobeHire.Application.WorkforcePlans;
using GlobeHire.Infrastructure.Identifiers;
using GlobeHire.Infrastructure.Employments;
using GlobeHire.Infrastructure.HireCases;
using GlobeHire.Infrastructure.Offers;
using GlobeHire.Infrastructure.Time;
using GlobeHire.Infrastructure.WorkforcePlans;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IRequestActor, GatewayRequestActor>();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<IIdGenerator, GuidIdGenerator>();
builder.Services.AddSingleton<InMemoryWorkforcePlanRepository>();
builder.Services.AddSingleton<IWorkforcePlanRepository>(services =>
    services.GetRequiredService<InMemoryWorkforcePlanRepository>());
builder.Services.AddSingleton<ICountryPolicyRepository, InMemoryCountryPolicyRepository>();
builder.Services.AddSingleton<IHireCaseRepository, InMemoryHireCaseRepository>();
builder.Services.AddSingleton<IOfferRepository, InMemoryOfferRepository>();
builder.Services.AddSingleton<IEmploymentRepository, InMemoryEmploymentRepository>();
// Use-case handlers hold no per-request state; singletons avoid six allocations per request.
builder.Services.AddSingleton<CreateWorkforcePlanHandler>();
builder.Services.AddSingleton<GetWorkforcePlanHandler>();
builder.Services.AddSingleton<OpenHireCaseHandler>();
builder.Services.AddSingleton<ChangeHireCaseHandler>();
builder.Services.AddSingleton<OfferHandler>();
builder.Services.AddSingleton<ActivateEmploymentHandler>();

var app = builder.Build();

app.UseExceptionHandler();
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Trace-Id"] = context.TraceIdentifier;
    await next(context).ConfigureAwait(false);
});

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    app.MapOpenApi();
}

app.MapGet("/health/live", () => Results.Ok(new { status = "healthy" }))
    .WithName("Liveness")
    .ExcludeFromDescription();
app.MapGet("/health/ready", () => Results.Ok(new
{
    status = "ready",
    storage = "in-memory",
    durable = false,
}))
    .WithName("Readiness")
    .ExcludeFromDescription();
app.MapWorkforcePlanEndpoints();
app.MapHireCaseEndpoints();
app.MapOfferEndpoints();
app.MapEmploymentEndpoints();
app.MapPayrollEndpoints();
app.MapPeopleOperationsEndpoints();
app.MapContractorEndpoints();
app.MapEmploymentLifecycleEndpoints();
app.MapIntegrationEndpoints();
app.MapBulkDataEndpoints();
app.MapComplianceEndpoints();

app.Run();

public partial class Program;
