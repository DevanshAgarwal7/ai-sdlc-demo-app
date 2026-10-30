# Implementation Plan: Invoice Approval Limits

## Phase 1: Manager and Director single-approver limits

### Goal
Add the role-based single-approver approval limits so that Manager and Director approvals are enforced before invoice state is updated.

### Scope
- Manager approval limit: 50,000
- Director approval limit: 500,000
- Validation of invalid role/amount combinations
- Clear rejection messaging
- No invoice mutation on invalid attempts

### Work items
1. Review the current approval flow to confirm invoice lifecycle and validation boundaries.
2. Add validation rules in the domain approval logic for Manager and Director limits.
3. Ensure invalid attempts fail before any state mutation occurs.
4. Return actionable rejection reasons that make the limit clear.
5. Add unit tests for valid and invalid Manager/Director cases.
6. Verify boundary cases at 50,000 and 500,000.

### Phase 1 acceptance criteria
- A Manager may approve 50,000 exactly.
- A Manager may not approve above 50,000.
- A Director may approve 500,000 exactly.
- A Director may not approve above 500,000 without the high-value two-approver path.
- Failed validation leaves the invoice unchanged.
- Clear rejection messages are returned.

### Phase 1 test requirements
- `Approve_AllowsManagerUpTo50000`
- `Approve_RejectsManagerAbove50000`
- `Approve_AllowsDirectorUpTo500000`
- `Approve_RejectsDirectorAbove500000`
- Assert the rejection reason text and unchanged invoice fields

## Phase 2: Two-approver validation, distinct users, and second Approver Director requirement

### Goal
Add the multi-approver validation for invoices above 500,000, requiring a second distinct approver who is a Director.

### Scope
- Require a second approver for invoice amounts above 500,000
- Enforce distinct users
- Enforce second approver role = Director
- Preserve all invoice state when validation fails
- Ensure clear reason text for each rejected case

### Work items
1. Define the high-value approval path used when amount exceeds 500,000.
2. Validate that a second approver is supplied.
3. Validate that the second approver is different from the primary approver.
4. Validate that the second approver role is Director.
5. Reject invalid cases before invoice mutation.
6. Add unit tests for each negative scenario and a valid success scenario.

### Phase 2 acceptance criteria
- Invoices above 500,000 require two distinct approvers.
- The second approver must be a Director.
- The same user cannot count as both approvers.
- Invalid high-value approvals return a reason and leave invoice state unchanged.
- Valid high-value approvals still pass with a valid Director second approver.

### Phase 2 test requirements
- `Approve_RequiresTwoDistinctApproversAbove500000`
- `Approve_RequiresSecondApproverToBeDirectorAbove500000`
- `Approve_AllowsTwoDistinctDirectorApproversAbove500000`
- Assert rejection message and unchanged invoice state for each invalid case

## Risk areas
- Duplicate approver validation is easy to implement superficially and incorrectly.
- Invalid approvals may mutate invoice fields if validation occurs after state changes.
- The implementation must preserve the invoice and approval history state for all failed attempts.

## Dependencies
- Existing approval lifecycle in the domain layer
- Existing repository abstraction and fake repository in tests
- Existing unit-test conventions in `backend/tests/InvoiceApp.Tests`

## Exit criteria
- All single-approver and two-approver rules are documented and covered by tests.
- Invalid approvals fail with clear reasons and no state mutation.
- The implementation is ready to be executed in the project’s normal `dotnet test` flow.
