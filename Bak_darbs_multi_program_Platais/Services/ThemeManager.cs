using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Bak_darbs_multi_program_Platais.Services
{
    public static class ThemeManager
    {
        public static event Action<string> ThemeChanged;
        private static string _currentTheme = "Default";

        public static string CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    ThemeChanged?.Invoke(_currentTheme);
                }
            }
        }
        public static void ApplyTheme(Window window, string themeName = null)
        {
            themeName = themeName ?? CurrentTheme;

            switch (themeName)
            {
                case "Default":
                    window.Background = new SolidColorBrush(Colors.White);
                    ApplyControlColors(window, Colors.White, Colors.Black, Colors.LightGray, themeName);
                    break;
                case "Dark":
                    window.Background = new SolidColorBrush(Color.FromRgb(45, 45, 48));
                    ApplyControlColors(window, Color.FromRgb(45, 45, 48), Colors.White, Color.FromRgb(62, 62, 66), themeName);
                    break;
                case "Blue":
                    window.Background = new SolidColorBrush(Color.FromRgb(30, 50, 100));
                    ApplyControlColors(window, Color.FromRgb(30, 50, 100), Colors.White, Color.FromRgb(50, 70, 120), themeName);
                    break;
            }
        }
        private static void ApplyControlColors(Window window, Color backgroundColor, Color textColor, Color buttonColor, string themeName)
        {
            var backgroundBrush = new SolidColorBrush(backgroundColor);
            var textBrush = new SolidColorBrush(textColor);
            var buttonBrush = new SolidColorBrush(buttonColor);

            window.Foreground = textBrush;

            foreach (var label in FindVisualChildren<Label>(window))
            {
                label.Background = backgroundBrush;
                label.Foreground = textBrush;
            }
            foreach (var button in FindVisualChildren<Button>(window))
            {
                button.Background = buttonBrush;
                button.Foreground = textBrush;
            }
            foreach (var textBlock in FindVisualChildren<TextBlock>(window))
            {
                textBlock.Foreground = textBrush;
            }
            foreach (var scrollViewer in FindVisualChildren<ScrollViewer>(window))
            {
                scrollViewer.Background = backgroundBrush;
                scrollViewer.Foreground = textBrush;
            }
            foreach (var panel in FindVisualChildren<Panel>(window)) panel.Background = backgroundBrush;
            if (window.FindName("ProgramsWrapPanel") is Panel wrapPanel) wrapPanel.Background = backgroundBrush;


            foreach (var comboBox in FindVisualChildren<ComboBox>(window))
            {
                comboBox.Background = buttonBrush;
                comboBox.Foreground = textBrush;

                var comboBoxItemStyle = new Style(typeof(ComboBoxItem));

                //default appearance
                comboBoxItemStyle.Setters.Add(new Setter(ComboBoxItem.BackgroundProperty, buttonBrush));
                comboBoxItemStyle.Setters.Add(new Setter(ComboBoxItem.ForegroundProperty, textBrush));

                //when selected - use black text
                comboBoxItemStyle.Triggers.Add(new Trigger{
                    Property = System.Windows.Controls.Primitives.Selector.IsSelectedProperty,
                    Value = true,
                    Setters ={ new Setter(ComboBoxItem.ForegroundProperty, Brushes.Black) }
                });
                comboBox.ItemContainerStyle = comboBoxItemStyle;

                foreach (var textBox in FindVisualChildren<TextBox>(comboBox)){
                    textBox.Background = buttonBrush;
                    textBox.Foreground = textBrush;
                }
                foreach (var border in FindVisualChildren<Border>(comboBox)){
                    border.Background = buttonBrush;
                }
            }
        }
        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            if (dependencyObject != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(dependencyObject, i);
                    if (child != null && child is T) yield return (T)child;
                    foreach (T childOfChild in FindVisualChildren<T>(child)) { yield return (T)childOfChild; }
                }
            }
        }
    }
}
