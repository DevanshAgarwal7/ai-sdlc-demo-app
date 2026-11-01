# Invoice Approval Demo App

A small invoice approval app used for live AI-in-SDLC demos. It is deliberately simple: a .NET 8 API, a plain web UI, and an in-memory store, so it runs with one command and no database.

## Run it

```
dotnet run --project backend/src/InvoiceApp.Api
```

Open http://localhost:5000 and sign in:

| User | Password | Role |
| --- | --- | --- |
| `demo.approver` | `ChangeMe123!` | Approver |
| `demo.admin` | `ChangeMe123!` | Admin |
| `demo.manager` | `ChangeMe123!` | Manager |
| `demo.director` | `ChangeMe123!` | Director |
| `demo.director2` | `ChangeMe123!` | Director |

These are demo accounts for a local, in-memory app. They are not real credentials.

## What it does

1. Create an invoice (it starts as a draft).
2. Submit it for approval.
3. An approver approves it, or rejects it with a reason.
4. For invoices above 500,000, a second Director approval is required.
5. The first Director enters the second approver and the invoice remains pending until the second Director confirms.
6. Invalid approvals return clear validation errors and do not mutate invoice state.

Invoices move through Draft, Pending Approval, then Approved or Rejected.

## Approval rules

- A Manager may approve invoices up to 50,000.
- A Director may approve invoices up to 500,000.
- Invoices above 500,000 require two distinct approvers.
- The second approver must be a Director.
- The same user cannot count as both approvers on the same invoice.
- Invalid approval attempts must leave the invoice unchanged.

## Tests

```
dotnet test backend/InvoiceApp.sln
```

## Project layout

```
backend/
  InvoiceApp.sln
  src/
    InvoiceApp.Domain/        business rules and approval lifecycle
    InvoiceApp.Api/           API controllers, auth, in-memory repo, and web UI
  tests/
    InvoiceApp.Tests/         xUnit tests for domain rules

docs/
  use-cases/                 feature briefs and workflow notes

e2e/
  pages/                     Playwright page objects
  tests/                     end-to-end test scenarios

AGENTS.md                    project conventions for AI agents
CLAUDE.md                    compatibility note for Claude
README.md                    project overview and branch summary
```

## Specialized agents

The repo includes a dedicated `agents/` folder with focused instruction files for different AI roles:

- `agents/approval-agent.md` — approval workflow and domain logic
- `agents/role-agent.md` — Manager/Director approval policy
- `agents/review-agent.md` — validation and safety review
- `agents/logging-agent.md` — AI interaction traceability and structured logging
- `agents/orchestrator-agent.md` — routes work to the correct specialized agent and keeps the workflow structured

## Assignment summary

This branch focuses on implementing the invoice approval policy and validating it in the domain layer. The important behaviors are:

- manager limit enforcement
- director limit enforcement
- staged high-value approval flow over 500,000
- second approver must be a Director
- duplicate approvers rejected
- invalid approvals do not mutate invoice state
- clear error messages and regression tests

## Use cases

- [UC-1 Bulk invoice approval](docs/use-cases/UC-1-bulk-approval.md)
- [UC-2 Partial approval](docs/use-cases/UC-2-partial-approval.md)

## Video link

Add the video link here when available.

## End-to-end tests

Make sure the app is already running before starting Playwright:

```
dotnet run --project backend/src/InvoiceApp.Api
```

Then install the E2E dependencies and run the tests:

```
cd e2e
npm install
npx playwright install chromium
npm test
```

If PowerShell blocks script execution on your machine, run this once in the active terminal session before the commands above:

```
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
```
