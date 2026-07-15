# Learner guide

Study each sprint as a decision trail rather than reading only the final code.

1. Read the milestone and stakeholder issue to understand the outcome and constraints.
2. Read commits in order. Predict the next design pressure before opening the next commit.
3. Start with domain tests, then trace the application use case, adapter, endpoint, and contract.
4. Read inline review comments at the referenced lines and compare them with the ADR.
5. Recreate the design debate: state which option you would choose and what evidence could change your mind.
6. Run the focused test, alter one invariant, and make the failure explain the business risk.
7. Complete the sprint exercises without changing public contracts accidentally.

After Sprint 15, read the [flaw harvest](docs/quality/FLAW-HARVEST.md), [over-engineering review](OEReadMe.md), and [retrospective](docs/curriculum/RETROSPECTIVE.md) together. For each choice, separate business complexity from framework complexity and curriculum ceremony from product delivery.

## Review questions

- Which business fact must always remain true?
- Which aggregate owns the decision?
- Is this rule effective-dated, country-specific, tenant-specific, or currency-specific?
- Does the API contract expose a stable business concept or an internal storage shape?
- Can a retry duplicate money, state transitions, audit records, or integration messages?
- What is the narrowest interface that lets another system integrate safely?
- Which tests would survive replacing ASP.NET Core, EF Core, or an external provider?
- Which abstraction has at least two real consumers or implementations, and which exists only for a hypothetical future?
- What evidence is still missing before an operational or production claim is honest?
