# Task Breakdown: Invoice Approval Limits

## Phase 1: Manager and Director single-approver limits

### Task 1: Confirm current invoice lifecycle and validation points
- Review the domain service and repository usage.
- Confirm how pending approvals are represented.
- Confirm which fields must remain unchanged on invalid approval.
- No route or interface invention.

### Task 2: Implement Manager limit validation
- Enforce the Manager limit of 50,000.
- Reject values above 50,000 with a clear reason.
- Validate before any state mutation.

### Task 3: Implement Director limit validation
- Enforce the Director limit of 500,000 for the single-approver path.
- Reject invoice values above 500,000 when the approval flow does not meet the higher-value rules.
- Ensure the error message is explicit and actionable.

### Task 4: Add Phase 1 tests
- Add success-path tests for Manager and Director boundaries.
- Add failure-path tests for Manager above 50,000 and Director above 500,000.
- Use `FakeInvoiceRepository` and assert reason and unchanged invoice state.

## Phase 2: Two-approver validation, distinct users, and second approver Director requirement

### Task 5: Implement high-value approval requirement
- Detect when the invoice exceeds 500,000.
- Require a second approver when the amount is above the threshold.
- Reject the request when the second approver is missing.

### Task 6: Enforce distinct approvers
- Compare the primary approver and second approver.
- Reject attempts where the same user appears in both approver roles.
- Return a clear reason referencing the duplicate user.

### Task 7: Enforce second approver Director role
- Require the second approver to be a Director.
- Reject any other role as invalid.
- Ensure reason text names the requirement.

### Task 8: Add Phase 2 tests
- Add valid two-approver success test.
- Add invalid duplicate-user test.
- Add invalid second-approver-role test.
- Assert the rejection reason and unchanged invoice values.

## Cross-cutting verification
- Review all invalid approval paths for state immutability.
- Check invoice amount, status, approver, decision time, and decision note remain unchanged.
- Ensure business logic remains in the domain project and not controller or Program code.
- Run the repository’s normal unit test command after implementation.

## Definition of ready
- Requirements are clear and derived from the assignment.
- Project conventions are known.
- Domain-layer validation pattern is understood.
- Test structure and fake repository are available.

## Definition of done
- Both phases of approval-limit validation are complete.
- All acceptance criteria are satisfied.
- All negative scenarios are tested.
- Invalid attempts leave the invoice unchanged.
- The feature is ready for review under the repository’s test and quality standards.
