# Test Review Skill

## Purpose

Use this skill to review tests, validate business rules, and confirm that implementation matches the specification and does not hide failures behind superficial assertions.

## Review responsibilities

- Check that success and failure paths are both tested.
- Validate that rejection reasons are explicit and actionable.
- Confirm failed validations do not mutate invoice state.
- Look for edge cases such as duplicate approvers and invalid second approver role.
- Ensure tests prove the rule, not just the exception type.

## Required checks

- Business logic stays in `backend/src/InvoiceApp.Domain`.
- Tests live in `backend/tests/InvoiceApp.Tests`.
- Use `FakeInvoiceRepository` in tests.
- Assert both the failure message and the unchanged invoice state.
- For invalid approvals, validate amount, status, approver, decision time, and decision note remain unchanged.

## Review prompt guidance

When reviewing work, ask:
- Does the implementation enforce role limits correctly?
- Does a rejected approval leave the invoice completely unchanged?
- Does the test assert the reason and the state?
- Are the duplicate approver and second approver rules covered?
- Is the code in the right layer?

## Output expectations

- Identify missing tests or weak assertions.
- Highlight state mutation risks.
- Recommend explicit tests for edge conditions.
- Leave implementation quality, not just pass/fail status, as the focus.
