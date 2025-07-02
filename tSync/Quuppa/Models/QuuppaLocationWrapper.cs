using SDK.Contracts.Data;

namespace tSync.Quuppa.Models
{
    public class QuuppaLocationWrapper
    {
        public QuuppaData QuuppaData { get; set; }

        public DeviceLocationContract DeviceLocationContract { get; set; }
    }
}
