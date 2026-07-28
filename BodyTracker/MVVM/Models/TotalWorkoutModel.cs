using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BodyTracker.MVVM.Models
{
    public class TotalWorkoutModel
    {
        public double PrimaryVolume { get; set; }
        public double SecondaryVolume { get; set; }
        public double TotalVolume => PrimaryVolume + SecondaryVolume;
        public int    TotalExercises { get; set; }
    }
}
