using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using ObjectCounting.RawFramesDecoding;
using ObjectCounting.RawFramesDecoding.FFmpeg;
using RtspClientSharp;
using RtspClientSharp.RawFrames.Video;

namespace ObjectCounting
{
    public class RtspCapture : ICaptureRun
    {
        private readonly Dictionary<FFmpegVideoCodecId, FFmpegVideoDecoder> _videoDecodersMap = new Dictionary<FFmpegVideoCodecId, FFmpegVideoDecoder>();
        private readonly Size resize;
        private readonly string url;
        private Bitmap bmp;
        private TransformParameters transformParameters;
        private CancellationTokenSource _cancellationTokenSource;
        public Action<Mat> ImageReceive { get; set; }

        public RtspCapture(int width, int height, string url)
        {
            this.resize = new Size(width, height);
            this.url = url;
        }

        public async Task Start() 
        {
            var connectionParameters = new ConnectionParameters(new Uri(url));
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
            _cancellationTokenSource = new CancellationTokenSource();
            TimeSpan delay = TimeSpan.FromSeconds(1);
            using (var rtspClient = new RtspClient(connectionParameters))
            {
                rtspClient.FrameReceived += RtspClient_FrameReceived;
                while (true)
                {
                    try
                    {
                        await rtspClient.ConnectAsync(_cancellationTokenSource.Token);
                        await rtspClient.ReceiveAsync(_cancellationTokenSource.Token);
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
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
            bmp?.Dispose();
            bmp = null;
        }

        private void RtspClient_FrameReceived(object sender, RtspClientSharp.RawFrames.RawFrame e)
        {
            if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            if (!(e is RawVideoFrame rawVideoFrame))
            {
                return;
            }

            var codecId = DetectCodecId(rawVideoFrame);
            if (!_videoDecodersMap.TryGetValue(codecId, out FFmpegVideoDecoder decoder))
            {
                decoder = FFmpegVideoDecoder.CreateDecoder(codecId);
                _videoDecodersMap.Add(codecId, decoder);
            }
            var decodedVideoFrame = decoder.TryDecode(rawVideoFrame);
            if (decodedVideoFrame != null)
            {
                if (bmp != null)
                {
                    bmp.Dispose();
                }

                transformParameters = new TransformParameters(RectangleF.Empty, new Size(resize.Width, resize.Height), ScalingPolicy.Stretch, PixelFormat.Bgra32, ScalingQuality.FastBilinear);
                bmp = new Bitmap(resize.Width, resize.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                // Lock the bitmap's bits.  
                var bmpData = bmp.LockBits(new Rectangle(0,0, resize.Width, resize.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);

                var ptr = bmpData.Scan0;
                decodedVideoFrame.TransformTo(ptr, bmpData.Stride, transformParameters);

                var pf = bmp.PixelFormat;
                var stride = pf == System.Drawing.Imaging.PixelFormat.Format32bppArgb ? bmp.Width * 4 : bmp.Width * 3;

                var cvImage = new Image<Bgra, byte>(bmp.Width, bmp.Height, stride, (IntPtr)bmpData.Scan0);
                var mat = cvImage.Mat;

                bmp.UnlockBits(bmpData);
                ImageReceive?.Invoke(mat);
            }
        }

        private FFmpegVideoCodecId DetectCodecId(RawVideoFrame videoFrame)
        {
            if (videoFrame is RawJpegFrame)
                return FFmpegVideoCodecId.MJPEG;
            if (videoFrame is RawH264Frame)
                return FFmpegVideoCodecId.H264;

            throw new ArgumentOutOfRangeException(nameof(videoFrame));
        }
    }
}
