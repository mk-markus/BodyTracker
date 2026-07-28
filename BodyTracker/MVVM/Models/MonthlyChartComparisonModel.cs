using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BodyTracker.MVVM.Models
{
    public class MonthlyChartComparisonModel
    {
        public TextBlock BodyWeightTrendArrow;
        public string BodyWeightDifference;
        public double BodyWeightPreviousBarValue;
        public double BodyWeightCurrentBarValue;
        public double BodyWeightCurrentMaxValue;

        public TextBlock BodyFatTrendArrow;
        public string BodyFatDifference;
        public double BodyFatPreviousBarValue;
        public double BodyFatCurrentBarValue;
        public double BodyFatCurrentMaxValue;

        public TextBlock BodyMuscleMassTrendArrow;
        public string BodyMuscleMassDifference;
        public double BodyMuscleMassPreviousBarValue;
        public double BodyMuscleMassCurrentBarValue;
        public double BodyMuscleMassCurrentMaxValue;

        public TextBlock BodyWaterTrendArrow;
        public string BodyWaterDifference;
        public double BodyWaterPreviousBarValue;
        public double BodyWaterCurrentBarValue;
        public double BodyWaterCurrentMaxValue;


        public TextBlock BodyBmiTrendArrow;
        public string BodyBmiDifference;
        public double BodyBmiPreviousBarValue;
        public double BodyBmiCurrentBarValue;
        public double BodyBmiCurrentMaxValue;

    }
}
