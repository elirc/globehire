# Over-engineering review

GlobeHire is intentionally more elaborate as a **teaching process** than this product needs at its current scale. The code itself is mostly restrained: it is a modular monolith with explicit business rules, not a distributed system or a framework showcase. A small commercial MVP should keep the business boundaries and tests while using far less delivery ceremony.

## Sprint 1 review

Sprint 1 shows several choices that can look over-engineered when judged only by the amount of behavior delivered.

| Choice | Assessment | What to do in a normal small product |
| --- | --- | --- |
| Five source projects and four test projects for one vertical slice | Borderline. Compile-time boundaries make dependency direction visible to a learner, but project navigation and build overhead arrive early. | Start with API, core, and tests if the team does not need separately versioned contracts. Split only when a boundary is repeatedly crossed. |
| A dedicated Contracts project | Slightly speculative in Sprint 1. It becomes valuable once external consumers and versioned schemas exist. | Keep DTOs in the API until a real SDK, schema package, or consumer needs independent contracts. |
| `Money`, `CountryCode`, `ClientId`, and business-rule exceptions | Proportionate. Money, tenancy, and country policy are high-risk concepts whose invalid states should be difficult to represent. | Keep these types; do not generalize every string into a value object. |
| Repository ports and explicit mapping | Proportionate. They isolate business code from in-memory storage and HTTP shapes, and later sprints actually replace or harden these seams. | Keep use-case-specific ports. Avoid a generic repository or mapping framework. |
| Central package versions, lock files, warnings as errors, formatting, secret scanning | More operational maturity than Sprint 1 needs, with real onboarding friction. It is justified here because supply-chain and CI habits are part of the curriculum. | Adopt incrementally, but package locking and a repeatable build are reasonable from day one. |
| Architecture tests | Useful while small because they protect only a few important dependency rules. | Keep the handful of rules that prevent expensive coupling; do not encode every folder convention as a test. |

The strongest evidence of restraint is what Sprint 1 did **not** add: MediatR, AutoMapper, a generic repository, microservices, an event bus, an ORM, or a custom application framework. Explicit code is longer in places, but easier for a junior engineer to trace and change.

## Whole-repository review

### Deliberate curriculum overhead

Fifteen milestones, roughly four issues per sprint, ordered commits, many inline review comments, a planted-flaw ledger, and a release at two course checkpoints are excessive for a team delivering this amount of production behavior. They exist to preserve the reasoning trail. A normal team should use the smallest number of issues and review artifacts needed to coordinate risk.

### Code pressure to watch

- `Program.cs` is becoming a composition inventory. Move registrations into feature-owned extension methods only when merge conflicts or navigation cost become recurring problems.
- The in-memory repositories are useful test and demo adapters, not a persistence architecture. Do not build transaction abstractions around them; implement a durable adapter and its transaction/outbox boundary together.
- Repeated ETag parsing, tenant-route conventions, and validation responses may justify tiny shared helpers. They do not justify a new internal framework.
- Interface count should grow only from a use case that needs substitution, isolation, or a transaction boundary. Do not create an interface for every class.
- Contractor, payroll, people operations, compliance, bulk, and integration models demonstrate breadth. A real MVP should prioritize the few workflows with validated customer value rather than implementing the whole catalog.

## Keep, simplify, defer, remove

| Decision | Guidance |
| --- | --- |
| Keep | Framework-free domain rules, typed money/tenant identifiers, versioned public contracts, explicit state transitions, tenant-bound ports, deterministic tests, idempotency identities, and the modular-monolith deployment. |
| Simplify | Project count if contracts are not independently consumed; issue/PR ceremony; repeated endpoint parsing after a proven third use. |
| Defer | Microservices, message brokers, orchestration engines, CQRS read stores, generic policy engines, plugin systems, and provider abstraction layers without a second implementation. |
| Remove | Any abstraction whose only argument is “we may need it someday,” duplicate test assertions at every layer, or documentation that promises operations the executable system cannot perform. |

## Extraction rule

Do not extract a service because a domain noun sounds independent. Extract only after evidence of a separate deployment cadence, scaling profile, data ownership, security boundary, or team ownership outweighs the cost of network failure, observability, compatibility, and distributed transactions. Until then, a well-partitioned monolith is the easier system to change and integrate.

