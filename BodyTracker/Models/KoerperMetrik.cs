using System;

namespace BodyTracker.Models
{
    public class KoerperMetrik
    {
        public int MetrikId { get; set; }
        public int PersonId { get; set; }
        public DateTime Messdatum { get; set; }
        public decimal? GewichtKg { get; set; }
        public decimal? Bmi { get; set; }
        public decimal? KoerperfettProzent { get; set; }
        public decimal? MuskelmasseProzent { get; set; }
        public int? Viszeralfett { get; set; }
    }
}
