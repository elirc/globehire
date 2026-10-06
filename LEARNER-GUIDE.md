# Learner guide

GlobeHire is a .NET 10 modular monolith (about 2.7k lines under `src/`) that teaches employer-of-record business logic over fifteen sprints. Storage is in-memory and there is **no authentication**: the tenant is whatever `{clientId}` the URL says. Keep both facts in mind while reading; most of the interesting design questions come from them.

## Repository map

| Path | What lives there | Read it when |
| --- | --- | --- |
| `src/GlobeHire.Domain` | Aggregates and value objects (`HireCase`, `EmploymentOffer`, `Employment`, `Money`, `ClientId`). No ASP.NET or storage references. | You want the business rule itself. |
| `src/GlobeHire.Application` | Use-case handlers plus the ports they need (`IHireCaseRepository`, `IOfferRepository`, `IClock`, `IIdGenerator`). | You want the order of operations for one request. |
| `src/GlobeHire.Infrastructure` | In-memory adapters: dictionaries guarded by a `Lock`. | You want to know what "saved" means today. |
| `src/GlobeHire.Contracts/V1` | Request/response records that form the public JSON shape. | You are about to change an API field. |
| `src/GlobeHire.Api` | `Program.cs` wiring, minimal-API endpoint files, `ApiExceptionHandler`. | You want routes, headers, and status codes. |
| `tests/` | Four xUnit projects: Domain (43 cases), Application (8), Api (8), Architecture (3). | Before you change anything. |
| `docs/sprints`, `docs/concepts`, `docs/adr` | The fifteen-sprint decision trail. | Studying one sprint in depth. |
| `astraupskill/` | A focused lab on tenant-scoped hire-case mutation. | After this guide. |
| `training/` | Code review and incident drills; the seeded bugs live on the `training/incidents/*` branches. | Once you can trace a request unaided. |

## Reading order (first day)

1. `src/GlobeHire.Api/Program.cs` — lines 31–45 register every adapter and handler; lines 72–82 map every feature. This is the whole application in one screen.
2. `src/GlobeHire.Domain/HireCases/HireCase.cs` — the smallest complete state machine (Draft → ReadyForReview, version check first).
3. `src/GlobeHire.Application/HireCases/HireCaseUseCases.cs` — how a handler turns a route string into a `ClientId` and finds the aggregate.
4. `src/GlobeHire.Infrastructure/HireCases/InMemoryHireCaseRepository.cs` — line 9: the dictionary key is `(ClientId, Guid)`, which is the whole tenant boundary.
5. `src/GlobeHire.Domain/Offers/EmploymentOffer.cs` and `src/GlobeHire.Infrastructure/Offers/InMemoryOfferRepository.cs` — approvals, separation of duties, and the atomic `MutateAsync`.
6. `tests/GlobeHire.Api.Tests/HireCases/HireCaseApiTests.cs` — the onboarding flow end to end over HTTP, including a cross-tenant 404.

## One request, end to end

The hiring path is: workforce plan → hire case → complete three tasks → submit → offer → two approvals → contract → employment. Here is the trace for one call in the middle, `PUT /api/v1/clients/client-acme/hire-cases/{id}/tasks/identity/complete` with `If-Match: "1"`:

| Step | Location | What happens |
| --- | --- | --- |
| 1 | `Program.cs:50-54` | Middleware stamps `X-Trace-Id` on the response. |
| 2 | `HireCases/HireCaseEndpoints.cs:16` | Route matches; `{id:guid}` rejects a non-Guid ID with 404 before any handler runs. |
| 3 | `HireCaseEndpoints.cs:93-101` | `ParseVersion` strips quotes from `If-Match`; a missing or non-numeric value throws `concurrency.if_match_required`. |
| 4 | `HireCaseUseCases.cs:58` | `ClientId.Parse` (trims, 1–64 chars) and `FindAsync((client, id))`. A wrong client returns `null`. |
| 5 | `HireCaseUseCases.cs:59` | `hireCase?.CompleteTask(...)` — the `?.` means a foreign or unknown case is never touched. |
| 6 | `Domain/HireCases/HireCase.cs:85-106` | Version check, Draft check, task lookup, then the task record is replaced and `Version++`. |
| 7 | `HireCaseEndpoints.cs:82-91` | `null` → 404; otherwise the new version goes out as `ETag: "2"` with a 200 body. |
| 8 | `Api/Errors/ApiExceptionHandler.cs:33` | Any `BusinessRuleViolationException` from steps 3–6 becomes a 422 Problem Details response with a stable `code`. |

Nothing is "saved" in step 6: the repository handed back the stored object, so mutating it mutates the store. A durable adapter would need an explicit save, which is a good reason to read step 5 critically.

