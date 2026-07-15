# ADR 0001: Start with a modular monolith and explicit boundaries

- Status: accepted
- Date: 2026-07-14
- Decision owners: product and engineering

## Context

GlobeHire must support dense employment rules and several future integrations. Those needs can tempt a team to begin with microservices, a message bus, generic repositories, and mapping frameworks. In Sprint 1, however, there is only one deployable workflow and no evidence that independent scaling or deployment is valuable.

The code still needs stable seams: HTTP contracts will evolve, storage will move beyond memory, country pricing will vary, and external systems will need to consume events and APIs.

## Decision

Use one deployable ASP.NET Core application with four explicit inner boundaries:

1. `Domain` owns framework-free business rules and value objects.
2. `Application` owns named use cases and the narrow ports they require.
3. `Contracts` owns versioned transport shapes and has no inward dependency.
4. `Infrastructure` implements application ports; `Api` composes everything and maps contracts explicitly.

Use case-specific repository ports are preferred over a generic repository. No mediator or object-mapping framework is introduced. Architecture tests protect only the dependency rules that matter.

## Consequences

- Business rules can be tested without a server or database.
- An in-memory repository can later be replaced by an EF Core adapter without changing handlers or HTTP contracts.
- Explicit mapping is repetitive in a few places, but contract changes remain visible in review.
- A single deployment is operationally simple while module seams preserve a future extraction path.
- If independent deployment becomes a measured need, extracting a module will still require work; this decision does not pretend distribution is free.

## Rejected alternatives

- **Microservices now:** deployment and consistency costs without a demonstrated scaling boundary.
- **One project with folders:** initially shorter, but it cannot enforce dependency direction at compile time.
- **Generic repository and mediator pipeline:** broad abstractions would hide the actual workforce-plan operations and add concepts before a second use case proves reuse.
