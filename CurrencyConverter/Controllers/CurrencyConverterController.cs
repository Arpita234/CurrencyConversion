using CurrencyConverter.Models;
using CurrencyConverter.Interface;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CurrencyConverter.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class CurrencyConverterController : ControllerBase
    {
        private readonly ILogger<CurrencyConverterController> _logger;

        private readonly IExchangeRateService _exchangeRateService;
        private readonly string[] _supportedCurrencies;

        public CurrencyConverterController(IExchangeRateService exchangeRateService, IConfiguration configuration, ILogger<CurrencyConverterController> logger)
        {
            _exchangeRateService = exchangeRateService;
            _supportedCurrencies = configuration.GetSection("SupportedCurrencies").Get<string[]>();
            _logger = logger;
        }

        // GET: /convert

        [HttpGet("/convert")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public IActionResult Get([FromQuery] QueryParameters parameters)      
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                
                // Check if currencies are valid
                else if (!IsValidCurrency(parameters.SourceCurrency) || !IsValidCurrency(parameters.TargetCurrency))
                {
                    _logger.LogError($"Invalid currency. Supported currency types are {string.Join(", ", _supportedCurrencies)} : SourceCurrency:{parameters.SourceCurrency}, TargetCurrency:{parameters.TargetCurrency}");
                   
                    return BadRequest($"Invalid currency. Supported currency types are {string.Join(", ",_supportedCurrencies)}");
                }

                _logger.LogInformation("Requesting Currency Conversion");

                _exchangeRateService.LoadExchangeRates();
                // Get exchange rate
                decimal exchangeRate = _exchangeRateService.GetExchangeRate(parameters.SourceCurrency, parameters.TargetCurrency);

                // Convert amount
                decimal convertedAmount = parameters.Amount * exchangeRate;

                _logger.LogInformation($"Successful Currency Conversion : SourceCurrency:{parameters.SourceCurrency}, TargetCurrency:{parameters.TargetCurrency}, Amount:{parameters.Amount}, ConvertedAmount:{convertedAmount}");

                return Ok( new { ExchangeRate = exchangeRate, ConvertedAmount = convertedAmount });

            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError($"Error in Currency Conversion : SourceCurrency:{parameters.SourceCurrency}, TargetCurrency:{parameters.TargetCurrency}, Amount:{parameters.Amount}, Error:{ex.Message}");

                return NotFound(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError($"Error in Currency Conversion : SourceCurrency:{parameters.SourceCurrency}, TargetCurrency:{parameters.TargetCurrency}, Amount:{parameters.Amount}, Error:{ex.Message}");

                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Currency Conversion : SourceCurrency:{parameters.SourceCurrency}, TargetCurrency:{parameters.TargetCurrency}, Amount:{parameters.Amount}, Error:{ex.Message}");

                return BadRequest(ex.Message);
            }

        }

        private bool IsValidCurrency(string currency)
        {
            // Validate currency against the list of supported currencies
            try
            {
                return Array.Exists(_supportedCurrencies, c => c.Equals(currency, StringComparison.OrdinalIgnoreCase));
            }
            catch(Exception)
            {
                throw;
            }
        }      
       
    }
}
