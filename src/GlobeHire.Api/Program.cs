using GlobeHire.Api.Errors;
using GlobeHire.Api.Employments;
using GlobeHire.Api.EmploymentLifecycle;
using GlobeHire.Api.Contractors;
using GlobeHire.Api.HireCases;
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

builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<IIdGenerator, GuidIdGenerator>();
builder.Services.AddSingleton<InMemoryWorkforcePlanRepository>();
builder.Services.AddSingleton<IWorkforcePlanRepository>(services =>
    services.GetRequiredService<InMemoryWorkforcePlanRepository>());
builder.Services.AddSingleton<ICountryPolicyRepository, InMemoryCountryPolicyRepository>();
builder.Services.AddSingleton<IHireCaseRepository, InMemoryHireCaseRepository>();
builder.Services.AddSingleton<IOfferRepository, InMemoryOfferRepository>();
builder.Services.AddSingleton<IEmploymentRepository, InMemoryEmploymentRepository>();
builder.Services.AddScoped<CreateWorkforcePlanHandler>();
builder.Services.AddScoped<GetWorkforcePlanHandler>();
builder.Services.AddScoped<OpenHireCaseHandler>();
builder.Services.AddScoped<ChangeHireCaseHandler>();
builder.Services.AddScoped<OfferHandler>();
builder.Services.AddScoped<ActivateEmploymentHandler>();

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
app.MapOfferEndpoints();
app.MapEmploymentEndpoints();
app.MapPayrollEndpoints();
app.MapPeopleOperationsEndpoints();
app.MapContractorEndpoints();
app.MapEmploymentLifecycleEndpoints();

app.Run();

public partial class Program;
