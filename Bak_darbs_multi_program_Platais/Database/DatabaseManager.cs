using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace Bak_darbs_multi_program_Platais.Database
{
    public static class DatabaseManager
    {
        private static readonly string dbFile = "programs.db";

        public static void InitializeDatabase()
        { //makes table if it doesn't exist
            using (var connection = new SqliteConnection($"Data Source={dbFile}"))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Programs(
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            ProgramNmae TEXT NOT NULL,
                            ProgramPath TEXT NOT NULL
                            ProfileName TEXT NOT NULL);";
                    command.ExecuteNonQuery();
                }
            }
        }
        public static void SaveProgram(string programName, string programPath, string profileName)
        {
            using (var connection = new SqliteConnection($"Data Source={dbFile}"))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        INSERT INTO Programs (ProgramName, ProgramPath, ProfileName)
                        VALUES($programName, $programPath, $profileName)";
                    command.Parameters.AddWithValue("$programName", programName);
                    command.Parameters.AddWithValue("$programPath", programPath);
                    command.Parameters.AddWithValue("$profileName", profileName);
                    command.ExecuteNonQuery();
                }
            }
        }
        public static List<(string Name, string Path)> GetProgramsByProfile(string profileName) { 
            var programs = new List<(string Name, string Path)>();
            using (var connection = new SqliteConnection($"Data Source={dbFile}")) { 
                connection.Open();

                using (var command = connection.CreateCommand()){
                    command.CommandText = @"
                        SELECT ProgramName, ProgramPath FROM Programs WHERE ProfileName = $profileName";
                    command.Parameters.AddWithValue("$profileName", profileName);
                    using (var reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            string name = reader.GetString(0);   
                            string path = reader.GetString(1);
                            programs.Add((name, path));
                        }
                    }
                }
            }
            return programs;
        }
    }
}
