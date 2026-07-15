# ADR 0000: Course charter and architecture constraints

## Status

Accepted before Sprint 1.

## Context

Global workforce software combines effective-dated legal policy, money, approvals, lifecycle workflows, sensitive personal data, tenant isolation, and many integrations. The curriculum must expose those hard parts without hiding them behind frameworks or splitting them into distributed services prematurely.

## Decision

Build a modular monolith on .NET 10 LTS. Domain projects remain framework-free. Application use cases depend on narrow ports. ASP.NET Core owns transport only. Infrastructure implements persistence and provider adapters. Public API contracts are versioned independently from domain and storage types.

Do not introduce MediatR, AutoMapper, generic repositories, microservices, or event-bus infrastructure until a demonstrated problem justifies them.

## Consequences

Business rules remain fast to test and easy to move. Integrations can be replaced through contract-tested ports. Mapping is explicit and occasionally repetitive. Cross-module transactions remain possible. Extraction into services is deferred until operational ownership or scaling requires it.
