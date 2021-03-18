using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ConsoleApp2
{

    class Program
    {
    
        static async Task Main(string[] args)
        {
            //http://www.insecam.org/
            var counter = new PeopleCounter();
            var root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            var file = $"{root}\\example_01.mp4";

            var videoFile = new FileCapture(file);
            videoFile.ImageReceive = counter.Process;
           // await videoFile.Start();

            // 16/9
            var test = new RtspCapture(592, 333, "rtsp://admin:philou@192.168.1.29:554/h264_ulaw.sdp");
            test.ImageReceive = counter.Process;
            await test.Start();

            //CvInvoke.DestroyAllWindows();
            /*
            "http://109.190.32.149:82/mjpg/video.mjpg"
http://83.56.31.69/mjpg/video.mjpg
            rtsp://admin:philou@192.168.1.29:554/h264_ulaw.sdp
            rtsp://admin:philou@192.168.1.29:554/ucast/11

            */
        }

    }
}
