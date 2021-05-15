using System;
using System.Diagnostics.Tracing;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;

namespace ObjectCounting
{
    public class ObjectVideoCapture : ICaptureRun
    {
        private readonly string _videoFile;
        private int _camera = -1;
        private VideoCapture _capture;
        private CancellationTokenSource _cancellationTokenSource;
        private object _verrou = new object();

        public ObjectVideoCapture(string videoFile)
        {
            _videoFile = videoFile;
            _capture = new VideoCapture(_videoFile);
        }

        public ObjectVideoCapture(int camera)
        {
            _camera = camera;
            _capture = new VideoCapture(camera);
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
                        if(_camera < 0)
                        {
                            _capture = new VideoCapture(_videoFile);
                        }
                        continue;
                    }

                    ImageReceive(frame);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    System.Diagnostics.Debug.WriteLine(e.Message);
                    await Task.Delay(delay);
                }
            }
        }

        public void Stop()
        {
            
        }
    }
}
