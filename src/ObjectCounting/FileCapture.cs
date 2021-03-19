using System;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;

namespace ObjectCounting
{
    public class FileCapture : ICaptureRun
    {
        private readonly string _videoFile;
        private VideoCapture _capture;
        private CancellationTokenSource _cancellationTokenSource;
        private object _verrou = new object();

        public FileCapture(string videoFile)
        {
            _videoFile = videoFile;
            _capture = new VideoCapture(_videoFile);
        }

        public Action<Mat> ImageReceive { get; set ; }

        public async Task Start()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
            _cancellationTokenSource = new CancellationTokenSource();
            TimeSpan delay = TimeSpan.FromSeconds(1);

            while (true)
            {
                try
                {
                    Mat frame = new Mat();
                    _capture.Read(frame);
                    if(frame.IsEmpty)
                    {
                        _capture = new VideoCapture(_videoFile);
                        _capture.Read(frame);
                    }

                    ImageReceive(frame);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception e)
                {
                    await Task.Delay(delay);
                }
            }
        }

        public void Stop()
        {
            
        }
    }
}
