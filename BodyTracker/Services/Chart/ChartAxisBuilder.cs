using LiveChartsCore.SkiaSharpView;


namespace BodyTracker.Services
{
    public static class ChartAxisBuilder
    {
        /// <summary>
        /// Creates and configures a new chart axis instance based on the provided definition parameters.
        /// </summary>
        /// <remarks>Maps properties such as limits, position, visibility settings, and the custom number formatting function from the given definition model to a newly initialized chart axis.</remarks>
        /// <param name="definition">The configuration definition containing the properties for the axis.</param>
        /// <returns>A configured <see cref="Axis"/> instance ready for use within the charting component.</returns>
        public static Axis Create(ChartYAxisDefinition definition)
        {
            return new Axis
            {
                Name = definition.Name,
                MinLimit = definition.MinLimit,
                MaxLimit = definition.MaxLimit,
                Position = definition.Position,
                ShowSeparatorLines = definition.ShowSeparatorLines,
                Labeler = value => value.ToString(definition.Format)
            };
        }
    }
}
