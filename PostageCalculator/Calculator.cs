using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PostageCalculator
{
    public class Calculator
    {
        private const int BASE_COST = 5;

        public async Task<double> CalculatePostageAsync(int postcode, Box box)
        {
            if (IsOverTheSizeLimit(box)) throw new ExcessiveVolumeException();

            if (IsSaturday() || IsFebruary29th()) return 0;

            double weightInKilos = box.WeightInGrams / 1000;
            if (weightInKilos < 1) return BASE_COST;

            double postageCost = BASE_COST;
            if (weightInKilos >= 1) postageCost += weightInKilos * 0.2;
            if (weightInKilos > 50) postageCost *= 1.17;

            return await IsVictorianPostcodeAsync(postcode) ? postageCost : postageCost + 9.5;
        }

        private bool IsOverTheSizeLimit(Box box)
        {
            return ((box.DepthInMM * box.WidthInMM * box.HeightInMM) / 1e9) > 3;
        }

        private bool IsSaturday()
        {
            return DateTime.Today.DayOfWeek == DayOfWeek.Saturday;
        }

        private bool IsFebruary29th()
        {
            return DateTime.Today.Month == 2
                && DateTime.Today.Day == 29;
        }

        private async Task<bool> IsVictorianPostcodeAsync(int postcode)
        {
            // TODO: Dont't instantiate this for every call
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://digitalapi.auspost.com.au/");
            client.DefaultRequestHeaders.Add("AUTH-KEY", "a7f62ca1-20da-4f2b-9431-cbf6a33ea00f");
            client.DefaultRequestHeaders.Add("User-Agent", "Curl");

            var request = new HttpRequestMessage(HttpMethod.Get,
                $"postcode/search.json?q={postcode}");
            var response = await client.SendAsync(request);
            // TODO: Cache response

            bool isInVic = false;
            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();

                var result = await JsonSerializer.DeserializeAsync<RootObject>(responseStream);
                isInVic = result.Localities.Locality.Any(x => x.State.ToUpper() == "VIC");
            }

            return isInVic;
        }
    }
}