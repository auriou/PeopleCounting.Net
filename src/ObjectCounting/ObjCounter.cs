using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Dnn;
using Emgu.CV.Structure;
using System.Linq;

namespace ObjectCounting
{
    //https://stackoverflow.com/questions/64552977/how-to-convert-from-h264-to-ts-using-ffmpeg-wrapper-for-c-net
    public class ObjCounter
    {
        private readonly ConfigCounter _config;
        private string _root;
        private Tracking _tracking;
        private Net _net;
        private DateTime _lastExecute;
        private DateTime _lastExecuteTracker;
        private object _verrou = new object();
        public string[] ClassesNetwork { get; }
        private List<int> _classesNetwork = new List<int>();
        public ObjCounter(ConfigCounter config)
        {
            _config = config;
            _root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            ClassesNetwork = new string[]{"background", "aeroplane", "bicycle", "bird", "boat",
                "bottle", "bus", "car", "cat", "chair", "cow", "diningtable",
                "dog", "horse", "motorbike", "person", "pottedplant", "sheep",
                "sofa", "train", "tvmonitor" };
            for (int i = 0; i < ClassesNetwork.Length; i++)
            {
                if (config.Classes.Contains(ClassesNetwork[i]))
                {
                    _classesNetwork.Add(i);
                }
            }

            _tracking = new Tracking(config);

            _net = DnnInvoke.ReadNetFromCaffe(
                $"{_root}\\MobileNetSSD_deploy.prototxt",
                $"{_root}\\MobileNetSSD_deploy.caffemodel");
            _net.SetPreferableBackend(Emgu.CV.Dnn.Backend.Default);
            _net.SetPreferableTarget(Emgu.CV.Dnn.Target.OpenCL);
            _lastExecute = DateTime.Now;
            _lastExecuteTracker = DateTime.Now;
        }

        public void SimpleProcess(Mat frame)
        {
            if (frame.IsEmpty) return;
           

            var img = frame.ToImage<Bgr, byte>();

            CvInvoke.Imshow("img", img);
            if (CvInvoke.WaitKey(1) == 27)
            {

            }

        }


        public void Process(Mat frameSend)
        {
            //lock (_verrou)
            //{
            if (frameSend.IsEmpty) return;
            using var frame = frameSend;
            if (_config.Tracker && frame.NumberOfChannels != 3)
            {
                CvInvoke.CvtColor(frame, frame, Emgu.CV.CvEnum.ColorConversion.Bgra2Bgr);
            }

            using var img = frame.ToImage<Bgr, byte>();

            if (DateTime.Now.Subtract(_lastExecute) > TimeSpan.FromMilliseconds(_config.TimeRunDetectionMs))
            {
                _lastExecute = DateTime.Now;

                using var imgDnn = frame.ToImage<Bgr, byte>();
                CvInvoke.CvtColor(img, imgDnn, ColorConversion.Bgr2Rgb);
                var size = new Size(
                    imgDnn.Width - (imgDnn.Width * 2 / 100),
                    imgDnn.Height - (imgDnn.Height * 2 / 100));
                var scalar = new MCvScalar(104, 117, 123);
                using var blob = DnnInvoke.BlobFromImage(imgDnn, 0.005, size, scalar, true);


                _net.SetInput(blob, "data");
                using var detections = _net.Forward();

                if (true/*classProb == 15*/)
                {
                    var rectList = new List<Rectangle>();
                    float[,,,] flt = (float[,,,])detections.GetData();

                    for (int x = 0; x < flt.GetLength(2); x++)
                    {
                        if (flt[0, 0, x, 2] > 0.4)
                        {
                            var classe = (int)flt[0, 0, x, 1];
                            if (_classesNetwork.Contains(classe))
                            {
                                int left = Convert.ToInt32(flt[0, 0, x, 3] * frame.Width);
                                int top = Convert.ToInt32(flt[0, 0, x, 4] * frame.Height);
                                int right = Convert.ToInt32(flt[0, 0, x, 5] * frame.Width);
                                int bottom = Convert.ToInt32(flt[0, 0, x, 6] * frame.Height);

                                var rectSelect = new Rectangle(left, top, right - left, bottom - top);
                                rectList.Add(rectSelect);
                            }
                        }
                    }

                    _tracking.AddObjects(rectList, frame);
                }
            }
            else if (_config.Tracker && DateTime.Now.Subtract(_lastExecuteTracker) > TimeSpan.FromMilliseconds(_config.TimeRunTrackingMs))
            {
                _lastExecuteTracker = DateTime.Now;
                _tracking.UpdateTrackers(frame);
            }

            foreach (var obj in _tracking.GetTrackings())
            {
                var rect = obj.Rectangle;
                img.Draw(rect, new Bgr(0, 0, 255), 2);
                CvInvoke.PutText(
                   img,
                   $"{obj.Id}-{obj.Origin}",
                   new Point(rect.Left, rect.Top + rect.Height / 3),
                   FontFace.HersheyDuplex,
                   0.5,
                   new Bgr(0, 255, 0).MCvScalar);
            }

            CvInvoke.PutText(
                img,
                $"Count : {_tracking.Entry - _tracking.Exit}",
                new Point(5, 10),
                FontFace.HersheyDuplex,
                0.5,
                new Bgr(0, 255, 0).MCvScalar);
            CvInvoke.PutText(
                img,
                $"Entry : {_tracking.Entry}",
                new Point(5, 30),
                FontFace.HersheyDuplex,
                0.5,
                new Bgr(0, 255, 0).MCvScalar);
            CvInvoke.PutText(
                img,
                $"Exit : {_tracking.Exit}",
                new Point(5, 50),
                FontFace.HersheyDuplex,
                0.5,
                new Bgr(0, 255, 0).MCvScalar);


            CvInvoke.Imshow("img", img);
            if (CvInvoke.WaitKey(1) == 27)
            {

            }
            //}

            frameSend.Dispose();
        }
    }
}
