using System;
using System.Collections.Generic;
using System.Drawing;

namespace ObjectCounting
{
    public class ConfigCounter
    {
        public bool Tracker { get; set; }
        public List<string> Classes { get; set; }
        public int TimeRunDetectionMs { get; set; }
        public int TimeRunTrackingMs { get; set; }
        public Func<Rectangle, bool> EntryArea { get; set; }
        public Func<Rectangle, bool> ExitArea { get; set; }
    }
}