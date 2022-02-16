using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdater.Updater
{
    public class UpdateEntry
    {
        public string path { get; set; }
        public string version { get; set; }
        public string sha256Digest { get; set; }
        public bool required { get; set; }
        public bool allowDigestMismatch { get; set; }
    }
}
