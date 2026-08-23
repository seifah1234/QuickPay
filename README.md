# QuickPay

QuickPay is a digital payment platform inspired by InstaPay — not a clone of it. It takes the same core idea (send/receive money, link accounts, split payments) and extends it with multiple personal wallets, shared group wallets, smart payment splitting, and real payment gateway integration. It is a **Digital Payment Platform**, not a full banking system.

Built as a .NET graduation project using **Clean Architecture** and multiple design patterns, split across a 5-person team.

---

## Table of Contents

- [What QuickPay Does](#what-quickpay-does)
- [Architecture](#architecture)
- [Modules](#modules)
- [Tech Stack](#tech-stack)
- [Feature Priority](#feature-priority)
- [Stretch Features](#stretch-features)
- [Team & Ownership](#team--ownership)
- [Workflow Rules](#workflow-rules)
- [Timeline](#timeline)
- [Getting Started](#getting-started)
- [Branching & PR Convention](#branching--pr-convention)
- [Database Migrations](#database-migrations)

---

## What QuickPay Does

QuickPay lets a user:

- Send and receive money
- Organize their money across multiple personal wallets ("Money Spaces")
- Link more than one bank account or external wallet
- Create wallets shared with other people — a trip, a shared apartment, a team, a savings group
- Split payments smartly between several users (Equal / Custom / Percentage)
- Track every transaction and get a clearer picture of their spending and budget

---

## Architecture

QuickPay follows **Clean Architecture** principles, chosen to properly separate concerns across auth, wallets, shared wallets, payments, smart splitting, notifications, and security — while allowing the codebase to be split cleanly across the team.

Key architectural notes:

- Each feature has its own **Controller** and **Service** class, so no two people ever edit the same file.
- Each task is owned end-to-end by one person, from data access through business logic to the presentation layer.
- Schema changes are made exclusively through **EF Core Migrations** committed to Git — never a raw database file.
- Real-time updates (Notifications, Shared Wallet activity, Smart Split reminders, Payment Gateway status) are delivered over **SignalR**, tied to the Observer pattern on the backend.
- Transfer methods and Smart Split types are implemented using the **Strategy pattern** (one strategy per transfer method; one strategy per split type).

---

## Modules

| # | Module | Core Features |
|---|--------|----------------|
| 1 | **Identity & Profile** | Authentication (Register, Login, JWT, Refresh Token, OTP), External Account Login (Google/Facebook via OAuth 2.0), User Profile |
| 2 | **Wallet Core** | Wallet (Balance, Deposit, Withdraw, Transfer), Linked Accounts, Payment Gateway Integration, Multiple Wallets (Money Spaces), Shared Wallet |
| 3 | **Payments & Transfers** | Money Transfer (phone/username/account number), Advanced Smart Split (Equal / Custom / Percentage) |
| 4 | **Notifications & History** | Real-time Notifications (SignalR), Transaction History (filter/search) |
| 5 | **Security & Analytics** | Security Center (change password, devices, active sessions) |

### 01 | Identity & Profile

Handles who the user is and how they get into the system. Foundational module — everything else depends on it for identifying the current user.

- **Authentication (core):** Register, Login, JWT, Refresh Token, OTP
- **External Account Login:** OAuth 2.0 sign-in via Google/Facebook; on first login, links or creates the local user and issues the same JWT/Refresh Token pair as standard auth
- **User Profile:** Edit personal data, upload profile photo, identity verification

**Key entities:** `User`, `Device`, `Session`, `ExternalLogin`

### 02 | Wallet Core

Owns the user's balance(s) and any connected external accounts, personal or shared. Requires careful handling of balances and concurrency — multiple wallets per user, multiple members writing to the same Shared Wallet, and asynchronous gateway callbacks that must update the ledger safely.

- **Wallet:** Balance, Deposit, Withdraw, Transfer
- **Linked Accounts:** Connect one or more bank accounts/wallets
- **Payment Gateway Integration:** Real integration (e.g. Paymob, Stripe) so Deposit/Withdraw/Linked Accounts move real money; handles charge/payout requests, gateway callbacks/webhooks (idempotent, signature-verified), and ledger reconciliation
- **Multiple Wallets (Money Spaces):** Create, rename, delete personal wallets (Main, Savings, Travel, etc.), each with independent balance and history
- **Shared Wallet:** Jointly-owned wallets with Owner/Members, add/remove-member management, shared visibility, dedicated transaction history, transfers in/out, admin permissions

**Key entities:** `Wallet`, `BankAccount`, `Transaction`, `SharedWallet`, `SharedWalletMember`, `PaymentGatewayTransaction`

### 03 | Payments & Transfers

Everything about moving money between users, individually or split across a group. Smaller in feature count but heavy in logic.

- **Money Transfer:** Send via phone number, username, or account number
- **Advanced Smart Split:** Equal Split, Custom Amount per person, or Percentage Split; per-participant Pending/Paid status; reminders; auto-close once everyone has settled

**Key entities:** `Payment`, `Transaction`, `SplitGroup`, `SplitParticipant`

### 04 | Notifications & History

Keeps the user informed and gives visibility into past activity.

- **Notifications:** Real-time alerts over SignalR (WebSocket hub, fallback to long-polling), persisted so missed alerts show on next login. Also the channel for Shared Wallet activity, Payment Gateway status updates, and Smart Split reminders.
- **Transaction History:** Filtering and searching past transactions

**Key entities:** `Notification`, `Transaction` (read-side)

### 05 | Security & Analytics

Protects the account and, if time allows, surfaces spending insight.

- **Security Center:** Change password, view registered devices, manage active sessions

**Key entities:** `AuditLog`, `Device`, `Session`

---

## Tech Stack

- **.NET** — Clean Architecture (DAL / BLL / PL layering)
- **Entity Framework Core** — data access + migrations
- **SignalR** — real-time notifications
- **JWT + Refresh Tokens** — authentication
- **OAuth 2.0** (Google / Facebook) — external login
- **Payment Gateway** (e.g. Paymob / Stripe) — real money movement

---

## Feature Priority

All features are **core / essential** except the 5 stretch features listed below. Stretch features are only tackled if time remains, in this order:

1. Scheduled Payments
2. QR Payment
3. Request Money
4. 2FA
5. Spending Analytics

## Stretch Features

Only added if the team agrees together **after the Aug 22 phase gate**, and only as an additional task inside Phase 3 — never a reason to start a phase early.

| Priority | Feature | Extends |
|----------|---------|---------|
| 1 | Scheduled Payments — recurring automatic transfers | Payments & Transfers |
| 2 | QR Payment — generate & pay via QR code | Payments & Transfers |
| 3 | Request Money — ask someone for money | Payments & Transfers |
| 4 | 2FA — two-factor auth on top of login | Identity & Profile |
| 5 | Spending Analytics — statistics & charts on spending | Security & Analytics |

---

## Team & Ownership

| Member | Primary Area |
|--------|---------------|
| **A** | Identity & Profile |
| **B** | Wallet Core (incl. Payment Gateway, Multiple Wallets) |
| **C** | Payments & Transfers (incl. Advanced Smart Split, Shared Wallet) |
| **D** | Notifications & History (SignalR) |

---

## Workflow Rules

- The project moves through **Phases**. Everyone works on one phase at a time — no one starts next-phase work early, even if their own part is done. Finished early = review or test others' work instead.
- A phase is only **"complete"** when every task in it is merged into `develop`.
- Each task is owned end-to-end by one person: **DAL → BLL → PL** (database to screen).
- Each feature has its own Controller and Service class — no two people ever edit the same file.
- Frontend pages are agreed on as a team first; each person builds their own Partial View; integration happens **after** development, not during.
- **One branch per task** (1–3 days max), **one branch = one PR**, PRs merge into `develop`.
- Schema changes go through **EF Core Migrations** committed to Git — never a raw database file. Whoever merges second rebases their migration on the latest one.

---

## Timeline

**Team of 4 | Aug 9 – Aug 28**

### Phase 0 — Setup & Contracts (Aug 9, 1 day)
All: repo & Clean Architecture setup, branch/PR rules, finalize DB schema + first EF Core migration from the ERD, agree on frontend page breakdown, agree on domain event shapes between modules, set up `.env.example`, build a temporary Auth stub.

### Phase 1 — Foundation (Aug 10–15, 6 days)
| Member | Tasks |
|--------|-------|
| A | Register, Login (JWT + Refresh Token), OTP |
| B | Wallet balance, Deposit, Withdraw (simulated); Multiple personal wallets (create/rename/delete) |
| C | Money Transfer (phone/username/account number); balance validation & rollback handling |
| D | SignalR hub setup; Notifications persist & push; Transaction History basic list view |

**Phase Gate — Aug 15:** all tasks merged into `develop` and migrations verified together before Phase 2 starts.

### Phase 2 — Depth & Integrations (Aug 16–22, 7 days)
| Member | Tasks |
|--------|-------|
| A | External Account Login (Google/Facebook OAuth 2.0); Security Center (devices, sessions) |
| B | Payment Gateway integration (charge/payout, webhook handling with signature verification + idempotency); Linked bank accounts/external wallets |
| C | Advanced Smart Split (Equal, Custom, Percentage); participant status & reminders; Shared Wallet (owner/member roles, add/remove members, shared balance & history) |
| D | Wire Notifications into events from every other module; Transaction History filtering & search; Admin Dashboard shell |

**Phase Gate — Aug 22:** all tasks merged into `develop` and migrations verified together before Phase 3 starts.

### Phase 3 — Admin & Integration (Aug 23–25, 3 days)
| Member | Tasks |
|--------|-------|
| A | Admin: user management view |
| B | Admin: wallets & payment gateway transactions monitoring |
| C | Admin: transactions & shared wallets monitoring |
| D | Admin: audit log view; assemble Admin partial views into the main page |
| All | Enforce Admin/User authorization on every endpoint; cross-module integration testing (Shared Wallet + Smart Split + Notifications + Gateway callbacks); review balance concurrency across wallets |

### Phase 4 — Testing, Docs & Deployment (Aug 26–27, 2 days)
All: run final migrations against a clean deployment/demo database, complete unit & integration tests, API documentation, deploy, prepare presentation materials split by module owner.

### Buffer Day (Aug 28)
All: fix remaining issues, rehearse demo & presentation, final submission.

> Stretch features are only added if the team agrees together after the Aug 22 phase gate, and only as an additional task inside Phase 3.
> Because phases are gated, the team's pace is set by whichever task in a phase takes longest — reviewing/testing teammates' open PRs during a phase is part of everyone's job, not optional.

---

## Getting Started

```bash
# Clone the repo
git clone https://github.com/seifah1234/QuickPay.git
cd QuickPay

# Restore dependencies
dotnet restore

# Apply EF Core migrations
dotnet ef database update --project QuickPay.DAL

# Run the application
dotnet run --project QuickPay.PL
```

> Copy `.env.example` to `.env` and fill in the required environment variables (DB connection string, JWT secret, OAuth client IDs/secrets, Payment Gateway API keys, SignalR config) before running.

### Project Structure

```
QuickPay/
├── QuickPay.DAL/     # Data Access Layer — EF Core, entities, migrations
├── QuickPay.BLL/      # Business Logic Layer — services, domain logic
├── QuickPay.PL/        # Presentation Layer — controllers, views/API endpoints
├── QuickPay.slnx
└── README.md
```

---

## Branching & PR Convention

- One branch per task, scoped to 1–3 days of work
- One branch = one Pull Request
- PRs target `develop`, never `master` directly
- Suggested branch naming: `feature/<module>-<short-description>` (e.g. `feature/payments-smart-split-equal`)
- Every PR should be reviewed by at least one teammate before merging

---

## Database Migrations

- All schema changes go through EF Core Migrations, committed to Git
- Never commit or share a raw `.mdf`/`.bak` database file
- If your migration isn't the first to merge in a phase, rebase it on the latest migration in `develop` before opening your PR
- Migrations are verified together as a team at each Phase Gate (Aug 15, Aug 22) and again from a clean state during Phase 4
