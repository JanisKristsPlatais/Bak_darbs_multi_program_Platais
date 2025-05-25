using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Bak_darbs_multi_program_Platais.Models;
using Bak_darbs_multi_program_Platais.Services;
using System.Collections.ObjectModel;
using Bak_darbs_multi_program_Platais.Database;
using Bak_darbs_multi_program_Platais.Windows;

namespace Bak_darbs_multi_program_Platais
{
    public partial class MainWindow : Window
    {
        private CustomHotkey launchHotkey = new CustomHotkey { Ctrl = true, MainKey = Key.Q }; //default is Ctrl+Q
        private bool isCapturingHotkey = false;

        public ObservableCollection<ProfileModel> profiles = new ObservableCollection<ProfileModel>();

        public MainWindow()
        {

            InitializeComponent();

            DatabaseManager.InitializeDatabase();
            this.PreviewKeyDown += MainWindow_PreviewKeyDown;
            LoadProfiles();

            //load saved hotkey
            var savedHotkey = DatabaseManager.LoadHotkey();
            if (savedHotkey != null) { 
                launchHotkey = savedHotkey;
                UpdateHotkeyLabel();
            }


            var dbProfiles = DatabaseManager.GetProfiles();
            if (dbProfiles.Count == 0) {
                DatabaseManager.AddProfile("Profile 1");
                profiles.Add(new ProfileModel { Name = "Profile 1" });
            }

            ProfileCombobox.ItemsSource = profiles;
            ProfileCombobox.SelectionChanged += ProfileCombobox_SelectionChanged;
            if(ProfileCombobox.Items.Count > 0) ProfileCombobox.SelectedIndex = 0;

            ThemeCombobox.SelectedIndex = 0;
            ThemeManager.CurrentTheme = "Default";
            ThemeManager.ApplyTheme(this, "Default");
            UpdateHotkeyLabel();
            
        }

        //Theme selection----
        private void ThemeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e){
            if (ThemeCombobox.SelectedItem is ComboBoxItem selectedItem){
                ThemeManager.CurrentTheme = selectedItem.Tag.ToString();
                ThemeManager.ApplyTheme(this);
            }
        }


