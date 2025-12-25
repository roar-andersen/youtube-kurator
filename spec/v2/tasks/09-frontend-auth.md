# Oppgave 09: Frontend - Autentisering

## Mål
Implementere login/logout UI og JWT-token-håndtering i PWA frontend.

## Kontekst

**Avhenger av:** Oppgave 02 (Auth API-endepunkter)

**Krav:**
- Login-skjerm med e-post og engangskode-felt
- Token-lagring (localStorage eller sessionStorage)
- Sikker token-sending (Authorization: Bearer header)
- Logout-funksjonalitet
- Session-persistering ved reload

## Implementering

### 1. Login UI

Opprett `src/YouTubeKurator.Api/wwwroot/login.html`:
- E-post-felt
- Engangskode-felt (vises først etter at e-post sendes)
- Submit-knapp (Hent kode → Verifiser kode)
- Error-meldinger
- Innlastings-state
- Link til hovedside for å logge inn

HTML-struktur:
```html
<div id="login-container">
  <h1>YouTube Kurator</h1>
  <form id="login-form">
    <input id="email" type="email" placeholder="E-postadresse" required />
    <button id="send-code-btn" type="button">Send kode</button>

    <div id="code-section" style="display:none;">
      <p>Engangskode sendt til <span id="email-display"></span></p>
      <input id="code" type="text" maxlength="6" placeholder="000000" />
      <button id="verify-btn" type="submit">Logg inn</button>
    </div>

    <div id="error" class="error"></div>
  </form>
</div>
```

### 2. API Service

Opprett `src/YouTubeKurator.Api/wwwroot/js/auth-service.js`:
```javascript
class AuthService {
  async sendAuthCode(email) {
    const response = await fetch('/api/auth/start', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email })
    });
    return response.json();
  }

  async verifyCode(email, code) {
    const response = await fetch('/api/auth/verify', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, code })
    });
    const data = await response.json();
    if (data.token) {
      localStorage.setItem('jwtToken', data.token);
      localStorage.setItem('userId', data.userId);
      localStorage.setItem('email', data.email);
    }
    return data;
  }

  getToken() {
    return localStorage.getItem('jwtToken');
  }

  logout() {
    localStorage.removeItem('jwtToken');
    localStorage.removeItem('userId');
    localStorage.removeItem('email');
  }

  isLoggedIn() {
    return !!this.getToken();
  }
}
```

### 3. HTTP Interceptor

Opprett `src/YouTubeKurator.Api/wwwroot/js/http-client.js`:
```javascript
class HttpClient {
  async request(url, options = {}) {
    const token = new AuthService().getToken();
    if (token) {
      options.headers = options.headers || {};
      options.headers['Authorization'] = `Bearer ${token}`;
    }
    return fetch(url, options);
  }

  async get(url) {
    return this.request(url, { method: 'GET' });
  }

  async post(url, body) {
    return this.request(url, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body)
    });
  }
  // PUT, DELETE, osv.
}
```

### 4. Main App Update

Oppdater `src/YouTubeKurator.Api/wwwroot/app.js`:
- Check om bruker er logget inn ved oppstart
- Hvis logget inn: vis hovedside
- Hvis ikke: vis login-side
- Legg til logout-knapp i header

### 5. Frontend Flow

```
Start → Check localStorage for token
  ├─ Token finnes → Vis hovedside
  └─ Ingen token → Vis login

Login-flow:
  1. Bruker oppgir e-post
  2. Click "Send kode" → POST /auth/start
  3. Vis code-field
  4. Bruker oppgir kode
  5. Click "Logg inn" → POST /auth/verify
  6. Lagre token, redirect til hovedside
  7. Alle API-kall inkluderer Authorization-header
```

### 6. Testing

Manual test:
- Send kode til gyldig e-post
- Verifiser kode
- Check at token lagres i localStorage
- Check at Authorization-header sendes på API-kall
- Reload og check at bruker fremdeles er innlogget
- Logout og check at token slettes

## Akseptansekriterier

- [ ] Login-HTML opprettet
- [ ] AuthService implementert
- [ ] HttpClient sender Authorization-header
- [ ] Token lagres i localStorage
- [ ] Token lesed ved oppstart
- [ ] Logout fungerer
- [ ] Redirect til login hvis token mangler
- [ ] Error-handling for failed login
- [ ] UI responsiv

## Leveranse

Nye filer:
- `wwwroot/login.html`
- `wwwroot/js/auth-service.js`
- `wwwroot/js/http-client.js`

Oppdaterte filer:
- `wwwroot/app.js` (initieringlogikk)
- `wwwroot/index.html` (logout-knapp)
