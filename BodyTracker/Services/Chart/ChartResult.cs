using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BodyTracker.Services.Chart
{
    public class ChartResult
    {

        public ISeries[] Series { get; set; }

        public ICartesianAxis[] XAxis { get; set; }

        public ICartesianAxis[] YAxis { get; set; }
    }
}
