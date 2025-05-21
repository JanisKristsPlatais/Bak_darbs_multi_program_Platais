using System;
using System.Collections.Generic;
using Bak_darbs_multi_program_Platais.Models;
using System.Data.SQLite;

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
            int profileId = GetProfileId(oldName);
            if (profileId == -1) return;

            using (var connection = new SQLiteConnection($"Data Source={dbFile}")) {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"UPDATE Profiles SET Name = $newName WHERE Id = $profileId";
                    command.Parameters.AddWithValue("newName", newName);
                    command.Parameters.AddWithValue("$profileId", profileId);
                    command.ExecuteNonQuery();
                }
                }
        }
        public static void DeleteProfile(string profileName)
        {
            int profileId = GetProfileId(profileName);
            if (profileId == -1) return;

            using (var connection = new SQLiteConnection($"Data Source={dbFile}"))
            {
                connection.Open();
                //deleted program info thats connected to profile
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"DELETE FROM Programs WHERE ProfileId = $profileId";
                    command.Parameters.AddWithValue("$profileId", profileId);
                    command.ExecuteNonQuery();
                }
                //delte profile
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"DELETE FROM Profiles WHERE Id = $profileId";
                    command.Parameters.AddWithValue("$profileId", profileId);
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
            //check if program exists in profile
            bool programExists = false;
            using (var connection = new SQLiteConnection($"Data Source={dbFile}"))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT COUNT(*) FROM Programs WHERE ProgramName = $programName AND ProfileId = $profileId";
                    command.Parameters.AddWithValue("$programName", programName);
                    command.Parameters.AddWithValue("$profileId", profileId);

                    var result = command.ExecuteScalar();
                    programExists = Convert.ToInt32(result) > 0;
                }
            }

            if (programExists)
            {
                using (var connection = new SQLiteConnection($"Data Source={dbFile}"))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            UPDATE Programs SET ProgramPath = $programPath, X = $x, Y = $y 
                            WHERE ProgramName = $programName AND ProfileId = $profileId";
                        command.Parameters.AddWithValue("$programName", programName);
                        command.Parameters.AddWithValue("$programPath", programPath);
                        command.Parameters.AddWithValue("$profileId", profileId);
                        command.Parameters.AddWithValue("$x", x);
                        command.Parameters.AddWithValue("$y", y);
                        command.ExecuteNonQuery();
                    }
                }
            }
            else {
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
        }
        public static void UpdateProgram(string oldProgramName, string newProgramName, string programPath, string profileName, int x, int y) {
            int profileId = GetProfileId(profileName);
            if (profileId == -1) {
                AddProfile(profileName);
                profileId = GetProfileId(profileName);
            }
            using (var connection = new SQLiteConnection($"Data Source={dbFile}"))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE Programs 
                        SET ProgramName = $newProgramName, ProgramPath = $programPath, ProfileId=$profileId, X=$x, Y=$y
                        WHERE ProgramName = $oldProgramName AND ProfileId = $profileId";
                    command.Parameters.AddWithValue("$oldProgramName", oldProgramName);
                    command.Parameters.AddWithValue("$newProgramName", newProgramName);
                    command.Parameters.AddWithValue("$programPath", programPath);
                    command.Parameters.AddWithValue("$profileId", profileId);
                    command.Parameters.AddWithValue("$x", x);
                    command.Parameters.AddWithValue("$y", y);

                    int rowsAffected = command.ExecuteNonQuery();
                    if(rowsAffected == 0) SaveProgram(newProgramName, programPath, profileName, x, y);
                }
            }
        }

        public static List<ProgramModel> GetProgramsByProfile(string profileName) { 
            var programs = new List<ProgramModel>();

            using (var connection = new SQLiteConnection($"Data Source={dbFile}")) { 
                connection.Open();

                using (var command = connection.CreateCommand()){
                    command.CommandText = @"
                        SELECT ProgramName, ProgramPath, X, Y FROM Programs
                        INNER JOIN Profiles ON Programs.ProfileId = Profiles.Id WHERE Profiles.Name = $profileName";
                    command.Parameters.AddWithValue("$profileName", profileName);
                    using (var reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            programs.Add(new ProgramModel {
                                ProgramName = reader.GetString(0),
                                Name = System.IO.Path.GetFileName(reader.GetString(1)),
                                Path = reader.GetString(1),
                                X = reader.GetInt32(2),
                                Y = reader.GetInt32(3),
                            });
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
