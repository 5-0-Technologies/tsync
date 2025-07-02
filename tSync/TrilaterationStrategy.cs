using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tSync
{
    using PrecogSync.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    namespace Core.Localization
    {
        public class TrilaterationItem
        {
            public Point point { get; set; }
            public double distance { get; set; }
        }

        public class TrilaterationStrategy 
        {
            public Point Localize(PointDistance[] pointDistances)
            {
                switch (pointDistances.Length)
                {
                    case 0: return null;
                    case 1: return pointDistances[0].Point;
                    case 2: return WhereIsBetweenTwo(pointDistances[0], pointDistances[1]);
                    case 3: return ThreeClosest(pointDistances[0], pointDistances[1], pointDistances[2]);
                }
                return null;
            }

            private static Point Triangulation(Point point0, Point point1, Point point2)
            {
                return new Point() { X = (point0.X + point1.X + point2.X) / 3, Y = (point0.Y + point1.Y + point2.Y) / 3 };
            }

            private static Point WhereIsBetweenTwo(PointDistance device0, PointDistance device1)
            {
                if (SMath.VectorLength(device0.Point, device1.Point) < (device0.Distance + device1.Distance))
                { //1,2,3,4,6,7
                    if (SMath.VectorLength(device0.Point, device1.Point) + device0.Distance - device1.Distance >= 0)
                    { //3,4,6,7
                        if (SMath.VectorLength(device0.Point, device1.Point) < device1.Distance)
                        { //3,4
                          //Console.WriteLine("3,4");
                            var substract = (device0.Distance + SMath.VectorLength(device0.Point, device1.Point) - device1.Distance) / 2;
                            var newDevice0 = new PointDistance() { Point = new Point() { X = device0.Point.X + substract * (float)Math.Cos(SMath.DegreeToRadian(SMath.SpecifyAngle(device0.Point, device1.Point))), Y = device0.Point.Y + substract * (float)Math.Sin(SMath.DegreeToRadian(SMath.SpecifyAngle(device0.Point, device1.Point))) }, Distance = device0.Distance };
                            var newDevice1 = new PointDistance() { Point = new Point() { X = device1.Point.X + substract * (float)Math.Cos(SMath.DegreeToRadian(SMath.SpecifyAngle(device1.Point, device0.Point))), Y = device1.Point.Y + substract * (float)Math.Sin(SMath.DegreeToRadian(SMath.SpecifyAngle(device1.Point, device0.Point))) }, Distance = device1.Distance };
                            var bT1 = new Point() { X = newDevice0.Point.X + newDevice0.Distance * (float)Math.Cos(SMath.DegreeToRadian(180 + SMath.SpecifyAngle(newDevice0.Point, newDevice1.Point))), Y = newDevice0.Point.Y + newDevice0.Distance * (float)Math.Sin(SMath.DegreeToRadian(180 + SMath.SpecifyAngle(newDevice0.Point, newDevice1.Point))) };
                            var bT2 = new Point() { X = newDevice1.Point.X + newDevice1.Distance * (float)Math.Cos(SMath.DegreeToRadian(SMath.SpecifyAngle(newDevice1.Point, newDevice0.Point))), Y = newDevice1.Point.Y + newDevice1.Distance * (float)Math.Sin(SMath.DegreeToRadian(SMath.SpecifyAngle(newDevice1.Point, newDevice0.Point))) };
                            return new Point() { X = SMath.MiddleOfVector(bT1, bT2).X, Y = SMath.MiddleOfVector(bT1, bT2).Y }; // calcPos by mohla byť aj bt1 !!!!DOKONCIT!!!!!!!
                        }
                        else
                        {//6,7
                         //Console.WriteLine("6,7");
                            var substract = (device0.Distance + device1.Distance - SMath.VectorLength(device0.Point, device1.Point)) / 2;
                            return new Point()
                            {
                                X = device0.Point.X + (device0.Distance - substract) * ((float)Math.Cos(SMath.DegreeToRadian(SMath.SpecifyAngle(device0.Point, device1.Point)))),
                                Y = device0.Point.Y + (device0.Distance - substract) * ((float)Math.Sin(SMath.DegreeToRadian(SMath.SpecifyAngle(device0.Point, device1.Point))))
                            };
                        }
                    }
                    else
                    { //1,2
                      //Console.WriteLine("1,2");
                        if ((device0.Point.X == device1.Point.X) && (device0.Point.Y == device1.Point.Y))
                        {

                        }
                        else
                        {
                            var substract = -(device0.Distance + SMath.VectorLength(device0.Point, device1.Point) - device1.Distance) * (device0.Distance / (device0.Distance + device1.Distance));
                            return new Point()
                            {
                                X = device0.Point.X + (substract + device0.Distance) * ((float)Math.Cos(SMath.DegreeToRadian(180 + SMath.SpecifyAngle(device0.Point, device1.Point)))),
                                Y = device0.Point.Y + (substract + device0.Distance) * ((float)Math.Sin(SMath.DegreeToRadian(180 + SMath.SpecifyAngle(device0.Point, device1.Point))))
                            };
                        }
                    }
                }
                else
                { //5,8
                  //Console.WriteLine("5,8");
                    var substract = (SMath.VectorLength(device0.Point, device1.Point) - device0.Distance - device1.Distance)
                        * (device0.Distance / (device0.Distance + device1.Distance));

                    return new Point()
                    {
                        X = device0.Point.X + (substract + device0.Distance) * (float)Math.Cos(SMath.DegreeToRadian(SMath.SpecifyAngle(device0.Point, device1.Point))),
                        Y = device0.Point.Y + (substract + device0.Distance) * (float)Math.Sin(SMath.DegreeToRadian(SMath.SpecifyAngle(device0.Point, device1.Point)))
                    };
                }
                return null;
            }

            private static Point ThreeClosest(PointDistance device0, PointDistance device1, PointDistance device2)
            {
                Point newPoint01, newPoint10, newPoint02, newPoint20, newPoint12, newPoint21;
                if (((SMath.VectorLength(device0.Point, device1.Point) > (device0.Distance + device1.Distance)) || (SMath.VectorLength(device0.Point, device1.Point) + device0.Distance - device1.Distance < 0)))
                {
                    //Console.WriteLine("A1");
                    newPoint01 = WhereIsBetweenTwo(device0, device1);
                    newPoint10 = null;
                }
                else
                {
                    //Console.WriteLine("B1");
                    var a = 0.25 *
                        Math.Sqrt((SMath.VectorLength(device0.Point, device1.Point) + device0.Distance + device1.Distance) *
                        (SMath.VectorLength(device0.Point, device1.Point) + device0.Distance - device1.Distance) *
                        (SMath.VectorLength(device0.Point, device1.Point) - device0.Distance + device1.Distance) *
                        (-SMath.VectorLength(device0.Point, device1.Point) + device0.Distance + device1.Distance));
                    newPoint01 = new Point()
                    {
                        X = (device0.Point.X + device1.Point.X) / 2 + (float)((Math.Pow(device0.Distance, 2) - Math.Pow(device1.Distance, 2)) / (2 * Math.Pow(SMath.VectorLength(device0.Point, device1.Point), 2)) * (device1.Point.X - device0.Point.X) + 2 * a * (device1.Point.Y - device0.Point.Y) / Math.Pow(SMath.VectorLength(device0.Point, device1.Point), 2)),
                        Y = (device0.Point.Y + device1.Point.Y) / 2 + (float)((Math.Pow(device0.Distance, 2) - Math.Pow(device1.Distance, 2)) / (2 * Math.Pow(SMath.VectorLength(device0.Point, device1.Point), 2)) * (device1.Point.Y - device0.Point.Y) - (2 * a * (device1.Point.X - device0.Point.X) / Math.Pow(SMath.VectorLength(device0.Point, device1.Point), 2)))
                    };
                    newPoint10 = new Point()
                    {
                        X = (device0.Point.X + device1.Point.X) / 2 + (float)((Math.Pow(device0.Distance, 2) - Math.Pow(device1.Distance, 2)) / (2 * Math.Pow(SMath.VectorLength(device0.Point, device1.Point), 2)) * (device1.Point.X - device0.Point.X) - 2 * a * (device1.Point.Y - device0.Point.Y) / Math.Pow(SMath.VectorLength(device0.Point, device1.Point), 2)),
                        Y = (device0.Point.Y + device1.Point.Y) / 2 + (float)((Math.Pow(device0.Distance, 2) - Math.Pow(device1.Distance, 2)) / (2 * Math.Pow(SMath.VectorLength(device0.Point, device1.Point), 2)) * (device1.Point.Y - device0.Point.Y) + (2 * a * (device1.Point.X - device0.Point.X) / Math.Pow(SMath.VectorLength(device0.Point, device1.Point), 2)))
                    };
                }

                if (((SMath.VectorLength(device0.Point, device2.Point) > (device0.Distance + device2.Distance)) || (SMath.VectorLength(device0.Point, device2.Point) + device0.Distance - device2.Distance < 0)))
                {
                    //Console.WriteLine("A2");
                    newPoint02 = WhereIsBetweenTwo(device0, device2);
                    newPoint20 = null;
                }
                else
                {
                    //Console.WriteLine("B2");
                    var a = 0.25 * Math.Sqrt((SMath.VectorLength(device0.Point, device2.Point) + device0.Distance + device2.Distance)
                        * (SMath.VectorLength(device0.Point, device2.Point) + device0.Distance - device2.Distance)
                        * (SMath.VectorLength(device0.Point, device2.Point) - device0.Distance + device2.Distance)
                        * (-SMath.VectorLength(device0.Point, device2.Point) + device0.Distance + device2.Distance));
                    newPoint02 = new Point() { X = (device0.Point.X + device2.Point.X) / 2 + (float)((Math.Pow(device0.Distance, 2) - Math.Pow(device2.Distance, 2)) / (2 * Math.Pow(SMath.VectorLength(device0.Point, device2.Point), 2)) * (device2.Point.X - device0.Point.X) + 2 * a * (device2.Point.Y - device0.Point.Y) / Math.Pow(SMath.VectorLength(device0.Point, device2.Point), 2)), Y = (device0.Point.Y + device2.Point.Y) / 2 + (float)((Math.Pow(device0.Distance, 2) - Math.Pow(device2.Distance, 2)) / (2 * Math.Pow(SMath.VectorLength(device0.Point, device2.Point), 2)) * (device2.Point.Y - device0.Point.Y) - (2 * a * (device2.Point.X - device0.Point.X) / Math.Pow(SMath.VectorLength(device0.Point, device2.Point), 2))) };
                    newPoint20 = new Point() { X = (device0.Point.X + device2.Point.X) / 2 + (float)((Math.Pow(device0.Distance, 2) - Math.Pow(device2.Distance, 2)) / (2 * Math.Pow(SMath.VectorLength(device0.Point, device2.Point), 2)) * (device2.Point.X - device0.Point.X) - 2 * a * (device2.Point.Y - device0.Point.Y) / Math.Pow(SMath.VectorLength(device0.Point, device2.Point), 2)), Y = (device0.Point.Y + device2.Point.Y) / 2 + (float)((Math.Pow(device0.Distance, 2) - Math.Pow(device2.Distance, 2)) / (2 * Math.Pow(SMath.VectorLength(device0.Point, device2.Point), 2)) * (device2.Point.Y - device0.Point.Y) + (2 * a * (device2.Point.X - device0.Point.X) / Math.Pow(SMath.VectorLength(device0.Point, device2.Point), 2))) };
                }

                if (((SMath.VectorLength(device1.Point, device2.Point) > (device1.Distance + device2.Distance)) || (SMath.VectorLength(device1.Point, device2.Point) + device1.Distance - device2.Distance < 0)))
                {
                    //Console.WriteLine("A3");
                    newPoint12 = WhereIsBetweenTwo(device1, device2);
                    newPoint21 = null;
                }
                else
                {
                    //Console.WriteLine("B3");
                    var a = 0.25 * Math.Sqrt((SMath.VectorLength(device1.Point, device2.Point) + device1.Distance + device2.Distance)
                        * (SMath.VectorLength(device1.Point, device2.Point) + device1.Distance - device2.Distance)
                        * (SMath.VectorLength(device1.Point, device2.Point) - device1.Distance + device2.Distance)
                        * (-SMath.VectorLength(device1.Point, device2.Point) + device1.Distance + device2.Distance));
                    newPoint12 = new Point() { X = (device1.Point.X + device2.Point.X) / 2 + (float)((Math.Pow(device1.Distance, 2) - Math.Pow(device2.Distance, 2)) / (2 * Math.Pow(SMath.VectorLength(device1.Point, device2.Point), 2)) * (device2.Point.X - device1.Point.X) + 2 * a * (device2.Point.Y - device1.Point.Y) / Math.Pow(SMath.VectorLength(device1.Point, device2.Point), 2)), Y = (device1.Point.Y + device2.Point.Y) / 2 + (float)((Math.Pow(device1.Distance, 2) - Math.Pow(device2.Distance, 2)) / (2 * Math.Pow(SMath.VectorLength(device1.Point, device2.Point), 2)) * (device2.Point.Y - device1.Point.Y) - (2 * a * (device2.Point.X - device1.Point.X) / Math.Pow(SMath.VectorLength(device1.Point, device2.Point), 2))) };
                    newPoint21 = new Point() { X = (device1.Point.X + device2.Point.X) / 2 + (float)((Math.Pow(device1.Distance, 2) - Math.Pow(device2.Distance, 2)) / (2 * Math.Pow(SMath.VectorLength(device1.Point, device2.Point), 2)) * (device2.Point.X - device1.Point.X) - 2 * a * (device2.Point.Y - device1.Point.Y) / Math.Pow(SMath.VectorLength(device1.Point, device2.Point), 2)), Y = (device1.Point.Y + device2.Point.Y) / 2 + (float)((Math.Pow(device1.Distance, 2) - Math.Pow(device2.Distance, 2)) / (2 * Math.Pow(SMath.VectorLength(device1.Point, device2.Point), 2)) * (device2.Point.Y - device1.Point.Y) + (2 * a * (device2.Point.X - device1.Point.X) / Math.Pow(SMath.VectorLength(device1.Point, device2.Point), 2))) };
                }

                //preparing for the arrays, which will be chosing smallest triangle of intersects (3 result points)
                List<TrilaterationItem> trilaterationList = new List<TrilaterationItem>();
                if (newPoint01 != null && newPoint02 != null && newPoint12 != null)
                {
                    trilaterationList.Add(new TrilaterationItem()
                    {
                        point = Triangulation(newPoint01, newPoint02, newPoint12),
                        distance = SMath.SumOfDistances(newPoint01, newPoint02, newPoint12)
                    });
                }
                if (newPoint01 != null && newPoint02 != null && newPoint21 != null)
                {
                    trilaterationList.Add(new TrilaterationItem()
                    {
                        point = Triangulation(newPoint01, newPoint02, newPoint21),
                        distance = SMath.SumOfDistances(newPoint01, newPoint02, newPoint21)
                    });
                }
                if (newPoint01 != null && newPoint20 != null && newPoint12 != null)
                {
                    trilaterationList.Add(new TrilaterationItem()
                    {
                        point = Triangulation(newPoint01, newPoint20, newPoint12),
                        distance = SMath.SumOfDistances(newPoint01, newPoint20, newPoint12)
                    });
                }
                if (newPoint01 != null && newPoint20 != null && newPoint21 != null)
                {
                    trilaterationList.Add(new TrilaterationItem()
                    {
                        point = Triangulation(newPoint01, newPoint20, newPoint21),
                        distance = SMath.SumOfDistances(newPoint01, newPoint20, newPoint21)
                    });
                }
                if (newPoint10 != null && newPoint02 != null && newPoint12 != null)
                {
                    trilaterationList.Add(new TrilaterationItem()
                    {
                        point = Triangulation(newPoint10, newPoint02, newPoint12),
                        distance = SMath.SumOfDistances(newPoint10, newPoint02, newPoint12)
                    });
                }
                if (newPoint10 != null && newPoint02 != null && newPoint21 != null)
                {
                    trilaterationList.Add(new TrilaterationItem()
                    {
                        point = Triangulation(newPoint10, newPoint02, newPoint21),
                        distance = SMath.SumOfDistances(newPoint10, newPoint02, newPoint21)
                    });
                }
                if (newPoint10 != null && newPoint20 != null && newPoint12 != null)
                {
                    trilaterationList.Add(new TrilaterationItem()
                    {
                        point = Triangulation(newPoint10, newPoint20, newPoint12),
                        distance = SMath.SumOfDistances(newPoint10, newPoint20, newPoint12)
                    });
                }
                if (newPoint10 != null && newPoint02 != null && newPoint21 != null)
                {
                    trilaterationList.Add(new TrilaterationItem()
                    {
                        point = Triangulation(newPoint10, newPoint02, newPoint21),
                        distance = SMath.SumOfDistances(newPoint10, newPoint02, newPoint21)
                    });
                }
                if (newPoint10 != null && newPoint20 != null && newPoint21 != null)
                {
                    trilaterationList.Add(new TrilaterationItem()
                    {
                        point = Triangulation(newPoint10, newPoint20, newPoint21),
                        distance = SMath.SumOfDistances(newPoint10, newPoint20, newPoint21)
                    });
                }

                if (trilaterationList.Count != 0)
                {
                    return trilaterationList.OrderBy(c => c.distance).FirstOrDefault().point;
                }
                else
                {
                    return new Point();
                }
            }
        }
    }

}
