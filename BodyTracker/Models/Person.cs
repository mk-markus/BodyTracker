using System;

namespace BodyTracker.Models
{
    public class Person
    {
        public int Id { get; set; }
        public string Vorname { get; set; } = string.Empty;
        public string Nachname { get; set; } = string.Empty;
        public DateTime? Geburtsdatum { get; set; }
        public override string ToString() => $"{Vorname} {Nachname}";
    }
}
