# Sprint 04: Offers, approvals, and contracts

## Goal

Prepare a versioned offer, collect an approval chain, and issue a contract through a replaceable provider port.

## Scope

Include approval policy, separation-of-duties checks, immutable offer versions, contract-provider adapter, provider correlation, and failure recovery. Exclude real e-signature and document storage.

## Teaching focus

Approval invariants, concurrency, ports/adapters, provider anti-corruption layers, and distinguishing business state from provider state.

## Planned artifacts

Fake contract provider, shared adapter tests, approval API, ADR, and integration-boundary note.

## Exercises

Add a second provider without changing application logic. Inject a timeout after provider acceptance and design safe reconciliation.
