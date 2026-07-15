using System.Net;
using System.Net.Http.Json;
using GlobeHire.Contracts.V1.HireCases;
using GlobeHire.Contracts.V1.WorkforcePlans;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GlobeHire.Api.Tests.HireCases;

public sealed class HireCaseApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HireCaseApiTests(WebApplicationFactory<Program> factory) =>
        _client = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Testing")).CreateClient();

    [Fact]
    public async Task OnboardingFlow_CompletesChecklistThenSubmits()
    {
        var clientId = $"client-{Guid.NewGuid():N}";
        var plan = await CreatePlanAsync(clientId);
        var openResponse = await _client.PostAsJsonAsync(
            $"/api/v1/clients/{clientId}/hire-cases",
            new OpenHireCaseRequest(plan.Id, "Ada Lovelace", "Ada@Example.com"));
        var hireCase = await openResponse.Content.ReadFromJsonAsync<HireCaseResponse>();

        Assert.Equal(HttpStatusCode.Created, openResponse.StatusCode);
        Assert.NotNull(hireCase);
        Assert.Equal("ada@example.com", hireCase.CandidateEmail);

        foreach (var task in hireCase.Tasks)
        {
            using var complete = new HttpRequestMessage(
                HttpMethod.Put,
                $"/api/v1/clients/{clientId}/hire-cases/{hireCase.Id:D}/tasks/{task.Code}/complete");
            complete.Headers.TryAddWithoutValidation("If-Match", $"\"{hireCase.Version}\"");
            var response = await _client.SendAsync(complete);
            hireCase = await response.Content.ReadFromJsonAsync<HireCaseResponse>();
            Assert.NotNull(hireCase);
        }

        using var submit = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/v1/clients/{clientId}/hire-cases/{hireCase.Id:D}/submit");
        submit.Headers.TryAddWithoutValidation("If-Match", $"\"{hireCase.Version}\"");
        var submitResponse = await _client.SendAsync(submit);
        var submitted = await submitResponse.Content.ReadFromJsonAsync<HireCaseResponse>();

        Assert.Equal(HttpStatusCode.OK, submitResponse.StatusCode);
        Assert.Equal("ReadyForReview", submitted?.Status);
        Assert.All(submitted?.Tasks ?? [], task => Assert.True(task.IsComplete));
    }

    private async Task<WorkforcePlanResponse> CreatePlanAsync(string clientId)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/v1/clients/{clientId}/workforce-plans");
        request.Headers.Add("Idempotency-Key", $"request-{Guid.NewGuid():N}");
        request.Content = JsonContent.Create(new CreateWorkforcePlanRequest(
            "Engineer",
            "US",
            new DateOnly(2099, 8, 1),
            120_000m,
            "USD"));
        var response = await _client.SendAsync(request);
        return await response.Content.ReadFromJsonAsync<WorkforcePlanResponse>()
            ?? throw new InvalidOperationException("Expected workforce plan response.");
    }
}
