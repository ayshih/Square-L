using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Square_L
{
    public class Identity
    {
        public string nickname { get; set; }
        public DateTime lastUsed { get; set; }
        public byte[] masterKey { get; set; }
        public byte[] passwordSalt { get; set; }
        public byte[] passwordHash { get; set; }
    }
}
