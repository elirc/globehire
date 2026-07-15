# Vertical slices and changeable boundaries

A vertical slice is the smallest useful behavior that crosses every real boundary. Sprint 1 does not stop at a domain class or a controller-shaped demo. A client can send an HTTP request, the application executes a use case, the domain enforces rules, an adapter stores the result, and the API returns a documented contract.

## Follow one request

```text
POST /api/v1/clients/{clientId}/workforce-plans
  -> CreateWorkforcePlanRequest (public contract)
  -> CreateWorkforcePlanHandler (orchestration)
  -> WorkforcePlan.Create / PlanQuote.Estimate (business decisions)
  -> IWorkforcePlanRepository (required capability)
  -> InMemoryWorkforcePlanRepository (current technical choice)
  -> WorkforcePlanResponse (explicit outward mapping)
```

The arrows are intentionally one-way. Domain code does not know HTTP exists. The application asks for a repository capability but does not know how it is implemented. The public response does not serialize the aggregate directly.

## Why each rule lives where it does

| Concern | Owner | Reason |
| --- | --- | --- |
| Positive compensation, money rounding, valid start date | Domain | True for every caller and storage technology |
| Generate an ID, read time, save or replay a request | Application ports/use case | Coordinates the business operation and nondeterministic dependencies |
| `Idempotency-Key`, status codes, ETag, JSON shape | API/Contracts | HTTP-specific behavior |
| Dictionary and lock strategy | Infrastructure | Replaceable implementation detail |

Validation is not a single middleware step. JSON must first be parseable, the endpoint checks transport requirements, and the domain rejects business-invalid values with stable codes.

## Change exercises

1. Add `department` to the public request, then decide whether it is merely descriptive or a domain invariant. Notice which projects actually need to change.
2. Implement a file-backed repository. The handler and domain tests should remain unchanged.
3. Make pricing country-aware in Sprint 2. The public quote contract can remain stable while the policy behind it changes.

The goal is not the fewest files. It is a small, explainable cost for the changes the product is likely to request.
