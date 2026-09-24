# Incident 001: offer approvals attributed to the wrong person

Branch: `training/incidents/001-approver-identity` · Severity: SEV-2 (audit integrity + blocked hiring)

## Alert / symptom (as on-call sees it)

> **#hiring-ops, 09:42:** "Legal can't approve anything since last night's deploy. Every Legal approval fails with
> *'One actor cannot satisfy multiple required approval roles'*, and the offers page says Finance was approved by **finance-ana**.
> We don't have a finance-ana. She's at Acme, a different client."
>
> **#hiring-ops, 09:51:** "Restarted `api-2` like the runbook says. Approvals worked for a bit, then broke again."

## Logs (api-1, trimmed)

```text
22:14:03 INF  Business rule approval.separation_of_duties rejected request 0HN6R2:0000001F
22:31:40 INF  Business rule approval.separation_of_duties rejected request 0HN6R2:0000004C
08:02:11 INF  Business rule approval.separation_of_duties rejected request 0HN6R2:000000A3
08:02:15 INF  Business rule approval.separation_of_duties rejected request 0HN6R2:000000A4
```

Gateway access log for 0HN6R2:000000A3: `POST /api/v1/clients/client-northwind/offers/7c1e.../approvals`, `X-GlobeHire-Actor: legal-lee`, body `{"role":"Legal","actorId":"legal-lee"}`, 422.

## Metrics (last 24 h)

| Metric | Before deploy | After deploy |
|---|---|---|
| `POST .../approvals` 2xx rate | 97 % | 41 % |
| `approval.separation_of_duties` 422s / hour | ~0.2 | 38 |
| Distinct approver IDs recorded per hour, **per pod** | 20–30 | **1** |
| p95 latency, all endpoints | 182 ms | 176 ms |

## What changed (last night's deploy: two merged PRs)

```diff
# PR #76  perf(api): register stateless use-case handlers as singletons
-builder.Services.AddScoped<OfferHandler>();
+// Use-case handlers hold no per-request state; singletons avoid six allocations per request.
+builder.Services.AddSingleton<OfferHandler>();
  (…same change for the other five handlers)

# PR #77  feat(offers): take the approving actor from the gateway identity header
+builder.Services.AddHttpContextAccessor();
+builder.Services.AddScoped<IRequestActor, GatewayRequestActor>();
 ...
 public sealed class OfferHandler(..., IClock clock, IRequestActor requestActor)
 ...
+        var approver = requestActor.ActorId ?? actorId;
```

Both PRs were reviewed and approved, and CI was green (`dotnet test GlobeHire.slnx`: every suite passes, including a new API test proving the header wins over the body).

## Your task

1. **Explain before touching:** write down which objects live for how long in this process, and what `finance-ana` has to do with a request for `client-northwind`. Predict which pod shows which wrong name.
2. Reproduce it with a failing test *before* fixing anything (hint: one request is never enough).
3. Fix it, and make sure this *class* of bug fails the build or the tests next time, not just this instance.
4. Write a 5-line postmortem: impact, trigger, root cause, why CI missed it, the follow-up.

Sealed answer: `training/_answers/incident-001-approver-identity.md`
