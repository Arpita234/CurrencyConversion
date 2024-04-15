namespace CurrencyConverter.Interface
{
    public interface IExchangeRateService
    {
        void LoadExchangeRates();
        decimal GetExchangeRate(string sourceCurrency, string targetCurrency);
    }
}
