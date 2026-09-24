# Compare: my review vs the model review

Fill this in *after* writing `MY_FINDINGS.md`, with `_answers/review.md` open.

## 1. Scorecard

| | Count |
|---|---|
| Model findings I also found (same root cause, not just the same file) | |
| Model Critical/High findings I missed | |
| My findings the model doesn't have (keep them if the evidence holds up) | |
| My findings that were wrong or unsupported | |

## 2. Misses → pattern

For each Critical/High finding you missed:

| Model # | What I missed | Why I missed it (didn't read that file / read it but trusted the docs / didn't think about retries / didn't think about two requests at once / other) | Question I will ask next time |
|---|---|---|---|
| | | | |

## 3. The pattern behind the misses

In 3–5 sentences: what kind of bug do you systematically not see? (Examples: "I trust a flaw ledger that says 'fixed'", "I review one request at a time and never picture two", "I check that the input is valid but not who chose it".)

## 4. One habit to change

Write one concrete habit for your next review, for example "for every POST, write down what a retry with the same key and a different body does".
