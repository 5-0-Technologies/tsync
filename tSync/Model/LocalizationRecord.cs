namespace tSync.Model
{
    public class LocalizationRecord
    {
        public string UserName { get; set; }
        public long TimeStampMobile { get; set; }
        public float? X { get; set; }
        public float? Y { get; set; }
        public decimal? Battery { get; set; }
        public bool IsMoving { get; set; }
        public int? SectorId { get; set; }
    }
}
