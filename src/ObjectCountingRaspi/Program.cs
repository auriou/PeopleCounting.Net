using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Emgu.CV;

namespace ObjectCounting
{
    class Program
    {
    
        static async Task Main(string[] args)
        {
            var config = new ConfigCounter
            {
                TimeRunDetectionMs = 300,
                TimeRunTrackingMs = 100,
                Tracker = true,
                Classes = new List<string> { "person" },
                EntryArea = r => r.Y < 20,
                ExitArea = r => r.Y + r.Height > 200
            };
            
            var counter = new ObjCounter(config);
            var root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            var file = $"{root}\\example_01.mp4";

            var videoFile = new ObjectVideoCapture(0) {ImageReceive = counter.Process};
            await videoFile.Start();
        }

    }
}
