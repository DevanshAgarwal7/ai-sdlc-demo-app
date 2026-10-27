# Agent context

This repository is a .NET 8 invoice approval demo used for AI-in-SDLC demos. Keep the implementation intentionally small and explicit.

## Purpose

The app models invoice approval workflows with a straightforward status lifecycle:
- Draft
- PendingApproval
- Approved
- Rejected

## Architectural expectations

- Business logic belongs in `backend/src/InvoiceApp.Domain`.
- API controllers belong in `backend/src/InvoiceApp.Api/Controllers`.
- In-memory storage belongs in `backend/src/InvoiceApp.Api`.
- Tests belong in `backend/tests/InvoiceApp.Tests` and should use `FakeInvoiceRepository`.
- Do not put business logic in controllers or in `Program.cs`.

## Approval rules

- Manager: may approve up to 50,000.
- Director: may approve up to 500,000.
- Amounts above 500,000 require two distinct approvers.
- The second approver must be a Director.
- The same user cannot count as both approvers.
- Rejected approval attempts must return a clear reason and leave the invoice unchanged.

## Critical review checks

- Validate both success and failure paths.
- Check that failed validations do not mutate invoice state.
- Assert on both the reason and the unchanged invoice object.
- Avoid superficial implementations that look correct but fail edge cases.

## Commands

- Run the app: `dotnet run --project backend/src/InvoiceApp.Api`
- Run the tests: `dotnet test backend/InvoiceApp.sln`
