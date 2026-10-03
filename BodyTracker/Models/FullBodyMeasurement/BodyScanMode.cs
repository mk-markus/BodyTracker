using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BodyTracker.Models.FullBodyMeasurement
{
    public static class BodyScanMode
    {

        public enum ScanMode
        {
                FullBodyScan,
                SegmentalBodyCompisitionScan,
                BodyWeightAndCompositionScan
        }

    }
}
