# Dealer UI Manual Test Plan

Bu dokuman `codex/feature/dealer-payment-admin-ui` branch'i icindir.

## 1. Kapsam

- Admin panelde `Customers > Dealers` ekrani
- Dealer CRUD (create/edit)
- Dealer-customer mapping
- Dealer allowed payment methods mapping
- Checkout tarafinda dealer bazli payment filtering
- StoreOwner yetkisi ile dealer ekranina scoped erisim

## 2. On Kosullar

- Branch: `codex/feature/dealer-payment-admin-ui`
- Uygulama build aliyor:
  - `dotnet build /Users/baris/Documents/Tedarik-SAAS/nopCommerce_4.90.3_Source/src/Presentation/Nop.Web/Nop.Web.csproj -c Debug --no-restore --nologo -m:1 /nr:false`
- En az 2 store var (Store A, Store B)
- En az 3 registered customer var:
  - `dealer-owner-a@example.com` (Store A)
  - `buyer-a1@example.com` (Store A)
  - `buyer-b1@example.com` (Store B)
- Payment pluginleri aktif:
  - `Payments.Manual`
  - `Payments.CheckMoneyOrder`

## 3. Admin UI Senaryolari

### A1 - Menu gorunurlugu

- [ ] Admin ile giris yap
- [ ] `Customers` menusu altinda `Dealers` itemi gorunuyor
- [ ] `Customers > Dealers` sayfasi aciliyor

### A2 - Dealer create

- [ ] `Customers > Dealers > Add new`
- [ ] `Name`, `Store`, `Active`, `Dealer customers`, `Allowed payment methods` alanlari gorunuyor
- [ ] Name: `Dealer A`, Store: `Store A`, Active: `true`
- [ ] Customers: `dealer-owner-a@example.com`, `buyer-a1@example.com` sec
- [ ] Payment methods: sadece `Payments.Manual` sec
- [ ] `Save`
- [ ] Listeye dondugunde `Dealer A` satiri gorunuyor
- [ ] `Customers` kolonu `2`
- [ ] `Payment methods` kolonu `Payments.Manual` iceriyor

### A3 - Dealer edit

- [ ] `Dealer A` satirinda `Edit`
- [ ] Store `Store A` olarak gorunuyor
- [ ] Customer listesinde secili customerlar korunmus
- [ ] Payment methods listesinde `Payments.Manual` secili
- [ ] `Payments.CheckMoneyOrder` da sec ve kaydet
- [ ] Listeye donup payment summary'de iki method da gorunuyor

### A4 - Bos payment mapping davranisi

- [ ] `Dealer A` edit ekraninda tum payment selection'lari kaldir
- [ ] Kaydet
- [ ] Liste ekraninda `Payment methods` kolonu `All active payment methods` olarak gorunuyor

### A5 - Store degistirme ve customer filtreleme

- [ ] `Dealer A` edit ekraninda Store'u `Store B` yap
- [ ] `buyer-a1@example.com` secili kalsa bile kaydet
- [ ] Tekrar edit ac
- [ ] `buyer-a1@example.com` mapping'den dusmus olmali (store uyumsuz customerlar temizlenir)

## 4. Checkout Davranis Senaryolari

### C1 - Mapping yoksa tum aktif methodlar

- [ ] Dealer A'da payment mapping bos olsun
- [ ] Dealer A'ya bagli bir customer ile storefront'ta checkout'a git
- [ ] Aktif payment methodlarin tamami gorunuyor

### C2 - Mapping varsa sadece secilen methodlar

- [ ] Dealer A'da sadece `Payments.Manual` sec
- [ ] Dealer A customer'i ile checkout'a git
- [ ] Sadece `Payments.Manual` gorunuyor

### C3 - Dealer pasif ise method yok

- [ ] Dealer A `Active=false` yap
- [ ] Dealer A customer'i ile checkout'a git
- [ ] Payment method listesi bos (odeme adiminda method cikmiyor)
- [ ] Test sonrasi tekrar `Active=true` yap

## 5. StoreOwner Senaryolari

> Bu senaryo icin test edilen kullanici `StoreOwners` rolunde olmali ve bir dealer'a mapli olmali.

### S1 - Menu ve sayfa erisimi

- [ ] StoreOwner ile admin panel girisi yap
- [ ] `Customers > Dealers` menusu gorunuyor
- [ ] `Dealers` listesinde sadece kendi dealer'i gorunuyor
- [ ] `Add new` butonu gorunmuyor

### S2 - Scope enforcement

- [ ] URL'den farkli dealer id acmaya calis (`/Admin/Dealer/Edit/{baskaId}`)
- [ ] Erişim engellenmeli (`Access denied`)

### S3 - Store sabitleme

- [ ] Kendi dealer edit ekraninda Store alani readonly
- [ ] Form post'ta store id ile oynansa bile kayit kendi store scope'u ile kalir

## 6. Negatif Testler

### N1 - Zorunlu alan kontrolu

- [ ] Dealer create ekraninda Name bos birak
- [ ] `Save`
- [ ] Validation hatasi gorunmeli

### N2 - Gecersiz store id

- [ ] Form post'ta StoreId 0 veya gecersiz id gonder
- [ ] Validation hatasi gorunmeli: `A valid store is required.`

## 7. Regresyon Kontrolu

- [ ] `Customers > Customers` liste ekrani normal calisiyor
- [ ] `Payments` plugin ayarlari bozulmadi
- [ ] Onceki StoreOwner menu kisitlari (Orders, Products, Categories vb.) devam ediyor

## 8. Beklenen Sonuc Ozet

- Dealer UI ile admin tarafindan dealer/customer/payment mapping yonetilebiliyor.
- Checkout'ta payment methodlar dealer mapping kurallarina gore filtreleniyor.
- StoreOwner ayni ekrani kendi scope'u ile gorebiliyor; cross-tenant erisim engelli.
