using Emgu.CV;
using System.Drawing;

namespace ConsoleApp2
{
    public class ObjTracking
    {
        public int Id { get; set; }
        public Tracker Tracker { get; set; }
        public Rectangle Rectangle { get; set; }
        public int NoView { get; set; }
        public Origin Origin { get; set; }
    }
    public enum Origin
    {
        Center,
        Top,
        Bottom
    }
}
