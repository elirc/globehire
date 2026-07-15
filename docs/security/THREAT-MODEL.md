# Tenant isolation threat model

## Assets and boundary

GlobeHire handles workforce plans, candidate identity, contracts, employment, payroll, and case data. Every business resource belongs to one client tenant. A valid resource ID is not authority to access that resource.

## Primary threat: object-level authorization

An attacker or misconfigured integration may place another tenant's GUID into a valid route. Sprint 3 originally allowed hire-case repositories to look up by GUID alone. Sprint 10 repairs the design by requiring `ClientId` in the repository port and by keying adapters with `(ClientId, Id)`.

Returning `404` for a cross-tenant lookup avoids confirming that the foreign object exists. The same tenant context flows through hire-case, offer, and employment-activation use cases.

## Controls

- tenant identity is parsed once and passed to every lookup port;
- repositories cannot perform an object lookup without tenant context;
- cross-tenant API tests exercise known IDs under a different client route;
- future authenticated claims must be matched to route tenant before the use case executes;
- logs and audit events should record tenant, actor, action, resource, and outcome without logging sensitive payloads.

## Residual risks

The teaching API does not implement a real identity provider. Route values demonstrate object scoping but are not authentication. Production deployment requires token validation, tenant-claim authorization, least-privilege roles, encrypted durable storage, and centralized audit retention.
