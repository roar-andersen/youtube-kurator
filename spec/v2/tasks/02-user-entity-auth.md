# Oppgave 02: User Entity og Autentisering

## Mål
Implementere brukerautentisering med e-post og engangskode (magic link), samt JWT-token-generering.

## Kontekst

**Avhenger av:** Oppgave 01 (User-tabell må finnes)

**v2-krav:**
- E-postbasert autentisering (magic link / one-time code)
- JWT-tokens for API-tilgang
- Ingen passord i første versjon
- POST /auth/start - Send engangskode
- POST /auth/verify - Valider kode, få JWT

## Implementering

### 1. AuthCode Entity

Opprett `src/YouTubeKurator.Api/Data/Entities/AuthCode.cs`:
```csharp
namespace YouTubeKurator.Api.Data.Entities;

public class AuthCode
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; }
    public DateTime ExpiresUtc { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedUtc { get; set; }
}
```

Legg til DbSet i AppDbContext og konfigurer i OnModelCreating.

### 2. AuthController

Opprett `src/YouTubeKurator.Api/Controllers/AuthController.cs` med endepunkter:
- `POST /api/auth/start` - Sender engangskode til e-post
- `POST /api/auth/verify` - Validerer kode og returnerer JWT

### 3. Services

Implementer tre services:

**IAuthService / AuthService:**
- `SendAuthCodeAsync(email)` - Genererer 6-sifret kode, lagrer den, sender e-post
- `VerifyCodeAsync(email, code)` - Validerer kode, oppretter/oppdaterer bruker, returnerer JWT

**IEmailService / EmailService:**
- `SendAuthCodeAsync(toEmail, code)` - Sender e-post via SMTP med engangskoden

**IJwtService / JwtService:**
- `GenerateToken(userId, email)` - Genererer JWT med userId og email claims
- `ValidateToken(token)` - Validerer token og returnerer userId

### 4. Configuration

Legg til i `appsettings.json`:
```json
{
  "Jwt": {
    "SecretKey": "min-32-karakterer-lang-hemmeleg-nøkkel",
    "Issuer": "YouTubeKurator",
    "Audience": "YouTubeKurator"
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUser": "email@gmail.com",
    "SmtpPassword": "app-password",
    "FromAddress": "email@gmail.com"
  }
}
```

### 5. Middleware Setup

Oppdater `Program.cs`:
- Legg til JWT authentication
- Registrer AuthService, EmailService, JwtService
- Legg til `app.UseAuthentication()` og `app.UseAuthorization()` før `MapControllers()`

### 6. Migration

```bash
cd src/YouTubeKurator.Api
dotnet ef migrations add AddAuthCode
dotnet ef database update
```

## Testing

Opprett `YouTubeKurator.Tests/Services/AuthServiceTests.cs`:
- `SendAuthCodeAsync_CreatesCodeInDatabase`
- `VerifyCodeAsync_ValidCode_ReturnsToken`
- `VerifyCodeAsync_InvalidCode_ReturnsError`
- `VerifyCodeAsync_ExpiredCode_ReturnsError`
- `VerifyCodeAsync_CreatesUserIfNotExists`

## Akseptansekriterier

- [ ] AuthCode-tabell opprettet i database
- [ ] POST /api/auth/start sender e-post med 6-sifret kode
- [ ] POST /api/auth/verify returnerer JWT ved gyldig kode
- [ ] JWT inneholder userId og email i claims (verifiser med jwt.io)
- [ ] Koder utløper etter 15 minutter
- [ ] Bruker opprettes automatisk ved første innlogging
- [ ] LastLoginUtc oppdateres ved hver innlogging
- [ ] Alle tester passerer (`dotnet test`)
- [ ] E-post sendes korrekt
- [ ] Eksisterende v1-API fortsatt fungerer uten endringer

## Leveranse

Nye filer:
- `AuthCode.cs`
- `AuthController.cs`
- `IAuthService.cs`, `AuthService.cs`
- `IEmailService.cs`, `EmailService.cs`
- `IJwtService.cs`, `JwtService.cs`
- `AuthServiceTests.cs`

Oppdaterte filer:
- `AppDbContext.cs`
- `Program.cs`
- `appsettings.json`

Nye migrasjoner:
- `<timestamp>_AddAuthCode.cs`
