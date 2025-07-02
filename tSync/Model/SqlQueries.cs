namespace tSync.Model
{
    public class SqlQueries
    {
        public const string GetLocalization = @"
SELECT 
  id,
  username,
  vehicle_type,
  valid_posx as posx,
  valid_posy as posy,
  is_moving,
  is_fall,
  battery_level,
  is_plugged,
  valid_timestampmobile as timestampmobile,
  clientdeviceid,
  pallet_presence,
  extract(epoch from now()) - valid_timestampmobile< 60 AS isrecent
FROM clientdevices
WHERE
    length(username) > 0
AND valid_posx IS NOT NULL
AND valid_posy IS NOT NULL
AND extract(epoch from now()) - valid_timestampmobile< 600";
    }
}
