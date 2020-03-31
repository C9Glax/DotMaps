
namespace DotMaps.Datastructures
{
    public class Line
    {
        public float startLat, startLon, finishLat, finishLon;
        public System.Drawing.Color color;
        public float width;
        public Line(float startLat, float startLon, float finishLat, float finishLon, System.Drawing.Color color, float width)
        {
            this.startLat = startLat;
            this.startLon = startLon;
            this.finishLat = finishLat;
            this.finishLon = finishLon;
            this.color = color;
            this.width = width;
        }
    }
}
