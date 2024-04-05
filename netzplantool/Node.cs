using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace netzplantool
{
    internal class Node
    {
        public string Vorgang { get; set; }
        public string Beschreibung { get; set; }
        public int Dauer { get; set; }
        public string[] Vorgänger { get; set; }
        public int FAZ { get; set; }
        public int FEZ { get; set; }
        public int SAZ { get; set; }
        public int SEZ { get; set; }
        public int Puffer { get; set; }
        public int Gesamtpuffer { get; set; }
    }
}
