using System.Reflection;

namespace ConsoleRpgEntities.Helpers
{
    public static class MigrationHelper
    {
        public static string GetMigrationScript(string migrationClassName, string scriptType)
        {
            // scriptType: "Up" or "Down"

            // Build the script file name
            string scriptFileName = scriptType == "Up"
                ? $"{migrationClassName}.sql"
                : $"{migrationClassName}.rollback.sql";

            // Get the directory of the executing assembly
            string assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? Directory.GetCurrentDirectory();

            // Build the path to the SQL script
            string scriptPath = Path.Combine(assemblyLocation, "Migrations", "Scripts", scriptFileName);

            // Check if the file exists
            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException($"SQL script file not found: {scriptPath}");
            }

            // Read and return the SQL script content
            return File.ReadAllText(scriptPath);
        }
    }
}
