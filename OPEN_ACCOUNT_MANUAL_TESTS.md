# Open Account Manual Test Plan

Bu dokuman `codex/feature/dealer-transaction-admin-ui` branch'i icindir.

## 0. Buradan Basla

Asagidaki sirayla git:

1. `P1 -> P2 -> P3` (plugin kurulum)
2. `E1 -> E2 -> E3 -> E4` (hizli uc-tan-uca akis)
3. `OA1 -> OA5` (detay kontroller)

## 1. On Kosullar

- Branch: `codex/feature/dealer-transaction-admin-ui`
- Build:
  - `dotnet build /Users/baris/Documents/Tedarik-SAAS/nopCommerce_4.90.3_Source/src/Presentation/Nop.Web/Nop.Web.csproj -c Debug --no-restore --nologo -m:1 /nr:false`
- Test verisi:
  - Store: `Store A`
  - Dealer: `Dealer A` (Store A)
  - Musteri: `buyer-a1` (Dealer A'ya mapli)
  - Urun: checkout yapilabilecek en az 1 urun

## 2. Plugin Kurulum (P1-P3)

### P1 - Plugin listede gorunuyor mu?

- [ ] Admin ile giris yap
- [ ] `Configuration > Local plugins`
- [ ] `Open Account (Payments.OpenAccount)` satiri gorunmeli
- [ ] Gorunmuyorsa `Reload list of plugins` yap ve sayfayi yenile

### P2 - Plugin install / update

- [ ] `Open Account` satirinda `Install` tikla
- [ ] Uygulama restart isterse restart et
- [ ] Tekrar `Configuration > Local plugins` ac
- [ ] Plugin durumu `Installed` olmali

### P3 - Payment method aktivasyonu + store limiti

- [ ] `Configuration > Payment > Methods`
- [ ] `Open Account` methodu aktif olmali
- [ ] `Configuration > Local plugins > Open Account > Edit`
- [ ] `Limited to stores` alaninda test store'u (`Store A`) secili olmali

## 3. Hizli Uctan Uca Akis (E1-E4)

### E1 - Dealer finans ayari

- [ ] `Customers > Dealers > Edit Dealer A`
- [ ] `Enable open account = true`, `Credit limit = 1000`
- [ ] Kaydet
- [ ] `Current debt = 0`, `Available credit = 1000` gorunmeli

### E2 - Checkout ve borc artisi

- [ ] `buyer-a1` ile checkout'a git
- [ ] Sepet toplami `200` gibi limit altinda olsun
- [ ] Odeme adiminda `Open account` gorunmeli
- [ ] Siparisi `Open account` ile tamamla
- [ ] Dealer edit ekranina donup debt'in arttigini dogrula

### E3 - Tahsilat ve borc azalis

- [ ] `Orders` ekraninda ilgili siparisi `Mark as paid` yap
- [ ] Dealer edit ekranini yenile
- [ ] `Current debt` dusmeli, `Available credit` artmali

### E4 - Manuel hareket

- [ ] Dealer editte `Manual transaction entry` kartinda `Manual credit adjustment` + `50` gir, `Add transaction`
- [ ] Debt 50 dusmeli
- [ ] `Manual debit adjustment` + `30` gir, `Add transaction`
- [ ] Debt 30 artmali

## 4. Detay Senaryolari (OA1-OA5)

### OA1 - Open account method visibility (uygun limit)

- [ ] Limit yeterliyken checkout'ta method gorunmeli

### OA2 - Open account method visibility (yetersiz limit)

- [ ] Credit limit `50` yap
- [ ] Sepet toplam `200` ile checkout ac
- [ ] `Open account` listede olmamali

### OA3 - Merkezi kontrol (bypass)

- [ ] Craft POST veya eski secimle `Payments.OpenAccount` dene
- [ ] Siparis olusmamali
- [ ] Hata: `Open account credit limit is exceeded...` veya `Selected payment method is not available`

### OA4 - Ledger tablosu kontrolu

- [ ] DB'de `DealerTransaction` tablosunu kontrol et
- [ ] Siparis sonrasi `OpenAccountOrder` + `Debit` kaydi olmali
- [ ] `Mark as paid` sonrasi `OpenAccountCollection` + `Credit` kaydi olmali

### OA5 - Dealer edit transaction listesi

- [ ] `Dealer transactions` karti gorunmeli
- [ ] Son hareketler (siparis, tahsilat, manuel hareketler) listelenmeli

### OA6 - Dealer transactions rapor sayfasi

- [ ] `Customers > Dealer transactions` ekranina git
- [ ] Store/Dealer/Tarih filtreleri calisiyor olmali
- [ ] Siparis/tahsilat/manuel hareket satirlari listede gorunmeli
- [ ] `Summary` bolumunde `Total debit`, `Total credit`, `Net balance` degerleri hesaplanmali

## 5. Kapanis Kriteri

Asagidaki maddeler `OK` ise Open Account + Dealer Ledger akisi kapanabilir:

- [ ] Plugin kurulum/aktiflestirme/store limitleme adimlari sorunsuz
- [ ] Open account sadece yetkili ve limit yeterli durumda secilebiliyor
- [ ] Bypass engeli calisiyor
- [ ] Debt/available credit hesaplari ledger ile tutarli
- [ ] Manuel credit/debit girisleri hesaplara yansiyor
- [ ] Dealer transactions rapor ekrani filtre ve ozetleri dogru calisiyor
