# CardCheckAssistant [![Codacy Badge](https://app.codacy.com/project/badge/Grade/f219c0fa9a484f4580085734c97cba85)](https://app.codacy.com/gh/c3rebro/CardCheckAssistant/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)
 
*Kartenprüfassistent für SimonsVoss Intern*

Systemvoraussetzungen:
   - Windows 10 `Build 1709` oder höher
   - Eine Installation von `RFiDGear 1.7` oder höher
   - Ein Unterstützter Kartenleser (Elatec TWN4 Multitech)
   - Zugang zur Omnitracker SQL Datenbank (Netzwerkverbindung und Benutzer)
   - Ein gültiges App-Zertifikat (Erhaltet ihr direkt bei mir)

Um die automatische Prüfung auf Updates zu aktivieren (empfohlen), muss die Installation mit dem Windows Appinstaller durchgeführt werden. Dieser kann hier direkt gestartet werden: 

[CardCheckAssistant jetzt installieren...](https://github.com/c3rebro/CardCheckAssistant/releases/latest/download/CardCheckAssistant_x64.appinstaller)

## Changelog

Siehe [Releases](https://github.com/c3rebro/CardCheckAssistant/releases) auf GitHub.

## Feedback

Bitte melde Fehler per Teams direkt an mich oder über [GitHub](https://github.com/c3rebro/CardCheckAssistant/issues).

## Letzte Änderungen:

Version 1.0.33
-----------------------------------------------------------------
Hintergrundfunktionen:
+ Änderung: Text "Sprachauswahl" ist keine Frage mehr. Anpassung der Funktion erfolgt später: Die Sprache soll zwar änderbar sein, jedoch erfolgt die Einstellung aus Omni heraus.
+ Änderung: Es werden Standardmäßig nur Aufträge mit dem Status "In Bearbeitung" angezeigt und nicht mehr alle. Dies kann über die Filterfunktion geändert werden.
+ Neue Funktion: Neue Einstellung: "Temporäre Berichtdateien löschen". Löscht alle editierbaren *_.pdf und *.pdf Dateien aus den Berichtverzeichnissen und behält nur die *_final.pdf* Dateien nach Fertigstellung. Ein Abschalten belässt diese Berichtdateien im Pfad.
+ Fehlerbehebung: InfoBar: "geöffneten Bericht wieder schließen" und Fortschrittverhinderung wenn dieser noch geöffnet ist. Dies verhindert ein "Steckenbleiben" wegen einer Zugriffsverweigerung auf die geöffnete PDF Datei.
+ Fehlerbehebung: "Schritt-Icon's" im linken Menüband (Häuschen-Icon und 1,2,3 - Icons) sind nicht mehr zur Navigation vorgesehen. Dies verhindert ein Springen zwischen Schritten wenn der Kartenleser noch beschäftigt ist.
+ Fehlerbehebung: Solange ein Bericht geöffnet ist, kann nicht von Schritt 2 auf Schritt 3 gewechselt werden. 
+ Fehlerbehebung: Ein Prüfvorgang kann nicht gestartet werden, wenn der Bericht bereits exitiert und nicht überschrieben werden kann oder werden soll.

Version 1.0.32
-----------------------------------------------------------------
Hintergrundfunktionen:
+ Fehlerbehebung: Endlos - "ToastMessage" bei neuen Aufträgen...
+ Erste Einträge auf der "Hilfeseite"

Version 1.0.31
-----------------------------------------------------------------
Hintergrundfunktionen:
+ Sortieroptionen: Sortieren nach Jobnummer erfolgt jetzt anhand der ID um die Chipnummer zu berücksichtigen
+ Sortieroptionen: Aufsteigend / Absteigend
+ "SetReadOnly" verschoben: Der Bericht wird jetzt nicht mehr direkt nach dem RFiDGear Aufruf in Schritt3 abgeschlossen sondern erst mit dem Klick auf "Fertigstellen"