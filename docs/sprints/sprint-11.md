# Sprint 11: Integration platform

## Goal

Publish versioned webhooks atomically with business changes and reconcile resources with external HRIS/finance identifiers.

## Scope

Include domain events, transactional outbox, webhook subscriptions/deliveries, stable envelopes, signatures, retry/backoff, dead letters, replay, and external references. Exclude a real broker.

## Teaching focus

At-least-once delivery, consumer idempotency, schema evolution, anti-corruption layers, and operational reconciliation.

## Planned artifacts

Outbox tests, webhook contract suite, fake HTTP sender, ADR, and integration-contract note.

## Exercises

Replay one event without duplicating consumer effects. Add a v2 payload while preserving a v1 subscriber.
