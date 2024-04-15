using CurrencyConverter.Interface;

namespace CurrencyConverter.Services
{
    public class ExchangeRateService : IExchangeRateService
    {

        private Dictionary<string, decimal>? _exchangeRates;
        private readonly string filesDirectory = "ExchangeRateFile";
     
        public void LoadExchangeRates()
        {
            try
            {
                var directory = Path.Combine(Directory.GetCurrentDirectory(), filesDirectory);

                if (!Directory.Exists(directory))
                {
                   throw new Exception("Exchange Rates Json folder is not present");
                }

                var filePath = Path.Combine(directory, "exchangeRates.json");

                if (filePath is null || !File.Exists(filePath))
                {
                    throw new FileNotFoundException("Exchange Rates Json file is not present");
                }

                var json = File.ReadAllText(filePath);
                _exchangeRates = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, decimal>>(json);

                // Overriding exchange rates from environment variables
                if (_exchangeRates is not null)
                {
                    foreach (var pair in _exchangeRates)
                    {
                        // for example I have set the environment variable USD_TO_INR = 100 in launchSeetings.json
                        // environment variables can be configured at machine level as well and we can change to EnvironmentVariableTarget.Machine in GetEnvironmentVariable call

                        var envVariable = Environment.GetEnvironmentVariable(pair.Key.ToUpper());
                        if (!string.IsNullOrEmpty(envVariable) && decimal.TryParse(envVariable, out decimal rate))
                        {
                            _exchangeRates[pair.Key] = rate;
                        }
                    }
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        public decimal GetExchangeRate(string sourceCurrency, string targetCurrency)
        {
            try
            {
                string key = $"{sourceCurrency.ToUpper()}_TO_{targetCurrency.ToUpper()}";

                if (_exchangeRates != null && _exchangeRates.ContainsKey(key))
                {
                    return _exchangeRates[key];
                }
                else
                {
                    throw new KeyNotFoundException($"Invalid currency pair {sourceCurrency} , {targetCurrency}");
                }
            }
            catch(Exception)
            {
                throw;
            }
        }

    }
}
