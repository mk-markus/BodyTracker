using BodyTracker.Models;
using CsvHelper.Configuration;

namespace BodyTracker.Mappings
{
    public sealed class BodyMetricCSVMapping : ClassMap<FullBodyMeasurementDatasModel>
    {
        public BodyMetricCSVMapping()
        {
        
            Map(m => m.MetricID).Name("metric_id");

            Map(m => m.MeasurementDate) .Name("create_at");
            Map(m => m.MeasurementDate) .Name("update_at");


            Map(m => m.BodyWeight).Name("weight_current");
            Map(m => m.BodyWeight).Name("weight_initial");

            Map(m => m.BMI).Name("bmi_current");
            Map(m => m.BMI).Name("bmi_initial");

            Map(m => m.BodyFatPercentage).Name("body_fat_percentage_current");
            Map(m => m.BodyFatPercentage).Name("body_fat_percentage_initial");

            Map(m => m.BodyFatPercentageTop) .Name("upper_body_fat_percentage_current");
            Map(m => m.BodyFatPercentageTop) .Name("upper_body_fat_percentage_initial");

            Map(m => m.BodyFatPercentageBottom) .Name("lower_body_fat_percentage_current");
            Map(m => m.BodyFatPercentageBottom) .Name("lower_body_fat_percentage_initial");

            Map(m => m.BodyMusclePercentage).Name("muscle_mass_percentage_current");
            Map(m => m.BodyMusclePercentage).Name("muscle_mass_percentage_initial");

            Map(m => m.BodyMusclePercentageTop).Name("upper_body_muscle_mass_percentage_current");
            Map(m => m.BodyMusclePercentageTop).Name("upper_body_muscle_mass_percentage_initial");

            Map(m => m.BodyMusclePercentageBottom).Name("lower_body_muscle_mass_percentage_current");
            Map(m => m.BodyMusclePercentageBottom).Name("lower_body_muscle_mass_percentage_initial");

            Map(m => m.BodyWaterPercentage).Name("body_water_percentage_current");
            Map(m => m.BodyWaterPercentage).Name("body_water_percentage_initial");

            Map(m => m.BodyBoneMass).Name("bone_mass_current");
            Map(m => m.BodyBoneMass).Name("bone_mass_initial");

            Map(m => m.BodyVisceralFat).Name("visceral_fat_current");
            Map(m => m.BodyVisceralFat).Name("visceral_fat_initial");

        }
    }
}