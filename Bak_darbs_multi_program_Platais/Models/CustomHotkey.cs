using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Bak_darbs_multi_program_Platais.Models
{
    public class CustomHotkey
    {
        public Key MainKey { get; set; }
        public bool Ctrl { get; set; }
        public bool Shift { get; set; }
        public bool Alt { get; set; }

        public override string ToString()
        {
            List<string> parts = new List<string>();
            if (Ctrl) parts.Add("Ctrl");
            if (Shift) parts.Add("Shift");
            if (Alt) parts.Add("Alt");
            parts.Add(MainKey.ToString());
            return string.Join(" + ", parts); //put in + between "parts" and other pressed keys
        }
        public bool isMatch(KeyEventArgs e)
        {
            bool ctrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            bool shiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            bool altPressed = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
            Key keyPressed = e.Key == Key.System ? e.SystemKey : e.Key;
            return keyPressed == MainKey && ctrlPressed == Ctrl && shiftPressed == Shift && altPressed == Alt;
        }
    }
}
