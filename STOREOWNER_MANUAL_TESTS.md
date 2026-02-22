# StoreOwner Manual Test Scenarios (V1)

## 1) Teste Baslamadan Once

1. Uygulamayi calistir:
   - `/Users/baris/Documents/Tedarik-SAAS/nopCommerce_4.90.3_Source/src/Presentation/Nop.Web`
2. Admin ile giris yap.
3. `Configuration > Stores` altinda en az 2 store oldugunu dogrula:
   - Store A
   - Store B
4. `Customers > Customer roles` altinda `Store Owners` rolunu dogrula.
   - Yoksa olustur: `System name = StoreOwners`, `Active = true`.
5. 2 test kullanicisi olustur:
   - `owner-a@test.local` -> role: `Store Owners`, `Registered in store = Store A`
   - `owner-b@test.local` -> role: `Store Owners`, `Registered in store = Store B`
   - `Administrators` rolunu bu kullanicilara verme.

## 2) Test Verisi Hazirligi

Admin kullanicisi ile asagidaki verileri olustur:

1. Product:
   - `P-A-1` sadece Store A'ya mapli
   - `P-B-1` sadece Store B'ye mapli
2. Category:
   - `C-A-1` sadece Store A
   - `C-B-1` sadece Store B
3. Manufacturer:
   - `M-A-1` sadece Store A
   - `M-B-1` sadece Store B
4. Customer:
   - `cust-a@test.local` -> `Registered in store = Store A`
   - `cust-b@test.local` -> `Registered in store = Store B`
5. Order:
   - En az 1 siparis Store A
   - En az 1 siparis Store B

Not: Siparis store baglantisi onemli. Store A/B siparisleri farkli olmali.

## 3) Senaryo Grubu A - Menu ve Genel Erisim

### A1) StoreOwner menusu kisitli mi?
1. `owner-a@test.local` ile admin panele gir.
2. Sol menuyu kontrol et.

Beklenen:
- Gorunen ana bolumler: `Dashboard`, `Catalog`, `Sales`, `Customers`
- Ornegin `Configuration`, `System`, `Promotions`, `Content management` gorunmemeli.

### A2) StoreOwner izinli olmayan controller'a direkt URL ile gidebiliyor mu?
1. StoreOwner login iken su URL'leri dene:
   - `/Admin/Setting/GeneralCommon`
   - `/Admin/Discount/List`
   - `/Admin/Store/List`

Beklenen:
- `AccessDenied` sayfasina yonlenmeli.

### A3) Administrator etkilenmis mi?
1. Admin login ol.
2. Tum klasik admin menulerinin gorundugunu dogrula.

Beklenen:
- Admin davranisi degismemis olmali.

## 4) Senaryo Grubu B - Listeleme Scope Testleri

Bu testlerde login: `owner-a@test.local`.

### B1) Product listesi store scope ile geliyor mu?
1. `Catalog > Products` git.
2. Listeyi kontrol et.

Beklenen:
- `P-A-1` gorunur.
- `P-B-1` gorunmez.
- Store filtresi gizli olmalidir (StoreOwner icin).

### B2) Category listesi store scope ile geliyor mu?
1. `Catalog > Categories` git.

Beklenen:
- `C-A-1` gorunur, `C-B-1` gorunmez.
- Store filtresi gizli.

### B3) Manufacturer listesi store scope ile geliyor mu?
1. `Catalog > Manufacturers` git.

Beklenen:
- `M-A-1` gorunur, `M-B-1` gorunmez.
- Store filtresi gizli.

### B4) Orders listesi store scope ile geliyor mu?
1. `Sales > Orders` git.

Beklenen:
- Sadece Store A siparisleri gorunmeli.
- Store filtresi gizli olmalidir.

### B5) Customers listesi store scope ile geliyor mu?
1. `Customers > Customers` git.

Beklenen:
- `Registered in store = Store A` musteriler gorunmeli.
- Store B musterileri gorunmemeli.

## 5) Senaryo Grubu C - Entity Seviyesi Guvenlik

Bu testlerde login: `owner-a@test.local`.

### C1) Store B urun edit URL'sine direkt erisim engelleniyor mu?
1. Admin ile `P-B-1` urununun id'sini al.
2. StoreOwner A ile:
   - `/Admin/Product/Edit/{P-B-1-id}`

Beklenen:
- AccessDenied (veya erisim engeli) olmali.

### C2) Store B category/manufacturer edit erisimi engelleniyor mu?
1. `/Admin/Category/Edit/{C-B-1-id}`
2. `/Admin/Manufacturer/Edit/{M-B-1-id}`

Beklenen:
- Her ikisi de engellenmeli.

### C3) Store B order detayina direkt erisim engelleniyor mu?
1. Store B order id'si ile:
   - `/Admin/Order/Edit/{orderBId}`

Beklenen:
- AccessDenied olmali.

### C4) Store B shipment detayina direkt erisim engelleniyor mu?
1. Store B shipment id'si ile:
   - `/Admin/Order/ShipmentDetails/{shipmentBId}`

Beklenen:
- AccessDenied olmali.

### C5) Store B customer editine direkt erisim engelleniyor mu?
1. `/Admin/Customer/Edit/{custBId}`

Beklenen:
- AccessDenied olmali.

## 6) Senaryo Grubu D - Search/Autocomplete

Login: `owner-a@test.local`.

### D1) Admin product autocomplete store scope uyguluyor mu?
1. Admin panelde product arama/autocomplete input'unda `P-` yaz.

Beklenen:
- `P-A-1` gelmeli.
- `P-B-1` gelmemeli.

## 7) Senaryo Grubu E - Ozel Kisitlar

Login: `owner-a@test.local`.

### E1) Return request reason/action ekranlari kapali mi?
URL dene:
- `/Admin/ReturnRequest/ReturnRequestReasonList`
- `/Admin/ReturnRequest/ReturnRequestActionList`

Beklenen:
- AccessDenied.

## 8) Store Izolasyon Cift Yon Test

1. Yukaridaki B/C/D/E testlerini `owner-b@test.local` ile tekrar et.

Beklenen:
- Simetrik sonuc: Store B verisini gorur, Store A verisini goremez.

## 9) Sonuc Kayit Formati

Her test icin su formatla not al:

- `Test Kodu` (ornek: B1)
- `Durum` (Pass/Fail)
- `Gozlem` (1-2 cumle)
- `Hata URL` (varsa)
- `Ekran goruntusu` (varsa)

Bu formatla donebilirsen fail olanlari cok hizli kapatiriz.
