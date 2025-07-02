using GeoAPI.CoordinateSystems.Transformations;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using tSync.ThingPark.Models;

public class GpsToSectorConverter
{
    private readonly GpsItem _topLeft;     // Top-left corner of the sector
    private readonly GpsItem _bottomRight; // Bottom-right corner of the sector
    private readonly ICoordinateTransformation _gpsToUtm;
    private readonly int _utmZone;

    public GpsToSectorConverter(GpsItem topLeft, GpsItem bottomRight)
    {
        _topLeft = topLeft;
        _bottomRight = bottomRight;

        // Determine UTM zone based on the center of the sector
        double centerLon = (topLeft.Longitude + bottomRight.Longitude) / 2;
        _utmZone = (int)((centerLon + 180) / 6) + 1;

        var csFactory = new CoordinateSystemFactory();
        var ctFactory = new CoordinateTransformationFactory();

        var wgs84 = GeographicCoordinateSystem.WGS84;
        var utmProjection = ProjectedCoordinateSystem.WGS84_UTM(_utmZone, centerLon > 0);

        _gpsToUtm = ctFactory.CreateFromCoordinateSystems(wgs84, utmProjection);
    }

    private (double X, double Y) ConvertGpsToUtm(double latitude, double longitude)
    {
        double[] point = _gpsToUtm.MathTransform.Transform(new[] { longitude , latitude });
        return (point[0], point[1]);
    }

    public (float X, float Y) ConvertGpsToSector(double latitude, double longitude)
    {
        var (utmX, utmY) = ConvertGpsToUtm(latitude, longitude);

        var (topLeftUtmX, topLeftUtmY) = ConvertGpsToUtm(_topLeft.Latitude, _topLeft.Longitude);

        var (bottomRightUtmX, bottomRightUtmY) = ConvertGpsToUtm(_bottomRight.Latitude, _bottomRight.Longitude);

        // Calculate the relative position in UTM coordinates
        var dX = utmX - topLeftUtmX;
        var dY = topLeftUtmY - utmY; // Note the reversal due to Y increasing downwards in sector

        // Calculate scaling factors
        var scaleX = (_bottomRight.X - _topLeft.X) / (bottomRightUtmX - topLeftUtmX);
        var scaleY = (_bottomRight.Y - _topLeft.Y) / (topLeftUtmY - bottomRightUtmY);

        // Convert to sector coordinates
        var x = _topLeft.X + (float)(dX * scaleX);
        var y = _topLeft.Y + (float)(dY * scaleY);

        return (x, y);
    }
}