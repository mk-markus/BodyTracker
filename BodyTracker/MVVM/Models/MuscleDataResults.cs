namespace BodyTracker.MVVM.Models
{
    public class MuscleDataResults
    {
        public string MuscleGroup { get; set; } = string.Empty;
        public double PrimaryVolume { get; set; }
        public double SecondaryVolume { get; set; }
        public double TotalVolume => PrimaryVolume + SecondaryVolume;
        public double PercentageShare { get; set; }
    }
}
