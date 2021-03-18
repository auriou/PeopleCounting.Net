using Emgu.CV;
using System;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    public interface ICaptureRun
    {
        Action<Mat> ImageReceive { get; set; }
        Task Start();
        void Stop();
    }
}