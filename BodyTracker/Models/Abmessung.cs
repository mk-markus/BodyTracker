using System;

namespace BodyTracker.Models
{
    public class Abmessung
    {
        public int AbmessungId { get; set; }
        public int PersonId { get; set; }
        public DateTime Messdatum { get; set; }
        public decimal? BrustumfangCm { get; set; }
        public decimal? BauchumfangCm { get; set; }
        public decimal? HueftumfangCm { get; set; }
    }
}
