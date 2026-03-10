# MVP Status

Last updated: 2026-03-10

## Current Baseline

- `main` is the source of truth.
- Latest merged commit on `main`: `34499be` `merge: storefront dealer finance v1`
- Local branch in workspace right now: `codex/feature/discount-single-store-policy`
- Local user note intentionally not committed:
  - `/Users/baris/Documents/Tedarik-SAAS/DISCOUNT_SCOPE_MANUAL_TESTS.md`

## Done

### Multi-store / tenant foundation

- Supplier isolation is built around `Store`.
- Store owner admin scope exists.
- Store-scoped entity visibility is in place for core B2B admin flows.

### Dealer domain

- Dealer entity and dealer-customer mapping exist.
- Dealer payment method authorization exists.
- Dealer/store relationship is enforced.

### Finance v1

- Open account payment plugin exists.
- Dealer credit limit, current debt, available credit exist.
- Dealer transaction ledger exists.
- Dealer collections exist.
- Financial instrument foundation exists:
  - `Check`
  - `Promissory note`
- Audit timeline exists.
- Allocation foundation exists.

### Storefront finance v1

- `My Account > Dealer Finance`
- Read-only finance summary
- Read-only dealer transactions
- Read-only financial instruments
- Checkout payment-step finance summary

### Discounts

- Discount management is store-aware.
- Store owner discount scope exists.
- Dealer segments exist and are store-based.
- Dealer segment discount rule exists:
  - `Must be in dealer segment`
- Discount policy is now single-store.

## Active Decisions

- `DealerSegment` is store-based and not shared across stores.
- Discounts are treated as single-store for this project.
- Segment rule uses:
  - `1 requirement = 1 segment`
- Multiple segment targeting is handled with multiple requirements / requirement groups.
- Smoke/E2E investment is deprioritized.
- Primary validation approach going forward:
  - service tests
  - manual tests

## Must Finish For MVP

### 1. Discount functional hardening

- Confirm real admin usage flow for:
  - segment create/edit
  - dealer edit segment assignment
  - discount requirement configuration
- Close any small runtime bugs from manual testing.
- Update manual test notes to reflect single-store discount policy.

Relevant files:
- `/Users/baris/Documents/Tedarik-SAAS/DISCOUNT_SCOPE_MANUAL_TESTS.md`
- `/Users/baris/Documents/Tedarik-SAAS/nopCommerce_4.90.3_Source/src/Plugins/Nop.Plugin.DiscountRules.DealerSegments`

### 2. Service-level coverage for dealer segments / discounts

- Add service/integration-style tests for:
  - dealer segment mapping service
  - dealer segment requirement validation logic
  - single-store discount rule assumptions

## Nice To Have Before MVP Freeze

- Short operator docs:
  - how StoreOwner creates dealer
  - how dealer gets segment
  - how discount rule is configured
  - how open account works

- Final localization cleanup if raw keys appear in UI.

## Later

- Deeper check/promissory note lifecycle
- Manual allocation / matching improvements
- Advanced risk rules
- Broader Playwright regression
- Dealer-specific discount rule revival if still needed:
  - `Must be assigned to dealer`

## Manual Test Docs

- `/Users/baris/Documents/Tedarik-SAAS/DEALER_UI_MANUAL_TESTS.md`
- `/Users/baris/Documents/Tedarik-SAAS/OPEN_ACCOUNT_MANUAL_TESTS.md`
- `/Users/baris/Documents/Tedarik-SAAS/STOREOWNER_MANUAL_TESTS.md`
- `/Users/baris/Documents/Tedarik-SAAS/DISCOUNT_SCOPE_MANUAL_TESTS.md`
- `/Users/baris/Documents/Tedarik-SAAS/DEALER_FINANCE_FIXTURE.md`

## Recommended Next Step

1. Finish discount functional hardening with manual testing and bug-fix pass.
2. Then add service tests for dealer segment / discount behavior.
