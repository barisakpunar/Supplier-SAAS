# Discount Store Scope Manual Test Plan

Bu dokuman `codex/feature/discount-store-scope-hybrid` branch'i icindir.

## 1. On Kosullar

- Build:
  - `dotnet build /Users/baris/Documents/Tedarik-SAAS/nopCommerce_4.90.3_Source/src/Presentation/Nop.Web/Nop.Web.csproj -c Debug --no-restore --nologo -m:1 /nr:false`
- Roller:
  - `Admin` (varsayilan tam yetkili)
  - `StoreOwner A` (Store A'ya bagli)
  - `StoreOwner B` (Store B'ye bagli)
- Veri:
  - Store A ve Store B
  - Her store'a ozel category/manufacturer/product

## 2. Migration ve Permission

### DS1 - Discount store scope kolonu

- [ ] Uygulamayi baslat
- [ ] `Discount` tablosunda `LimitedToStores` kolonu oldugunu dogrula

### DS2 - StoreOwner discount permission

- [ ] `Configuration > Access control list`
- [ ] `StoreOwners` rolunde su izinleri gor:
  - [ ] `Promotions.DiscountsView`
  - [ ] `Promotions.DiscountsCreateEditDelete`

## 3. StoreOwner Akisi

### DS3 - StoreOwner menude Discounts gorunurlugu

- [ ] `StoreOwner A` ile admin panele gir
- [ ] `Promotions > Discounts` menusu gorunmeli

### DS4 - StoreOwner create/edit store scope

- [ ] `Add new discount` ac
- [ ] Store secimi serbest olmamali (sadece kendi store scope)
- [ ] Kaydet
- [ ] Kaydedilen discount sadece Store A kapsaminda olmali

### DS5 - StoreOwner category/manufacturer popup scope

- [ ] `Assigned to categories` popup ac
- [ ] Listede sadece Store A kategorileri gorunmeli
- [ ] `Assigned to manufacturers` popup ac
- [ ] Listede sadece Store A manufacturer'lari gorunmeli

### DS6 - StoreOwner direct URL engeli

- [ ] Store B'ye ait discount `Edit` URL'sine StoreOwner A ile direkt git
- [ ] Access denied donmeli

## 4. Admin Akisi

### DS7 - Admin global discount (hibrit model)

- [ ] `Admin` ile yeni discount olustur
- [ ] Store secmeden kaydet
- [ ] Kayit basarili olmali (global scope)

### DS8 - Admin store-scoped discount

- [ ] Yeni discount olustururken belirli store(lar) sec
- [ ] Kaydet
- [ ] Discount sadece secili store(lar)da gecerli olmali

### DS9 - Admin listede store filtresi

- [ ] `Promotions > Discounts` ekraninda Store filtresi ile ara
- [ ] Sonuclar secilen store kapsamiyla uyumlu gelmeli

## 5. Checkout Scope

### DS10 - Coupon store izolasyonu

- [ ] Store A'ya ozel coupon ile Store A checkout dene -> uygulanmali
- [ ] Ayni coupon ile Store B checkout dene -> uygulanmamali

### DS11 - Order subtotal/shipping/order total discount store scope

- [ ] Store A icin tanimli indirimlerle Store A checkout yap
- [ ] Indirim hesaplara yansimali
- [ ] Ayni indirimler Store B checkout'ta uygulanmamali

## 6. Kapanis Kriteri

- [ ] StoreOwner discount yonetebiliyor ama sadece kendi store scope'unda
- [ ] Category/manufacturer/product popup listeleri store scope'a uyuyor
- [ ] Direkt URL ile baska store discount erisimi engelli
- [ ] Admin global discount yetkisi calisiyor
- [ ] Admin store filtreli arama dogru
- [ ] Checkout hesaplama ve coupon uygulama store bazinda dogru
