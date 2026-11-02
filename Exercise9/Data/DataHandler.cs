using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Exercise6.Facilities;

namespace Exercise6.Data
{
    internal static class DataHandler
    {
        internal static async Task<string?> WriteToDatabase(List<Garage> facilities)
        {
            if (facilities.Count == 0)
            {
                return "There is nothing to save, Aborting";
            }
            string solutionDirectory = Path.GetFullPath(
                Path.Combine(AppContext.BaseDirectory, @"..\..\..\")
            );

            string? fileName;

            while (true)
            {
                Console.WriteLine("Please enter a filename to backup to (Leave empty to abort)");
                fileName = Console.ReadLine();

                if (string.IsNullOrEmpty(fileName))
                {
                    return "File name can not be empty";
                }

                string pattern = @"^[a-zA-Z0-9]+$";
                Regex regex = new(pattern, RegexOptions.IgnoreCase);

                if (!regex.IsMatch(fileName))
                {
                    Console.WriteLine("Filname can only contain a-z, 0-9");
                    Console.ReadKey();
                    continue;
                }
                break;
            }
            fileName += ".json";

            string directory = Path.Combine(solutionDirectory, "Data", "Backups");
            string path = Path.Combine(directory, fileName);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var options = new JsonSerializerOptions { IncludeFields = true, WriteIndented = true };

            await using FileStream createStream = File.Create(path);

            try
            {
                await JsonSerializer.SerializeAsync(createStream, facilities, options);
                return $"Data written to file {path}";
            }
            catch (Exception)
            {
                return "Something went wrong";
            }
        }

        internal static string[] GetAvailableBackups()
        {
            string solutionDirectory = Path.GetFullPath(
                Path.Combine(AppContext.BaseDirectory, @"..\..\..\")
            );
            string directory = Path.Combine(solutionDirectory, "Data", "Backups");

            if (!Directory.Exists(directory))
            {
                Console.WriteLine("Du har inga sparade backups");
                Console.ReadKey();
                return [];
            }

            return Directory.GetFiles(directory);
        }

        internal static async Task<string?> ReadFromDatabase(
            List<Garage> facilities,
            string filename
        )
        {
            string solutionDirectory = Path.GetFullPath(
                Path.Combine(AppContext.BaseDirectory, @"..\..\..\")
            );
            string path = Path.Combine(solutionDirectory, @"Data\Backups", filename);

            if (!File.Exists(path))
                return "The file does not exist";

            await using FileStream stream = File.OpenRead(path);
            try
            {
                List<Garage>? loadedFacilities = await JsonSerializer.DeserializeAsync<
                    List<Garage>
                >(stream);

                if (loadedFacilities != null)
                {
                    facilities.Clear();
                    facilities.AddRange(loadedFacilities);
                }

                return "File loaded successfully.";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return $"Something went wrong {ex.Message}";
            }
        }
    }
}
