using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Emgu.CV;

namespace ObjectCounting
{
    public class Tracking
    {
        private readonly ConfigCounter _config;
        public int MaxId { get; set; }
        private readonly List<ObjTracking> _trackers;
        public int Entry { get; set; }
        public int Exit { get; set; }

        public Tracking(ConfigCounter config)
        {
            _config = config;
            _trackers = new List<ObjTracking>();
        }

        public void AddObjects(List<Rectangle> rects, Mat mat)
        {
            var ids = new List<ObjTracking>();

            foreach (var rect in rects)
            {
                var res = Find(rect);
                if (res == null)
                {
                    var origin = _config.EntryArea(rect) ? Origin.Entry : (_config.ExitArea(rect) ? Origin.Exit : Origin.Transit);
                    if (origin != Origin.Transit)
                    {
                        var obj = new ObjTracking { Rectangle = rect, Origin = origin };
                        if (_config.Tracker)
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
                var outRect = Rectangle.Empty;
                _trackers[i].Tracker?.Update(mat, out outRect);
                var isIn = ids.Where(p => p.Id > 0).Select(p => p.Id).Contains(_trackers[i].Id);
                if (!isIn)
                {
                    _trackers[i].NoView++;
                    if (_trackers[i].NoView > 10 || (_config.Tracker && outRect.IsEmpty && _trackers[i].NoView > 5))
                    {
                        if (_trackers[i].Destination == Origin.Exit)
                            Entry++;
                        if (_trackers[i].Destination == Origin.Entry)
                            Exit++;
                        _trackers[i].Tracker?.Dispose();
                        _trackers.RemoveAt(i);
                    }
                    else if(_config.Tracker)
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
                        if (update.Origin == Origin.Entry && _config.ExitArea(update.Rectangle))
                            update.Destination = Origin.Exit;
                        if (update.Origin == Origin.Exit && _config.EntryArea(update.Rectangle))
                            update.Destination = Origin.Entry;
                        update.Rectangle = id.Rectangle;
                        update.NoView = id.NoView;
                    }
                }
            }
        }

        public void UpdateTrackers(Mat mat)
        {
            if (_config.Tracker)
            {
                foreach (var tracker in _trackers)
                {
                    tracker.Tracker.Update(mat, out var outRect);
                    tracker.Rectangle = outRect;
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
