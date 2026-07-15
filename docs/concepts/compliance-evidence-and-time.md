# Compliance evidence and half-open time ranges

Compliance cases track required evidence by type and reference, not sensitive document bytes. A case cannot reach review until every requirement is present and unexpired; approval is a separate expert decision.

Temporal policy ranges now use the half-open convention `[effectiveFrom, effectiveTo)`. When one version ends on 2027-01-01 and the next starts that day, exactly the new policy applies. This removes the inclusive-end overlap introduced in Sprint 2 and makes adjacent versions safe to compose.

Country fixtures and compliance workflows remain educational examples, not immigration or legal advice.
