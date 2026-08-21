# CvToExcel

AI destekli CV → Excel dönüştürme sistemi. ASP.NET Core Clean Architecture, PostgreSQL + EF Core. Kişisel/portfolyo öğrenme projesi — Ecenaz'ın kendi mentörü de var, tasarım kararlarında zaman zaman onun geri bildirimi devreye giriyor.

## Çalışma prensibi (kritik, oturumlar arası geçerli)

**Öğrenme kodunu (Domain/Application/Infrastructure/Controller) Ecenaz kendisi yazar.** Claude ne/neden/nerede/nasıl'ı adım adım anlatır, kod örneği chat'te gösterebilir (Ecenaz kendi dosyasına geçirir) ama dosyaya doğrudan yazmaz — Ecenaz açıkça "sen yaz"/"yap" demediği sürece.

**İstisna — tooling Ecenaz'ın kodu sayılmaz, Claude bunları doğrudan yapar:** `dotnet new`/`sln add`/`add reference`/`add package`, güvenlik açığı için versiyon güncellemeleri, `dotnet build`, klasör taşıma/silme, NuGet paket meşruiyet kontrolü (sahte/squat paket riski).

**`dotnet ef migrations add`/`database update` istisna DEĞİL (2026-08-20'de düzeltildi)** — bunları Ecenaz kendisi çalıştırır. Claude sadece nasıl çalıştırılacağını anlatır, komutu kendisi çalıştırmaz.

**Runtime test etme de Ecenaz'ın işi** — Swagger'dan istek atma, DB'yi psql/pgAdmin'den kontrol etme gibi adımları kendisi çalıştırır; Claude nasıl test edeceğini anlatır ama onun yerine çalıştırmaz. (İstisna: aktif debug sırasında, Ecenaz'ın onayıyla, teşhis amaçlı salt-okunur DB sorguları Claude tarafından çalıştırılabilir.)

Her adımda ilerlerken kısa bir "ne yaptık, neden yaptık" özeti istenir — bunu vermeyi unutma.

## Sabit mimari kararlar

- 4 proje, solution kökünde (`src/` klasörü yok): `CvToExcel.Domain` / `Application` / `Infrastructure` / `API`. Bağımlılık yönü: Application→Domain, Infrastructure→Application, API→Application+Infrastructure. **Application asla Infrastructure'a referans veremez** — bu yüzden DB erişimi Application'da hep bir interface (`ICvDocumentRepository` gibi) üzerinden yapılır, `AppDbContext`'e doğrudan değil.
- PostgreSQL + EF Core, CQRS via MediatR. Fluent API config `AppDbContext.OnModelCreating` içinde tek dosyada (ayrı `IEntityTypeConfiguration<T>` sınıflarına bölünmüyor — hız için bilinçli tercih).
- **PDF, Gemini'ye base64 `inline_data` olarak doğrudan gönderiliyor** — önce metne çevirip sonra AI'a vermek yok (`PdfPig` paketi kurulu ama kullanılmıyor, bilinçli olarak).
- **Tek AI çağrısı ile hem extraction hem Excel placement kararı** (adım 9'da işlenecek, aşağıda detay var) — ayrı bir "evaluation" adımı yok.
- ClosedXML/OpenXML **pasif yazıcı** — hardcoded hücre eşlemesi yok, AI hangi veri nereye gidecek karar veriyor, kod sadece mekanik olarak yazıyor. (Not: Gemini API fiziksel .xlsx byte'ı doğrudan üretemez — "AI dosyayı oluşturuyor" demek pratikte "AI %100 içerik+layout kararı veriyor, kod sıfır iş mantığıyla transkribe ediyor" anlamına geliyor.)
- Upload + process tek endpoint'te (`POST /api/cv`), ayrı `/process` yok.
- MVP kapsamı dışı (bilinçli): auth, Hangfire, Polly, Unit of Work, MediatR pipeline behaviors, SignalR, mock AI (testler gerçek AI çağrısı kullanacak).
- Enum'lar (`ProcessingStatus`, `SkillType`) DB'de plain `int` olarak saklanıyor (EF Core varsayılanı).

### Excel canlı güncelleme tasarımı (2026-08-19'da kilitlendi)

- Excel dosyası her CV upload'ında **canlı güncelleniyor** (batch/ayrı export tetikleme yok).
- "Mevcut excel durumu" AI'a **DB kayıtlarından** besleniyor (fiziksel dosyayı tekrar okumuyoruz).
- Sütun/satır tutarlılığını garanti etmek için: her upload'da AI'a **DB'deki TÜM daha önce çıkarılmış adaylar** (ham PDF değil, zaten çıkarılmış JSON'ları) + yeni CV'nin PDF'i birlikte gönderiliyor, AI **tüm tabloyu** (tüm sütunlar + tüm satırlar) her seferinde yeniden üretiyor. Excel writer dosyayı her seferinde sıfırdan yazıyor, incremental "satır ekleme" mantığı yok — bu, ayrı bir "layout" tablosu tutup incremental append yapmaktan **daha az yeni parça/daha az yeni hata riski** taşıdığı için bilinçli tercih edildi (trade-off: aday sayısı arttıkça çağrı maliyeti büyür, portfolyo ölçeğinde kabul edilebilir).
- `IAiExtractor`, DB'ye **doğrudan erişmiyor** — mevcut aday durumunu Application katmanındaki handler DB'den okuyup parametre olarak AI çağrısına geçiriyor (Clean Architecture sınırı).
- Eski "Excel export endpoint" fikri artık AI çağırmıyor — sadece güncel fiziksel dosyayı indiren, AI'sız statik bir GET endpoint'e dönüşüyor.
- **`OtherSection` → Excel'de tek bir "Diğer" sütununa birleşiyor** (2026-08-20'de kilitlendi, adım 9'da uygulandı): DB'de `OtherSection` satır başına `Title`+`Content` olarak kalıyor (değişmedi), ama `GeminiAiExtractor` prompt'undaki "Mevcut adaylar ve tablo" kural bloğu, bir adayın TÜM `OtherSection` satırlarını tek bir Excel sütununda ("Diğer") birleştiriyor. Sütun patlaması yok, sabit kategori enum'u da yok.
- **Excel sütun isimleri Türkçe** (2026-08-20) — prompt'ta "Sütun isimlerini Türkçe kullan (örn. Ad Soyad, E-posta, ...)" kuralı var. Başta karışık dilde geliyordu (bir örnekte yanlışlıkla "Diğer" Türkçe, geri kalanı İngilizce yazılmıştı, AI da birebir öyle üretti) — düzeltildi.
- **Telefon numaraları normalize ediliyor** (2026-08-20): prompt kuralı, AI'a sadece baştaki `+` ve rakamları bırakıp boşluk/parantez/tire'yi kaldırmasını söylüyor (`"+90 532 315 41 02"` → `"+905323154102"`) — CV'ler arası tutarlı format için.
- **Nested alt-başlıklar ayrı `otherSections` elemanı olmalı** (2026-08-20, gerçek bir CV'de yakalandı): bir CV'de "Hobbies and Interests" gibi bir alt-başlık, üst başlığın (`"COMMUNICATION AND INTERPERSONAL SKILLS"`) `Content`'ine düz metin olarak gömülmüştü — veri kaybı yoktu ama yapı yanlıştı. Prompt'a "alt-başlığı üst başlığa gömme, ayrı eleman yap" kuralı eklendi.
- **Projeler sütununda teknolojiler eksikti** (2026-08-20): AI, `table`'daki Projeler hücresine sadece proje başlıklarını yazıyordu, `technologiesUsed`'ı atlıyordu — prompt'a "başlık + teknolojiler birlikte" kuralı eklenince düzeldi. `Description` bilerek bu hücreye dahil edilmedi (çok uzun, tabloyu kalabalıklaştırır) — DB'de duruyor, sadece Excel özet satırına girmiyor.
- **Aynı CV'nin iki kez yüklenmesini engelleme** (2026-08-20): `CvDocument.Email`'e unique index (`HasIndex().IsUnique()` — pgAdmin'de "Constraints" değil **"Indexes"** altında görünür, işlevsel olarak aynı şey). Bu DB seviyesinde son çare; asıl kullanıcı deneyimi `UploadCvCommandHandler`'da — zaten AI çağrısı için çekilen `existingCandidates` listesinde eşleşen email varsa, `AddAsync`'ten önce anlaşılır bir `InvalidOperationException` fırlatılıyor (ekstra DB sorgusu yok). Not: AI çağrısı email'i öğrenmeden önce zaten yapılıyor, o maliyeti önleyemiyoruz — sadece yanlış DB kaydını/Excel yazımını önlüyoruz. Telefon değil email seçildi çünkü telefon formatları CV'ler arası tutarsız olabiliyordu (normalize kuralından önce).

## Güncel roadmap (16 adım, revize edilmiş sıralama)

1-5: Solution skeleton, Domain, PostgreSQL+EF Core, File Storage, Upload+extraction — **tamamlandı.**

6-8: Validation (FluentValidation, sadece AI kontratı — `skillType` 4 değerden biri mi; içerik tamlığı/tarih formatı validate edilmiyor, bilinçli) → DTO→Entity mapping → DB persistence — **tamamlandı, uçtan uca doğrulandı** (gerçek CV yüklendi, `CvDocuments` + child tablolarda satır görüldü).

`Project`/`OtherSection` entity + Fluent API config + migration — **tamamlandı** (2026-08-20, `AddProjectAndOtherSection` migration DB'ye uygulandı).

`Project`/`OtherSection`'ı `GeminiAiExtractor` Prompt'una, `CvExtractionResult` DTO'suna ve `CvDocumentMapper`'a bağlama — **tamamlandı** (2026-08-20). Bu arada `endDate`/"hâlâ devam ediyor" ayrımı için `Education`/`WorkExperience`'a `IsCurrent bool` eklendi (AI `endDate: "current"` döndürünce mapper `EndDate=null` + `IsCurrent=true` yazıyor), ayrı migration'la (`AddIsCurrentToEducationAndWorkExperience`) uygulandı — DB'ye migrate edildi.

9. **Tamamlandı ve uçtan uca doğrulandı (2026-08-20).** `IAiExtractor.ProcessCvAsync` birleşik hale getirildi: `ICvDocumentRepository.GetAllAsync()` (yeni, `Include` ile tüm child collection'ları çekiyor) + `CvDocumentMapper.ToDto()` (yeni, Entity→DTO ters yönü — `IsCurrent=true` olan kayıtları `"current"` string'ine geri çeviriyor) ile DB'deki mevcut adaylar okunuyor, yeni CV'nin PDF'i ile birlikte AI'a gidiyor. AI cevabı artık `CvProcessingResult { NewCandidate, Table }` — `NewCandidate` DB'ye kaydediliyor (`CvDocumentMapper.ToEntity`, değişmedi), `Table` (serbest `Columns`/`Rows` yapısı, AI'ın kendi kararı) Excel'e gidiyor. 5 gerçek CV ile test edildi, mevcut adaylar + yeni aday hepsi doğru şekilde tabloya giriyor.
10. **Tamamlandı (2026-08-20).** `IExcelWriter` (Application) + `ClosedXmlExcelWriter` (Infrastructure, `ClosedXML` NuGet paketi) — tamamen mekanik, `ExcelTableResult.Columns`/`Rows`'u sabit tek dosyaya (`storage/excel/candidates.xlsx`, GUID'siz — her upload'da komple üzerine yazılıyor) yazıyor. `Program.cs`'te `AddSingleton<IExcelWriter, ClosedXmlExcelWriter>`. Henüz indirme endpoint'i yok, dosya elle (`open` komutu/Finder) açılıyor.
11. **[SIRADA, netleştirme bekliyor]** Status endpoint — tam kapsamı henüz kararlaştırılmadı: tekil CV sorgulama (`GET /api/cv/{id}`), tüm adayları listeleme (`GET /api/cv`), yoksa ikisi birden mi? `CvDocument.ProcessingStatus` şu an her zaman `Completed` sabit yazılıyor (işlem senkron, ara durum yok). Bir sonraki oturum bu soruyla devam edecek.
12. Excel indirme endpoint'i (AI'sız, statik dosya servisi, `storage/excel/candidates.xlsx`'i döndürür).
13. Global exception handling + Serilog.
14. Swagger e2e testing.
15. Unit tests (gerçek AI çağrılarıyla, mock yok).
16. *(MVP sonrası)* Hangfire / frontend / Docker / deployment.

**Ayrıca (adım 6-16 dışında, ihtiyaç oldukça):** Caching — Gemini cevaplarını tekrar aynı CV yüklenirse tekrar ödeme yapmamak için cache'leme fikri var ama kapsamı netleşmedi (`IMemoryCache` vs distributed, ne cache'lenecek), bilinçli olarak ertelendi.

## Domain model

`CvDocument` (parent) → `Education`, `WorkExperience`, `Skill`, `Language`, `Project`, `OtherSection` (children, hepsi `CvDocumentId` FK + `ON DELETE CASCADE`). `Education`/`WorkExperience` ayrıca `IsCurrent bool` taşıyor (2026-08-20 eklendi) — AI'ın `endDate: "current"` demesi karşılığı, `EndDate` bu durumda `null` kalıyor.

**2026-08-19'da eklenen, 2026-08-20'de DB'ye migrate edilen iki entity:**
- **`Project`**: `Id`, `CvDocumentId`, `CvDocument` (nav), `Title` (required, proje adı — akademik/kişisel proje ayrımı **bilinçli olarak yok**, CV'lerde tutarlı değil ve Excel layout kararı zaten AI'a ait), `TechnologiesUsed` (nullable, `HasMaxLength(200)` — bounded kaldı), `Description` (nullable, **`.HasColumnType("text")`** — bkz. CLOB notu aşağıda). **Not:** ileride ihtiyaç olursa nullable `ProjectType?` enum eklenebilir, şimdilik ertelendi.
- **`OtherSection`**: `Id`, `CvDocumentId`, `CvDocument` (nav), `Title` (`HasMaxLength(150)`), `Content` (nullable, **`.HasColumnType("text")`**) — CV'de sabit şemaya uymayan her bölüm için AI'ın doldurduğu free-form "catch-all" alan (sabit kategori enum'u yok). Excel'e nasıl yansıyacağı için bkz. "Excel canlı güncelleme tasarımı" bölümündeki `OtherSection` maddesi.

**`Education.Description`/`WorkExperience.Description` kaldırıldı, tamamlandı (2026-08-20)** — hem mentörün önerisiyle hem Ecenaz'ın bağımsız "Excel'de çok uzun oluyor" gözlemiyle örtüşüyor. Domain/DTO/Prompt/Mapper güncellendi, migration oluşturulup uygulandı (aşağıdaki CLOB migration'ıyla birleşti).

**CLOB/`text` kararı (2026-08-20, mentörden geldi, birkaç tur netleştirme gerektirdi):** Mentörün "CLOB'a bak" demesi Oracle'a özgü bir terim — PostgreSQL'in karşılığı `text` tipi (ayrı bir veritabanı gerekmiyor, `varchar(n)` ile `text` arasında Postgres'te performans farkı yok, sadece `text` sınırsız). Kapsam netleşti: **`Project.Description`** (yeni eklendi) ve **`OtherSection.Content`** `.HasColumnType("text")` oldu; `Project.TechnologiesUsed` yanlışlıkla `text`'e çevrilmişti, `HasMaxLength(200)`'e geri alındı. Tek migration'da toplandı (`AddProjectDescriptionAndClobColumns`), uygulandı. `Project.Description` zincirinin tamamı (Domain/DTO/Prompt şema+kural/Mapper — hem `ToEntity` hem yeni `ToDto` yönü) da bu migration'la birlikte tamamlandı.

## Bilinen gotcha'lar / önemli teknik detaylar

- **`UglyToad.PdfPig` NuGet paketi squat/hijack** — doğru paket sadece `PdfPig` (gerçek maintainer'lar). Herhangi bir yeni paket eklerken sahiplik/açıklama/versiyon şemasını kontrol et.
- **Gemini istek JSON alanları snake_case** (`inline_data`, `mime_type`, `response_mime_type`) — empirik olarak doğrulandı, değiştirme.
- **Model adı sık değişiyor** — 404 alırsan Google'ın önerdiği yeni model adını user-secrets'teki `Gemini:Model`'e yaz, kod değişikliği gerekmez.
- **User-secrets'e API key yapıştırırken `<`/`>` karakterleri sızabilir** — `dotnet user-secrets list` ile her zaman doğrula.
- **HTTP hata body'sini `IsSuccessStatusCode` kontrolünden ÖNCE oku** (`GeminiAiExtractor`'da yapılan) — yoksa Google'ın gerçek hata mesajı kaybolur, debug çok zorlaşır.
- **Gemini'nin cevabı iki katmanlı JSON** — dış zarf (`GeminiGenerateContentResponse`) çözülüyor, içindeki `Text` alanı AYRICA `CvExtractionResult`'a deserialize ediliyor.
- **`Enum.Parse<SkillType>` try/catch'siz güvenli** çünkü validation mapping'den önce çalışıyor (6→7 sırası bilinçli) — mapping, validate edilmiş veriye güvenebiliyor.
- **AI bazen `null` yerine `""` dönebilir** — `EmptyStringToNullConverter` (custom `JsonConverter<string?>`) ile normalize ediliyor, `CvDataJsonOptions` adlı ayrı bir `JsonSerializerOptions`'a bağlı (Gemini'nin kendi zarfını çözen orijinal `JsonOptions`'tan bilinçli olarak ayrı tutuluyor).
- **`DateTime.TryParse` ile parse edilen tarihler `Kind=Unspecified` çıkar, Npgsql `timestamptz` sütununa yazamaz** — `DateTime.SpecifyKind(date, DateTimeKind.Utc)` ile düzeltildi (`CvDocumentMapper.ParseDate`). İleride bu tarihler saat taşımadığı için `DateOnly?`'ye geçiş daha doğru olur (migration gerektirir, ertelendi).
- **Kod değişikliği yapıp `dotnet run`'ı yeniden başlatmayı unutma** — çalışan eski süreç yeni kodu yansıtmaz, "API 200 dönüyor ama DB'ye yazmıyor" gibi yanıltıcı sonuçlara yol açar.
- **Repository çağrısını (`AddAsync`) eklemeyi unutmak sessiz başarısızlık yaratır** — mapping yapılıp entity oluşturulsa bile, kaydetme satırı olmadan API yine de 200 dönebilir (AI cevabı zaten doğru geldiği için). "DB boş ama API çalışıyor" durumunda önce bunu kontrol et.
- **pgAdmin dışarıdan yapılan DB değişikliklerini otomatik yansıtmaz** — hem tree'yi hem açık olan "View/Edit Data" sekmesinin kendi refresh/execute butonunu yenilemek gerekebilir.
- **Recurring yazım hatası:** "Infrastructure" kelimesini namespace'lerde yanlış yazma eğilimi var (`Infrasturcutre`, `Infrastrcutre` gibi görülmüştü) — namespace satırlarını özellikle kontrol et.
- **`Prompt` sabitindeki JSON şema hataları `dotnet build`'de hiç görünmez** (sadece bir C# string) — AI'a bozuk şema gider, sessizce yanlış/boş veri döner. Şema değiştiğinde mutlaka ayrıca JSON olarak doğrulanmalı (örn. `python3 -c "import json; json.loads(...)"` ile) — bu oturumda en az 3 kez virgül/parantez/tırnak hatası bu şekilde yakalandı, build "başarılı" derken.
- **Entity→DTO ters mapping (`CvDocumentMapper.ToDto`) eklenince, yeni bir alan her eklendiğinde İKİ mapping yönü de güncellenmeli** (`ToEntity` VE `ToDto`) — birini unutmak sessiz veri kaybına yol açar (örn. yeni alan DB'ye yazılır ama bir sonraki upload'da "mevcut adaylar" olarak AI'a geri gönderilmez). `Project.Description` eklenirken bu ikisi ayrı ayrı hatırlatılıp kontrol edildi.
- **CLOB, ayrı bir veritabanı değil, sadece bir sütun tipi** (Oracle terimi) — PostgreSQL'deki karşılığı `text`. Mentörden/başka kaynaklardan farklı DB'lere özgü terimler geldiğinde önce "bunun Postgres karşılığı ne" diye çevirmek gerekiyor, gereksiz yere mimari değiştirmeye kalkmadan önce.

## Lokal ortam

- PostgreSQL, Postgres.app (v18) üzerinden: `/Applications/Postgres.app/Contents/Versions/18/bin/psql`, PATH'te değil. Trust auth (şifresiz).
- DB adı `CvToExcelDb`, connection string `CvToExcel.API/appsettings.json` → `DefaultConnection`.
- GitHub: `Ecenaz11/CvToExcel`, `main` branch.
