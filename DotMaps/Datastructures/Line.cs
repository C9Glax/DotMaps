using System.Drawing;

namespace DotMaps.Datastructures
{
    public struct Line
    {
        public _2DNode from, to;
        public Pen pen;
        public Line(Pen pen, _2DNode from, _2DNode to)
        {
            this.from = from;
            this.to = to;
            this.pen = pen;
        }

        public override string ToString()
        {
            return string.Format("Line {0} to {1}", from, to);
        }
    }
}
