# GlobeHire STAR stories (complete these *after* doing the drill)

Fill each one with what you actually did: your commands, your test names, your numbers. A story you can't back up with a file or commit doesn't go in an interview.

## 1. Debugging: "the approver was someone from another company" (incident 001)
- **S:** A .NET 10 EOR API. After a deploy, every Legal approval failed and Finance approvals across tenants showed one person's name, a different one per pod.
- **T:** Find the cause without guessing, fix it, and make the *class* of bug impossible to ship again.
- **A:** (your diagnostic steps: how "one ID per pod + restart helps" pointed you to process-lifetime state; the two-request test you wrote; the DI change; turning on scope validation)
- **R:** (tests before/after; what now fails at startup)
- **Interview hook:** "Tell me about a bug caused by two changes that were each correct."

## 2. Trade-off: "fix the symptom or the lifetime?" (incident 001)
- **S:** Deleting the memoization in `GatewayRequestActor` made the failing test pass.
- **T:** Decide between that one-line fix and changing the handler lifetimes plus validation.
- **A:** (why the one-liner leaves a captive dependency behind; what you'd tell a reviewer who prefers the smaller diff)
- **R:**
- **Interview hook:** "When did you reject a fix that made the tests pass?"

## 3. Incident + postmortem: "the error rate dropped, and that was the bug" (incident 002)
- **S:** Start-date 422s for US clients fell from ~40 to ~3 a day after a fix. Payroll then found start dates in the past.
- **T:** Explain why a green regression test and a falling error rate both lied.
- **A:** (your hand calculation for 10:12 New York; the boundary table you designed; fixing plan *and* activation together)
- **R:** (how many plans needed repair; what invariant alert you'd add)
- **Interview hook:** "Tell me about a time a metric improved and something was still wrong."

## 4. Code review: "the flaw ledger said it was fixed" (review finding 4)
- **S:** The project docs said the approval race was fixed with atomic mutation in Sprint 14.
- **T:** Review the codebase before a customer pilot.
- **A:** (how you compared the hire-case write path with the offer write path; the evidence lines)
- **R:** (severity you gave it; what you'd change)
- **Interview hook:** "Tell me about a review finding the author disagreed with, or didn't know about."

## 5. Agent-caught / agent-caused bug (optional)
If you use an agent for either incident fix, record where it went wrong. For example: did it propose deleting the memoization? Did it change `EmploymentUseCases.cs` too? What did you verify by hand?
