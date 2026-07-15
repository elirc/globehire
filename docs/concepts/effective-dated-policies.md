# Effective-dated policies and explainable estimates

Global employment rules change over time. Editing one country configuration row in place would make an old quote impossible to explain and could apply tomorrow's assumptions to yesterday's hire. GlobeHire therefore gives every policy version an identity and an effective range.

The proposed employment start date selects the policy; the request date does not. A quote stores the selected policy ID and itemized values, so a support engineer can later answer which assumptions produced the result.

## Selection before calculation

The workflow separates two business decisions:

1. `CountryPolicySelector` selects exactly one country/version for the effective date.
2. `PlanQuote` validates the policy currency and calculates salary, employer contributions, service fee, and total.

Zero matches means the product does not support that country/date. More than one match is a configuration error, not an invitation to pick whichever row happened to load first.

## Explain, do not imply precision

The estimates are fictional fixtures, not legal or payroll advice. The API includes policy identity and assumptions alongside line items. This is more honest and more useful to integrations than returning one unexplained number.

When real providers are added, preserve this boundary: an adapter may fetch source data, but domain code decides whether it is applicable and how an estimate is assembled.
