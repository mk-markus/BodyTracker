using BodyTracker.Models;
using CsvHelper.Configuration;

namespace BodyTracker.Mappings
{
    public sealed class FullBodyMeasurementCSVMapping : ClassMap<FullBodyMeasurementDatasModel>
    {
        public FullBodyMeasurementCSVMapping()
        {
            Map(m => m.MetricID).Name("MetricID");
            Map(m => m.DemensionID).Name("DemensionID");
            Map(m => m.MeasurementDate).Name("MeasurementDate");

            Map(m => m.BodyWeight).Name("BodyWeight");
            Map(m => m.BMI).Name("BMI");

            Map(m => m.BodyFatPercentage).Name("BodyFatPercentage");
            Map(m => m.BodyFatPercentageTop).Name("BodyFatPercentageTop");
            Map(m => m.BodyFatPercentageBottom).Name("BodyFatPercentageBottom");

            Map(m => m.BodyWaterPercentage).Name("BodyWaterPercentage");

            Map(m => m.BodyMusclePercentage).Name("BodyMusclePercentage");
            Map(m => m.BodyMusclePercentageTop).Name("BodyMusclePercentageTop");
            Map(m => m.BodyMusclePercentageBottom).Name("BodyMusclePercentageBottom");

            Map(m => m.FFM_kg).Name("FFM_kg");
            Map(m => m.FFM_index).Name("FFM_index");
            Map(m => m.FFM_index_describing).Name("FFM_index_describing");

            Map(m => m.BodyBoneMass).Name("BodyBoneMass");

            Map(m => m.BodyVisceralFat).Name("BodyVisceralFat");

            Map(m => m.ChestCircumference).Name("ChestCircumference");
            Map(m => m.WaistCircumference).Name("WaistCircumference");
            Map(m => m.HipsCircumference).Name("HipsCircumference");

            Map(m => m.FatTongBreastCrease).Name("FatTongBreastCrease");
            Map(m => m.FatTongArmpitCrease).Name("FatTongArmpitCrease");
            Map(m => m.FatTongAbdominalCrease).Name("FatTongAbdominalCrease");
            Map(m => m.FatTongHipCrease).Name("FatTongHipCrease");
            Map(m => m.FatTongThighCrease).Name("FatTongThighCrease");
            Map(m => m.FatTongBackCrease).Name("FatTongBackCrease");
            Map(m => m.FatTongTricepsCrease).Name("FatTongTricepsCrease");

            Map(m => m.CaliperBodyFatPercentage).Name("CaliperBodyFatPercentage");
        }
    }
}