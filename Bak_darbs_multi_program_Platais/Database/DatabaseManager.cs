using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows.Input;
using Bak_darbs_multi_program_Platais.Models;

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
                using (var command = connection.CreateCommand())
                { //create hotkey table
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Hotkeys(
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            MainKey TEXT NOT NULL,
                            Ctrl INTEGER NOT NULL,
                            Shift INTEGER NOT NULL,
                            Alt INTEGER NOT NULL);";
                    command.ExecuteNonQuery();
                }
                using (var command = connection.CreateCommand()){ //create themes table
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Themes(
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            ThemeName TEXT NOT NULL);";
                    command.ExecuteNonQuery();
                }
                using (var command = connection.CreateCommand()) {
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Settings (
                            Key TEXT PRIMARY KEY,
                            Value TEXT NOT NULL  );";
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


        public static bool ProfileNameExists(string name)
        {
            using (var connection = new SQLiteConnection($"Data Source={dbFile}"))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"SELECT COUNT(*) FROM Profiles WHERE Name=$name";
                    command.Parameters.AddWithValue("$name", name);
                    return Convert.ToInt32(command.ExecuteScalar())>0;
                }
            }
        }
        public static bool UpdateProfileName(string oldName, string newName) {
            if (string.IsNullOrWhiteSpace(newName) || oldName == newName) return false;
            if (ProfileNameExists(newName)) return false;

            int profileId = GetProfileId(oldName);
            if (profileId == -1) return false;

            using (var connection = new SQLiteConnection($"Data Source={dbFile}")) {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"UPDATE Profiles SET Name = $newName WHERE Id = $profileId";
                    command.Parameters.AddWithValue("$newName", newName);
                    command.Parameters.AddWithValue("$profileId", profileId);
                    command.ExecuteNonQuery();
                } return true;
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
                //delete profile
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
        //hotkey--------------
        public static void SaveHotkey(CustomHotkey hotkey) {
            using (var connection = new SQLiteConnection($"Data Source={dbFile}"))
            {
                connection.Open();
                using (var command = connection.CreateCommand()){
                    command.CommandText = @"
                        DELETE FROM Hotkeys"; //clear existing hotkey
                    command.ExecuteNonQuery();
                }
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        INSERT INTO Hotkeys (MainKey, Ctrl, Shift, Alt) VALUES($mainKey, $ctrl, $shift, $alt)";
                    command.Parameters.AddWithValue("$mainKey", hotkey.MainKey.ToString());
                    command.Parameters.AddWithValue("$ctrl", hotkey.Ctrl ? 1 : 0);
                    command.Parameters.AddWithValue("$shift", hotkey.Shift ? 1 : 0);
                    command.Parameters.AddWithValue("$alt", hotkey.Alt ? 1 : 0);
                    command.ExecuteNonQuery();
                }
            }
        }
        public static CustomHotkey LoadHotkey() {
            using (var connection = new SQLiteConnection($"Data Source={dbFile}"))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT MainKey, Ctrl, Shift, Alt FROM Hotkeys LIMIT 1;";
                    using (var reader = command.ExecuteReader()) {
                        if (reader.Read()) {
                            return new CustomHotkey { 
                                MainKey = (Key)Enum.Parse(typeof(Key),reader.GetString(0)),
                                Ctrl = reader.GetInt32(1) == 1,
                                Shift = reader.GetInt32(2) == 1,
                                Alt = reader.GetInt32(3) == 1,
                            };
                        }
                    }
                }
            }
            return null;
        }


        //theme------------
        public static void SaveTheme(string themeName)
        {
            using (var connection = new SQLiteConnection($"Data Source={dbFile}"))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        DELETE FROM Themes";
                    command.ExecuteNonQuery();
                }
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        INSERT INTO Themes (ThemeName) VALUES($themeName)";
                    command.Parameters.AddWithValue("$themeName", themeName);
                    command.ExecuteNonQuery();
                }
            }
        }
        public static string LoadTheme()
        {
            using (var connection = new SQLiteConnection($"Data Source={dbFile}"))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT ThemeName FROM Themes LIMIT 1;";
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.GetString(0);
                        }
                    }
                }
            }
            return "Default";
        }

        public static void SaveWindowSize(double width, double height)
        {
            using (var connection = new SQLiteConnection($"Data Source={dbFile}"))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT OR REPLACE INTO Settings (Key, Value) VALUES ('WindowWidth', @width)";
                    command.Parameters.AddWithValue("@width", width.ToString());
                    command.ExecuteNonQuery();
                }
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT OR REPLACE INTO Settings (Key, Value) VALUES ('WindowHeight', @height)";
                    command.Parameters.AddWithValue("@height", height.ToString());
                    command.ExecuteNonQuery();
                }
            }
        }
        public static (double width, double height) LoadWindowSize() {
            using (var connection = new SQLiteConnection($"Data Source={dbFile}"))
            {
                connection.Open();
                double width = 750;
                double height = 800;
                using (var command = connection.CreateCommand()){
                    command.CommandText = "SELECT Value FROM Settings WHERE Key = @key";
                    command.Parameters.AddWithValue("@key", "WindowWidth");
                    var widthResult = command.ExecuteScalar()?.ToString();
                    if (double.TryParse(widthResult, out double savedWidth)) width = savedWidth;
                }
                using (var command = connection.CreateCommand()){
                    command.CommandText = "SELECT Value FROM Settings WHERE Key = @key";
                    command.Parameters.AddWithValue("@key", "WindowHeight");
                    var heightResult = command.ExecuteScalar()?.ToString();
                    if (double.TryParse(heightResult, out double savedHeight)) height = savedHeight;
                }
                return (width, height);
            }
        }
    }
}
