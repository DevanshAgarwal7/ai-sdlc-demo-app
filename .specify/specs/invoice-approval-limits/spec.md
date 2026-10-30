# Spec: Invoice Approval Limits

## Purpose
Specify the approval-limit rules for invoice decisions under the repository’s domain-driven workflow.

## Requirements
- A Manager may approve invoices up to 50,000.
- A Director may approve invoices up to 500,000.
- Invoices above 500,000 require two distinct approvers.
- The second approver must be a Director.
- The same user must never count as both approvers.
- Rejected attempts must return a clear reason.
- Rejected attempts must not change the invoice or its approval history.

## Acceptance criteria
- Manager approval limit is enforced.
- Director approval limit is enforced.
- High-value approvals require a second approver.
- The second approver must be a Director.
- Duplicate approvers are rejected.
- Invalid attempts do not mutate invoice state.
- All scenarios are covered by unit tests.

## Phase breakdown
### Phase 1
- Manager limit validation
- Director limit validation
- Boundary checks for 50,000 and 500,000
- Clear rejection reasons and no mutation on invalid attempts

### Phase 2
- Two-approver requirement above 500,000
- Distinct user validation
- Second approver Director requirement
- Complete negative and positive coverage

## Test requirements
- Use `FakeInvoiceRepository`.
- Assert both failure reason and unchanged invoice state.
- Cover valid and invalid cases for Manager and Director approval paths.
- Cover duplicate approver and invalid second-approver-role scenarios.
