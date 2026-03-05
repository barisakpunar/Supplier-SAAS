# Dealer Finance Fixture

Bu dokuman `codex/test/dealer-finance-fixture` branch'i icindir.

## Amac

Bu fixture, `dealer-collections` ve `open account` testlerini ayni baslangic durumundan calistirmak icin lokal Postgres veritabanini normalize eder.

Kapsam:
- fixture kullanicilari
- dealer/dealer-customer mapping
- dealer open account profili
- dealer payment methods
- cart / customer checkout state temizligi
- store-specific fixture urunleri
- dealer transaction / dealer collection tablolarinin sifirlanmasi

## Varsayimlar

Bu fixture su varsayimlarla calisir:

- Docker container adi: `nopcommerce_postgres_server`
- Veritabani adi: `supplier`
- nopCommerce sample data ile daha once kurulmus
- Bu branch'teki migration'lar bir kez uygulanmis

Migration on kosulu:
- `DealerCollection` tablosu yoksa script bilerek durur.
- Bu durumda web uygulamasini bu branch'te bir kez acip nop migration'larini calistir.

## Fixture Kullanici Bilgileri

Tum fixture kullanicilarinda sifre aynidir:

- sifre: `Test123!`

Kullanicilar:
- `owner-a@test.local` -> `Store A`, `StoreOwners`
- `owner-b@test.local` -> `Store B`, `StoreOwners`
- `buyer-a1@example.com` -> `Store A`, registered
- `buyer-b1@example.com` -> `Store B`, registered

## Fixture Dealer Yapisi

- `Dealer A` -> `Store A`
  - open account: `enabled`
  - credit limit: `1000`
  - mapped customers:
    - `owner-a@test.local`
    - `buyer-a1@example.com`
  - allowed payment methods:
    - `Payments.CheckMoneyOrder`
    - `Payments.OpenAccount`

- `Dealer B` -> `Store B`
  - open account: `disabled`
  - mapped customers:
    - `owner-b@test.local`
    - `buyer-b1@example.com`
  - allowed payment methods:
    - `Payments.CheckMoneyOrder`

## Fixture Catalog Yapisi

Store-specific fixture kayitlari:
- `Fixture Category A` -> `Store A`
- `Fixture Category B` -> `Store B`
- `Fixture Product A` -> `Store A`, fiyat `200`
- `Fixture Product B` -> `Store B`, fiyat `150`

Bu urunler checkout smoke test icin kullanilabilir.

## Komutlar

Fixture uygula:

```bash
/tmp/tedarik-dealer-collections/scripts/dealer-finance-fixture/apply.sh
```

Fixture dogrula:

```bash
/tmp/tedarik-dealer-collections/scripts/dealer-finance-fixture/verify.sh
```

Istege bagli env var'lar:

```bash
DEALER_FIXTURE_DB_CONTAINER=nopcommerce_postgres_server
DEALER_FIXTURE_DB_NAME=supplier
DEALER_FIXTURE_DB_USER=postgres
```

## Bir Kerelik Manuel Kontrol

Bu fixture DB state'i normalize eder, fakat plugin descriptor `Limited to stores` bilgisini source/runtime plugin descriptor tarafindan yonetildigi icin zorla yazmaz.

Testten once admin panelde sunu kontrol et:

1. `Configuration > Local plugins > Open Account > Edit`
2. `Limited to stores` icinde test senaryona uygun store secili olsun
3. `Configuration > Payment > Methods`
4. `Open Account`, `Check / Money Order`, `Credit Card` aktif olsun

Not:
- `paymentsettings.activepaymentmethodsystemnames` fixture tarafinda normalize edilir.
- Plugin descriptor store limiti ise UI'dan dogrulanmalidir.

## Beklenen Sonuc

Fixture uygulandiktan sonra:
- dealer transaction tablosu bos olur
- dealer collection tablosu bos olur
- fixture kullanicilarinin cart'lari bos olur
- `Dealer A` debt baslangici `0` olur
- `Dealer A` available credit `1000` olur
- `Fixture Product A` ile open account checkout testi tek urunle yapilabilir
