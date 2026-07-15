# Simulated incident: duplicate webhook side effect

## Summary

A fictional HRIS consumer created two onboarding tasks after a webhook timed out and was retried. The first request completed at the consumer but its response was lost. The consumer keyed deduplication by attempt number, so the retry appeared new.

## Impact

One synthetic tenant received a duplicate task. No real user, PII, payroll, or external service was involved. The exercise tests reasoning; it is not evidence from an operated production system.

## Timeline (UTC)

- 10:00 — delivery attempt 1 times out after the consumer commits its side effect;
- 10:01 — retry is scheduled;
- 10:02 — attempt 2 creates a duplicate and returns `200`;
- 10:05 — reconciliation detects two tasks for one message ID;
- 10:10 — subscription is disabled and the duplicate is removed;
- 10:30 — stable delivery identity regression passes and replay is verified.

## Root cause and correction

The producer originally assigned a new delivery ID during retry, weakening the consumer's stable deduplication key. Sprint 14 changed retry to retain delivery and message IDs while incrementing only the attempt. The consumer must deduplicate on the stable message or delivery identity, not timing or attempt number.

## Follow-up

- keep the regression in `IntegrationModelsTests`;
- alert on repeated success for one message identity at the consumer;
- document pause/replay ownership before a real integration launches;
- never infer successful processing solely from an HTTP timeout.

