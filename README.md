# 💪 BodyTracker – Körperdaten-Verwaltung & Analyse

Eine moderne WPF-Anwendung zur Erfassung, Verwaltung und Visualisierung deiner Körpermetriken mit MySQL-Datenbankanbindung und AES-Verschlüsselung.

## 📋 Überblick

BodyTracker ermöglicht es dir, deine Körperdaten systematisch zu erfassen und zu analysieren. Mit intuitive Benutzeroberfläche, leistungsstarken Diagrammen und einer sicheren Datenbankanbindung behältst du deine Fortschritte immer im Blick.

### ✨ Kernfeatures

- ✅ **Benutzerverwaltung** – Verwaltung mehrerer Benutzerprofile mit Personenstammdaten
- ✅ **Körpermetriken-Erfassung** – Gewicht, BMI, Körperfett, Muskelmasse, Viszeralfett
- ✅ **Körpermessungen** – Brustumfang, Bauchumfang, Hüftumfang, Fettzange
- ✅ **Interaktive Diagramme** – Dual-Achsen-Visualisierung mit LiveCharts2
- ✅ **Datenbankanbindung** – MySQL-Integration für zentrale Datenspeicherung
- ✅ **Verschlüsselte Anmeldedaten** – AES-256-Verschlüsselung für Sicherheit
- ✅ **Excel-Import** – Import historischer Daten mit Deduplikation
- ✅ **MSI-Installer** – Setup-Projekt für einfache Installation

---

## 🚀 Hauptfunktionen

### 1. 🔐 Benutzerauswahl & Anmeldung (UserSelectWindow)

- **Benutzerverwaltung**: Wähle einen vorhandenen Benutzer oder erstelle einen neuen
- **Datenbankverbindung**: Eingabe von MySQL-Benutzerdaten mit verschlüsselte Speicherung
- **Auto-Login**: Automatische Verbindung bei gespeicherten Credentials
- **AES-Verschlüsselung**: Passwörter werden verschlüsselt in `appsettings.json` abgelegt

### 2. 📊 Hauptfenster – Datentabelle (NewDataEnntry)

Übersichtliche Tabellenansicht aller erfassten Messungen:

| Spalte | Beschreibung |
|--------|-------------|
| **Datum** | Format: `dd.MM.yyyy` |
| **Gewicht** | in kg (Komma als Dezimaltrennzeichen) |
| **BMI** | Body-Mass-Index |
| **Körperfett %** | Körperfettanteil in Prozent |
| **Muskelmasse %** | Muskelmasseanteil in Prozent |
| **Viszeralfett** | Viszerales Fett (Organfett) |
| **Brustumfang** | in cm |
| **Bauchumfang** | in cm |
| **Hüftumfang** | in cm |
| **Fettzange** | Hautfaltenmessung in mm |

**Funktionen:**

- 📝 **Eingabe** – Neue Messung hinzufügen
- 📈 **Diagramm** – Visualisierung der Daten öffnen
- 🔄 **Neu laden** – Tabelle aktualisieren
- 🗑️ **Löschen** – Markierte Zeile entfernen (mit Bestätigungsdialog)
- ❌ **Schließen** – Anwendung beenden

### 3. ✍️ Dateneingabe (DataEntryWindow)

- **Vollständige Eingabefelder** für alle Metriken und Messungen
- **Komma-Eingabe** – Dezimaltrennzeichen wird automatisch konvertiert
- **Nullable Werte** – Leere Felder sind optional
- **Vorbelegung** – Automatische Vorbelegung mit Werten vom Vortag
- **Benutzerfreundlich** – Tab-Fokus markiert den gesamten Feldinhalt
- **Validierung** – Eingabebeschränkungen für korrekte Daten

### 4. 📈 Diagrammansicht (ChartsWindow)

Interaktive Visualisierung deiner Fortschritte:

- **Dual-Achsen-Design**:
  - 📍 **Linke Y-Achse** – Gewicht (kg)
  - 📍 **Rechte Y-Achse** – Körperfett % & Muskelmasse %
- **X-Achse** – Datum (Format: `dd.MM.yyyy`)
- **Liniendiagramm** – Trend-Visualisierung
- **LiveCharts2-Integration** – Moderne, performante Grafiken
- **Interaktiv** – Zoom, Pan und Hover-Details

### 5. ℹ️ Info-Page

- **Lizenzanzeige** – `license.txt` in ScrollViewer-Fenster
- **ViewModel-Integration** – Datei wird beim Start automatisch geladen
- **Leseschutz** – Schreibgeschützte Anzeige

---

## 🔒 Sicherheit & Datenschutz

### Verschlüsselung

- **AES-256-Verschlüsselung** für Datenbankpasswörter
- **Verschlüsselte Speicherung** in `appsettings.json`
- **Sichere Anmeldedaten** – Keine Passwörter im Klartext im Code

### Datenbankzugriff

- **MySQL-Authentifizierung** – Benutzerdefinierte Datenbank-Credentials
- **Separate Benutzer-Profile** – Isolierung von Benutzerdaten
- **Audit-Sicherheit** – Zentrale Datenspeicherung

---

## 🛠️ Technische Besonderheiten

### Dateneingabe & -format

- ✅ **Komma statt Punkt** – Deutsche Eingabekonvention (1,75 statt 1.75)
- ✅ **Nullable Werte** – Flexible Erfassung ohne Pflichtfelder
- ✅ **Dezimalformatierung** – Einheitliche Anzahl von Dezimalstellen

### Datenimport

- 📊 **Excel-Import** – Import historischer Messdaten
- 🔍 **Deduplikation** – Automatische Entfernung von Duplikaten
- 🎯 **Fettzange-Support** – Import aller Metriken inklusive Fettzange

### Installation & Deployment

- 📦 **MSI-Installer** – Setup-Projekt für Windows-Installation
- 🔗 **Verknüpfungen** – Desktop- und Startmenü-Verknüpfungen
- 📄 **Lizenzintegration** – Lizenztext beim Setup mit installiert

---

## 💻 Technologie-Stack

| Komponente | Technologie |
|-----------|------------|
| **UI-Framework** | WPF (Windows Presentation Foundation) |
| **Datenbank** | MySQL 8.0+ |
| **Diagramme** | LiveCharts2 |
| **Verschlüsselung** | AES-256 |
| **Zielframework** | .NET 8 |
| **Sprache** | C# |

---

## 📥 Installation

### Voraussetzungen

- Windows 10/11 oder höher
- .NET 8 Runtime
- MySQL Server 8.0+ (lokal oder remote)
- Datenbankzugriff (Benutzer und Passwort)
