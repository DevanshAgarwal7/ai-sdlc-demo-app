# Spec: Invoice Approval Limits

## Status
Draft

## Summary
Define the role-based approval limits for invoice approvals and the validation rules that protect invoice state during failed approval attempts. The specification is intentionally scoped to the approval policy already described in the project guidance and implementation patterns.

## Problem statement
The invoice approval workflow currently allows approval decisions without enforcing the role-based approval limits expected by the assignment. The business rules must be applied in the domain layer and must reject invalid approvals with clear, actionable reasons while leaving the invoice completely unchanged.

## Scope
In scope:
- Manager approval limit validation
- Director approval limit validation
- High-value invoice requirement for two distinct approvers
- Second approver must be a Director
- Duplicate approver prevention
- Rejection reason clarity
- Invoice immutability on invalid approval attempts
- Unit test coverage for success and failure paths

Out of scope:
- Creating new application routes
- Introducing new controller contracts
- Introducing new service classes or repository interfaces
- Adding database or persistent storage changes
- UI redesign or route changes
- Any feature beyond invoice approval validation

## Business requirements
1. A Manager may approve invoices up to 50,000.
2. A Director may approve invoices up to 500,000.
3. Invoices above 500,000 require two distinct approvers.
4. The second approver must be a Director.
5. The same user must never count as both approvers on one invoice.
6. Rejected approval attempts must return a clear reason.
7. Rejected approval attempts must not change the invoice or its approval history.

## User stories
### Manager approval limit
As a Manager, I want to approve invoices up to 50,000 so that standard approvals are allowed within my limit.

### Director approval limit
As a Director, I want to approve invoices up to 500,000 so that large but still valid approvals are allowed within my authority.

### High-value approval escalation
As a business process, when an invoice exceeds 500,000, I want a second approver requirement so that approval is not granted by a single role alone.

### Second approver governance
As a business process, I want the second approver to be a Director so that only the highest authority can finalize oversized approvals.

### Duplicate approver prevention
As a business process, I want the same user to be rejected as both approvers on one invoice so that approval integrity is preserved.

### Clear invalid approval handling
As a reviewer or operator, I want an invalid approval attempt to return a clear reason so that I know exactly why the action was rejected.

### State safety
As a business process, I want invalid approval attempts to leave the invoice completely unchanged so that the system remains consistent and auditable.

## Functional rules
### Rule 1: Manager limit
- If the approver role is Manager and the invoice amount is greater than 50,000, the approval request is invalid.
- The invalid approval must produce a clear reason that references the limit and the invoice amount.

### Rule 2: Director limit
- If the approver role is Director and the invoice amount is greater than 500,000, the approval request is invalid unless the invoice qualifies for a second approver flow.
- For approval values above 500,000, the two-approver path is required.

### Rule 3: Two-approver requirement for high-value invoices
- When an invoice amount exceeds 500,000, approval requires two distinct approvers.
- If no second approver is provided, the approval attempt is invalid.

### Rule 4: Second approver must be Director
- For invoices above 500,000, the second approver must have a role of Director.
- Any other second approver role is invalid.

### Rule 5: Distinct approvers
- The primary approver and the second approver must be different users.
- If they are the same user, the approval attempt is invalid.

### Rule 6: Rejection reason
- All rejected approval attempts must surface a clear, actionable reason.
- Reason text should identify the specific validation breach, such as role limit, second approver requirement, duplicate user, or invalid second approver role.

### Rule 7: Transaction safety
- A failed approval validation must not mutate invoice status.
- A failed approval validation must not update approver fields.
- A failed approval validation must not update decision timestamps.
- A failed approval validation must not change the approval decision note.
- A failed approval validation must not alter approval history beyond preserving the original values.

## Boundary cases
- Amount exactly 50,000 with a Manager approver is valid.
- Amount exactly 500,000 with a Director approver is valid.
- Amount 50,001 with a Manager approver is invalid.
- Amount 500,001 with a Director approver without a second approver is invalid.
- Amount greater than 500,000 with two distinct approvers where the second approver is Director is valid.
- Amount greater than 500,000 with duplicate user names is invalid.
- Amount greater than 500,000 with second approver role not equal to Director is invalid.
- Invalid approval attempts must be validated before any invoice field mutation occurs.

## Negative scenarios
- Manager approves 50,001
- Director approves 500,001 without second approver
- Two approvers are the same user
- Second approver is Manager or Approver instead of Director
- The invoice is not in pending approval state
- The invoice does not exist
- Rejected approval request returns an empty or weak reason
- Rejected attempt mutates invoice state

## Acceptance criteria
1. A Manager can approve invoices up to and including 50,000.
2. A Manager cannot approve invoices above 50,000.
3. A Director can approve invoices up to and including 500,000.
4. A Director cannot approve an invoice above 500,000 without the required second approver.
5. An invoice above 500,000 requires two distinct approvers.
6. The second approver must be a Director.
7. The same user cannot be both approvers.
8. Invalid approval attempts return a clear reason.
9. Invalid approval attempts leave the invoice unchanged.
10. Unit tests cover all valid and invalid approval scenarios using the existing test structure and repository pattern.

## Test requirements
- Tests must live in `backend/tests/InvoiceApp.Tests`.
- Tests must use `FakeInvoiceRepository`.
- Each invalid approval scenario must assert both the reason and the unchanged invoice values.
- At minimum, tests must cover:
  - Manager valid boundary: 50,000
  - Manager invalid boundary: 50,001
  - Director valid boundary: 500,000
  - Director invalid boundary: 500,001 without second approver
  - High-value approval with valid second Director
  - High-value approval with same user as both approvers
  - High-value approval with second approver not Director
  - Unchanged state on validation failure

## Definition of done
- Approval logic is enforced in the Domain layer.
- Business rules are tested and validated in the unit test suite.
- Invalid approval attempts produce explicit reasons.
- Invoice state remains unchanged after validation failure.
- Specification is aligned with assignment rules and repository conventions.