## Tenant and authority table

There is no login, role claim, or membership check anywhere in `src/`. The table below shows what each route actually uses as authority.

| Route (under `/api/v1/clients/{clientId}`) | Tenant enforced by | Actor/role comes from | Cross-tenant result |
| --- | --- | --- | --- |
| `POST/GET workforce-plans` | `(ClientId, PlanId)` and `(ClientId, Idempotency-Key)` keys in `InMemoryWorkforcePlanRepository` | n/a | GET → 404 |
| `POST/GET/PUT hire-cases...` | `(ClientId, Guid)` key | n/a | 404 (tested in `HireCaseApiTests` and `HireCaseTenantIsolationTests`) |
| `POST offers` | hire case looked up by `(ClientId, HireCaseId)` | n/a | 422 `hire_case.not_found` (body reference, so 422 not 404) |
| `POST offers/{id}/approvals` | `(ClientId, Guid)` in `MutateAsync` | **request body** `role` and `actorId` (`ApproveOfferRequest`) | 404 |
| `POST offers/{id}/contract` | `(ClientId, Guid)` | n/a | 404 |
| `POST employments` | offer looked up by `(ClientId, OfferId)`; employment stored by `OfferId` only | body `workerReference` | 422 `offer.not_found` |
| `payroll/preview`, `people-operations/*`, `bulk/...validate`, `employment-changes/validate`, `reports/workforce` | **not used** — the handler never reads `clientId` | n/a | same response for any client |
| `contractor-invoices/validate`, `webhook-subscriptions/validate`, `compliance-cases/validate` | `ClientId.Parse` only (format check, no lookup) | n/a | same response for any client |

The separation-of-duties rule (`EmploymentOffer.cs:67-72`) compares two strings the caller typed. It stops an honest mistake, not an impersonator. `docs/adr/ADR-0002-honest-production-boundary.md` and `docs/security/THREAT-MODEL.md` record this limit; `training/incidents/001-approver-identity.md` shows the failure mode.

## Running and testing

The SDK is pinned in `global.json` (10.0.302, latest patch).

```powershell
dotnet restore GlobeHire.slnx --locked-mode
dotnet test GlobeHire.slnx --configuration Release                          # 62 test cases
dotnet test tests/GlobeHire.Domain.Tests --filter FullyQualifiedName~HireCaseTests
dotnet test tests/GlobeHire.Application.Tests --filter FullyQualifiedName~OfferConcurrencyTests
dotnet run --project src/GlobeHire.Api                                      # http://localhost:5229
```

`src/GlobeHire.Api/GlobeHire.Api.http` sets its host to `http://localhost:5077`, but `Properties/launchSettings.json` listens on 5229. Change the variable at the top of the `.http` file before sending requests.

## Studying a sprint

Treat each sprint as a decision trail, not just final code.

1. Read the milestone and stakeholder issue for the outcome and constraints.
2. Read commits in order and predict the next design pressure before opening the next one.
3. Start with the domain tests, then trace use case → adapter → endpoint → contract.
4. Read inline review comments and compare them with the ADR.
5. Run the focused test, break one invariant, and make the failure explain the business risk.

After Sprint 15, read the [flaw harvest](docs/quality/FLAW-HARVEST.md), [over-engineering review](OEReadMe.md), and [retrospective](docs/curriculum/RETROSPECTIVE.md) together.

## Check yourself

- **Goal:** find the one hire-case route that changes state without `If-Match`. **Check:** `git grep -n "MapP" src/GlobeHire.Api/HireCases` lists three routes (lines 14, 16, 17); only `OpenAsync` (POST `/`) has no `ifMatch` parameter, because a new case has no prior version.
- **Goal:** explain why a hire case owned by `client-acme` is invisible to `Client-Acme`. **Check:** `ClientId` is a `record struct` over a `string` (`Domain/Common/ClientId.cs`), so key equality is ordinal and case-sensitive; `Parse` trims whitespace but does not lower-case.
- **Goal:** predict the status code when an offer is created for a hire case that is still Draft. **Check:** `EmploymentOffer.Create` throws `offer.hire_case_not_ready` (`EmploymentOffer.cs:40-43`), which `ApiExceptionHandler` turns into 422.

## Review questions

- Which business fact must always remain true, and which aggregate owns it?
- Is this rule effective-dated, country-specific, tenant-specific, or currency-specific?
- Can a retry duplicate money, state transitions, audit records, or integration messages?
- Which tests would survive replacing ASP.NET Core or the in-memory adapters?
- What evidence is still missing before an operational or production claim is honest?
