using BodyTracker.Models.Calculation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BodyTracker.Services.Calculation
{
    public static class TimeFrameCalculator
    {
        /// <summary>
        /// Calculates comparative date ranges based on a target date: the immediate preceding month as the current period, 
        /// and a previous period shifted further backward by the specified number of months.
        /// </summary>
        /// <param name="targetDate">The reference date used as the anchor for the period calculations.</param>
        /// <param name="monthsBackwards">The number of months to shift backwards from the target date to define the previous comparison period.</param>
        /// <returns>
        /// A <see cref="DateRangePair"/> containing the calculated start and end dates for both the current and previous periods.
        /// </returns>
        public static DateRangePair GetComparisonPeriod(DateTime targetDate, int monthsBackwards = 0)
        {
            // Base: The first day of the target date's month
            var firstOfTargetMonth = new DateTime(targetDate.Year, targetDate.Month, 1);

            // 1. Current Period (e.g., the month of the target date)
            var curStartDate = firstOfTargetMonth.AddMonths(-1);
            var curEndDate = firstOfTargetMonth.AddDays(-1);

            // 2. Previous Period (shifted backward by monthsBackwards)
            // Example: If targetDate is September and monthsBackwards = 1, prev starts in August.
            // If monthsBackwards = 12, it represents the same month in the previous year (YoY).
            var prevStartDate = firstOfTargetMonth.AddMonths(-monthsBackwards);
            var prevEndDate = prevStartDate.AddDays(-1).AddMonths(1);

            return new DateRangePair
            {
                CurStartDate = curStartDate,
                CurEndDate = curEndDate,
                PrevStartDate = prevStartDate,
                PrevEndDate = prevEndDate
            };
        }

        /// <summary>
        /// Calculates comparative date ranges relative to the current day (<see cref="DateTime.Today"/>) 
        /// using the immediate preceding month and a specified backward offset.
        /// </summary>
        /// <param name="monthsBackwards">The number of months to shift backwards to define the previous comparison period (defaults to 1).</param>
        /// <returns>
        /// A <see cref="DateRangePair"/> containing the calculated start and end dates for both the current and previous periods.
        /// </returns>
        public static DateRangePair GetComparisonPeriodForToday(int monthsBackwards = 1)
        {
            return GetComparisonPeriod(DateTime.Today, monthsBackwards);
        }


    }
}
