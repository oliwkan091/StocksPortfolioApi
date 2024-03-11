using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using StocksPortfolio.Application.Interfaces.Collections;
using StocksPortfolio.Application.Interfaces.Enums;
using StocksPortfolio.Application.Interfaces.Interfaces;
using StocksPortfolio.Stocks;
using System.Text.Json;

namespace StocksPortfolio.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortfolioController : ControllerBase
    {
        private readonly ICurrencyService _currencyService;
        private readonly IPortfolioService _portfolioService;
        private readonly IMapper _mapper;

        public PortfolioController(
            IPortfolioRepository portfolioRepository,
            ICurrencyService currencyService,
            IPortfolioService portfolioService,
            IMapper mapper
            )
        {
            _currencyService = currencyService ?? throw new NullReferenceException(nameof(currencyService));
            _portfolioService = portfolioService ?? throw new NullReferenceException(nameof(portfolioService));
            _mapper = mapper ?? throw new NullReferenceException(nameof(mapper));
        }

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            var portfolio = _portfolioService.GetPortfolio(id);
            if (portfolio == null)
                return BadRequest(ExceptionCodes.portfolioDoesNotExist);
            return Ok(portfolio);
        }

        [HttpGet("/value")]
        public IActionResult GetTotalPortfolioValue(string portfolioId, string currency = "USD")
        {
            var portfolio = _dataService.GetPortfolio(ObjectId.Parse(portfolioId)).Result;
            var totalAmount = 0m;
            var stockService = new StocksService.StocksService();
            var apiAccessKey = "78c057e28b2abf54f48110356bb9d1ce";
            using (var httpClient = new HttpClient { BaseAddress = new Uri("http://api.currencylayer.com/") })
            {
                // Docs: https://currencylayer.com/documentation
                var foo = httpClient.GetAsync($"live?access_key={apiAccessKey}").Result;
                var data = JsonSerializer.DeserializeAsync<Quote>(foo.Content.ReadAsStream()).Result;

                foreach (var stock in portfolio.Stocks)
                {
                    if (stock.Currency == currency)
                    {
                        totalAmount += stockService.GetStockPrice(stock.Ticker).Result.Price * stock.NumberOfShares;
                    }
                    else
                    {
                        if (currency == "USD")
                        {
                            var stockPrice = stockService.GetStockPrice(stock.Ticker).Result.Price;
                            var rateUsd = data.quotes["USD" + stock.Currency];
                            totalAmount += stockPrice / rateUsd * stock.NumberOfShares;
                        }
                        else
                        {
                            var stockPrice = stockService.GetStockPrice(stock.Ticker).Result.Price;
                            var rateUsd = data.quotes["USD" + stock.Currency];
                            var amount = stockPrice / rateUsd * stock.NumberOfShares;
                            var targetRateUsd = data.quotes["USD" + currency];
                            totalAmount += amount * targetRateUsd;
                        }
                    }
                }
            }

            return Ok(totalAmount);
        }

        [HttpGet("/delete")]
        public IActionResult DeletePortfolio(string portfolioId)
        {
            var dataService = new DataProviderService();
            dataService.DeletePortfolio(ObjectId.Parse(portfolioId));
            return Ok();
        }
    }
}
