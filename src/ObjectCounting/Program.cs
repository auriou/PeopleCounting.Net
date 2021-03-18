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
                TimeRunDetectionMs = 500,
                TimeRunTrackingMs = 100,
                Tracker = true,
                Classes = new List<string> { "person" },
                EntryArea = r => r.Y < 20,
                ExitArea = r => r.Y + r.Height > 200
            };
            
            var counter = new ObjCounter(config);
            var root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            var file = $"{root}\\example_01.mp4";

            var videoFile = new FileCapture(file) {ImageReceive = counter.Process};
            await videoFile.Start();

            // 16/9
            var test = new RtspCapture(592, 333, "rtsp://admin:philou@192.168.1.29:554/h264_ulaw.sdp")
            {
                ImageReceive = counter.Process
            };
            //await test.Start();

            CvInvoke.DestroyAllWindows();
            /*//http://www.insecam.org/
            "http://109.190.32.149:82/mjpg/video.mjpg"
            http://83.56.31.69/mjpg/video.mjpg
            rtsp://admin:philou@192.168.1.29:554/h264_ulaw.sdp
            rtsp://admin:philou@192.168.1.29:554/ucast/11
            */
        }

    }
}