        //Hotkey management-----------------


        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e) { //if hotkey is pressed, launches all programs
            if (launchHotkey != null && launchHotkey.isMatch(e)) { 
                LaunchAllButton_Click(null, null);
                e.Handled = true;
            }
        }
        private void ChangeHotkeyButton_Click(object sender, RoutedEventArgs e) { //lets user change hotkey
            if (!isCapturingHotkey) { 
                isCapturingHotkey = true;
                HotkeyLabel.Content = ("Press your hotkey combination.\n (ESC to cancel)");
                this.PreviewKeyDown -= CaptureHotkey;
                this.PreviewKeyDown += CaptureHotkey;
            }
        }
        private void CaptureHotkey(object sender, KeyEventArgs e) {
            e.Handled = true;
            if (e.Key == Key.Escape) { //if user presses ESC, end hotkey capture
                EndHotkeyCapture();
                return;
            }
            Key key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (key == Key.LeftCtrl || key == Key.RightCtrl ||
                key == Key.LeftShift || key == Key.RightShift ||
                key == Key.LeftAlt || key == Key.RightAlt ||
                key == Key.LWin || key == Key.RWin) { return; } //skip modifier keys
            launchHotkey = new CustomHotkey
            {
                Ctrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl),
                Shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift),
                Alt = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt),
                MainKey = key
            };

            UpdateHotkeyLabel();
            DatabaseManager.SaveHotkey(launchHotkey);
            MessageBox.Show($"New hotkey set: {launchHotkey}");
            EndHotkeyCapture();
        }
        private void EndHotkeyCapture() { 
            this.PreviewKeyDown -= CaptureHotkey;
            isCapturingHotkey = false;
            HotkeyLabel.Content = $"Current: {launchHotkey}";
        }
        private void UpdateHotkeyLabel() { HotkeyLabel.Content = $"Current: {launchHotkey}"; } //updates hotkey label




        // Program tiles-buttons
        private Button CreateControlButton(string content, RoutedEventHandler clickHandler, Thickness margin, ProgramModel program = null) {
            var button = new Button //make default button
            {
                Content = content,
                Width = 40,
                Height = 20,
                Margin = margin,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Right,
                Tag = program
            };
            button.Click += clickHandler;
            return button;
        }
        private Grid CreateProgramGrid(Button programButton, ProgramModel program) { //defualt program grid with empty buttons
            var grid = new Grid();
            grid.Children.Add(programButton);

            var closeButton = CreateControlButton("X", CloseButton_Click, new Thickness(0, 10, 10, 0));
            grid.Children.Add(closeButton);

            if (program != null) { //if its a program button (has path), also add open button to it
                var openButton = CreateControlButton("O", OpenButton_Click, new Thickness(0, 10, 45, 0), program);
                grid.Children.Add(openButton);
            }
            return grid;

        }
        //empty and add button------------
        private void AddEmptyButton_Click(object sender, RoutedEventArgs e){ AddEmptyProgramTile(); }
        private void CreateEmptyProgramButton() {
            for (int i = 0; i < 4; i++) {       // how many empty buttons
                AddEmptyProgramTile();}
        }
        private void AddEmptyProgramTile(){ //make and add empty button to tile layout
            var emptyButton = new Button{
                Content = "Click or Drag here",
                Width = 150,
                Height = 100,
                Margin = new Thickness(10),
                Tag = null
            };
            emptyButton.Click += ProgramButton_Click;
            emptyButton.AllowDrop = true;
            emptyButton.Drop += ProgramButton_Drop;

            var grid = CreateProgramGrid(emptyButton, null);
            var stackPanel = new StackPanel { Orientation = Orientation.Vertical };
            stackPanel.Children.Add(grid);

            if (ProgramsWrapPanel.Children.Contains(AddEmptyButton)){ //add button before "+" button so that that would be last
                int insertIndex = ProgramsWrapPanel.Children.IndexOf(AddEmptyButton);
                ProgramsWrapPanel.Children.Insert(insertIndex, stackPanel);
            }else ProgramsWrapPanel.Children.Add(stackPanel);
            UpdateAddButtonVisibility();
        }
       

        private void UpdateAddButtonVisibility() { 
            ThemeManager.ApplyTheme(this);
            int programTileCount = ProgramsWrapPanel.Children
                .OfType<StackPanel>().
                Count(sp=>sp.Children.OfType<Grid>().Any());        //count program buttons

            if(programTileCount >= 50) AddEmptyButton.Visibility = Visibility.Collapsed; //max 50 empty/program buttons
            else AddEmptyButton.Visibility = Visibility.Visible;
        }


        private void CreateProgramTile(ProgramModel program) {  //tile for each program
        var programButton = new Button { 
            Content = program.Name, 
            Width = 150, Height = 100, 
            Margin = new Thickness(10), 
            Tag = program }; //store program info in button

            programButton.Click += ProgramButton_Click;
            programButton.AllowDrop = true;
            programButton.Drop += ProgramButton_Drop;

            var grid = CreateProgramGrid(programButton, program);
            var stackPanel = new StackPanel { Orientation = Orientation.Vertical };
            stackPanel.Children.Add(grid);

            if (ProgramsWrapPanel.Children.Contains(AddEmptyButton))
            {
                int insertIndex = ProgramsWrapPanel.Children.IndexOf(AddEmptyButton);
                ProgramsWrapPanel.Children.Insert(insertIndex, stackPanel);
            }
            else ProgramsWrapPanel.Children.Add(stackPanel);

            if (!isLoadingFromDatabase && ProfileCombobox.SelectedItem != null) {
                string profileName = ProfileCombobox.SelectedItem.ToString();
                DatabaseManager.SaveProgram(program.Name, program.Path, profileName, program.X, program.Y);
            }
        }
        private bool isLoadingFromDatabase = false; //used to prevent duplicates



        //program button------------
        private void ProgramButton_Click(object sender, RoutedEventArgs e) { //when clicking on program button, open Info Window
            var button = sender as Button;
            var program = button?.Tag as ProgramModel;
            if (program == null){ //no info => enter name, path
                var inputWindow = new ProgramInfoWindow(program);
                inputWindow.OnSubmit += (name, path, x, y) =>
                {
                    program = new ProgramModel { Name = name, ProgramName = System.IO.Path.GetFileName(path), Path = path , X = x, Y = y};
                    button.Content = name;
                    button.Tag = program;
                    ConvertToProgramButton(button, program);
                    if ((ProfileCombobox.SelectedItem as ProfileModel) != null){
                        string profileName = ProfileCombobox.SelectedItem.ToString();
                        DatabaseManager.SaveProgram(name, path, profileName, x, y);
                    }
                };
                inputWindow.Show();
            } else {    //has info => update info
                string oldName = program.Name;
                var infoWindow = new ProgramInfoWindow(program);

                infoWindow.OnSubmit += (newName, newPath, newX, newY) => { 
                    program.Name = newName;
                    program.ProgramName = System.IO.Path.GetFileName(newPath);
                    program.Path = newPath;
                    program.X = newX;
                    program.Y = newY;
                    button.Content = newName;

                    if (ProfileCombobox.SelectedItem != null){
                        string profileName = ProfileCombobox.SelectedItem.ToString();
                        DatabaseManager.DeleteProgram(oldName, profileName);
                        DatabaseManager.UpdateProgram(oldName, newName, newPath, profileName, newX, newY);
                    }
                };
                infoWindow.Show();
            }
        }
        private void ProgramButton_Drop(object sender, DragEventArgs e){ //handles drag-and-drop of a program onto the button
            var button = sender as Button;

            if (e.Data.GetDataPresent(DataFormats.FileDrop)) { 
                var filePaths = (string[])e.Data.GetData(DataFormats.FileDrop); //dropped might be multiple files
                if (filePaths.Length > 0) { 
                    string filePath = filePaths[0]; //only use first file from dropped ones
                    var program = new ProgramModel { Name = System.IO.Path.GetFileName(filePath), Path = filePath };
                    button.Content = program.Name;
                    button.Tag = program;
                    ConvertToProgramButton(button, program);

                    if (ProfileCombobox.SelectedItem != null) { 
                        string profileName = ProfileCombobox.SelectedItem.ToString();
                        DatabaseManager.SaveProgram(program.Name, program.Path, profileName, program.X, program.Y);
                    }
                }
            }
        }
        private void ConvertToProgramButton(Button button, ProgramModel program){ //make empty button into program button
            var oldGrid = button.Parent as Grid;
            oldGrid?.Children.Remove(button);

            var newGrid = CreateProgramGrid(button, program);
            var parentStack = oldGrid?.Parent as StackPanel;
            if (parentStack != null)
            {
                parentStack.Children.Clear();
                parentStack.Children.Add(newGrid);

            }
            button.Click -= ProgramButton_Click; //clear old
            button.Click += ProgramButton_Click; //add new
        }


        //Other buttons------------
        private void CloseButton_Click(object sender, RoutedEventArgs e) //close selected empty/program button
        {
            var button = sender as Button;
            var grid = button.Parent as Grid;
            if (grid != null) {
                var stackPanel = grid.Parent as StackPanel;
                if (stackPanel != null) {
                    foreach (var child in grid.Children) {
                        if (child is Button programButton && programButton.Tag is ProgramModel program) {
                            if (ProfileCombobox.SelectedItem != null && program != null){
                                string currentProfile = ProfileCombobox.SelectedItem.ToString();
                                DatabaseManager.DeleteProgram(program.Name, currentProfile);  
                            }
                            break;
                        }
                    }

                    ProgramsWrapPanel.Children.Remove(stackPanel); 
                    UpdateAddButtonVisibility();
                }
            }
        }
        private async void OpenButton_Click(object sender, RoutedEventArgs e) //open program and move it to coords | open website in new tab
        {
            int findTimeout = 6000; //in miliseconds
            var button = sender as Button;
            var program = button.Tag as ProgramModel;
            if (program != null && !string.IsNullOrWhiteSpace(program.Path)) {
                try{
                    var process = System.Diagnostics.Process.Start(program.Path);
                    if (process != null){
                        await Task.Delay(500);
                        IntPtr windowHandle = IntPtr.Zero; //IntrPtr.Zero is like null
                        DateTime startTime = DateTime.Now;
                        while (windowHandle == IntPtr.Zero && (DateTime.Now - startTime).TotalMilliseconds < findTimeout){ //find window handle during time
                            windowHandle = WindowManager.FindWindow(null, program.ProgramName);
                            await Task.Delay(250);} //gives a bit of time for program to open
                        if (windowHandle != IntPtr.Zero) //if program window handle found
                        {
                            await Task.Delay(500);
                            WindowManager.ShowWindow(windowHandle, 9); //window in restored state to move easier
                            WindowManager.MoveWindow(program.ProgramName, program.X, program.Y);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to open program: \n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else MessageBox.Show($"Program path not set.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);

        }

        private void LaunchAllButton_Click(object sender, RoutedEventArgs e) //launch all programs = press all open buttons
        {
            foreach (var stackPanel in ProgramsWrapPanel.Children.OfType<StackPanel>()){
                if (stackPanel.Children[0] is Grid grid){
                    foreach (var child in grid.Children.OfType<Button>().Where(b => b.Content.ToString() == "O")){
                        if (child.Tag is ProgramModel program && !string.IsNullOrWhiteSpace(program.Path)){
                            OpenButton_Click(child, new RoutedEventArgs());
                        }
                    }
                }
            }
        }



        //Profile------------
        public void LoadProfiles()
        {
            profiles.Clear();
            var dbProfiles = DatabaseManager.GetProfiles();
            if (dbProfiles.Count == 0) {
                DatabaseManager.AddProfile("Profile 1");
                dbProfiles = DatabaseManager.GetProfiles(); //refresh list
            }
            foreach (var profile in dbProfiles) {
                profiles.Add(new ProfileModel { Name = profile });
            }
            ProfileCombobox.ItemsSource=profiles;
            if (profiles.Count > 0 && ProfileCombobox.SelectedIndex == -1) {
                ProfileCombobox.SelectedIndex = 0;
            }
        }

        private void ProfileCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProfileCombobox.SelectedItem == null) return;

            ProgramsWrapPanel.Children.Clear();
            var selectedProfileName = ProfileCombobox.SelectedItem.ToString();

            isLoadingFromDatabase =true;

            var loadedPrograms = DatabaseManager.GetProgramsByProfile(selectedProfileName);

            foreach (var program in loadedPrograms) CreateProgramTile(program);

            if(loadedPrograms.Count == 0 ) CreateEmptyProgramButton();
            if(ProgramsWrapPanel.Children.Contains(AddEmptyButton)) ProgramsWrapPanel.Children.Remove(AddEmptyButton);
            ProgramsWrapPanel.Children.Add(AddEmptyButton);
            isLoadingFromDatabase = false;
            UpdateAddButtonVisibility();
            ThemeManager.ApplyTheme(this);
        }

        private void DeleteProfile_Click(object sender, RoutedEventArgs e)
        {
            if (profiles.Count <= 1) { MessageBox.Show("Cannot Delete The Last Profile!"); return; }
            var button = sender as Button;
            string profileName = button.Tag as string;

            DatabaseManager.DeleteProfile(profileName);
            for (int i = 0; i < profiles.Count; i++) {
                if (profiles[i].Name == profileName) { profiles.RemoveAt(i); break; }
            }

            if (ProfileCombobox.SelectedIndex == -1 && profiles.Count > 0)
            {
                ProfileCombobox.SelectedIndex = 0;
            }
        }
        private void EditProfile_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            string oldName = button.Tag.ToString();
            bool validNameEntered = false;

            while (!validNameEntered) { 

            var profileInfoWindow = new ProfileInfoWindow("Rename Profile", "Enter new profile name:", oldName);
                if (profileInfoWindow.ShowDialog() == true)
                {
                    string newName = profileInfoWindow.ResponseText;
                    if (!string.IsNullOrWhiteSpace(newName) && newName != oldName)
                    {
                        ProfileModel profile = profiles.FirstOrDefault(p => p.Name == oldName);
                        if (profile != null)
                        {
                            if (DatabaseManager.UpdateProfileName(oldName, newName))
                            {
                                profile.Name = newName;
                                ProfileCombobox.SelectedItem = profile;
                                LoadProfiles();
                                ProfileCombobox.SelectedItem = profiles.FirstOrDefault(p => p.Name == newName);
                                ProfileCombobox_SelectionChanged(ProfileCombobox, null);
                                validNameEntered = true;
                            }
                            else MessageBox.Show("Failed to update profile name. The name may already exist.", "Error",
                                                  MessageBoxButton.OK, MessageBoxImage.Error);
                        }else validNameEntered = true;
                    }
                } else validNameEntered = true;
            }
        }

        private void CreateProfile_Click(object sender, RoutedEventArgs e)
        {
            var existingNumbers = profiles.Select(p => int.TryParse(p.Name.Replace("Profile ", ""), out int num) ? num : 0).ToList();
            int nextNumber = 1;
            while (existingNumbers.Contains(nextNumber)) nextNumber++;

            string newProfileName = $"Profile {nextNumber}";
            DatabaseManager.AddProfile(newProfileName);
            profiles.Add(new ProfileModel { Name = newProfileName });
        }

        private void ImportProfile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog { 
                DefaultExt = ".json",
                Filter = "JSON Files (*.json)|.json"
            };
            if (dialog.ShowDialog() == true)
            {
                try { 
                    string json = System.IO.File.ReadAllText(dialog.FileName);
                    var importedProfile = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, List<ProgramModel>>>(json);
                    if (importedProfile != null && importedProfile.Count > 0) {
                        var profileName = importedProfile.Keys.First();
                        var programs = importedProfile[profileName];

                        DatabaseManager.AddProfile(profileName);
                        foreach (var program in programs) DatabaseManager.SaveProgram(program.Name, program.Path, profileName, program.X, program.Y);
                        LoadProfiles();
                        ProfileCombobox.SelectedItem = profiles.FirstOrDefault(p=>p.Name == profileName);
                        MessageBox.Show("Profile imported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }catch(Exception ex) {
                    MessageBox.Show($"Error importing profile: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void ExportProfile_Click(object sender, RoutedEventArgs e)
        {
            if (ProfileCombobox.SelectedItem == null) return;
            var profileName = ProfileCombobox.SelectedItem.ToString();
            var programs = DatabaseManager.GetProgramsByProfile(profileName);
            var exportData = new Dictionary<string, List<ProgramModel>>{ { profileName, programs } };
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                DefaultExt = ".json",
                Filter = "JSON Files (*.json)|.json",
                FileName = $"{profileName}.json"
            };
            if (dialog.ShowDialog() == true) {
                try {
                    string json = System.Text.Json.JsonSerializer.Serialize(exportData, new System.Text.Json.JsonSerializerOptions { WriteIndented=true});
                    System.IO.File.WriteAllText(dialog.FileName, json);
                    MessageBox.Show("Profile exported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex) {
                    MessageBox.Show($"Error exporting profile: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ScanActivePrograms_Click(object sender, RoutedEventArgs e) {
            try{
                var activeWindows = WindowManager.GetActiveWindows();
                if (activeWindows.Count == 0){
                    MessageBox.Show("No active programs found / insufficient permissions.\nTry running the application as administrator.",
                        "No Programs Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                //make new profile
                var existingNumbers = profiles.Select(p => int.TryParse(p.Name.Replace("Scan ", ""), out int num) ? num : 0).ToList();
                int nextNumber = 1;
                while (existingNumbers.Contains(nextNumber)) nextNumber++;
                string newProfileName = $"Scan {nextNumber}";

                DatabaseManager.AddProfile(newProfileName);
                profiles.Add(new ProfileModel { Name = newProfileName });

                foreach (var program in activeWindows){
                    DatabaseManager.SaveProgram(program.Name, program.Path, newProfileName, program.X, program.Y);
                }
                // Switch to new profile
                ProfileCombobox.SelectedItem = profiles.FirstOrDefault(p => p.Name == newProfileName);
                MessageBox.Show($"Added {activeWindows.Count} programs to new profile: {newProfileName}",
                    "Scan Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error scanning active programs: {ex.Message}\nTry running the application as administrator.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
