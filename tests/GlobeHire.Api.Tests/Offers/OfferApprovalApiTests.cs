using System.Net;
using System.Net.Http.Json;
using GlobeHire.Contracts.V1.HireCases;
using GlobeHire.Contracts.V1.Offers;
using GlobeHire.Contracts.V1.WorkforcePlans;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GlobeHire.Api.Tests.Offers;

public sealed class OfferApprovalApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OfferApprovalApiTests(WebApplicationFactory<Program> factory) =>
        _client = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Testing")).CreateClient();

    [Fact]
    public async Task Approve_WithGatewayActorHeader_RecordsGatewayActorInsteadOfBodyActor()
    {
        var clientId = $"client-{Guid.NewGuid():N}";
        var offer = await CreateOfferAsync(clientId);

        using var approve = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/v1/clients/{clientId}/offers/{offer.Id:D}/approvals");
        approve.Headers.TryAddWithoutValidation("If-Match", $"\"{offer.Version}\"");
        approve.Headers.TryAddWithoutValidation("X-GlobeHire-Actor", "finance-ana");
        approve.Content = JsonContent.Create(new ApproveOfferRequest("Finance", "spoofed-by-body"));
        var response = await _client.SendAsync(approve);
        var approved = await response.Content.ReadFromJsonAsync<OfferResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(approved);
        Assert.Equal("finance-ana", Assert.Single(approved.Approvals).ActorId);
    }

    private async Task<OfferResponse> CreateOfferAsync(string clientId)
    {
        using var planRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/clients/{clientId}/workforce-plans");
        planRequest.Headers.Add("Idempotency-Key", $"request-{Guid.NewGuid():N}");
        planRequest.Content = JsonContent.Create(new CreateWorkforcePlanRequest(
            "Engineer", "US", new DateOnly(2099, 8, 1), 120_000m, "USD"));
        var plan = await (await _client.SendAsync(planRequest)).Content.ReadFromJsonAsync<WorkforcePlanResponse>()
            ?? throw new InvalidOperationException("Expected a workforce plan.");

        var hireCase = await (await _client.PostAsJsonAsync(
                $"/api/v1/clients/{clientId}/hire-cases",
                new OpenHireCaseRequest(plan.Id, "Ada Lovelace", "ada@example.com")))
            .Content.ReadFromJsonAsync<HireCaseResponse>()
            ?? throw new InvalidOperationException("Expected a hire case.");

        foreach (var task in hireCase.Tasks)
        {
            using var complete = new HttpRequestMessage(
                HttpMethod.Put,
                $"/api/v1/clients/{clientId}/hire-cases/{hireCase.Id:D}/tasks/{task.Code}/complete");
            complete.Headers.TryAddWithoutValidation("If-Match", $"\"{hireCase.Version}\"");
            hireCase = await (await _client.SendAsync(complete)).Content.ReadFromJsonAsync<HireCaseResponse>()
                ?? throw new InvalidOperationException("Expected a hire case.");
        }

        using var submit = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/v1/clients/{clientId}/hire-cases/{hireCase.Id:D}/submit");
        submit.Headers.TryAddWithoutValidation("If-Match", $"\"{hireCase.Version}\"");
        _ = await _client.SendAsync(submit);

        var offerResponse = await _client.PostAsJsonAsync(
            $"/api/v1/clients/{clientId}/offers",
            new CreateOfferRequest(hireCase.Id));
        return await offerResponse.Content.ReadFromJsonAsync<OfferResponse>()
            ?? throw new InvalidOperationException("Expected an offer.");
    }
}
