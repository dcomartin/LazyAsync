using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LazyAsync
{
    public class LazyAsync<T> : Lazy<Task<T>>
    {
        public LazyAsync(Func<Task<T>> taskFactory) : base(
            () => Task.Factory.StartNew(taskFactory).Unwrap()) { }
    }

    class Program
    {
        public static async Task Main(string[] args)
        {
            var currencyService = new CurrencyService();
            var rates = await currencyService.ExchangeRates.Value;
            var cad = rates.Rates.Single(x => x.Key == "CAD");
            Console.WriteLine($"USD to CAD = {cad.Value}");
            Console.ReadKey();
        }
    }

    public class CurrencyService : IDisposable
    {
        private readonly HttpClient _httpClient;
        public LazyAsync<ExchangeRates> ExchangeRates { get; set; }

        public CurrencyService()
        {
            _httpClient = new HttpClient();
            ExchangeRates = new LazyAsync<ExchangeRates>(async () =>
            {
                var json = await _httpClient.GetStringAsync("http://api.fixer.io/latest?base=USD");
                return JsonConvert.DeserializeObject<ExchangeRates>(json);
            });
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    public class ExchangeRates
    {
        public string Base { get; set; }
        public DateTime Date { get; set; }
        public Dictionary<string, decimal> Rates { get; set; }
    }
}
