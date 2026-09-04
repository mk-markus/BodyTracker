using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BodyTracker.Services.AI
{
    public class AiJsonDeserializerService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static BodyMetricsAiAnalysisJsonModel? DeserializeBodyMetricsAiAnalysisJsonModel(string json)
        {
            return JsonSerializer.Deserialize<BodyMetricsAiAnalysisJsonModel>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
    }
}
