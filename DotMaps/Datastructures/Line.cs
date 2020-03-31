using System.Drawing;

namespace DotMaps.Datastructures
{
    public struct Line
    {
        public float startX, startY, finishX, finishY;
        public Color color;
        public float width;
        public Line(float startX, float startY, float finishX, float finishY, Color color, float width)
        {
            this.startX = startX;
            this.startY = startY;
            this.finishX = finishX;
            this.finishY = finishY;
            this.color = color;
            this.width = width;
        }
    }
}
