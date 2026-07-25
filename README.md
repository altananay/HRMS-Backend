# HRMS-Backend

İşverenlerin iş ilanı yayınladığı, iş arayanların kayıt olup başvurduğu ve sistem personelinin
moderasyon yaptığı bir İnsan Kaynakları Yönetim Sistemi'nin **REST API**'si.

![C#][csharp] ![.NET][dotnet] ![PostgreSQL][postgres] ![Docker][docker] ![JWT][jwt]

---

## Hızlı başlangıç

Gereksinim: **.NET 10 SDK** ve **Docker**. Başka bir kurulum yok.

```bash
git clone <repo> && cd HRMS-Backend
docker compose up -d
dotnet run --project Presentation/WebAPI
```

Swagger: `https://localhost:xxxx/swagger` · Seq (loglar): `http://localhost:8081`

Migration'lar ve seed verisi Development'ta **otomatik** uygulanır. Seed edilen yönetici hesabı
`appsettings.Development.json` içindedir.

> PostgreSQL container'ı host'ta **5433** portunu kullanır, 5432'yi değil — makinede kurulu bir
> PostgreSQL servisiyle çakışmasın diye.

### Testler

```bash
dotnet test HRMS.sln
```

Functional testler **Testcontainers** ile kendi PostgreSQL'ini ayağa kaldırır; bulut kimlik bilgisi
veya elle kurulum gerekmez.

---

## Mimari

Onion, tek yönlü bağımlılık:

```
Domain  ←  Application  ←  { Infrastructure, Persistence }  ←  WebAPI
```

`Core/Domain` **hiçbir pakete bağımlı değildir**. İstek akışı:

```
Controller → IMediator.Send → Handler → I*Service (Manager) → I*Repository → HrmsDbContext
```

Cross-cutting işler MediatR `IPipelineBehavior`'ları ile yapılır (validation, logging, performance).
Hatalar tipli exception olarak fırlatılır ve RFC 9457 **ProblemDetails**'e dönüşür.

Ayrıntı ve çalışma kuralları için [CLAUDE.md](CLAUDE.md).

---

## Yapılandırma

`appsettings.json` **commit edilir ve gizli değer içermez**. Sırlar:

```bash
cd Presentation/WebAPI
dotnet user-secrets set "TokenOptions:SecurityKey" "<en az 32 karakter>"
```

Production'da environment variable (`TokenOptions__SecurityKey` gibi). Eksik yapılandırmada uygulama
`ValidateOnStart` ile **anlaşılır bir mesajla ve hemen** durur.

| Anahtar | Açıklama |
|---|---|
| `ConnectionStrings:Postgres` | Veritabanı bağlantısı |
| `TokenOptions:*` | Issuer, audience, imza anahtarı, token ömürleri |
| `Storage:Provider` | `Local` (varsayılan) veya `R2` |
| `IdentityVerification:Provider` | `Null` (varsayılan, fail-closed) veya `Mernis` |
| `Cors:AllowedOrigins` | İzinli origin listesi |
| `RateLimiting:Auth:PermitLimit` | Auth uçlarındaki limit (varsayılan 10 / 5 dk) |

### Dosya depolama

Varsayılan **local disk** (`App_Data/uploads` — `wwwroot` dışında, statik servis edilmez).
Production için **Cloudflare R2** (S3 uyumlu):

```json
"Storage": { "Provider": "R2", "R2": { "AccountId": "...", "BucketName": "hrms-cv-files" } }
```

S3 uyumlu olduğu için aynı adapter AWS S3, MinIO ve DigitalOcean Spaces ile de çalışır — sadece
`ServiceUrl` değişir.

CV dosyaları kişisel veridir: bucket private, presigned URL üretilmez, indirme **yetkili proxy**
üzerinden yapılır (`GET /api/Cvs/files/{id}`).

---

## API

Kimlik doğrulama tek yüzeyden yapılır ve her yanıt aynı zarfı döndürür:

```
POST /api/auth/register/jobseeker | register/employer | register/system-staff (admin)
POST /api/auth/login | refresh | logout | logout-all | change-password
GET  /api/auth/me
```

Access token 15 dk, refresh token 7 gün ve **rotasyonlu**. İptal edilmiş bir refresh token tekrar
sunulursa hırsızlık kabul edilir ve o kullanıcının **tüm oturumları** düşer — bekleyen access
token'lar dahil.

Yetkilendirme **deny-by-default**: global fallback policy kimlik doğrulaması ister. Herkese açık
uçlar (iş ilanı listesi, pozisyon listesi, iletişim formu, auth uçları) açıkça `[AllowAnonymous]`
işaretlidir ve `SecuritySmokeTests` içindeki listede kayıtlıdır.

Liste uçları sayfalıdır:

```json
{ "data": { "items": [...], "page": 1, "pageSize": 20, "totalCount": 42, "totalPages": 3 },
  "isSuccess": true, "message": null }
```

---

## Kullanılan teknikler

Onion Architecture · CQRS (MediatR) · Repository + Unit of Work · Pipeline Behaviors ·
Result Types · ProblemDetails (RFC 9457) · JWT + Refresh Token Rotation · Security Stamp ·
Role Based Authorization · Rate Limiting · FluentValidation · Riok.Mapperly (source generator) ·
EF Core (TPT inheritance, owned types, soft delete, optimistic concurrency) · Serilog · Docker ·
Testcontainers · xUnit v3 / NSubstitute / Shouldly

[csharp]:https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white
[dotnet]:https://img.shields.io/badge/.NET%2010-5C2D91?style=for-the-badge&logo=.net&logoColor=white
[postgres]:https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql&logoColor=white
[docker]:https://img.shields.io/badge/docker-%230db7ed.svg?style=for-the-badge&logo=docker&logoColor=white
[jwt]:https://img.shields.io/badge/JWT-black?style=for-the-badge&logo=JSON%20web%20tokens
