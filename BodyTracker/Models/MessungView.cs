using System;

namespace BodyTracker.Models
{
    public class MessungView
    {
        public int? MetrikId { get; set; }
        public int? AbmessungId { get; set; }
        public DateTime Messdatum { get; set; }
        public decimal? GewichtKg { get; set; }
        public decimal? Bmi { get; set; }
        public decimal? KoerperfettProzent { get; set; }
        public decimal? MuskelmasseProzent { get; set; }
        public int? Viszeralfett { get; set; }
        public decimal? BrustumfangCm { get; set; }
        public decimal? BauchumfangCm { get; set; }
        public decimal? HueftumfangCm { get; set; }
    }
}
