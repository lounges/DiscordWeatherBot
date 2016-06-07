using ServiceStack;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DiscordWeatherBot
{
    class Program
    {
        static string wundergroundKey = "";

        async static Task<CurrentObservation> GetWeather(string postalCode)
        {
            var requestUrl = $"http://api.wunderground.com/api/{wundergroundKey}/conditions/q/{postalCode}.json";
            using (var client = new HttpClient())
            {
                try
                {
                    var rawJson = await client.GetStringAsync(requestUrl);
                    return rawJson.FromJson<CurrentConditionsResponse>().current_observation;
                }
                catch(Exception ex)
                {
                    Console.Out.WriteLine(ex.ToString());
                }
            }

            return null;
        }

        static void Main(string[] args)
        {
            using (var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("DiscordWeatherBot.Api.key")))
                wundergroundKey = reader.ReadToEnd();

            Debug.WriteLine($"Found Weather Underground Key: {wundergroundKey}");

            var weatherTask = GetWeather("89147");
            weatherTask.Wait();

            var weather = weatherTask.Result;
            Console.Out.WriteLine($"The weather in {weather.display_location.full} is now: {weather.temp_c} ({weather.temp_f} F)");
            Console.In.ReadLine();
        }
    }
}
