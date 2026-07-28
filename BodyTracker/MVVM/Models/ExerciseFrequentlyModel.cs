using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BodyTracker.MVVM.Models
{
    public class ExerciseFrequencyModel
    {
        public string ExerciseName { get; set; } = string.Empty;

        public int Count { get; set; }

        public double Percentage { get; set; }
    }

}
