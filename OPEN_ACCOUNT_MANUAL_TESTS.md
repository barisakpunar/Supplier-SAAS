# Open Account Manual Test Plan

Bu dokuman `codex/feature/open-account-limit-ledger` branch'i icindir.

## 1. Kapsam

- Dealer finans profili (`OpenAccountEnabled`, `CreditLimit`)
- Dealer edit ekraninda finans ozeti (`Current debt`, `Available credit`)
- Checkout'ta open account gorunurlugu (limit + yetki)
- Siparis olusturma aninda merkezi limit kontrolu (`Payments.OpenAccount` plugini)
- Tahsilat sonrasi limitin geri acilmasi (payment status `Paid`)
- Dealer transaction ledger kayitlari (`DealerTransaction`)

## 2. On Kosullar

- Branch: `codex/feature/open-account-limit-ledger`
- Build:
  - `dotnet build /Users/baris/Documents/Tedarik-SAAS/nopCommerce_4.90.3_Source/src/Presentation/Nop.Web/Nop.Web.csproj -c Debug --no-restore --nologo -m:1 /nr:false`
- `Payments.OpenAccount` plugin aktif ve ilgili store'a limitli
- Test dealer (`Dealer A`) + test customer (`buyer-a1`) mapping hazir

## 3. Test Senaryolari

### OA1 - Finans profil kaydi

- [ ] Admin ile `Customers > Dealers > Edit Dealer A`
- [ ] `Enable open account = true`, `Credit limit = 1000`
- [ ] Kaydet
- [ ] Sayfa tekrar acildiginda degerler korunmali

### OA2 - Finans ozeti goruntuleme

- [ ] Dealer edit ekraninda `Current debt` ve `Available credit` alanlari gorunmeli
- [ ] Ilk durumda `Current debt = 0`, `Available credit = 1000` beklenir

### OA3 - Checkout method visibility (uygun limit)

- [ ] `buyer-a1` ile checkout'a git
- [ ] Sepet toplami limitten dusuk olsun (ornek `200`)
- [ ] `Open account` odeme yontemi listede gorunmeli

### OA4 - Checkout method visibility (yetersiz limit)

- [ ] `Credit limit` degerini kucult (ornek `50`)
- [ ] `buyer-a1` checkout'ta sepet toplam `200`
- [ ] `Open account` odeme yontemi listeden kalkmali

### OA5 - Merkezi servis kontrolu (bypass guvenlik)

- [ ] Checkout adiminda eski secili method veya craft POST ile `Payments.OpenAccount` dene
- [ ] Siparis olusmamali
- [ ] Hata mesaji: `Open account credit limit is exceeded...` veya `Selected payment method is not available`

### OA6 - Borc artis kontrolu

- [ ] Limit uygunken `Open account` ile siparis ver (ornek `300`)
- [ ] Dealer edit ekranini tekrar ac
- [ ] `Current debt` artmali, `Available credit` azalmali

### OA7 - Tahsilat sonrasi limit geri acilmasi

- [ ] Admin `Orders` ekraninda ilgili siparisi `Mark as paid` yap
- [ ] Dealer edit ekranini yenile
- [ ] `Current debt` dusmeli, `Available credit` artmali

### OA8 - Dealer transaction ledger kontrolu

- [ ] DB'de `DealerTransaction` tablosunu kontrol et
- [ ] OA6 sonrasi `OpenAccountOrder` tipinde `Debit` bir kayit olusmali
- [ ] OA7 sonrasi ayni siparis icin `OpenAccountCollection` tipinde `Credit` bir kayit olusmali

### OA9 - Dealer edit ekraninda hareket listesi

- [ ] `Customers > Dealers > Edit Dealer A` ac
- [ ] `Dealer transactions` karti gorunmeli
- [ ] Son hareketler listesinde OA6/OA7 kayitlari satir olarak gorunmeli

## 4. Kapanis Kriteri

Asagidaki maddeler `OK` ise Open Account V1 kapanabilir:

- [ ] Open account sadece yetkili ve limit yeterli durumlarda secilebiliyor
- [ ] Merkezi servis kontrolu ile bypass engelleniyor
- [ ] Debt/available credit hesaplari siparis ve tahsilatla tutarli ilerliyor
- [ ] DealerTransaction tablosu siparis/tahsilat akisini dogru yansitiyor
- [ ] Dealer edit ekranindaki hareket listesi ledger kayitlariyla tutarli
