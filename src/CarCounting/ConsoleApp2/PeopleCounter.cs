using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Dnn;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace ConsoleApp2
{
    //https://stackoverflow.com/questions/64552977/how-to-convert-from-h264-to-ts-using-ffmpeg-wrapper-for-c-net
    public class PeopleCounter
    {
        private string _root;
        private string[] _classesNetwork;
        private Tracking _tracking;
        private Net _net;
        private DateTime _lastExecute;
        private object _verrou = new object();

        public PeopleCounter(bool noTrack = false)
        {
            _root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            _classesNetwork = new string[]{"background", "aeroplane", "bicycle", "bird", "boat",
                "bottle", "bus", "car", "cat", "chair", "cow", "diningtable",
                "dog", "horse", "motorbike", "person", "pottedplant", "sheep",
                "sofa", "train", "tvmonitor" };

            _tracking = new Tracking(r => r.Y > 0, r => r.Y > 10000000, noTrack);

            _net = Emgu.CV.Dnn.DnnInvoke.ReadNetFromCaffe(
                $"{_root}\\MobileNetSSD_deploy.prototxt",
                $"{_root}\\MobileNetSSD_deploy.caffemodel");
            _net.SetPreferableBackend(Emgu.CV.Dnn.Backend.Default);
            _net.SetPreferableTarget(Emgu.CV.Dnn.Target.OpenCL);
            _lastExecute = DateTime.Now;
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

            lock (_verrou)
            {
                if (frameSend.IsEmpty) return;
                var frame = frameSend.Clone();
                var img = frame.ToImage<Bgr, byte>();
                var imgDnn = frame.ToImage<Bgr, byte>();

                CvInvoke.CvtColor(img, imgDnn, ColorConversion.Bgr2Rgb);

                if (DateTime.Now.Subtract(_lastExecute) > TimeSpan.FromMilliseconds(500))
                {
                    _lastExecute = DateTime.Now;
                    var size = new Size(
                        imgDnn.Width - (imgDnn.Width * 2 / 100),
                        imgDnn.Height - (imgDnn.Height * 2 / 100));
                    var scalar = new MCvScalar(104, 117, 123);
                    var blob = DnnInvoke.BlobFromImage(imgDnn, 0.005, size, scalar, true);

                    _net.SetInput(blob, "data");
                    var detections = _net.Forward();

                    if (true/*classProb == 15*/)
                    {
                        var rectList = new List<Rectangle>();
                        float[,,,] flt = (float[,,,])detections.GetData();

                        for (int x = 0; x < flt.GetLength(2); x++)
                        {
                            if (flt[0, 0, x, 2] > 0.4)
                            {
                                var classe = flt[0, 0, x, 1];
                                if (classe == 15)
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

                foreach (var obj in _tracking.GetTrackings())
                {
                    var rect = obj.Rectangle;
                    img.Draw(rect, new Bgr(0, 0, 255), 2);
                    CvInvoke.PutText(
                       img,
                       $"{obj.Id}-{obj.Origin}",
                       new Point(rect.Left, rect.Top + rect.Height / 3),
                       FontFace.HersheyComplex,
                       0.5,
                       new Bgr(0, 255, 0).MCvScalar);
                }


                CvInvoke.Imshow("img", img);
                if (CvInvoke.WaitKey(1) == 27)
                {

                }
            }
        }
    }
}
