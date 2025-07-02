namespace PrecogSync.Models
{
    public class PointDistance
    {
        public PointDistance()
        {
        }

        public PointDistance(float X, float Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public PointDistance(double X, double Y)
        {
            this.X = (float)X;
            this.Y = (float)Y;
        }

        public float X { get; set; }
        public float Y { get; set; }
        public float Distance { get; set; }

        public Point Point
        {
            get
            {
                return new Point(X, Y);
            }

            set
            {
                X = (float)value.X;
                Y = (float)value.Y;
            }
        }
    }
}
