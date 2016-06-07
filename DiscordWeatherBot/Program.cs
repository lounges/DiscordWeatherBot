using Discord;
using ServiceStack;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace DiscordWeatherBot
{
    class Program
    {
        //client id is the public client id of your bot, it can be found here:
        //https://discordapp.com/developers/applications/me

        //goto this URL to add your bot to your server:
        //https://discordapp.com/oauth2/authorize?client_id=YOUR_CLIENT_ID&scope=bot&permissions=19456

        static string wundergroundKey = "";
        static string discordKey = "";

        static DiscordClient client;

        async static Task<CurrentObservation> GetWeather(string postalCode)
        {
            var requestUrl = $"http://api.wunderground.com/api/{wundergroundKey}/conditions/q/{postalCode}.json";
            Debug.WriteLine($"Getting the weather using url: {requestUrl}");
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
            using (var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("DiscordWeatherBot.weather.key")))
                wundergroundKey = reader.ReadToEnd();
            Debug.WriteLine($"Found Weather Underground Key: {wundergroundKey}");

            using (var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("DiscordWeatherBot.discord.key")))
                discordKey = reader.ReadToEnd();
            Debug.WriteLine($"Found Discord Token: {discordKey}");

            client = new DiscordClient();
            client.MessageReceived += async (s,e) =>
            {
                const string weatherCommand = "!weather";

                var lower = e.Message.RawText.ToLower().Trim();
                var isWeatherRequest = lower.StartsWith(weatherCommand);
                if( isWeatherRequest )
                {
                    var location = lower.Substring(weatherCommand.Length);
                    if( location.Length > 0 )
                    {
                        var results = await GetWeather(location.Trim());
                        var message = $"Unable to locate weather for: \"{location}";
                        if (results != null)
                            message = $"The weather in {results.display_location.full} is now: {results.temp_c} ({results.temp_f} F)";

                        await e.Channel.SendMessage(message);


                        

                    }
                }
            };

            Console.WriteLine("Connecting to Discord...");
            var connectTask = client.Connect(discordKey);
            connectTask.Wait();
            client.SetStatus(UserStatus.Online);
            client.SetGame("WeatherUnderground");


            Console.In.ReadLine();
        }
    }
}
