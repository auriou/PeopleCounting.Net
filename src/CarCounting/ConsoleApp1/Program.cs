using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using Emgu.CV.Util;

namespace ConsoleApplication2
{
    class Program
    {
        // chargement de la vidéo
        static VideoCapture cap = new VideoCapture(@"C:\Users\alexis\Documents\PeopleCounting.Net\videos\cars.mp4");
        static BackgroundSubtractorMOG2 subtrac = new BackgroundSubtractorMOG2();
        static void Main(string[] args)
        {
            Mat frame = new Mat();
            Image<Bgr, byte> img;

            Image<Bgr, byte> copy;


            Image<Gray, byte> sub;
            Image<Gray, byte> filter;


            VectorOfVectorOfPoint cont = new VectorOfVectorOfPoint();
            int cars = 0;
            while (true)
            {
                cap.Read(frame);


                img = frame.ToImage<Bgr, byte>();

                CvInvoke.Circle(img, new Point(0, (img.Rows - 150)), 5, new MCvScalar(255, 255, 255), 5);
                CvInvoke.Circle(img, new Point((img.Cols / 2) - 110, (img.Rows / 2) - 50), 5, new MCvScalar(255, 255, 255), 5);
                CvInvoke.Circle(img, new Point((img.Cols / 2) + 150, (img.Rows / 2) - 50), 5, new MCvScalar(255, 255, 255), 5);
                CvInvoke.Circle(img, new Point(img.Cols - 1, img.Rows - 1), 5, new MCvScalar(255, 255, 255), 5);

                CvInvoke.Circle(img, new Point(0, img.Rows - 1), 5, new MCvScalar(255, 255, 255), 5);

                Point[] pp = new Point[] { new Point(0, (img.Rows - 150)), new Point((img.Cols / 2) - 110, (img.Rows / 2) - 50),
                new Point((img.Cols / 2) +150, (img.Rows / 2) - 50),
                new Point(img.Cols -1, img.Rows -1) , new Point(0, img.Rows -1),};

                VectorOfPoint points_road = new VectorOfPoint();
                points_road.Push(pp);

                filter = new Image<Gray, byte>(img.Size);
                CvInvoke.FillPoly(filter, new VectorOfVectorOfPoint(points_road), new MCvScalar(255, 255, 255));


                copy = new Image<Bgr, byte>(img.Size);



                CvInvoke.BitwiseAnd(img, img, copy, filter);

                // //////////////////////////////////////// copy without sky and green fidles /////////////////////


                sub = new Image<Gray, byte>(copy.Size);

                copy._SmoothGaussian(9);




                subtrac.Apply(copy, sub);


                Mat ker = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Ellipse, new System.Drawing.Size(5, 5), new System.Drawing.Point(-1, -1));


                CvInvoke.MorphologyEx(sub, sub, Emgu.CV.CvEnum.MorphOp.Close, ker, new System.Drawing.Point(-1, -1), 2
                    , Emgu.CV.CvEnum.BorderType.Default, new MCvScalar(0, 0, 0));

                sub._ThresholdBinary(new Gray(200), new Gray(255));



                CvInvoke.FindContours(sub, cont, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
                CvInvoke.Line(img, new Point(25, 550), new Point(1200, 550), new MCvScalar(255, 0, 0), 3);


                for (int i = 0; i < cont.Size; i++)
                {
                    Rectangle rect = CvInvoke.BoundingRectangle(cont[i]);
                    if (rect.Width >= 50 && rect.Height >= 50)
                    {
                        img.Draw(rect, new Bgr(0, 255, 0));

                        int cx = rect.Width / 2;
                        int cy = rect.Height / 2;

                        Point center = new Point(rect.X + cx, rect.Y + cy);

                        CvInvoke.Circle(img, center, 3, new MCvScalar(0, 0, 255), 3);
                        if (center.Y <= 555 && center.Y >= 545)
                        {
                            cars++;
                            CvInvoke.Line(img, new Point(25, 550), new Point(1200, 550), new MCvScalar(0, 0, 255), 3);
                            Console.WriteLine($"cars : {cars}");
                        }
                    }


                }

                CvInvoke.Imshow("sub", sub);
                CvInvoke.Imshow("copy", copy);
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
