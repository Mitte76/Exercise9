using System;
using System.Linq;
using System.Threading.Tasks;
using Exercise9.Data;
using Exercise9.Services;
using Exercise9.UI.Menus;

namespace Exercise9.UI
{
    internal static class DataMenu
    {
        internal static Menu<GarageSummary> Create(IGarageHandler garageHandler)
        {
            return new Menu<GarageSummary>(
                () => "Manage backups",
                () =>
                    [
                        new MenuItem<GarageSummary>(
                            "Load garage data from file",
                            taskAction: () => LoadBackup(garageHandler)
                        ),
                        new MenuItem<GarageSummary>(
                            "Save garage data to file",
                            taskAction: () => SaveBackup(garageHandler)
                        ),
                        new MenuItem<GarageSummary>("Back"),
                    ]
            );
        }

        private static async Task<string?> SaveBackup(IGarageHandler garageHandler)
        {
            if (garageHandler.GarageCount == 0)
                return "There are no garages to save.";

            Console.Write("Enter a backup file name (leave empty to cancel): ");
            string? fileName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(fileName))
                return "Saving cancelled.";

            return await DataHandler.WriteToFile(garageHandler, fileName.Trim());
        }

        private static async Task<string?> LoadBackup(IGarageHandler garageHandler)
        {
            string[] backups = DataHandler.GetAvailableBackups();
            if (backups.Length == 0)
                return "No saved backups were found.";

            BackupFile? selectedBackup = await Menu<BackupFile>
                .CreateSelectionMenu(
                    () => "Select a backup",
                    backups.Select(fileName => new BackupFile(fileName)),
                    backup => $"{backup.Name}.json"
                )
                .ShowMenu();

            if (selectedBackup == null)
                return "Loading cancelled.";

            return await DataHandler.ReadFromFile(garageHandler, selectedBackup.Name);
        }

        private sealed record BackupFile(string Name);
    }
}
