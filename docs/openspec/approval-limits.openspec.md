# OpenSpec: Approval limits

## Feature summary

Add role-based approval limits to the invoice approval service.

## Business rules

1. A Manager may approve invoices up to 50,000.
2. A Director may approve invoices up to 500,000.
3. Anything above 500,000 requires two distinct approvers.
4. The second approver must be a Director.
5. The same user may never count as both approvers on one invoice.
6. A rejected approval attempt must return a clear reason and leave the invoice unchanged.

## Acceptance criteria

- Approval service enforces the limits in the Domain layer.
- Invalid attempts throw clear domain errors or return explicit rejection reasons.
- No invoice state changes when a validation fails.
- Unit tests cover success and failure paths using `FakeInvoiceRepository`.

## Risks

- Duplicate approver validation is easy to implement incorrectly.
- State mutation on validation failure is the most important review issue.
