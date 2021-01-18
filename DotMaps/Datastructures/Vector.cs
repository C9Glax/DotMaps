using System;

namespace DotMaps.Datastructures
{
    public class Vector
    {
        public double x { get; }
        public double y { get; }
        public double z { get; }
        public double length { get; }
        public Vector(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.length = Math.Sqrt(x * x + y * y + z * z);
        }

        public Vector MultiplyWith(Vector secondVector)
        {
            return new Vector(this.x * secondVector.x, this.y * secondVector.y, this.z * secondVector.z);
        }

        public double AngleTo(Vector secondVector)
        {
            return Math.Acos(this.DotProductWith(secondVector) / (this.length * secondVector.length));
        }

        public double DotProductWith(Vector secondVector)
        {
            return this.x * secondVector.x + this.y * secondVector.y + this.z * secondVector.z;
        }

        public Vector CrossProductWith(Vector secondVector)
        {
            return new Vector(this.y * secondVector.z - this.y * secondVector.x,
                this.z * secondVector.x - this.x * secondVector.z,
                this.x * secondVector.y - this.y * secondVector.x);
        }

        public Vector Add(Vector secondVector)
        {
            return new Vector(this.x + secondVector.x,
                this.y + secondVector.y,
                this.z + secondVector.z);
        }

        public Vector Scale(double factor)
        {
            return new Vector(this.x * factor,
                this.y * factor,
                this.z * factor);
        }

        public Vector Subtract(Vector secondVector)
        {
            return new Vector(this.x - secondVector.x,
                this.y - secondVector.y,
                this.z - secondVector.z);
        }
    }
}
