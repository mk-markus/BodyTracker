using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BodyTracker.MVVM.Models
{
    public class GymWorkoutEntryModel
    {
        public DateTime ExcerciseDate { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public double Weight { get; set; }
        public double Reps { get; set; }
        public int SetIndex { get; set; }
    }
}
