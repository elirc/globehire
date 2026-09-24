# SEALED: incident 001, approvals attributed to the wrong person

Branch `training/incidents/001-approver-identity` (commits `fb15c04` singletons, `5217ca6` gateway actor). BUILD: verified (`dotnet test` on the Api test project, 2026-09-24).
**Reproduced live:** two approvals on two different tenants' offers, the first with `X-GlobeHire-Actor: finance-ana` and the second with `finance-bo`. The second was recorded as `finance-ana` (Expected "finance-bo", Actual "finance-ana"). The committed single-request test stays green.

## Root cause: a captive dependency plus a memoized read

1. PR #76 made `OfferHandler` a **singleton**. PR #77 gave it a new **scoped** dependency, `IRequestActor`.
2. A singleton is built once, from the **root** service provider. When the root resolves a scoped service it hands out one instance for the app's lifetime, so `GatewayRequestActor` became a process-wide singleton "captured" by the handler.
3. `GatewayRequestActor` memoizes: `ActorId => _actorId ??= Read()`. The first request that carries the header latches its value **for the life of the pod**. (A request *without* the header reads `null`, and `??=` doesn't latch null, so the pod latches the first *non-null* actor it ever sees.)
4. From then on, every approval on that pod, for every tenant, uses `finance-ana` (`OfferUseCases.cs`: `requestActor.ActorId ?? actorId`). Legal's approval then collides with the Finance approval "by" the same actor, so `approval.separation_of_duties` fires.
5. Each pod latches a *different* first actor ("distinct approver IDs per pod = 1"). A restart clears it until the next approval, which is why the runbook restart "worked for a bit".

Neither PR is wrong on its own. #76 was true when written ("handlers depend only on singletons"). #77 added a scoped dependency to a class whose lifetime its author didn't check. **The bug lives in the interaction, which is why neither review caught it.**

## Why CI missed it

- The new test sends **one** approval with **one** header, and a latched value is correct for the first request.
- Every API fixture uses `UseEnvironment("Testing")`. ASP.NET Core only turns on `ValidateScopes`/`ValidateOnBuild` in **Development**, and there `builder.Build()` is expected to throw *"Cannot consume scoped service 'IRequestActor' from singleton 'OfferHandler'"*. (Expected from documented .NET behaviour; not run in this drill: `UNVERIFIED`.)
- `Program.cs` never sets `ValidateScopes`/`ValidateOnBuild` explicitly (grep: no hits).

## Diagnostic path a senior would take

1. "Exactly one approver ID per pod" plus "a restart fixes it briefly" points to **process-lifetime state**, not a data or DB bug.
2. Diff the deploy for lifetime changes and new per-request dependencies. Both PRs touch `Program.cs`, so read the DI registrations together.
3. For each singleton, list the constructor dependencies and their lifetimes. `OfferHandler(singleton) → IRequestActor(scoped)` is the smell.
4. Confirm with a two-request test (below) before touching code.

## Fix (small, one PR)

```csharp
// Program.cs: handlers go back to Scoped (they now hold per-request state through IRequestActor)
builder.Services.AddScoped<OfferHandler>();   // … and the other five

// Make lifetime mistakes fail at startup in every environment, not only Development
builder.Host.UseDefaultServiceProvider(o => { o.ValidateScopes = true; o.ValidateOnBuild = true; });
```

Also acceptable: keep handlers singleton and pass the actor as a method argument from the endpoint (`ApproveAsync(..., actor)`), so the handler holds no request state at all. Removing the memoization in `GatewayRequestActor` alone is **not** a fix. It makes this symptom disappear, because `IHttpContextAccessor` happens to flow through `AsyncLocal`, but the captive dependency stays and the next scoped dependency (a `DbContext`) breaks in a worse way.

### Separate security follow-up (don't fold it into the hotfix)

The header is only trustworthy if the gateway **strips any client-supplied `X-GlobeHire-Actor`** and the API is not reachable except through the gateway. The body fallback (`?? actorId`) keeps the original spoofing hole open until it's removed. Track it as its own ticket with a sunset date.

## Regression tests

```csharp
[Fact] // API level, two requests, two tenants, two actors
public async Task Approvals_OnSequentialRequests_RecordEachRequestsOwnActor()
{
    var a = await ApproveAsync(NewClient(), actor: "finance-ana");
    var b = await ApproveAsync(NewClient(), actor: "finance-bo");
    Assert.Equal("finance-ana", a);
    Assert.Equal("finance-bo", b);   // fails on the incident branch: "finance-ana"
}

[Fact] // guards the whole class of bug
public void ServiceProvider_WithScopeValidation_Builds()
{
    using var factory = new WebApplicationFactory<Program>()
        .WithWebHostBuilder(b => b.UseEnvironment("Development"));
    _ = factory.Services; // throws if any singleton captures a scoped service
}
```

## Model postmortem (5 lines)

- **Impact:** about 10 h during which every offer approval on each pod was attributed to one person. Legal approvals were blocked (422), and the audit trail for contract approvals is wrong for that window (records need correcting from gateway logs).
- **Trigger:** two independently safe PRs deployed together: handlers became singletons, and a handler gained a scoped request-identity dependency.
- **Root cause:** a captive dependency (singleton → scoped) combined with a memoized per-request value turned "the current user" into "the first user this pod ever saw".
- **Why undetected:** DI scope validation only runs in Development and all tests run in "Testing", and the new test used a single request.
- **Follow-ups:** enable `ValidateScopes`/`ValidateOnBuild` everywhere; add a two-actor API test; add a review checklist item ("lifetime change? list every dependency's lifetime"); gateway header stripping and body-fallback sunset.

## Skills

.NET DI lifetimes and captive dependencies · verify-before-trust (both PR descriptions were true) · testing (one request proves nothing about request-scoped state) · release safety (interaction of two safe changes).
