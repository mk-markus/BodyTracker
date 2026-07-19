using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BodyTracker.MVVM.Models
{

    public class StepDailyTrendChartModel
    {
        public DateTime CreateTime { get; set; }

        public int SourceType { get; set; }

        public int Count { get; set; }

        public double Distance { get; set; }

        public double Calorie { get; set; }
    }

}
