BodyTracker – Funktionsübersicht \& Installationsanleitung



✅ Überblick

BodyTracker ist eine WPF-Anwendung zur Verwaltung und Analyse von Körperdaten. Sie bietet:



Benutzerverwaltung (Personenstammdaten)

Erfassung von Körpermetriken (Gewicht, BMI, Körperfett, Muskelmasse, Viszeralfett)

Erfassung von Abmessungen (Brustumfang, Bauchumfang, Hüftumfang)

Zusätzliche Werte: Fettzange

Diagrammansicht mit zwei Y-Achsen (Gewicht links, Prozentwerte rechts)

Datenbankanbindung (MySQL)

AES-Verschlüsselung für gespeicherte Zugangsdaten

Excel-Import für historische Daten

Setup-Projekt zur Erstellung eines MSI-Installers





🔑 Hauptfunktionen der WPF-App

1\. Startfenster (UserSelectWindow)



Auswahl eines vorhandenen Benutzers oder Neuanlage.

Eingabe von DB-Benutzer und Passwort (verschlüsselte Speicherung in appsettings.json).

Automatischer Verbindungsaufbau bei gespeicherten Credentials.



2\. Hauptfenster (MainWindow)



Anzeige aller Messungen in einer Tabelle:



Datum, Gewicht, BMI, Körperfett %, Muskelmasse %, Viszeralfett

Brustumfang, Bauchumfang, Hüftumfang





Formatierung: Komma als Dezimaltrennzeichen, Nachkommastellen.

Buttons:



Eingabe: Öffnet Eingabemaske.

Diagramm: Öffnet Diagrammansicht.

Neu laden: Aktualisiert Tabelle.

Löschen: Entfernt markierte Zeile aus der Datenbank (mit Bestätigungsdialog).

Schließen: Beendet die Anwendung.



3\. Eingabefenster (DataEntryWindow)



Felder für alle Werte inkl. Fettzange.

Komma-Eingabe erlaubt (über Konverter).

Leere Felder möglich (nullable).

Tab-Fokus markiert den gesamten Inhalt.

Automatische Vorbelegung mit Werten vom Vortag (falls vorhanden).



4\. Diagrammfenster (ChartsWindow)



Liniendiagramm mit:



X-Achse: Datum (Format dd.MM.yyyy)

Y-Achse links: Gewicht (kg)

Y-Achse rechts: Körperfett %, Muskelmasse %



LiveCharts2-Integration.



5\. InfoPage

Anzeige einer license.txt in einem ScrollViewer.

ViewModel lädt Datei beim Start.





🔒 Sicherheit



AES-Verschlüsselung für DB-Passwort in appsettings.json.

Klartext-Passwort nur bei Terminal-Importer (Hardcoded oder CLI).



✅ Besondere Features



Komma statt Punkt bei Eingabe und Anzeige.

Nullable Werte für Löschbarkeit.

AES-Verschlüsselung für Credentials.

Diagramm mit zwei Y-Achsen.

Excel-Import mit Fettzange und Deduplikation.

Setup mit Lizenz und Verknüpfungen.

