# C# Övning 9 – Garage 3.0

En konsolapplikation för att skapa och hantera garage och parkerade fordon. Programmet har huvudmenyer och undermenyer för att öppna anläggningar, parkera och hämta fordon, söka bland parkerade fordon och hantera säkerhetskopior.

## Funktioner

- Skapa flera garage med eget namn, kapacitet och tillåtna fordonstyper. Ett garage kan även fyllas med slumpmässiga fordon.
- Parkera, redigera och ta bort fordon. Fordonstyper som inte får plats visas inte som parkeringsalternativ.
- Visa parkerade fordon, filtrera listan på fordonstyp och bläddra mellan sidor. Typväljaren visar antalet fordon per typ och totalt.
- Söka efter fordon med ett eller flera kriterier: märke, färg, cylindervolym, bränsletyp, längd, registreringsnummer, antal motorer, antal säten, antal hjul och fordonstyp.
- Kontrollera registreringsnummer och se till att de är unika mellan garagen. Jämförelsen är inte skiftlägeskänslig.
- Spara garage och fordon till JSON-backuper och läsa in en tidigare backup.
- Visa garagekapacitet, fordon och hur mycket parkeringsutrymme som används.

Fordonstyperna är `Airplane`, `Boat`, `Bus`, `Car` och `Motorcycle`. Alla fordon har märke, färg, registreringsnummer och antal hjul. Flygplan har antal motorer, båtar har längd, bussar har antal säten, bilar har bränsletyp och motorcyklar har cylindervolym.

Parkeringsutrymmet beräknas i tredjedelsplatser: bil använder 1 plats, buss och båt 2 platser, flygplan 3 platser och motorcykel 1/3 plats.

## Krav

- .NET 10 SDK
- Windows, macOS eller Linux med en kompatibel terminal

## Starta programmet

Kör följande kommandon från projektets rotmapp:

```sh
dotnet restore
dotnet run --project Exercise9/Exercise9.csproj
```

Menyerna kan navigeras med piltangenterna och val bekräftas med Enter. Menyerna visar även siffergenvägar där det är möjligt.

## Köra tester

Kör enhetstesterna med:

```sh
dotnet test Exercise9.Tests/Exercise9.Tests.csproj
```

Testprojektet innehåller tester för bland annat parkering, kapacitet, registreringsnummer, parkeringsutrymme och säkerhetskopiering.

## Säkerhetskopior

Välj **Manage backups** i huvudmenyn för att spara eller läsa in garage. Backupfiler lagras som JSON under `Exercise9/Data/Backups`. Ange ett filnamn utan filändelse när du sparar; tillåtna tecken är bokstäver, siffror, bindestreck och understreck.

## Projektstruktur

- `Exercise9/Program.cs` – startar applikationen och kopplar ihop meny och hanterare.
- `Exercise9/UI/` – konsolmenyer för anläggningar, garage, fordon och backuper.
- `Exercise9/Services/IGarageHandler.cs` och `GarageHandler.cs` – förmedlar operationer mellan gränssnittet och garage-/fordonsmodellen.
- `Exercise9/Facilities/Garage.cs` – generisk garageklass med `where T : Vehicle`, kapacitet och `IEnumerable<T>`-stöd.
- `Exercise9/Facilities/ParkingSpot.cs` – hanterar fördelningen av parkeringsutrymme i tredjedelsplatser.
- `Exercise9/Vehicles/` – basklassen `Vehicle` och dess fem fordonstyper.
- `Exercise9/Services/GaragePopulationService.cs` – skapar slumpmässiga fordon till ett garage.
- `Exercise9/Data/DataHandler.cs` – läser och skriver JSON-backuper.
- `Exercise9.Tests/` – enhetstester.
