# Garaget

Detta projekt är ett konsolprogram för att hantera olika typer av parkeringsplatser i ett garage. Programmet visar vilka anläggningar som finns, hur många platser som är lediga, hur fordon kan parkera, sökas upp och sparas i backupfiler.

## Hur programmet startas

1. Öppna terminalen i projektets rotmapp.
2. Kör: `dotnet restore` och `dotnet run --project Exercise6/Exercise6.csproj`

## Klasser och ansvar

- Program: startpunkt för programmet
- Menu: visar menyer och hanterar användarval
- MenuItem: ett menyval
- MenuFactory: bygger upp menyn
- Garage: basklass för alla garage
- ParkingSpot: en parkeringsplats
- CarGarage: garage för bilar, motorcyklar och bussar
- MCGarage: garage för motorcyklar
- Hangar: garage för flygplan
- Marina: garage för båtar
- Vehicle: basklass för fordon
- Car, Bus, Motorcycle, Boat, Airplane: olika fordonstyper
- GaragePopulationService: lägger in slumpmässiga fordon
- DataHandler: sparar och läser JSON-backupfiler

## Feedback

Jag vill ha feedback på projektets struktur och hur väl klassansvaret är fördelat.
