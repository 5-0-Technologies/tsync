using PrecogSync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tSync
{
    public static class SMath
    {
        public static float[] PossitionInCoordinates(int x, int y, float tileWidth, float tileHeight)
        {
            float posX = x * tileWidth;
            float posY = y * tileHeight;

            return new float[] { posX, posY };
        }

        public static int[] PossitionInGrid(float x, float y, float tileWidth, float tileHeight, float gridWidth, float gridHeight)
        {
            int gridX = (int)Math.Floor(x / tileWidth);
            int gridY = (int)Math.Floor(y / tileHeight);

            if (gridX >= gridWidth || gridY >= gridHeight)
            {
                return null;
            }

            return new int[] { gridX, gridY };
        }

        public static PointDistance CalculatePoint(PointDistance a, PointDistance b, float distance)
        {

            // a. calculate the vector from o to g:
            double vectorX = b.X - a.X;
            double vectorY = b.Y - a.Y;

            if (vectorX == 0 && vectorY == 0)
            {
                return a;
            }

            // b. calculate the proportion of hypotenuse
            double factor = distance / Math.Sqrt(vectorX * vectorX + vectorY * vectorY);

            // c. factor the lengths
            vectorX *= factor;
            vectorY *= factor;

            // d. calculate and Draw the new vector,
            return new PointDistance(a.X + vectorX, a.Y + vectorY);
        }

        public static bool IsWithin(this float value, float minimum, float maximum)
        {
            return value >= minimum && value <= maximum;
        }
        private static decimal PointDistance(this Point p1, Point p2)
        {
            return Convert.ToDecimal(Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.X - p2.X, 2)));
        }

        public static Point ClosestPoint(this IEnumerable<Point> points, Point referencePoint)
        {
            return points.OrderBy(X => X.PointDistance(referencePoint)).FirstOrDefault();
        }

        public static float VectorLength(Point p1, Point p2)
        {
            float diffX = p2.X - p1.X;
            float diffY = p2.Y - p1.Y;
            return (float)Math.Sqrt((diffX * diffX) + (diffY * diffY));
        }

        public static Point MiddleOfVector(Point point0, Point point1)
        {
            return new Point() { X = (point0.X + point1.X) / 2, Y = (point0.Y + point1.Y) / 2 };
        }

        public static Point Differences(Point point0, Point point1)
        {
            return new Point() { X = point1.X - point0.X, Y = point1.Y - point0.Y };
        }

        public static float SizeOfAngle(Point point0, Point point1)
        {
            return RadianToDegree((float)Math.Atan(Differences(point0, point1).Y / Differences(point0, point1).X)); // 0 in denominator is caught inside SpecifyAngle function
        }

        public static float SpecifyAngle(Point point0, Point point1)
        {
            var diff = Differences(point0, point1);
            //if X coordinate for both beacons is the same
            if (diff.X == 0)
            {
                // if Y coordinates are same too, then there is a problem because either the beacon is duplicated, or there are two beacons in a same spot
                if (diff.Y == 0)
                {

                }
                // b2 is right above b1 in the cartesian system
                if (diff.Y > 0)
                {
                    return 90;
                }
                // b2 is under b1 in the cartesian system
                if (diff.Y < 0)
                {
                    return 270;
                }
            }
            //if X coordinate for b2 is bigger than b1
            if (diff.X > 0)
            {
                //beacons are on the line with same X coordinate
                if (diff.Y == 0)
                {
                    return 0;
                }
                //X2>X1 and Y2>Y1 easiest and natural solution
                if (diff.Y > 0)
                {
                    return SizeOfAngle(point0, point1);
                }
                //
                if (diff.Y < 0)
                {
                    return (360 + SizeOfAngle(point0, point1));
                }
            }
            //if X coordinate for b2 is lower than b1
            if (diff.X < 0)
            {
                if (diff.Y == 0)
                {
                    return 180;
                }//X1>X2 Y1<Y2
                if (diff.Y > 0)
                {
                    return (180 + SizeOfAngle(point0, point1));
                }//X1,Y1 > X2,Y2
                if (diff.Y < 0)
                {
                    return (180 + SizeOfAngle(point0, point1));
                }
            }
            throw new System.Exception();
        }

        public static float DegreeToRadian(float angle)
        {
            return (float)(Math.PI * angle / 180.0);
        }

        public static float RadianToDegree(float angle)
        {
            return (float)(angle * (180.0 / Math.PI));
        }

        public static double SumOfDistances(Point point0, Point point1, Point point2)
        {
            return VectorLength(point0, point1) + VectorLength(point0, point2) + VectorLength(point1, point2);
        }

        //public double CalculateDistance(double rssi) 
        //{
        //    //formula adapted from David Young's Radius Networks Android iBeacon Code
        //    if (rssi == 0) {
        //        return -1.0; // if we cannot determine accuracy, return -1.
        //    }


        //    double txPower = -70;
        //        double ratio = rssi * 1.0 / txPower;
        //    if (ratio< 1.0) {
        //        return Math.Pow(ratio,10);
        //    }
        //    else {
        //        double accuracy = (0.89976) * pow(ratio, 7.7095) + 0.111;
        //        return accuracy;
        //    }
        //}
    }
}
