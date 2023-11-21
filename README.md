# CardCheckAssistant [![Codacy Badge](https://app.codacy.com/project/badge/Grade/f219c0fa9a484f4580085734c97cba85)](https://app.codacy.com/gh/c3rebro/CardCheckAssistant/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)
 
Kartenprüfassistent für SimonsVoss Intern

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

Version 1.0.30
-----------------------------------------------------------------
Hintergrundfunktionen:
+ Automatisches Update aktiviert

Version 1.0.29
-----------------------------------------------------------------
Benutzeroberfläche:
+ Verbesserung: InfoBar: Leseranschluss Belegt
+ Neue Funktion: Unterverzeichnis für CardChecks

Hintergrundfunktionen
+ Fehlerbehebung 1.0.21 / Verbesserung: Trennung der TimerEvents zwischen Schrittwechsel um belegte Ports zu verhindern

v1.0.21
------------------------------------------------------
+ 2 Sek. Wartezeit zwischen Schritt 1 und 2 soll belegte Ports verhindern

v1.0.20
------------------------------------------------------
+ Neue Funktionen: Abfrage der LSM Programmieroptionen: Wie wurde programmiert?
   Dies kann jetzt direkt eingegeben werden ohne das PDF öffnen zu müssen.
+ Mifare Classic wird während der Prüfung anders behandelt (z.B. Angabe verwendeter Sektoren)
+ Händler und ADM als neue Spalten.