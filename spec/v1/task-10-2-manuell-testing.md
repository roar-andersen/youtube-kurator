# Oppgave 10.2: Manuell end-to-end testing

## Fase
Fase 10: Testing og validering

## Avhengigheter
- Alle forrige oppgaver (hele appen må være implementert)

## Formål
Utføre manuell testing av alle features for å sikre at akseptansekriteria er oppfylt.

## Oppgavebeskrivelse

### Test-sjekkliste

Bruk denne sjekklisten for å teste alle features:

#### 1. Oppstart
- [ ] Appen starter uten feil: `dotnet run`
- [ ] Frontend lastes på `http://localhost:5000`
- [ ] Service Worker registreres (DevTools → Application → Service Workers)
- [ ] Manifest laster korrekt (DevTools → Application → Manifest)

#### 2. Spillelister – Opprett
- [ ] Klikk "+ Ny Spilleliste"
- [ ] Modal åpnes
- [ ] Fyll inn navn og søkeord
- [ ] Klikk "Opprett"
- [ ] Playlist vises i listen
- [ ] Success-melding vises
- [ ] Modal lukkes

#### 3. Spillelister – Se alle
- [ ] Åpne appen
- [ ] Alle spillelister vises i grid
- [ ] Hvert kort viser navn, søkeord og oppdateringsdato
- [ ] Kort er responsive (fungerer på mobil)

#### 4. Spillelister – Redigér
- [ ] Klikk på playlist
- [ ] Detalj-view åpnes
- [ ] Navn- og søkeord-felt er editerbare
- [ ] Endre navn og/eller søkeord
- [ ] Klikk "Lagre"
- [ ] Endringer lagres
- [ ] Success-melding vises
- [ ] Breadcrumb viser oppdatert navn

#### 5. Videoer – Refresh
- [ ] I detalj-view, klikk "Oppdater Videoer"
- [ ] Loading-spinner vises
- [ ] Button er disabled mens laster
- [ ] Videoer vises i grid
- [ ] Minst 12 videoer vises
- [ ] Hver video viser: thumbnail, tittel, kanal, varighet, dato, visninger

#### 6. Videoer – Cache
- [ ] Rett etter refresh, klikk "Oppdater Videoer" igjen
- [ ] Cache-info vises: "Videoene er hentet fra cache..."
- [ ] `fromCache: true` i DevTools Network-fanen (eller sjekk konsoll)
- [ ] Samme videoer vises
- [ ] Ventelister ~1 time, så refresh igjen
- [ ] Cache skal være utløpt, nye videoer skal hentes

#### 7. Videoer – Paginering
- [ ] Hvis mer enn 12 videoer:
  - [ ] "Neste"-knapp vises
  - [ ] Klikk "Neste"
  - [ ] Neste side med 12 videoer vises
  - [ ] "Forrige"-knappen aktiveres
  - [ ] Klikk "Forrige"
  - [ ] Første side vises igjen

#### 8. Videoer – Klikk
- [ ] Klikk på en video
- [ ] YouTube åpnes i ny tab med riktig video-ID
- [ ] Original app forblir åpen

#### 9. Spillelister – Slett
- [ ] I detalj-view, klikk "Slett"
- [ ] Bekreftelsesdialog vises
- [ ] Klikk "OK"
- [ ] Playlist blir slettet
- [ ] App vender tilbake til oversikt
- [ ] Success-melding vises
- [ ] Playlist er borte fra listen

#### 10. Feilhåndtering – Nettverksfeil
- [ ] Åpne DevTools → Application → Service Workers
- [ ] Kryss av "Offline"
- [ ] Forsøk å oppdatere videoer
- [ ] Feilmelding vises: "Kunne ikke koble til..."
- [ ] If cache exists, vis cached data
- [ ] Kryss av "Offline" for å gå online igjen

#### 11. Feilhåndtering – Validering
- [ ] Klikk "+ Ny Spilleliste"
- [ ] La navn eller søkeord være tomt
- [ ] Klikk "Opprett"
- [ ] Feilmelding vises: "Navn og søkeord er påkrevd"

#### 12. Feilhåndtering – YouTube Quota (valgfritt)
- [ ] Bruk invalid API-nøkkel (sett i appsettings.Development.json)
- [ ] Klikk refresh
- [ ] Hvis cache exists: vis cached data + "YouTube-grensen er nådd..."
- [ ] Hvis ingen cache: vis "Noe gikk galt"

#### 13. PWA – Installering
- [ ] Åpne Chrome-meny
- [ ] Velg "Install YouTube Kurator"
- [ ] App installeres på home screen / Start menu
- [ ] Klikk app-ikonet for å starte
- [ ] App åpnes i fullskjerm (ikke som nettleserfane)

#### 14. PWA – Offline
- [ ] Med app installert, slå av internett
- [ ] Åpne app
- [ ] Statiske sider (list/detail) lastes fra cache
- [ ] API-kall feiler med offline-melding
- [ ] Slå på internett
- [ ] Klikk refresh
- [ ] Data lastes fra server

#### 15. Responsivitet
- [ ] Test på desktop (1920px)
  - [ ] Layout fungerer
  - [ ] Video-grid vises korrekt
- [ ] Test på tablet (768px)
  - [ ] Layout wraps korrekt
  - [ ] Buttons er klikkbare
- [ ] Test på mobil (375px)
  - [ ] Alle elementer synlige
  - [ ] Ingen horisontalt scroll
  - [ ] Modal-dialog får plass

#### 16. Performance
- [ ] Åpne DevTools → Performance
- [ ] Kjør performance-test
- [ ] Laste-tid skal være < 3 sekunder
- [ ] Service Worker cacher assets
- [ ] Chrome DevTools → Network: statiske filer har status 200 (cached)

#### 17. Accessibility
- [ ] Bruk tastaturnavigasjon (Tab-taster)
- [ ] Alle buttons og lenker skal være fokuserable
- [ ] Feilmeldinger skal være leserbare
- [ ] Fargekontrast skal være OK (WCAG AA)
- [ ] Skjermleser (ScreenReader) skal kunne navigere (valgfritt)

### Rapport
Dokumenter alle resultater:
```
TEST RESULTS
============

Dato: [YYYY-MM-DD]
Tester: [Navn]

✓ Oppstart
✓ Opprett Playlist
✓ Se alle Playlists
✓ Redigér Playlist
✓ Refresh Videoer
✓ Cache fungerer
✓ Paginering
✓ Klikk Video
✓ Slett Playlist
✓ Nettverksfeil håndteres
✓ Validering
✓ YouTube Quota håndteres
✓ PWA Installering
✓ PWA Offline
✓ Responsivitet
✓ Performance
✓ Accessibility

BLOKKERE:
[Liste eventuelle bugs eller mangler]

NOTATER:
[Andre observasjoner]
```

## Akseptansekriterier
- [ ] Alle 17 test-kategorier er gjennomgått
- [ ] Minst 90% av tests er bestått (eller blokkere er dokumentert)
- [ ] Ingen JavaScript-feil i console
- [ ] Appen fungerer på desktop, tablet og mobil
- [ ] Service Worker er registrert og cacher filer
- [ ] All feilhåndtering fungerer som forventet

## Referanser
- [Spesifikasjon: Akseptansekriterier](youtube-kurator-v1-spec.md#11-akseptansekriterier)
- [WCAG 2.1](https://www.w3.org/WAI/WCAG21/quickref/)
- [Lighthouse Audits](https://developers.google.com/web/tools/lighthouse)
