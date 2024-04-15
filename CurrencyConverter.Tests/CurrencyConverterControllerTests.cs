using System;
using System.Collections.Generic;
using System.Text;
using FakeItEasy;
using CurrencyConverter.Controllers;
using CurrencyConverter.Interface;
using CurrencyConverter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using CurrencyConverter.Services;
using Microsoft.Extensions.Logging;


namespace CurrencyConverter.Tests
{
    public class CurrencyConverterControllerTests
    {
        private readonly IExchangeRateService _exchangeRateService;
        private readonly ILogger<CurrencyConverterController> _logger;
        
        public CurrencyConverterControllerTests()
        {
            _exchangeRateService = A.Fake<IExchangeRateService>();
            _logger = A.Fake<ILogger<CurrencyConverterController>>();
        }

        [Fact]

        public void ConvertCurrency_USD_TO_EUR_ReturnsCorrectConvertedAmount()
        {
            // Arrange
          
            A.CallTo(() => _exchangeRateService.LoadExchangeRates()).DoesNothing();
            // Mock exchange rate from JSON
            A.CallTo(() => _exchangeRateService.GetExchangeRate("USD", "EUR")).Returns(0.85m); 


            var configuration = new ConfigurationBuilder()
                                .AddInMemoryCollection(new Dictionary<string, string>
                                        {
                                             ["SupportedCurrencies:0"] = "USD",
                                             ["SupportedCurrencies:1"] = "INR",
                                             ["SupportedCurrencies:2"] = "EUR"
                                        })
                                 .Build();

            //  Environment.SetEnvironmentVariable("USD_TO_EUR", "0.8"); // Set environment variable to override USD to EUR exchange rate
            var controller = new CurrencyConverterController(_exchangeRateService, configuration, _logger);

            // Act
            QueryParameters parameters = new QueryParameters()
            {
                SourceCurrency = "USD",
                TargetCurrency = "EUR",
                Amount = 10.0m
            };

            var result = controller.Get(parameters) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            // Ensure that the response object is not null and inspect its structure
            var responseObject = result.Value;
            Assert.NotNull(responseObject);


            // Ensure that the response object contains the expected properties
            // Inspect the structure of the response object and adjust assertions accordingly
            var responseProperties = responseObject.GetType().GetProperties();
            var exchangeRateProperty = responseProperties.FirstOrDefault(p => p.Name == "ExchangeRate");
            var convertedAmountProperty = responseProperties.FirstOrDefault(p => p.Name == "ConvertedAmount");

            // Assert that the ExchangeRate and ConvertedAmount properties exist
            Assert.NotNull(exchangeRateProperty);
            Assert.NotNull(convertedAmountProperty);

            // Assert the values of the properties
            Assert.Equal(0.85m, (decimal)exchangeRateProperty.GetValue(responseObject));
            Assert.Equal(8.5m, (decimal)convertedAmountProperty.GetValue(responseObject));
        }

        [Fact]
        public void ConvertCurrency_EUR_TO_INR_ReturnsCorrectConvertedAmount()
        {
            // Arrange

            A.CallTo(() => _exchangeRateService.LoadExchangeRates()).DoesNothing();
            // Mock exchange rate from JSON
            A.CallTo(() => _exchangeRateService.GetExchangeRate("EUR", "INR")).Returns(88);


            var configuration = new ConfigurationBuilder()
                                .AddInMemoryCollection(new Dictionary<string, string>
                                {
                                    ["SupportedCurrencies:0"] = "USD",
                                    ["SupportedCurrencies:1"] = "INR",
                                    ["SupportedCurrencies:2"] = "EUR"
                                })
                                 .Build();

            var controller = new CurrencyConverterController(_exchangeRateService, configuration, _logger);

            // Act
            QueryParameters parameters = new QueryParameters()
            {
                SourceCurrency = "EUR",
                TargetCurrency = "INR",
                Amount = 20
            };

            var result = controller.Get(parameters) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var responseObject = result.Value;
            Assert.NotNull(responseObject);


            var responseProperties = responseObject.GetType().GetProperties();
            var exchangeRateProperty = responseProperties.FirstOrDefault(p => p.Name == "ExchangeRate");
            var convertedAmountProperty = responseProperties.FirstOrDefault(p => p.Name == "ConvertedAmount");

            Assert.NotNull(exchangeRateProperty);
            Assert.NotNull(convertedAmountProperty);

            
            Assert.Equal(88, (decimal)exchangeRateProperty.GetValue(responseObject));
            Assert.Equal(1760, (decimal)convertedAmountProperty.GetValue(responseObject));
        }
        [Fact]
        public void ConvertCurrency_InvalidCurrency_ReturnsBadRequest()
        {

            // Arrange
            
            var configuration = new ConfigurationBuilder()
                              .AddInMemoryCollection(new Dictionary<string, string>
                              {
                                  ["SupportedCurrencies:0"] = "USD",
                                  ["SupportedCurrencies:1"] = "INR",
                                  ["SupportedCurrencies:2"] = "EUR"
                              })
                               .Build();

            var controller = new CurrencyConverterController(_exchangeRateService, configuration, _logger);

            QueryParameters parameters = new QueryParameters()
            {
                SourceCurrency = "USD",
                TargetCurrency = "ABC",
                Amount = 10.0m
            };


            // Act
            var result = controller.Get(parameters) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Invalid currency. Supported currency types are USD, INR, EUR", result.Value);
        }

        [Fact]
        public void ConvertCurrency_AmountLessThanZero_ReturnsBadRequest()
        {
            // Arrange
           
            var configuration = new ConfigurationBuilder()
                          .AddInMemoryCollection(new Dictionary<string, string>
                          {
                              ["SupportedCurrencies:0"] = "USD",
                              ["SupportedCurrencies:1"] = "INR",
                              ["SupportedCurrencies:2"] = "EUR"
                          })
                           .Build();

            var controller = new CurrencyConverterController(_exchangeRateService, configuration, _logger);
            controller.ModelState.AddModelError("Amount", "Amount must be greater than zero"); // Simulate invalid model state

            QueryParameters parameters = new QueryParameters()
            {
                SourceCurrency = "USD",
                TargetCurrency = "EUR",
                Amount = -10.0m
            };

            // Act
            var result = controller.Get(parameters) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public void ConvertCurrency_InvalidAmountFormat_ReturnsBadRequest()
        {
            // Arrange

            var configuration = new ConfigurationBuilder()
                                      .AddInMemoryCollection(new Dictionary<string, string>
                                      {
                                          ["SupportedCurrencies:0"] = "USD",
                                          ["SupportedCurrencies:1"] = "INR",
                                          ["SupportedCurrencies:2"] = "EUR"
                                      })
                                       .Build();

            var controller = new CurrencyConverterController(_exchangeRateService, configuration, _logger);
            controller.ModelState.AddModelError("Amount", "Invalid Amount format"); // Simulate invalid model state
            // decimal upto 4 places allowed
            QueryParameters parameters = new QueryParameters()
            {
                SourceCurrency = "USD",
                TargetCurrency = "EUR",
                Amount = 10.1237878768m
            };

            // Act
            var result = controller.Get(parameters) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

    }

}
