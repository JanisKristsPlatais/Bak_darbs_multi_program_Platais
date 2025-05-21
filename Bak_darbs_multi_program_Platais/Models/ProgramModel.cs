using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bak_darbs_multi_program_Platais.Models
{
    public class ProgramModel{
        public int Id { get; set; } = 0;
        public string Name { get; set; }
        public string ProgramName {  get; set; }
        public string Path { get; set; }
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;
        public override string ToString() => Name;
    }
}
