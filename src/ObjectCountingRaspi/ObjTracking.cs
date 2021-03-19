using System.Drawing;
using Emgu.CV;

namespace ObjectCounting
{
    public class ObjTracking
    {
        public int Id { get; set; }
        public Tracker Tracker { get; set; }
        public Rectangle Rectangle { get; set; }
        public int NoView { get; set; }
        public Origin Origin { get; set; }
        public Origin Destination { get; set; }
    }
    public enum Origin
    {
        Transit,
        Entry,
        Exit
    }
}
