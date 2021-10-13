using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nihon_Player
{
    public class Settings
    {
        public double Volume { get; set; } = 100;
        public bool AllowSeek { get; set; } = false;
        public string LatestPlay { get; set; }
        public bool RepeatTrack { get; set; } = false;
        public string AudioLocation { get; set; } = null;
    }
}
