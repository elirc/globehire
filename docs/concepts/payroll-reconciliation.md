# Payroll statements and reconciliation

Payroll is modeled as explicit money lines and a period state transition. A statement must satisfy `gross = deductions + net`; employer contributions remain a separate employer cost. A period rejects duplicate employment statements and becomes immutable after finalization.

Rounding each line is easy; allocating a rounded total across many workers or cost centers without losing cents is harder. Sprint 14 will add a largest-remainder allocation and prove that every allocated cent reconciles to the source total.
