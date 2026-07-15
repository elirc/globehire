using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GlobeHire.Contracts.V1.WorkforcePlans;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GlobeHire.Api.Tests.WorkforcePlans;

public sealed class WorkforcePlanApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public WorkforcePlanApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Testing")).CreateClient();
    }

    [Fact]
    public async Task PostThenGet_ReturnsVersionedContractAndLocation()
    {
        var clientId = $"client-{Guid.NewGuid():N}";
        var request = CreateRequest(clientId, $"request-{Guid.NewGuid():N}");

        var createResponse = await _client.SendAsync(request);
        var created = await createResponse.Content.ReadFromJsonAsync<WorkforcePlanResponse>();

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(created);
        Assert.NotNull(createResponse.Headers.Location);
        Assert.Equal("US", created.CountryCode);
        Assert.Equal("US-2026.1", created.Quote.PolicyId);
        Assert.Equal(1_200m, created.Quote.MonthlyEmployerContributions.Amount);
        Assert.Equal(11_700m, created.Quote.EstimatedMonthlyTotal.Amount);
        Assert.NotEmpty(created.Quote.Assumptions);
        Assert.Equal("\"1\"", createResponse.Headers.ETag?.Tag);

        var getResponse = await _client.GetAsync(createResponse.Headers.Location);
        var retrieved = await getResponse.Content.ReadFromJsonAsync<WorkforcePlanResponse>();

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(retrieved);
        Assert.Equal(created.Id, retrieved.Id);
        Assert.Equal(created.Quote.EstimatedMonthlyTotal, retrieved.Quote.EstimatedMonthlyTotal);
        Assert.Equal(created.Quote.Assumptions, retrieved.Quote.Assumptions);
    }

    [Fact]
    public async Task Post_ReplayedIdempotencyKey_ReturnsOriginalWithOk()
    {
        var clientId = $"client-{Guid.NewGuid():N}";
        var idempotencyKey = $"request-{Guid.NewGuid():N}";

        var firstResponse = await _client.SendAsync(CreateRequest(clientId, idempotencyKey));
        var replayResponse = await _client.SendAsync(CreateRequest(clientId, idempotencyKey));
        var first = await firstResponse.Content.ReadFromJsonAsync<WorkforcePlanResponse>();
        var replay = await replayResponse.Content.ReadFromJsonAsync<WorkforcePlanResponse>();

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, replayResponse.StatusCode);
        Assert.Equal(first?.Id, replay?.Id);
    }

    [Fact]
    public async Task Post_InvalidBusinessInput_ReturnsStableProblemDetailsCode()
    {
        var request = CreateRequest($"client-{Guid.NewGuid():N}", $"request-{Guid.NewGuid():N}");
        request.Content = JsonContent.Create(new CreateWorkforcePlanRequest(
            "Engineer",
            "USA",
            new DateOnly(2099, 8, 1),
            120_000m,
            "USD"));

        var response = await _client.SendAsync(request);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("country_code.invalid", problem.RootElement.GetProperty("code").GetString());
        Assert.True(problem.RootElement.TryGetProperty("traceId", out _));
    }

    [Fact]
    public async Task OpenApi_ContainsVersionedWorkforcePlanPaths()
    {
        var response = await _client.GetAsync("/openapi/v1.json");
        var document = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("/api/v1/clients/{clientId}/workforce-plans", document, StringComparison.Ordinal);
        Assert.Contains("/api/v1/clients/{clientId}/offers", document, StringComparison.Ordinal);
    }

    private static HttpRequestMessage CreateRequest(string clientId, string idempotencyKey)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/v1/clients/{Uri.EscapeDataString(clientId)}/workforce-plans");
        request.Headers.Add("Idempotency-Key", idempotencyKey);
        request.Content = JsonContent.Create(new CreateWorkforcePlanRequest(
            "Senior Engineer",
            "US",
            new DateOnly(2099, 8, 1),
            120_000m,
            "USD"));
        return request;
    }
}
