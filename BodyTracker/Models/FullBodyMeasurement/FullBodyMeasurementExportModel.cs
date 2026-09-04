using BodyTracker.Models.Export;
using System.Collections.Generic;

namespace BodyTracker.Models.FullBodyMeasurement
{
    /// <summary>
    /// Represents the data model for exporting complete body measurements, including physical dimensions and metrics.
    /// </summary>
    public class FullBodyMeasurementExportModel
    {
        /// <summary>
        /// Gets or sets the list of export models containing specific body dimensions.
        /// </summary>
        public List<BodyDimensionsExportModel>? BodyDimensions { get; set; }

        /// <summary>
        /// Gets or sets the list of export models containing body-related metrics and key figures.
        /// </summary>
        public List<BodyMetricsExportModel>? BodyMetrics { get; set; }
    }
}