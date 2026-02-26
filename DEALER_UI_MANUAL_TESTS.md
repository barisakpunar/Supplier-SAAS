# Dealer + Payment Authorization Manual Test Plan

Bu dokuman `codex/feature/dealer-payment-admin-ui` branch'i icindir.

## 1. Kapsam

- Admin panelde `Customers > Dealers` ekrani
- Dealer CRUD (create/edit)
- Customer->Dealer mapping (`Customer Edit`)
- Dealer allowed payment methods mapping
- Checkout'ta dealer bazli payment filtering
- StoreOwner yetkisi ile store-scope dealer yonetimi

## 2. Adminde Store'a Payment Method Atama Nerede?

Store bazli payment method kapsamlandirmasi nop'ta plugin uzerinden yapilir:

1. `Configuration > Local plugins`
2. Ilgili payment plugin satiri `Edit`
3. `Limited to stores` alanindan store sec
4. `Is enabled` acik olmali

Ayrica global aktif/pasif kontrolu:

1. `Configuration > Payment > Methods`
2. Method `Is active` durumunu ac/kapat

Not: Dealer ekraninda gorulen payment method listesi, bu iki filtreyi zaten respect eder (aktif + store'a limitli pluginler).

## 3. Yetkilendirme Kurallari (Koddan Cikan Final Kurallar)

- Admin:
  - Tum store/dealer kayitlarini gorebilir.
  - Dealer payment method secimi sadece ilgili store'da aktif/gorunur pluginlerden olabilir.
- StoreOwner:
  - `Add new dealer` yapamaz.
  - Sadece kendi store'undaki dealer'lari listeler ve editler.
  - Payment method seciminde ust limit: kendi dealer'i icin izinli methodlar.
  - Kendi dealer mapping'i bos ise bu `all active in store` kabul edilir.
- Dealer ekraninda customer mapping:
  - Create/Edit ekraninda customer alani readonly listeleme amaclidir.
  - Gercek mapping `Customers > Customers > Edit > Dealer` alanindan yapilir.
- Checkout:
  - Dealer mapping bos ise tum aktif+store uygun odeme methodlari gelir.
  - Dealer mapping dolu ise sadece secili methodlar gelir.
  - Dealer pasif ise odeme methodu gelmez.

## 4. On Kosullar

- Branch: `codex/feature/dealer-payment-admin-ui`
- Build:
  - `dotnet build /Users/baris/Documents/Tedarik-SAAS/nopCommerce_4.90.3_Source/src/Presentation/Nop.Web/Nop.Web.csproj -c Debug --no-restore --nologo -m:1 /nr:false`
- En az 2 store: `Store A`, `Store B`
- En az 4 registered customer:
  - `owner-a@example.com` (Store A, StoreOwners rolunde)
  - `buyer-a1@example.com` (Store A)
  - `buyer-a2@example.com` (Store A)
  - `buyer-b1@example.com` (Store B)
- En az 2 payment plugin aktif ve store kapsamli:
  - `Payments.Manual`
  - `Payments.CheckMoneyOrder`

## 5. Full Test Cycle

### P0 - Store bazli plugin kapsam test hazirligi

- [ ] `Configuration > Local plugins` ekraninda `Payments.Manual` ve `Payments.CheckMoneyOrder` icin `Limited to stores` kontrol et
- [ ] `Store A` icin ikisini de acik birak
- [ ] `Store B` icin en az birini kapat (farki gozlemek icin)
- [ ] `Configuration > Payment > Methods` ekraninda iki method da `Is active=true`

### A1 - Dealer menu ve create/edit UI

- [ ] Admin ile giris
- [ ] `Customers > Dealers` menusu gorunuyor
- [ ] `Add new` ile create ekrani aciliyor
- [ ] `Dealer customers` alani sadece listeleme (secim yok)
- [ ] `Allowed payment methods` checkbox listesi gorunuyor

### A2 - Dealer create ve payment kaydi

- [ ] `Name=Dealer A`, `Store=Store A`, `Active=true`
- [ ] `Allowed payment methods`: `Payments.Manual` + `Payments.CheckMoneyOrder` sec
- [ ] `Save`
- [ ] Listeye donup `Payment methods` kolonunda iki methodu gor
- [ ] Edit'e girip secimlerin korundugunu dogrula

### A3 - Multi-select persistence (kritik)

- [ ] Edit ekraninda methodlardan birini kaldir, digerini birak
- [ ] `Save`
- [ ] Tekrar edit ac, ayni secim korunmus olmali
- [ ] Tekrar iki methodu da sec, `Save`
- [ ] Tekrar edit ac, iki secim de korunmus olmali

### A4 - Customer->Dealer mapping (tek kaynak)

- [ ] `Customers > Customers > Edit (buyer-a1)`
- [ ] `Dealer` alanindan `Dealer A` sec ve kaydet
- [ ] `Dealer A` edit ac, `Dealer customers` listesinde `buyer-a1` gorunmeli
- [ ] `Customer Edit`ten dealer'i `None` yap
- [ ] `Dealer A` editte listeden dustugunu dogrula

### A5 - Checkout filter

- [ ] `buyer-a1` ile storefront checkout'a git
- [ ] Dealer A'da iki method seciliyse ikisi de gorunmeli
- [ ] Dealer A'da sadece bir method birakip tekrar checkout test et
- [ ] Yalniz secili method gorunmeli

### A6 - Dealer pasif kontrolu

- [ ] `Dealer A` icin `Active=false`
- [ ] Dealer'a bagli musteri ile checkout test et
- [ ] Odeme adiminda method listesi bos olmali
- [ ] Test sonunda `Active=true` geri al

### S1 - StoreOwner liste scope

- [ ] `owner-a@example.com` ile admin giris
- [ ] `Customers > Dealers` ac
- [ ] Sadece `Store A` dealer'lari listelenmeli
- [ ] `Add new` gorunmemeli

### S2 - StoreOwner edit scope

- [ ] `Store A` icindeki farkli dealer kayitlarini acip edit yapabildigini dogrula
- [ ] URL ile `Store B` dealer edit acmaya calis
- [ ] `Access denied` beklenir

### S3 - StoreOwner payment upper-bound

- [ ] Admin ile `owner-a`'nin kendi dealer'inda sadece `Payments.Manual` secili birak
- [ ] `owner-a` ile diger `Store A` dealer edit ac
- [ ] Allowed payment methods listesinde sadece `Payments.Manual` gorunmeli
- [ ] Bu dealer'a `CheckMoneyOrder` kaydetmeye calis (UI veya craft POST)
- [ ] Kaydedilmemeli (storeowner kendinde olmayan methodu veremez)

### S4 - StoreOwner mapping bos davranisi

- [ ] Admin ile `owner-a`'nin kendi dealer payment mapping'ini bosalt (all active)
- [ ] `owner-a` ile tekrar diger dealer edit ac
- [ ] Store A'daki aktif methodlarin tamami secilebilir olmalı

## 6. Regresyon Kontrolu

- [ ] `Customers > Customers` create/edit normal calisiyor
- [ ] Customer editte `Dealer` dropdown store scope ile uyumlu
- [ ] `Configuration > Payment > Methods` sayfasi normal calisiyor
- [ ] `Configuration > Local plugins > Edit` (Limited to stores) kayitlari normal
- [ ] StoreOwner admin menu kisitlari (Orders, Products, Categories vb.) bozulmadi

## 7. Kapanis Kriteri

Asagidaki maddeler `OK` ise payment authorization konusu kapanabilir:

- [ ] Admin tarafinda store+plugin+dealer katmanlari beklenen sekilde calisiyor
- [ ] StoreOwner sadece kendi store scope'unda kalip ust limit disina cikamiyor
- [ ] Multi-select kaydi stabil
- [ ] Checkout filtreleme davranisi beklenen sonuclari veriyor
