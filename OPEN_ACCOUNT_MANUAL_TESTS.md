# Open Account + Collections Manual Test Plan

Bu dokuman `codex/feature/dealer-collections` branch'i icindir.

## 0. Buradan Basla

Asagidaki sirayla git:

1. `P1 -> P3` (plugin ve store kapsam hazirligi)
2. `B1 -> B5` (open account temel akis)
3. `C1 -> C8` (dealer collections akis)
4. `S1 -> S3` (statement/source dogrulama)

## 1. On Kosullar

- Branch: `codex/feature/dealer-collections`
- Build:
  - `dotnet build /Users/baris/Documents/Tedarik-SAAS/nopCommerce_4.90.3_Source/src/Presentation/Nop.Web/Nop.Web.csproj -c Debug --no-restore --nologo -m:1 /nr:false`
- Test verisi:
  - Store: `Store A`
  - Dealer: `Dealer A` (`Store A`)
  - StoreOwner: `owner-a@example.com`
  - Buyer: `buyer-a1@example.com` (`Dealer A`'ya mapli)
  - Urun: checkout yapilabilecek en az 1 urun
- Tavsiye edilen finans baslangici:
  - `Enable open account = true`
  - `Credit limit = 1000`

## 2. Plugin ve Store Hazirligi (P1-P3)

### P1 - Plugin kurulu mu?

- [ ] Admin ile giris yap
- [ ] `Configuration > Local plugins`
- [ ] `Open Account (Payments.OpenAccount)` gorunmeli
- [ ] Gerekirse `Reload list of plugins`

### P2 - Plugin aktif mi?

- [ ] `Open Account` durumu `Installed` olmali
- [ ] `Configuration > Payment > Methods`
- [ ] `Open Account` aktif olmali

### P3 - Store kapsami dogru mu?

- [ ] `Configuration > Local plugins > Open Account > Edit`
- [ ] `Limited to stores` icinde `Store A` secili olmali
- [ ] Test store'u disindaki store'lar ihtiyaca gore kapatilmali

## 3. Open Account Temel Akis (B1-B5)

### B1 - Dealer finans karti baslangic degerleri

- [ ] `Customers > Dealers > Edit Dealer A`
- [ ] `Enable open account = true`
- [ ] `Credit limit = 1000`
- [ ] `Save`
- [ ] `Current debt = 0`
- [ ] `Available credit = 1000`

### B2 - Siparis ile borc olusmasi

- [ ] `buyer-a1` ile storefront'a gir
- [ ] Sepete toplam `200` civari urun ekle
- [ ] Checkout payment step'te `Open account` gorunmeli
- [ ] Siparisi tamamla
- [ ] `Customers > Dealers > Edit Dealer A`
- [ ] `Current debt = 200`
- [ ] `Available credit = 800`

### B3 - Statement ekraninda order debit

- [ ] `Customers > Dealer transactions`
- [ ] `Dealer A` sec
- [ ] Son siparis satiri gorunmeli
- [ ] `Type = Open account order`
- [ ] `Debit = 200`
- [ ] `Credit = -`
- [ ] `Running balance` artmis olmali

### B4 - Limit yetersizse method gizlenmeli

- [ ] `Dealer A` credit limit'i `100` yap
- [ ] `buyer-a1` ile toplam `200` sepetle checkout ac
- [ ] `Open account` gorunmemeli
- [ ] Test sonunda credit limit'i tekrar `1000` yap

### B5 - Manuel adjustment regresyonu

- [ ] `Customers > Dealers > Edit Dealer A`
- [ ] `Manual credit adjustment = 50`
- [ ] `Add transaction`
- [ ] `Current debt` `50` azalmalı
- [ ] `Manual debit adjustment = 30`
- [ ] `Add transaction`
- [ ] `Current debt` `30` artmali

## 4. Dealer Collections Akisi (C1-C8)

### C1 - Collections menusu ve liste ekrani

- [ ] Admin ile `Customers > Dealer collections`
- [ ] Liste ekrani acilmali
- [ ] `Store`, `Dealer`, `Collection method`, `Status`, `Collection date` filtreleri gorunmeli

### C2 - Yeni collection create

- [ ] `Add new collection`
- [ ] `Store = Store A`
- [ ] `Dealer = Dealer A`
- [ ] `Customer = buyer-a1@example.com`
- [ ] `Collection method = Bank transfer`
- [ ] `Amount = 125`
- [ ] `Reference no = COL-001`
- [ ] `Note = Test collection`
- [ ] `Save`
- [ ] Listeye donmeli

### C3 - Collection create sonucu finans etkisi

- [ ] `Customers > Dealers > Edit Dealer A`
- [ ] `Current debt` onceki borca gore `125` azalmalı
- [ ] `Available credit` `125` artmali

### C4 - Collections list satiri

- [ ] `Customers > Dealer collections`
- [ ] Yeni kayit listede gorunmeli
- [ ] `Dealer`, `Store`, `Customer`, `Method`, `Status`, `Amount`, `Reference no`, `Note` kolonlari dogru olmali
- [ ] `Status = Posted`

### C5 - Collection detail ekrani

- [ ] Listeden `View`
- [ ] Detail ekraninda asagidakiler gorunmeli:
- [ ] Dealer
- [ ] Store
- [ ] Customer
- [ ] Collection method
- [ ] Status
- [ ] Amount
- [ ] Collection date
- [ ] Reference no
- [ ] Note
- [ ] Audit kartinda `Created by`, `Created on`, `Dealer transaction` gorunmeli

### C6 - Statement ekraninda source link

- [ ] `Customers > Dealer transactions`
- [ ] `Dealer A` sec
- [ ] Az once eklenen tahsilat satiri gorunmeli
- [ ] `Type = Open account collection`
- [ ] `Credit = 125`
- [ ] `Source = Collection #...` linki gorunmeli
- [ ] Linke tiklayinca ilgili collection detail ekrani acilmali

### C7 - Collection cancel

- [ ] Collection detail ekraninda `Cancel collection`
- [ ] Iptal et
- [ ] Detail ekranina donmeli
- [ ] `Status = Cancelled`
- [ ] `Cancelled by`, `Cancelled on`, `Cancellation dealer transaction` alanlari dolu olmali

### C8 - Cancel sonucu reversal

- [ ] `Customers > Dealers > Edit Dealer A`
- [ ] `Current debt` iptal oncesine gore tekrar artmali
- [ ] `Available credit` geri dusmeli
- [ ] `Customers > Dealer transactions`
- [ ] Ayni collection icin ters yonlu bir satir gorunmeli
- [ ] Bu satir `Debit` tarafinda olmali

## 5. Statement ve Export Kontrolleri (S1-S3)

### S1 - Running balance tutarliligi

- [ ] `Customers > Dealer transactions`
- [ ] `Dealer A` secili iken satirlari yukaridan asagi kontrol et
- [ ] Order debitleri bakiyeyi arttirmali
- [ ] Collection creditleri bakiyeyi dusurmeli
- [ ] Cancel reversal debit satiri bakiyeyi tekrar arttirmali

### S2 - Source gorunurlugu

- [ ] Open account order satirlarinda `Source` bos veya `-` olabilir
- [ ] Collection posting satirinda `Source = Collection #...`
- [ ] Cancellation posting satirinda da ayni collection source'u gorunmeli

### S3 - CSV export

- [ ] `Customers > Dealer transactions`
- [ ] `Dealer A` filtreli halde `Export to CSV`
- [ ] CSV'de `Source` kolonu olmali
- [ ] Collection posting satirinda `Collection #...` yazmali
- [ ] Export satirlari ekranla uyumlu olmali

## 6. StoreOwner Scope Kontrolleri (O1-O3)

### O1 - StoreOwner collections list scope

- [ ] `owner-a@example.com` ile admin paneline gir
- [ ] `Customers > Dealer collections`
- [ ] Sadece `Store A` dealer kayitlari gorunmeli

### O2 - StoreOwner create scope

- [ ] `Add new collection`
- [ ] Store alani sabit veya readonly olmali
- [ ] Sadece `Store A` dealer'lari secilebilmeli
- [ ] Store A disi dealer craft POST / URL ile denenirse islem olmamali

### O3 - StoreOwner detail/cancel scope

- [ ] Store A collection detail gorulebilmeli
- [ ] Store A collection iptal edilebilmeli
- [ ] Store B collection URL'si ile acilmaya calisilinca `Access denied` beklenir

## 7. Kapanis Kriteri

Asagidaki maddeler `OK` ise bu branch'teki finans kapsami kapanabilir:

- [ ] Open account checkout ve limit kontrolu dogru
- [ ] Dealer debt / available credit hesaplari dogru
- [ ] Dealer collections list/create/detail akisi dogru
- [ ] Collection cancel ters ledger hareketi uretiyor
- [ ] Dealer statement ekraninda source link ve running balance dogru
- [ ] CSV export source kolonunu dogru veriyor
- [ ] StoreOwner kendi store scope'u disina cikamiyor
