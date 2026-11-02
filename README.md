# Garaget

Detta projekt är ett konsolprogram för att hantera olika typer av parkeringsplatser i ett garage. Programmet visar vilka anläggningar som finns, hur många platser som är lediga, hur fordon kan parkera, sökas upp och sparas i backupfiler.

## Hur programmet startas

1. Öppna terminalen i projektets rotmapp.
2. Kör: `dotnet restore` och `dotnet run --project Exercise9/Exercise9.csproj`

## Köra tester

Kör `dotnet test Exercise9.Tests/Exercise9.Tests.csproj` för enhetstesterna av garageklassen.

## Klasser och ansvar

- `Program` och `UI`: startar programmet och hanterar konsolens menyer
- `IGarageHandler` och `GarageHandler`: förmedlar garagefunktioner mellan UI och datamodell
- `Garage<T>`: generisk samling med kapacitet, tillåtna fordonstyper och `IEnumerable<T>`-stöd
- `Vehicle`: gemensamma egenskaper för registreringsnummer, märke, färg och hjul
- `Airplane`, `Boat`, `Bus`, `Car`, `Motorcycle`: fordon med egna egenskaper
- `GaragePopulationService`: lägger in slumpmässiga fordon
- `DataHandler`: sparar och läser JSON-backupfiler via **Manage backups** i huvudmenyn

Garagevyn kan lista och gruppera parkerade fordon, söka på typ, registreringsnummer, märke, färg och antal hjul samt parkera, redigera och hämta ut fordon. Registreringsnummer jämförs utan hänsyn till versaler/gemener och ska vara unika mellan garagen.

Backupmenyn kan spara alla garage och parkerade fordon till en JSON-fil och läsa tillbaka en tidigare backup.

## Feedback

Jag vill ha feedback på projektets struktur och hur väl klassansvaret är fördelat.
