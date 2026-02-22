# StoreOwner V1 Checklist (nopCommerce)

## Hedef Tarih
- Code freeze: `2026-02-27` (Cuma)
- Hedef: feature branch(ler) merge ready

## V1 Kapsami
- [x] StoreOwner rolu ve temel yetki kurgusu
- [x] StoreOwner menu kisitlamasi
- [x] Admin panelde StoreOwner icin request-scope filtresi
- [x] Product listesinde store bazli scope
- [x] Category listesinde store bazli scope
- [x] Manufacturer listesinde store bazli scope
- [x] Orders listesinde store bazli scope
- [x] Customers listesinde store bazli scope
- [x] Shipments listesinde store bazli scope
- [x] Return Requests listesinde store bazli scope
- [x] Store disi entity'ye direkt URL erisiminde engelleme (kritik akislarda)

## Bu Turda Eklenen Duzeltmeler
- [x] Shipments: StoreOwner sadece kendi store siparislerine ait gonderileri gorur
- [x] Shipment export (all): sadece erisilebilir gonderiler export edilir
- [x] Return Requests: StoreOwner listesi `storeId` ile filtrelenir
- [x] Catalog listelerinde global (`LimitedToStores = false`) kayitlar StoreOwner'a kapatildi

## Merge Oncesi Zorunlu Testler
- [x] A1: StoreOwner menu sadece izinli alanlari gosteriyor
- [x] B1-B5: Product/Category/Manufacturer/Orders/Customers store scope
- [x] C1-C5: Direkt URL ile store disi entity erisimi engelli
- [x] S1: Shipments listesi store scope
- [x] S2: Shipment export (all/selected) store scope
- [x] R1: Return Requests listesi store scope
- [x] R2: ReturnRequestReason/Action ekranlari StoreOwner icin kapali
- [x] X1: `owner-a` ve `owner-b` ile cift yon izolasyon testi

## Teknik Kapanis Kriterleri
- [x] `dotnet msbuild ... /t:Compile` basarili
- [x] Kritik akislarda runtime exception yok
- [x] Manuel test fail listesi sifirlandi
- [ ] Son degisiklikler tek commit veya net commit seti halinde toparlandi

## Merge Plani
1. Manuel test sonucunu bu dosyada isaretle
2. Fail varsa fix + retest
3. Feature branch'i `main` ile rebase/merge et
4. Son compile al
5. `main`e merge et
