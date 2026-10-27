# Approval rules skill

## Purpose

Use this skill when implementing or reviewing invoice approval rules.

## Required rules

- Manager approval limit: 50,000
- Director approval limit: 500,000
- Values above 500,000 require two distinct approvers
- The second approver must be a Director
- The same user cannot be both approvers
- Rejected approval attempts must return a clear reason and leave the invoice unchanged

## Project constraints

- Keep all business logic in `backend/src/InvoiceApp.Domain`.
- Do not place business logic in controllers or `Program.cs`.
- Add tests in `backend/tests/InvoiceApp.Tests`.
- Use `FakeInvoiceRepository` in tests.

## Review checklist

- Did the code reject invalid approvals before mutating invoice state?
- Did the test assert the actual reason and the unchanged invoice values?
- Did the implementation avoid duplicate approver logic mistakes?
