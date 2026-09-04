using BodyTracker.Models;
using BodyTracker.Models.FullBodyMeasurement;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BodyTracker.Services.AI
{
    /// <summary>
    /// Provides helper methods and constants for building prompts and structured JSON payloads related to body metric analyses.
    /// </summary>
    public static class AiPromptBuilderService
    {
        /// <summary>
        /// Defines the task identifier string used for fitness progress analysis.
        /// </summary>
        public static string taskAnalyseBodyMetrics = "fitness_progress_analysis";

        /// <summary>
        /// Defines the core instructions given to evaluate body metric trends, developments, and recommendations.
        /// </summary>
        public static string instructionAnalyseBodyMetrics = @"Analysiere die Entwicklung der Körperwerte und erstelle eine kompakte, übersichtliche Zusammenfassung.

                    WICHTIGE REGELN:
                    - Antworte ausschließlich auf Deutsch.
                    - Verwende Markdown-Formatierung.
                    - Formuliere prägnant und verständlich.
                    - Konzentriere dich auf die wichtigsten Erkenntnisse.
                    - Vermeide lange Fließtexte.
                    - Wenn keine aussagekräftigen Trends erkennbar sind, weise darauf hin.

                    Verwende exakt folgende Struktur:

                    ## Fitness Analyse 

                    ## 📈 Langfristiger Trend 
                    - Beschreibe die Entwicklung über den gesamten Zeitraum.
                    - Bewerte Veränderungen bei Gewicht, BMI, Körperfett, Muskelanteil und viszeralem Fett.

                    ## 📅 Kurzfristiger Trend 
                    - Beschreibe die Entwicklung der letzten Messungen.
                    - Hebe aktuelle Veränderungen hervor.

                    ## ✅ Positive Entwicklungen 
                    - Liste die wichtigsten Verbesserungen auf.

                    ## ⚠️ Auffälligkeiten
                    - Nenne mögliche Rückschritte, Stagnationen oder ungewöhnliche Schwankungen.

                    ## 💡 Zusammenfassung
                    - Gib ein kurzes Gesamtfazit in maximal 3 Sätzen.
                    - Formuliere motivierend und sachlich.

                    Berücksichtige alle bereitgestellten Messwerte bei der Analyse.";


        /// <summary>
        /// Creates a structured JSON payload containing analysis instructions and filtered body metric measurements.
        /// </summary>
        /// <param name="metrics">The list of body metric models to include in the analysis payload.</param>
        /// <returns>A formatted JSON string representing the analysis payload, or "{}" if the metrics collection is null or empty.</returns>
        public static string CreateBodyMetricsAnalysisJson(List<BodyMetricModel> metrics)
        {
            if (metrics == null || metrics.Count == 0)
                return "{}";


            var payload = new
            {
                task = taskAnalyseBodyMetrics,
                instructions = instructionAnalyseBodyMetrics,

                measurementCount = metrics.Count,

                measurements = metrics.Select(x => new
                {
                    date = x.MeasurementDate.ToString("yyyy-MM-dd"),

                    weight = x.BodyWeight,

                    bmi = x.BMI,

                    bodyMusclePercentage = x.BodyMusclePercentage,

                    bodyFatPercentage = x.BodyFatPercentage,

                    bodyWaterPercentage = x.BodyWaterPercentage
                })
            };

            return JsonSerializer.Serialize(
                payload,
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition =
                        JsonIgnoreCondition.WhenWritingNull
                });
        }

        /// <summary>
        /// Creates a structured JSON payload containing analysis instructions and chronologically ordered weekly average body metric measurements.
        /// </summary>
        /// <param name="metrics">The list of weekly average body metric models to include in the analysis payload.</param>
        /// <returns>A formatted JSON string representing the analysis payload, or "{}" if the metrics collection is null or empty.</returns>
        public static string CreateBodyMetricsAnalysisJson(List<WeeklyAverageBodyMetricModel> metrics)
        {
            if (metrics == null || metrics.Count == 0)
                return "{}";

            var orderedMetrics = metrics
                .OrderBy(x => x.WeekStartDate)
                .ToList();

            var payload = new
            {
                task = taskAnalyseBodyMetrics,

                instructions = instructionAnalyseBodyMetrics,

                measurementCount = orderedMetrics.Count,

                measurements = orderedMetrics.Select(x => new
                {
                    calendarWeek = x.CalendarWeek,

                    weekStartDate = x.WeekStartDate.ToString("yyyy-MM-dd"),

                    weekEndDate = x.WeekEndDate.ToString("yyyy-MM-dd"),

                    weight = x.AverageValues.BodyWeight,

                    bmi = x.AverageValues.BMI,

                    bodyFatPercentage = x.AverageValues.BodyFatPercentage,
                    bodyMusclePercentage = x.AverageValues.BodyMusclePercentage,

                    bodyWaterPercentage = x.AverageValues.BodyWaterPercentage
                })
            };

            return JsonSerializer.Serialize(
                payload,
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition =
                        JsonIgnoreCondition.WhenWritingNull
                });
        }
    }
}