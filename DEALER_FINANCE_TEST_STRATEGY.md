# Dealer Finance Test Strategy

## Current state

Dealer finance tarafinda su an uc test katmani var:

1. Service-level integration tests
2. PostgreSQL smoke/integration runner
3. Manuel UI test dokumanlari

Bu katmanlarin amaci farkli:

1. Service tests
   - Hizli calisir.
   - Cekirdek business rule regresyonunu yakalar.
   - Ornek: allocation FIFO, allocation cancel.

2. PostgreSQL smoke runner
   - Gercek migration + gercek fixture + gercek app login yolunu kontrol eder.
   - SQLite test altyapisinin yakalayamadigi schema drift ve FK sorunlarini yakalar.

3. Manuel UI testler
   - Kullanici akislarini ve ekran gorunumunu dogrular.
   - Ornek: collection detail, statement, store owner scope.

## What is automated today

### Service tests

Dosya:
- `/Users/baris/Documents/Tedarik-SAAS/nopCommerce_4.90.3_Source/src/Tests/Nop.Tests/Nop.Services.Tests/Customers/DealerServiceTests.cs`

Senaryolar:
- `CreateAutomaticAllocationsAsync`
- `CancelDealerTransactionAllocationsByCollectionAsync`

Calistirma:

```bash
dotnet test /Users/baris/Documents/Tedarik-SAAS/nopCommerce_4.90.3_Source/src/Tests/Nop.Tests/Nop.Tests.csproj --no-restore --filter FullyQualifiedName~Nop.Tests.Nop.Services.Tests.Customers.DealerServiceTests -m:1 /nr:false
```

### PostgreSQL smoke runner

Script:
- `/Users/baris/Documents/Tedarik-SAAS/scripts/dealer-finance-integration/run-smoke.sh`

Ne kontrol eder:
- fixture apply
- fixture verify
- app login page ve antiforgery token
- `owner-a@test.local` ile login POST -> `/admin` redirect
- auth cookie set ediliyor mu
- `DealerCollection`, `DealerFinancialInstrument`, `DealerTransactionAllocation`, `DealerFinanceAuditLog` tablolarinin varligi
- fixture customer/dealer/payment mapping verileri
- finance localization resource kayitlari

Calistirma:

```bash
bash /Users/baris/Documents/Tedarik-SAAS/scripts/dealer-finance-integration/run-smoke.sh
```

## Important technical note

Mevcut `Nop.Tests` altyapisi SQLite kullaniyor. Dealer finance tarafinda yeni tablolar ve FK iliskileri geldiginde tek basina SQLite yeterli degil.

Bunu test ederken iki sey goruldu:

1. Test DB yeni finance tablolarini otomatik tasimadi
2. PostgreSQL tarafinda dogru olan migration/FK davranislari SQLite tarafinda farkli gorunebiliyor

Bu nedenle finance icin dogru model:

1. Hizli servis testleri
2. Gercek PostgreSQL smoke/integration
3. Browser E2E

## Why Playwright is still needed

`curl` ile login POST ve response-level smoke yapilabiliyor. Fakat nop admin auth/cookie davranisi ve UI form akislari icin bu yeterli degil.

Playwright gerekli cunku:

1. Gercek admin oturumunu browser gibi kullanir
2. Antiforgery, cookie, redirect ve JS akislarini dogru tasir
3. Admin form postback, select list refresh, grid content ve button akislarini dogrular
4. Storefront checkout ve admin ekranlarini ayni testte baglayabilir

## Recommended next layer

Bir sonraki branch:

- `codex/test/dealer-finance-playwright`

Minimum Playwright smoke senaryolari:

1. `owner-a@test.local` login -> `Dealer collections` page visible
2. `CreateCollection` formu -> kayit olusur
3. `CollectionDetails` -> allocation/audit alanlari gorunur
4. `FinancialInstrumentDetails` -> status action calisir
5. `buyer-a1@example.com` -> open account checkout gorunur
6. `owner-a@test.local` -> Store B datasina erisim engellenir

## Suggested execution policy

Gelistirme sirasinda:

1. Service tests her committe
2. PostgreSQL smoke her feature branch sonunda
3. Playwright smoke merge oncesi

Bu finance modulu icin maliyet/fayda dengesi dogru siralama budur.
