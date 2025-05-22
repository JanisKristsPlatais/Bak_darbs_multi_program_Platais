using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using Bak_darbs_multi_program_Platais.Models;
using IWshRuntimeLibrary;

namespace Bak_darbs_multi_program_Platais.Services
{
    public class WindowManager
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string classOfWindow, string titleOfWindow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr windowHandle, IntPtr windowZOrder, int newX, int newY, int width, int height, uint Flags);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr windowHandle, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam); //lists all top level windows

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount); //window title text

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex); //get info about window's style

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

        public static void MoveWindow(string windowTitle, int x, int y){
            IntPtr windowHandle = FindWindow(null, windowTitle);
            if (windowHandle != IntPtr.Zero)
            {
                SetWindowPos(windowHandle, IntPtr.Zero, x, y, 0, 0, 0x0004); //move window to coords | 0x0004 = no window z-order change
            }
        }

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        //exclude from enumeration (system stuff or irrelevent windows/background processes) so only running files would be left
        private static readonly HashSet<string> ExcludedProcesses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ApplicationFrameHost",
            "TextInputHost",
            "dwm",
            "SearchUI",
            "ShellExperienceHost",
            "sihost",
            "RuntimeBroker",
            "System",
            "SystemSettings",
            "LockApp",
            "explorer",
            "Bak_darbs_multi_program_Platais", //own app
            "chrome", // Exclude browsers
            "msedge",
            "firefox",
            "Photos", // Exclude photo
            "ServiceHub.ThreadedWaitDialog"
            //others might need to be added
        };

        //gets full file path using WMI (Windows Management Instrumentation - lets program ask OS for info) - used as backup when normal access is blocked
        private static string GetProcessPathByWMI(int processId) {
            try {
                string query = $"SELECT ExecutablePath FROM Win32_Process WHERE ProcessId = {processId}";
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query)){
                    foreach (ManagementObject obj in searcher.Get()){
                        return obj["ExecutablePath"]?.ToString();
                    }
                }
            }catch (Exception ex){
                Debug.WriteLine($"WMI fallback failed: {ex.Message}");}
            return null;
        }

        // resolve .lnk shortcut to target .exe path using COM
        public static string ResolveShortcut(string shortcutPath) {
            try{
                var shell = new WshShell();
                IWshShortcut link = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                return link.TargetPath;
            }catch (Exception ex) {
                Debug.WriteLine($"Failed to resolve shortcut '{shortcutPath}': {ex.Message}");
                return null;
            }
        }

        //enumerates all visible windows that are not tools/excluded and returns ProgramModel with name, path, current position
        public static List<ProgramModel> GetActiveWindows(){
            var activeWindows = new List<ProgramModel>();
            var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            //shortcut map: key = exe path, value = shortcut path
            var shortcutMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var startMenuPaths = new List<string>{
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu)
            };

            foreach (var startMenuPath in startMenuPaths){
                if (!Directory.Exists(startMenuPath)) continue;

                try{
                    foreach (var shortcutFile in Directory.EnumerateFiles(startMenuPath, "*.lnk", SearchOption.AllDirectories)){
                        var target = ResolveShortcut(shortcutFile);
                        if (!string.IsNullOrEmpty(target)){
                            var normalizedTarget = Path.GetFullPath(target);  //normalize paths for comparison
                            if (!shortcutMap.ContainsKey(normalizedTarget)){
                                shortcutMap[normalizedTarget] = shortcutFile;
                            }
                        }
                    }
                } catch (Exception ex){
                    Debug.WriteLine($"Error enumerating shortcuts in '{startMenuPath}': {ex.Message}");
                }
            }


            EnumWindows((hWnd, lParam) => {
                if (!IsWindowVisible(hWnd)) return true;

                int style = GetWindowLong(hWnd, GWL_EXSTYLE);
                if ((style & WS_EX_TOOLWINDOW) != 0) return true;

                StringBuilder title = new StringBuilder(256);
                GetWindowText(hWnd, title, 256);
                string windowTitle = title.ToString().Trim();

                if (string.IsNullOrEmpty(windowTitle)) return true;

                GetWindowThreadProcessId(hWnd, out uint pid);

                try{
                    var process = Process.GetProcessById((int)pid);

                    if (ExcludedProcesses.Contains(process.ProcessName)) return true;
                    string processPath = string.Empty;

                    try{
                        processPath = process.MainModule?.FileName ?? string.Empty;
                    }catch (System.ComponentModel.Win32Exception){
                        processPath = GetProcessPathByWMI((int)pid) ?? string.Empty;
                    }

                    if (string.IsNullOrEmpty(processPath)){
                        processPath = process.ProcessName + ".exe";
                    }
                    //normalize exe path for comparison
                    var normalizedProcessPath = Path.GetFullPath(processPath);

                    //if shortcut exists for this exe, use the shortcut path instead
                    if (shortcutMap.TryGetValue(normalizedProcessPath, out string shortcutPath) && System.IO.File.Exists(shortcutPath)){
                        processPath = shortcutPath;
                    }

                    if (!seenPaths.Contains(processPath)){
                        string finalName = windowTitle.Contains(" - ") ? windowTitle.Substring(0, windowTitle.IndexOf(" - ")) : windowTitle;

                        if (!GetWindowRect(hWnd, out RECT rect)){
                            rect = new RECT { Left = 0, Top = 0, Right = 0, Bottom = 0 };
                        }

                        activeWindows.Add(new ProgramModel{
                            Name = finalName,
                            ProgramName = process.ProcessName,
                            Path = processPath,
                            X = rect.Left,
                            Y = rect.Top
                        });
                        seenPaths.Add(processPath);
                    }
                }
                catch (Exception ex){
                    Debug.WriteLine($"Error processing window '{windowTitle}': {ex.Message}");
                }
                return true;
            }, IntPtr.Zero);

            return activeWindows;
        }
    }
}
