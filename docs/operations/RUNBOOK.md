# GlobeHire teaching-service runbook

## Start and verify

```powershell
dotnet restore GlobeHire.slnx --locked-mode
dotnet run --project src/GlobeHire.Api
```

Verify `GET /health/live` for process liveness, `GET /health/ready` for the current dependency declaration, and `/openapi/v1.json` in Development. Every response includes `X-Trace-Id`; use that value to correlate a caller report with structured server logs.

Readiness deliberately reports `storage: in-memory` and `durable: false`. A restart loses all business data. This is a teaching signal, not a production health guarantee.

## Triage sequence

1. Capture UTC time, route, tenant, status, `X-Trace-Id`, deployment version, and whether the caller retried. Never copy PII or webhook secrets into a ticket.
2. Check liveness. If it fails, inspect process startup and unhandled exception logs.
3. Check readiness. If the process is ready but a workflow fails, classify it as a business rejection (`422` with stable `code`), concurrency conflict, tenant miss, or unexpected server failure.
4. Reproduce with a synthetic tenant and the HTTP examples. Preserve `Idempotency-Key` when replaying the same logical command.
5. Escalate financial, tenant-isolation, policy-version, or duplicate-delivery symptoms immediately; stop the affected workflow rather than guessing at data repair.

## Recovery and rollback

For this in-memory build, restart restores an empty known state but cannot recover data. There is no supported backup or restore. In a real deployment, rollback must not reverse a database migration blindly: deploy the previous compatible application, pause consumers if needed, validate outbox lag, and exercise the documented database restore procedure.

Candidate production objectives—not measured claims—are 99.9% monthly API availability, 99% of synchronous requests below 500 ms, webhook delivery attempted within 60 seconds, recovery time under 60 minutes, and recovery point under 15 minutes. Owners must validate these against user impact and cost before adoption.

## Webhook duplicate drill

Deliver the same message and delivery identity twice. The consumer should process it once and return success for the duplicate. Retry keeps the delivery/message IDs stable and increments the attempt. If duplicate side effects occur, disable the subscription, preserve the identities and trace, repair the consumer deduplication boundary, and replay only after verification.

## Payroll close drill

If a close fails, do not create a second payroll run. Preserve the run ID, inspect validation failures, and retry the same command only after the cause is fixed. Reconcile statement totals and invoice totals before reopening downstream delivery.

## Privacy and secrets

Log opaque resource IDs, rule codes, versions, timings, and trace IDs. Do not log names, document contents, compensation payloads, tokens, bank details, or webhook secrets. Production secrets belong in a managed secret store with rotation and access audit; this repository contains no working secrets.

## Exit criteria

An incident is resolved only when customer impact stops, invariant checks pass, queued work is reconciled, monitoring is restored, and a regression test or explicit risk acceptance records what was learned. Use the [incident drill](INCIDENT-DRILL.md) as the example.

