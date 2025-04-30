using System;
using System.Collections.Generic;
using Bak_darbs_multi_program_Platais.Models;
using System.Data.SQLite;
using SQLitePCL;

namespace Bak_darbs_multi_program_Platais.Database
{
    public static class DatabaseManager
    {
        private static readonly string dbFile = "programs.db";

        public static void InitializeDatabase()
        { //makes table if it doesn't exist
            using (var connection = new SQLiteConnection($"Data Source={dbFile}"))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                { //create Profile table
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Profiles(
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT NOT NULL UNIQUE
                            );";
                    command.ExecuteNonQuery();
                }

                using (var command = connection.CreateCommand())
                { //create Programs table
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Programs(
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            ProgramName TEXT NOT NULL,
                            ProgramPath TEXT NOT NULL,
                            ProfileId INTEGER NOT NULL,
                            X INTEGER DEFAULT 0,
                            Y INTEGER DEFAULT 0,
                            FOREIGN KEY(ProfileId) REFERENCES Profiles(Id) ON DELETE CASCADE);";
                    command.ExecuteNonQuery();
                }
            }
        }

        //profiles----------------------

        public static void AddProfile(string profileName) {
            using (var connection = new SQLiteConnection($"Data Source={dbFile}")) { 
                connection.Open();

                using (var command = connection.CreateCommand()) {
                    command.CommandText = @"
                        INSERT OR IGNORE INTO Profiles (Name)
                        VALUES($profileName)";
                    command.Parameters.AddWithValue("$profileName", profileName);
                    command.ExecuteNonQuery();
                }
            }
        }
        public static List<string> GetProfiles() { 
            var profiles = new List<string>();
            using (var connection = new SQLiteConnection($"Data Source={dbFile}"))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT Name FROM Profiles;";
                    using (var reader = command.ExecuteReader()) {
                        while (reader.Read()) profiles.Add(reader.GetString(0));
                    }
                }
            }
            return profiles;
        }
        public static int GetProfileId(string profileName) {
            using (var connection = new SQLiteConnection($"Data Source={dbFile}"))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT Id FROM Profiles WHERE Name = $profileName";
                    command.Parameters.AddWithValue("$profileName", profileName);
                    var result = command.ExecuteScalar(); //shows only first column of the first row (ex. Id)
                    if(result != null) return Convert.ToInt32(result);
                    else return -1; //not found
                }
            }
        }

        public static void UpdateProfileName(string oldName, string newName) {
            using (var connection = new SQLiteConnection($"Data Source={dbFile}")) {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"UPDATE Profiles SET Name = $newName WHERE Name = $oldName";
                    command.Parameters.AddWithValue("$oldName", oldName);
                    command.Parameters.AddWithValue("$newName", newName);
                    command.ExecuteNonQuery();
                }
                }
        }

        //programs--------------
        public static void SaveProgram(string programName, string programPath, string profileName, int x, int y)
        {
            int profileId = GetProfileId(profileName);
            if (profileId == -1)
            {
                AddProfile(profileName);
                profileId = GetProfileId(profileName);
            }

            using (var connection = new SQLiteConnection($"Data Source={dbFile}"))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        INSERT INTO Programs (ProgramName, ProgramPath, ProfileId, X, Y)
                        VALUES($programName, $programPath, $profileId, $x, $y)";
                    command.Parameters.AddWithValue("$programName", programName);
                    command.Parameters.AddWithValue("$programPath", programPath);
                    command.Parameters.AddWithValue("$profileId", profileId);
                    command.Parameters.AddWithValue("$x", x);
                    command.Parameters.AddWithValue("$y", y);
                    command.ExecuteNonQuery();
                }
            }
        }
        public static List<ProgramModel> GetProgramsByProfile(string profileName) { 
            var programs = new List<ProgramModel>();
            int profileId = GetProfileId(profileName);
            if(profileId == -1) return programs;

            using (var connection = new SQLiteConnection($"Data Source={dbFile}")) { 
                connection.Open();

                using (var command = connection.CreateCommand()){
                    command.CommandText = @"
                        SELECT ProgramName, ProgramPath, X, Y FROM Programs WHERE ProfileId = $profileId";
                    command.Parameters.AddWithValue("$profileId", profileId);
                    using (var reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            var program = new ProgramModel {
                                 ProgramName = reader.GetString(0),
                                 Path = reader.GetString(1),
                                    X = reader.GetInt32(2),
                                    Y = reader.GetInt32(3),
                                    Name = reader.GetString(0)
                            };
                            
                            programs.Add(program);
                        }
                    }
                }
            }
            return programs;
        }
        public static void DeleteProgram(string programName, string profileName) {
            int profileId = GetProfileId(profileName);
            if (profileId == -1) return;

            using (var connection = new SQLiteConnection($"Data Source={dbFile}"))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        DELETE FROM Programs WHERE ProgramName = $programName AND ProfileId = $profileId;";
                    command.Parameters.AddWithValue("$programName", programName);
                    command.Parameters.AddWithValue("$profileId", profileId);
                    command.ExecuteNonQuery();
                }
            }

        }
    }
}
