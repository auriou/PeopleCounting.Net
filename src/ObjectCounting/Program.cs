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
                ExitArea = r => r.Y + r.Height > 200,
                ChangeCount = ChangeCount
            };
            
            var counter = new ObjCounter(config);
            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "example_01.mp4");

            var videoFile = new ObjectVideoCapture(file)
            {
                ImageReceive = counter.Process,
            };
            await videoFile.Start();
        }

        static void ChangeCount(ChangeCounter count)
        {
            Console.WriteLine($"{count.Entry}-{count.Exit}={count.Entry- count.Exit}");
        }

    }
}
