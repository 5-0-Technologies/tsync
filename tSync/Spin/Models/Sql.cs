namespace tSync.Spin.Models
{
    public static class Sql
    {
        public const string GetLocalization = @"SELECT 
                id AS Id,
                username AS Username,
                vehicle_type AS VehicleType,
                valid_sector AS SectorId,
                valid_posx AS X,
                valid_posy AS Y,
                is_moving AS IsMoving,
                is_fall AS IsFall,
                battery_level AS BatteryLevel,
                is_plugged AS IsPlugged,
                valid_timestampmobile As TimestampMobile,
                clientdeviceid AS ClientDeviceId,
                pallet_presence AS PalletPresence
            FROM clientdevices
            WHERE
                length(username) > 0
                AND valid_posx IS NOT NULL
                AND valid_posy IS NOT NULL
                AND NOT valid_posx::text = 'NaN'
                AND NOT valid_posy::text = 'NaN'
                AND NOT battery_level::text = 'NaN'
                AND @ts - valid_timestampmobile <= 60";
    }

}