# Outbox and webhook delivery

An outbox message is durable business-event intent written with the source transaction. A delivery is a separate attempt to send one message to one subscription. Keeping these identities separate makes retries, response evidence, and dead-letter handling observable.

Webhook endpoints require HTTPS and explicit event subscriptions. Signature input is deterministic and includes timestamp, stable message ID, and exact payload bytes. The infrastructure adapter—not the domain—owns secret storage, HMAC implementation, HTTP, backoff, and jitter.

Consumers must deduplicate using the message identity because networks can acknowledge late or deliver more than once. Sprint 14 tests retry identity explicitly and repairs any ambiguity discovered there.
