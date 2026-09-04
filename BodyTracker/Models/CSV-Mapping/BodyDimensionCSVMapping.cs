using BodyTracker.Models;
using CsvHelper.Configuration;

namespace BodyTracker.Mappings
{
    public sealed class BodyDimensionCSVMapping : ClassMap<FullBodyMeasurementDatasModel>
    {
        public BodyDimensionCSVMapping()
        {
            Map(m => m.DemensionID).Name("dimension_id");

            Map(m => m.ChestCircumference) .Name("chest_circumference_current");
            Map(m => m.ChestCircumference) .Name("chest_circumference_initial");

            Map(m => m.WaistCircumference).Name("abdomen_circumference_current");
            Map(m => m.WaistCircumference).Name("abdomen_circumference_initial");

            Map(m => m.HipsCircumference).Name("hip_circumference_current");
            Map(m => m.HipsCircumference).Name("hip_circumference_intial");

            Map(m => m.FatTongBreastCrease).Name("chest_skin_fold_current");
            Map(m => m.FatTongBreastCrease).Name("chest_skin_fold_initial");

            Map(m => m.FatTongArmpitCrease).Name("axilla_skin_fold_current");
            Map(m => m.FatTongArmpitCrease).Name("axilla_skin_fold_initial");

            Map(m => m.FatTongAbdominalCrease) .Name("abdomen_skin_fold_current");
            Map(m => m.FatTongAbdominalCrease) .Name("abdomen_skin_fold_initial");

            Map(m => m.FatTongHipCrease) .Name("hip_skin_fold_current");
            Map(m => m.FatTongHipCrease) .Name("hip_skin_fold_initial");

            Map(m => m.FatTongThighCrease).Name("thigh_skin_fold_current");
            Map(m => m.FatTongThighCrease).Name("thigh_skin_fold_initial");

            Map(m => m.FatTongBackCrease) .Name("back_skin_fold_current");
            Map(m => m.FatTongBackCrease) .Name("back_skin_fold_initial");

            Map(m => m.FatTongTricepsCrease).Name("tricep_skin_fold_current");
            Map(m => m.FatTongTricepsCrease).Name("tricep_skin_fold_inital");

        }
    }
}