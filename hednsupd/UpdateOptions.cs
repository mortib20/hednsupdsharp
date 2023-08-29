using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hednsupd
{
    public class UpdateOptions
    {
        public string Key { get; set; } = "EMPTY";
        public string Hostname { get; set; } = "EMPTY";
        public int Delay { get; set; } = 600;
    }
}
