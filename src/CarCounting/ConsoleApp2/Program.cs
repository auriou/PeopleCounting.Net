using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            var classesNetwork = new string[]{"background", "aeroplane", "bicycle", "bird", "boat",
                "bottle", "bus", "car", "cat", "chair", "cow", "diningtable",
                "dog", "horse", "motorbike", "person", "pottedplant", "sheep",
                "sofa", "train", "tvmonitor" };
            var lastExecute = DateTime.Now;
            var root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            var videoFile = $"{root}\\example_01.mp4";

            var cap = new VideoCapture(videoFile);

            if (!cap.IsOpened)
            {
                Debug.WriteLine("Unable to connect to camera");
                return;
            }

            var net = Emgu.CV.Dnn.DnnInvoke.ReadNetFromCaffe(
                $"{root}\\MobileNetSSD_deploy.prototxt",
                $"{root}\\MobileNetSSD_deploy.caffemodel");
            net.SetPreferableBackend(Emgu.CV.Dnn.Backend.Default);
            net.SetPreferableTarget(Emgu.CV.Dnn.Target.OpenCL);

            var tracker = new Emgu.CV.TrackerCSRT();
            //tracker.Init()

            Mat frame = new Mat();
            Image<Bgr, byte> img;
            Image<Gray, byte> img2;

            var totalDown = 0;

            var totalUp = 0;

            while (true)
            {
                
                cap.Read(frame);
                if (frame.IsEmpty) continue;
                img = frame.ToImage<Bgr, byte>();
                //CvInvoke.Resize(img, img, new Size(260, 84));

                int cols2 = img.Width;

                int rows2 = img.Height;



                CvInvoke.CvtColor(img, img, Emgu.CV.CvEnum.ColorConversion.Bgr2Rgb);

                if (DateTime.Now.Subtract(lastExecute) > TimeSpan.FromMilliseconds(300))
                {
                    lastExecute = DateTime.Now;

                    //blob = Emgu.CV.Dnn.lobFromImage(frame, 0.007843, (W, H), 127.5)

                    var size = new Size(224, 224);
                    var scalar = new MCvScalar(104, 117, 123);
                    var blob = Emgu.CV.Dnn.DnnInvoke.BlobFromImage(frame, 0.007843, frame.Size, scalar, true);

                    net.SetInput(blob, "data");

                    var test = net.LayerNames;

                    var detections = net.Forward();

                    

                    //var probMat = detections.Reshape(1, 1);
                    //var classNumber = new Point();

                    //var tmp = new Point();
                    //double tmpdouble = 0;
                    //double classProb = 0;
                    //CvInvoke.MinMaxLoc(probMat, ref tmpdouble, ref classProb, ref tmp, ref classNumber);

                    //var classId = classNumber.X;

                    //Debug.WriteLine($"{tmpdouble}-{classProb}-{classNumber}-{tmp}-{classId}");

                    //var toto = detections[2];
                    if (true/*classProb == 15*/)
                    {

                        int rows = detections.SizeOfDimension[2];
                        int cols = detections.SizeOfDimension[3];

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

                                    img.Draw(new Rectangle(left, top, right - left, bottom - top), new Bgr(0, 0, 255), 2);
                                }
                            }
                        }
                    }
                }
                CvInvoke.Imshow("img", img);
                if (CvInvoke.WaitKey(1) == 27)
                {
                    break;
                }
            }
            CvInvoke.DestroyAllWindows();
            cap.Dispose();
        }
    }
}
