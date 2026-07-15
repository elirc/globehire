# Aggregate state machines

A hire case is not a row with unrelated booleans. Its checklist, status, and version must change together under rules that remain true for HTTP, imports, and future message consumers.

`HireCase` is the consistency boundary. It owns the allowed transitions:

```text
Draft --complete required tasks--> Draft --submit--> ReadyForReview
```

Submission with incomplete tasks fails. Editing after submission fails. Every real mutation increments a version, and API commands require the caller's `If-Match` ETag. This turns a lost update into an explicit reload/retry outcome.

Do not add a state-machine framework for two states. The enum, named methods, and tests form a readable transition table. Add machinery only when the transition graph, entry actions, or visualization needs justify it.
