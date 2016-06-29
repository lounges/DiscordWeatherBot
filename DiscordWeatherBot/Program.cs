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
        public static List<string> bustedImages = new List<string>
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
        };

        public static List<string> confirmedImages = new List<string>
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
        };

        public static List<string> plausibleImages = new List<string>
        {
            "Images/plausible1.jpg",
            "Images/plausible2.jpg",
            "Images/plausible3.jpg",
            "Images/plausible4.jpg",
            "Images/plausible5.jpg",
            "Images/plausible6.jpg",
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
                const string weatherCommand = "!weather";

                var lower = e.Message.RawText.ToLower().Trim();
                var isWeatherRequest = lower.StartsWith(weatherCommand);
                if( isWeatherRequest )
                {
                    var location = lower.Substring(weatherCommand.Length);
                    if( location.Length > 0 )
                    {
                        var results = await GetWeather(location.Trim());
                        var message = $"Unable to locate weather for: \"{location}\"";
                        if (results != null)
                            message = $"The temperature in {results.display_location.full} is now: {results.temp_c} ({results.temp_f} F)\r\nHumidity: {results.relative_humidity}\r\nWinds {results.wind_string}\r\nIt is presently: {results.weather}\r\n";

                        await e.Channel.SendMessage(message);
                    }
                }






                const string confirmedCommand = "!confirmed";
                const string bustedCommand = "!busted";
                const string plausibleCommand = "!plausible";

                lower = e.Message.RawText.ToLower().Trim();

                var isConfirmed = lower.StartsWith(confirmedCommand);
                var isBusted = lower.StartsWith(bustedCommand);
                var isPlausible = lower.StartsWith(plausibleCommand);

                if (isConfirmed)
                    await e.Channel.SendFile(confirmedImages[r.Next(confirmedImages.Count)]);
                else if (isBusted)
                    await e.Channel.SendFile(bustedImages[r.Next(bustedImages.Count)]);
                else if (isPlausible)
                    await e.Channel.SendFile(plausibleImages[r.Next(plausibleImages.Count)]);


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
