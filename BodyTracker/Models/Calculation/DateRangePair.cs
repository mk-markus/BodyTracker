using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BodyTracker.Models.Calculation
{
    /// <summary>
    /// Represents a pair of date ranges consisting of a current period and a previous comparison period, 
    /// used for analytical comparisons and historical evaluations.
    /// </summary>
    public class DateRangePair
    {
        /// <summary>
        /// Gets or sets the start date of the current period.
        /// </summary>
        public DateTime CurStartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date of the current period.
        /// </summary>
        public DateTime CurEndDate { get; set; }

        /// <summary>
        /// Gets or sets the start date of the previous comparison period.
        /// </summary>
        public DateTime PrevStartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date of the previous comparison period.
        /// </summary>
        public DateTime PrevEndDate { get; set; }
    }
}