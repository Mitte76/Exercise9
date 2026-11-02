using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Exercise9.Services;

namespace Exercise9.Data
{
    internal static class DataHandler
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
        private static readonly Regex BackupNamePattern = new(@"^[a-zA-Z0-9_-]+$");

        internal static async Task<string> WriteToFile(IGarageHandler garageHandler, string fileName)
        {
            if (garageHandler.GarageCount == 0)
                return "There are no garages to save.";
            if (!IsValidBackupName(fileName))
                return "The file name may only contain letters, numbers, underscores, and hyphens.";

            string path = GetBackupPath(fileName);
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                string json = JsonSerializer.Serialize(garageHandler.CreateBackupSnapshot(), JsonOptions);
                await File.WriteAllTextAsync(path, json);
                return $"Backup saved as {fileName}.json.";
            }
            catch (IOException exception)
            {
                return $"Could not save backup: {exception.Message}";
            }
            catch (UnauthorizedAccessException exception)
            {
                return $"Could not save backup: {exception.Message}";
            }
            catch (JsonException exception)
            {
                return $"Could not serialize backup: {exception.Message}";
            }
        }

        internal static string[] GetAvailableBackups()
        {
            string directory = GetBackupDirectory();
            if (!Directory.Exists(directory))
                return [];

            string[] backups = Directory.GetFiles(directory, "*.json");
            Array.Sort(backups, StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < backups.Length; index++)
                backups[index] = Path.GetFileNameWithoutExtension(backups[index]);

            return backups;
        }

        internal static async Task<string> ReadFromFile(IGarageHandler garageHandler, string fileName)
        {
            if (!IsValidBackupName(fileName))
                return "The selected backup name is invalid.";

            string path = GetBackupPath(fileName);
            if (!File.Exists(path))
                return "The backup file does not exist.";

            try
            {
                string json = await File.ReadAllTextAsync(path);
                GarageSnapshot[]? snapshots = JsonSerializer.Deserialize<GarageSnapshot[]>(
                    UpgradeLegacyBackup(json),
                    JsonOptions
                );
                if (snapshots == null)
                    return "The backup file does not contain valid garage data.";

                garageHandler.RestoreBackupSnapshot(snapshots);
                return $"Backup {fileName}.json loaded successfully.";
            }
            catch (IOException exception)
            {
                return $"Could not read backup: {exception.Message}";
            }
            catch (UnauthorizedAccessException exception)
            {
                return $"Could not read backup: {exception.Message}";
            }
            catch (JsonException exception)
            {
                return $"The backup file is invalid: {exception.Message}";
            }
            catch (InvalidDataException exception)
            {
                return $"The backup data is invalid: {exception.Message}";
            }
            catch (ArgumentException exception)
            {
                return $"The backup data is invalid: {exception.Message}";
            }
        }

        private static string UpgradeLegacyBackup(string json)
        {
            if (JsonNode.Parse(json) is not JsonArray garages)
                return json;

            HashSet<string> existingRegistrationNumbers = new(StringComparer.OrdinalIgnoreCase);
            foreach (JsonObject garage in garages.OfType<JsonObject>())
            {
                if (garage["Vehicles"] is not JsonArray vehicles)
                    continue;

                foreach (JsonObject vehicle in vehicles.OfType<JsonObject>())
                {
                    string? registrationNumber = (string?)vehicle["LicensePlateNumber"];
                    if (!string.IsNullOrWhiteSpace(registrationNumber))
                        existingRegistrationNumbers.Add(registrationNumber);
                }
            }

            int generatedNumber = 0;
            foreach (JsonObject garage in garages.OfType<JsonObject>())
            {
                if (garage["Vehicles"] is not JsonArray vehicles)
                    continue;

                foreach (JsonObject vehicle in vehicles.OfType<JsonObject>())
                {
                    if (vehicle["LicensePlateNumber"] is null)
                    {
                        string candidate;
                        do
                        {
                            if (generatedNumber > 999)
                                throw new JsonException(
                                    "The backup contains too many vehicles without registration numbers."
                                );
                            candidate = $"ZZZ{generatedNumber++:D3}";
                        } while (!existingRegistrationNumbers.Add(candidate));

                        vehicle["LicensePlateNumber"] = candidate;
                    }

                    if (vehicle["NumberOfWheels"] is null)
                    {
                        vehicle["NumberOfWheels"] = (string?)vehicle["type"] switch
                        {
                            "Airplane" => 6,
                            "Boat" => 0,
                            "Bus" => 6,
                            "Car" => 4,
                            "Motorcycle" => 2,
                            _ => 0,
                        };
                    }
                }
            }

            return garages.ToJsonString();
        }

        private static bool IsValidBackupName(string fileName)
        {
            return !string.IsNullOrWhiteSpace(fileName) && BackupNamePattern.IsMatch(fileName);
        }

        private static string GetBackupPath(string fileName)
        {
            return Path.Combine(GetBackupDirectory(), $"{fileName}.json");
        }

        private static string GetBackupDirectory()
        {
            return Path.GetFullPath(
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Data", "Backups")
            );
        }
    }
}
