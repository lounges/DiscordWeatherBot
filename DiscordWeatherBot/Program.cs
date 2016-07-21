using Discord;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace DiscordWeatherBot
{
    class Program
    {
        public static Dictionary<string, List<string>> imageCommands = new Dictionary<string, List<string>>
        {
            ["confirmed"] = new List<string>
            {
                "Images/confirmed1.jpg",
                "Images/confirmed2.jpg",
                "Images/confirmed3.jpg",
                "Images/confirmed4.jpg",
                "Images/confirmed5.jpg",
                "Images/confirmed6.jpg",
                "Images/confirmed7.jpg",
                "Images/confirmed8.jpg",
                "Images/confirmed9.jpg",
            },
            ["plausible"] = new List<string>
            {
                "Images/plausible1.jpg",
                "Images/plausible2.jpg",
                "Images/plausible3.jpg",
                "Images/plausible4.jpg",
                "Images/plausible5.jpg",
                "Images/plausible6.jpg",
            },
            ["busted"] = new List<string>
            {
                "Images/busted1.jpg",
                "Images/busted2.jpg",
                "Images/busted3.jpg",
                "Images/busted4.jpg",
                "Images/busted5.png",
                "Images/busted6.jpg",
                "Images/busted7.jpg",
                "Images/busted8.jpg",
                "Images/busted9.jpg",
                "Images/busted10.jpg",
                "Images/busted11.jpg",
            },
            ["cmon"] = new List<string>
            {
                "Images/cmon.png",
                "Images/cmon2.jpg",
                "Images/cmon3.jpg",
                "Images/cmon4.jpg",
            },
            ["mars"] = new List<string>
            {
                "Images/mars.jpg",
            },
            ["maga"] = new List<string>
            {
                "Images/maga1.jpg",
                "Images/maga2.jpg",
                "Images/maga3.jpg",
                "Images/maga4.jpg",
            }
        };

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
            imageCommands.Add("trump", imageCommands["maga"]);

            Random r = new Random();

            using (var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("DiscordWeatherBot.weather.key")))
                wundergroundKey = reader.ReadToEnd();
            Debug.WriteLine($"Found Weather Underground Key: {wundergroundKey}");

            using (var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("DiscordWeatherBot.discord.key")))
                discordKey = reader.ReadToEnd();
            Debug.WriteLine($"Found Discord Token: {discordKey}");

            client = new DiscordClient();
            client.MessageReceived += async (s,e) =>
            {
                if (e.User.Id == client.CurrentUser.Id)
                    return;

                const string weatherCommand = "weather";
                const string temperatureCommand = "temperature";

                var lower = e.Message.RawText.ToLower().Trim();
                var isWeatherRequest = lower.Contains(weatherCommand);
                if (!isWeatherRequest && lower.Contains(temperatureCommand))
                    isWeatherRequest = true;
                if ( isWeatherRequest )
                {
                    try
                    {
                        lower = lower.Replace(" in", "");
                        lower = lower.Replace("?", "");
                        lower = lower.Replace(" like", "");

                        var weatherStartPos = lower.IndexOf(weatherCommand);
                        if (weatherStartPos > 0) weatherStartPos += weatherCommand.Length + 1;

                        if (weatherStartPos < 1) weatherStartPos = lower.IndexOf(temperatureCommand) + temperatureCommand.Length + 1;
                        var nextSpace = lower.IndexOf(" ", weatherStartPos);

                        if (nextSpace < 0)
                            nextSpace = lower.Length;

                        var location = lower.Substring(weatherStartPos, nextSpace - weatherStartPos);
                        location = location.Trim();
                        if (location.Length > 0)
                        {
                            var results = await GetWeather(location.Trim());
                            var message = $"Unable to locate weather for: \"{location}\"";
                            if (results != null)
                                message = $"The temperature in {results.display_location.full} is now: {results.temp_c} ({results.temp_f} F)\r\nHumidity: {results.relative_humidity}\r\nWinds {results.wind_string}\r\nIt is presently: {results.weather}\r\n";

                            await e.Channel.SendMessage(message);
                        }
                    }
                    catch(Exception)
                    { }
                    
                }

                lower = e.Message.RawText.ToLower().Trim().Replace("'", "");
                foreach ( var kvp in imageCommands )
                {
                    var isCommand = lower.Contains(kvp.Key);
                    if (isCommand)
                        await e.Channel.SendFile(kvp.Value[r.Next(kvp.Value.Count)]);
                }

            };

            Console.WriteLine("Connecting to Discord...");
            var connectTask = client.Connect(discordKey);
            connectTask.Wait();
            client.SetStatus(UserStatus.Online);
            client.SetGame("ChicaChica");


            Console.In.ReadLine();
        }
    }
}
