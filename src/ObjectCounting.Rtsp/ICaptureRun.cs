using System;
using System.Threading.Tasks;
using Emgu.CV;

namespace ObjectCounting
{
    public interface ICaptureRun
    {
        Action<Mat> ImageReceive { get; set; }
        Task Start();
        void Stop();
    }
}