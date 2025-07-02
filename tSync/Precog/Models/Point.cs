namespace PrecogSync.Models
{
    public class Point
    {
        public Point()
        {
            
        }

        public Point(float X, float Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public Point(double X, double Y)
        {
            this.X = (float)X;
            this.Y = (float)Y;
        }


        public float X { get; set; }

        public float Y { get; set; }
    }
}
