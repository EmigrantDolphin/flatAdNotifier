using System.Diagnostics;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using System.Data;
using System.Net.Http.Headers;
using System.Net;
using System.Net.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace rentNotifier
{
    class Program
    {
        static readonly HttpClient client = new HttpClient();
        static async Task Main(string[] args)
        {
            try
            {
                var prevAdvIds = new List<string>();

                while(true)
                {
                    var pageWithRentAdsString = await GetPageWithRentAds();
                    CheckForNewAdsAndPrint(prevAdvIds, pageWithRentAdsString);
                    Thread.Sleep(1000 * 60);
                }

            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void CheckForNewAdsAndPrint(List<string> prevAdIds, string pageWithRentAdsString)
        {
            var currentAdIds = new List<string>();
            var adIdMatch = Regex.Match(pageWithRentAdsString, "id=\"ads[0-9]{8}\"");

            while (adIdMatch.Success)
            {
                // Console.WriteLine(adIdMatch.Value);
                currentAdIds.Add(adIdMatch.Value);
                adIdMatch = adIdMatch.NextMatch();
            }

            var doAdsMatch = currentAdIds.All(x => prevAdIds.Contains(x));

            if (!doAdsMatch)
            {
                Console.WriteLine($"New ads! {DateTimeOffset.UtcNow.LocalDateTime:yyyy/MM/dd HH:mm}");
                prevAdIds.Clear();
                prevAdIds.AddRange(currentAdIds);
            }
        }

        private static async Task<string> GetPageWithRentAds()
        {
            var response = await client.GetAsync("https://www.skelbiu.lt/skelbimai/?autocompleted=1&keywords=&cost_min=&cost_max=350&type=0&space_min=30&space_max=&rooms_min=&rooms_max=&year_min=&year_max=&floor_min=&floor_max=&floor_type=0&price_per_unit_min=&price_per_unit_max=&searchAddress=&district=0&quarter=0&streets=0&ignorestreets=0&cities=465%2C466%2C491&distance=1&mainCity=1&search=1&category_id=322&user_type=0&ad_since_min=0&ad_since_max=0&visited_page=1&orderBy=-1&detailsSearch=1");

            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();

            return responseBody;
        }
    }
}
