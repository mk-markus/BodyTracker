using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BodyTracker.Services.AI
{
    public class OllamaAPIService
    {

            
        public static async Task<string> OllamaModelApiQwen2_5_3b(string prompt)
        {
            var request = new
            {
                model = "qwen2.5:3b",
                prompt = prompt,
                stream = false
            };

            var json = JsonSerializer.Serialize(request);

            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(20)
            };

            var response = await client.PostAsync(
                "http://localhost:11434/api/generate",
                new StringContent(json, Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();


        }
    }



}
