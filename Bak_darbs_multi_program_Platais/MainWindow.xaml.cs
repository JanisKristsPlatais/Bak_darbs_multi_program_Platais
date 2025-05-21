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

namespace Bak_darbs_multi_program_Platais
{
    public partial class MainWindow : Window
    {
        private List<ProgramModel> programs = new List<ProgramModel>();
        private CustomHotkey launchHotkey = new CustomHotkey { Ctrl = true, MainKey = Key.Q }; //default is Ctrl+Q
        private bool isCapturingHotkey = false;

        public ObservableCollection<ProfileModel> profiles = new ObservableCollection<ProfileModel>();

        public MainWindow()
        {

            InitializeComponent();

            DatabaseManager.InitializeDatabase();
            this.PreviewKeyDown += MainWindow_PreviewKeyDown;
            LoadProfiles();

            var dbProfiles = DatabaseManager.GetProfiles();
            if (dbProfiles.Count == 0) {
                DatabaseManager.AddProfile("Profile 1");
                profiles.Add(new ProfileModel { Name = "Profile 1" });
            }

            ProfileCombobox.ItemsSource = profiles;
            ProfileCombobox.SelectionChanged += ProfileCombobox_SelectionChanged;
            if(ProfileCombobox.Items.Count > 0) ProfileCombobox.SelectedIndex = 0;

            UpdateHotkeyLabel();
            
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
                HotkeyLabel.Content = ("Press your hotkey combination. (ESC to cancel)");
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
            for (int i = 0; i < 12; i++) {       // how many empty buttons
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
            foreach (var stackPanel in ProgramsWrapPanel.Children.OfType<StackPanel>())
            {
                if (stackPanel.Children[0] is Grid grid)
                {
                    foreach (var child in grid.Children.OfType<Button>())
                    {
                        if (child.Tag is ProgramModel program && program != null && !string.IsNullOrWhiteSpace(program.Path) && child.Content.ToString() == "O")
                        {
                            RoutedEventArgs args = new RoutedEventArgs();
                            OpenButton_Click(child, args);

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

            CreateEmptyProgramButton();
            if(ProgramsWrapPanel.Children.Contains(AddEmptyButton)) ProgramsWrapPanel.Children.Remove(AddEmptyButton);
            ProgramsWrapPanel.Children.Add(AddEmptyButton);
            isLoadingFromDatabase = false;
            UpdateAddButtonVisibility();
        }

        private void RenameProfile_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string profileName = button.Tag.ToString();

            var selectedItem = ProfileCombobox.ItemContainerGenerator.ContainerFromItem(button.DataContext) as FrameworkElement;
            var textBlock = selectedItem?.FindName("ProfileNameTextBlock") as TextBlock;
            var textBox = selectedItem?.FindName("ProfileNameTextBox") as TextBox;

            if (textBlock != null && textBox != null) { 
                textBlock.Visibility = Visibility.Collapsed;
                textBox.Visibility = Visibility.Visible;
                textBox.Focus();
            }
            textBox.KeyDown += (s, keyEventArgs) =>
            {
                if (keyEventArgs.Key == Key.Enter) { 
                    textBlock.Text = textBox.Text;
                    textBlock.Visibility = Visibility.Visible;
                    textBox.Visibility = Visibility.Collapsed;

                    ProfileModel profile = profiles.FirstOrDefault(p => p.Name == profileName);
                    if (profile != null) { 
                        profile.Name = textBox.Text;
                        DatabaseManager.UpdateProfileName(profileName, textBox.Text);
                    }
                }
            };

        }

        private void ProfileNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.IsReadOnly = false;
        }

        private void ProfileNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            textbox.IsReadOnly = true;
            
            string oldName = textbox.Tag.ToString();
            string newName = textbox.Text;

            if (oldName != newName)
            {
                ProfileModel profile = profiles.FirstOrDefault(p=>p.Name == oldName);
                if (profile != null) {
                    profile.Name = newName;
                    DatabaseManager.UpdateProfileName(oldName, newName);
                }

            }

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

        private void CreateProfile_Click(object sender, RoutedEventArgs e)
        {
            var existingNumbers = profiles.Select(p => int.TryParse(p.Name.Replace("Profile ", ""), out int num) ? num : 0).ToList();
            int nextNumber = 1;
            while (existingNumbers.Contains(nextNumber)) nextNumber++;

            string newProfileName = $"Profile {nextNumber}";
            DatabaseManager.AddProfile(newProfileName);
            profiles.Add(new ProfileModel { Name = newProfileName });
        }

        
    }
}
