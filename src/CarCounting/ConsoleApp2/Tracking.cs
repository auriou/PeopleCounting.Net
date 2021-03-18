using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ConsoleApp2
{
    public class Tracking
    {
        public int MaxId { get; set; }
        private List<ObjTracking> _trackers;
        private readonly Func<Rectangle, bool> enterFunc;
        private readonly Func<Rectangle, bool> outFunc;
        private readonly bool noTrack;

        public Tracking(Func<Rectangle, bool> enterFunc, Func<Rectangle, bool> outFunc, bool noTrack = false)
        {
            _trackers = new List<ObjTracking>();
            this.enterFunc = enterFunc;
            this.outFunc = outFunc;
            this.noTrack = noTrack;
        }

        public void AddObjects(List<Rectangle> rects, Mat mat)
        {
            if (!noTrack && mat.NumberOfChannels != 3)
            {
                CvInvoke.CvtColor(mat, mat, Emgu.CV.CvEnum.ColorConversion.Bgra2Bgr);
            }
            //for (int i = _trackers.Count - 1; i >= 0; i--)
            //{
            //    var exist = Find(_trackers[i].Rectangle, rects);
            //    if(!exist) _trackers.RemoveAt(i);
            //    else
            //    {
            //        _trackers[i].Tracker.Update(mat, out var outRect);
            //        //if (outRect.IsEmpty) _trackers.RemoveAt(i);
            //    }
            //}

            //if(rects.Count != _trackers.Count)
            //{
            //    if (rects.Count == 0) _trackers.RemoveAll(p => p != null);
            //}

            var ids = new List<ObjTracking>();

            foreach (var rect in rects)
            {
                var res = Find(rect);
                if (res == null)
                {
                    var origin = enterFunc(rect) ? Origin.Top : (outFunc(rect) ? Origin.Bottom : Origin.Center);
                    if (origin != Origin.Center)
                    {
                        var obj = new ObjTracking { Rectangle = rect, Origin = origin };
                        if (!noTrack)
                        {
                            var tracker = new TrackerCSRT();
                            tracker.Init(mat, rect);
                            obj.Tracker = tracker;
                        }
                        ids.Add(obj);
                    }
                }
                else
                {
                    res.Rectangle = rect;
                    res.NoView = 0;
                    ids.Add(res);
                }
            }

            for (int i = _trackers.Count - 1; i >= 0; i--)
            {
                Rectangle outRect = Rectangle.Empty;
                _trackers[i].Tracker?.Update(mat, out outRect);
                var isIn = ids.Where(p => p.Id > 0).Select(p => p.Id).Contains(_trackers[i].Id);
                if (!isIn)
                {
                    _trackers[i].NoView++;
                    if (_trackers[i].NoView > 10)
                    {
                        _trackers[i].Tracker?.Dispose();
                        _trackers.RemoveAt(i);
                    }
                    else if (outRect.IsEmpty && _trackers[i].NoView > 5)
                    {
                        _trackers[i].Tracker?.Dispose();
                        _trackers.RemoveAt(i);
                    }
                    else
                    {
                        _trackers[i].Rectangle = outRect;
                    }
                }
            }

            foreach (var id in ids)
            {
                if (id.Id == 0)
                {
                    id.Id = ++MaxId;
                    _trackers.Add(id);
                }
                else
                {
                    var update = _trackers.FirstOrDefault(p => p.Id == id.Id);
                    if (update != null)
                    {
                        if ((id.Origin == Origin.Top && outFunc(id.Rectangle)) ||
                            (id.Origin == Origin.Bottom && enterFunc(id.Rectangle)))
                        {
                            //_trackers.Remove(update);
                        }

                        update.Rectangle = id.Rectangle;
                        update.NoView = id.NoView;
                    }
                }
            }
        }

        public int Count()
        {
            return _trackers.Count;
        }

        public List<Rectangle> GetRectangles()
        {
            return _trackers.Select(p => p.Rectangle).ToList();
        }

        public List<ObjTracking> GetTrackings()
        {
            return _trackers;
        }

        private Point GetCenter(Rectangle rect)
        {
            return new Point(rect.Left + rect.Width / 2,
                     rect.Top + rect.Height / 2);
        }

        private double GetDistance(Point p1, Point p2)
        {
            double dX = p1.X - p2.X;
            double dY = p1.Y - p2.Y;
            double multi = dX * dX + dY * dY;
            double rad = Math.Round(Math.Sqrt(multi), 3, MidpointRounding.AwayFromZero);
            return rad;
        }

        public ObjTracking Find(Rectangle rect)
        {
            var obj = new List<ObjTracking>();
            foreach (var track in _trackers)
            {
                var rectIntersect = Rectangle.Intersect(track.Rectangle, rect);
                if (!rectIntersect.IsEmpty)
                {
                    obj.Add(track);
                }
            }
            if (obj.Count < 1) return null;
            else if (obj.Count == 1) return obj.FirstOrDefault();
            else
            {
                var pointRect = GetCenter(rect);
                double distance = 9999999999;
                ObjTracking returnObj = null;
                foreach (var ob in obj)
                {
                    var testCenter = GetCenter(ob.Rectangle);
                    var distanceTemp = GetDistance(pointRect, testCenter);
                    if (distanceTemp < distance)
                    {
                        distance = distanceTemp;
                        returnObj = ob;
                    }
                }
                return returnObj;
            }
        }

        public bool Find(Rectangle rect, List<Rectangle> rects)
        {
            foreach (var rectin in rects)
            {
                var rectIntersect = Rectangle.Intersect(rectin, rect);
                if (!rectIntersect.IsEmpty)
                {
                    return true;
                }
            }
            return false;
        }

        public bool Isin(Rectangle rect1, Rectangle rect2)
        {
            if (rect1.IntersectsWith(rect2))
            {
                Rectangle overlap = Rectangle.Intersect(rect1, rect2);

                if (overlap.IsEmpty)
                    return overlap.Width * overlap.Height > 0;
            }

            return true;
        }

    }
}
