using AutoMapper;
using MongoDB.Bson;
using StocksPortfolio.Application.Interfaces.Collections;
using StocksPortfolio.Application.Interfaces.Enums;
using StocksPortfolio.Application.Interfaces.Interfaces;
using StocksPortfolio.Application.Interfaces.Models;
using StocksPortfolio.Stocks;
using System.Net.Http.Headers;
using System.Text.Json;

namespace StocksPortfolio.Application.Services
{
    public class PortfolioService : IPortfolioService
    {
        private readonly IPortfolioRepository _portfolioRepository;
        private readonly IMapper _mapper;
        private readonly IStocksService _stocksService;
        public PortfolioService(
            IPortfolioRepository portfolioRepository,
            IMapper mapper,
            IStocksService stocksService
            )
        {
            _portfolioRepository = portfolioRepository ?? throw new NullReferenceException(nameof(portfolioRepository));
            _mapper = mapper ?? throw new NullReferenceException(nameof(mapper));
            _stocksService = stocksService ?? throw new NullReferenceException(nameof(stocksService));
        }

        public PortfolioViewModel GetPortfolio(string portfolioId)
        {
            ObjectId objectId;
            try
            {
                objectId = ObjectId.Parse(portfolioId);
            }
            catch (FormatException ex)
            {
                throw new FormatException(ExceptionCodes.notAValid24String);
            }

            var portfolio = _portfolioRepository.GetPortfolioByIdThatIsNotSoftDeleted(objectId);
            if (portfolio == null)
                throw new NullReferenceException(ExceptionCodes.portfolioDoesNotExist);
            return portfolio;
        }


        public async Task<decimal> GetTotalPortfolioValue(PortfolioCollection portfolio, string currency, CurrencyViewModel currencyData)
        {
            if (portfolio == null)
                throw new NullReferenceException(ExceptionCodes.portfolioDoesNotExist);

            if (currency == null)
                throw new NullReferenceException(ExceptionCodes.currencyDoesNotExist);

            if (!currency.Equals(Currencies.Usd) && !currencyData.Quotes.ContainsKey(Currencies.Usd + currency)) 
                throw new Exception(ExceptionCodes.currencyNotSupported);

            var totalAmount = 0m;
            foreach (var stock in portfolio.Stocks)
            {
                var price = await _stocksService.GetStockPrice(stock.Ticker);
                if (stock.Currency == currency)
                {
                    totalAmount += price.Price * stock.NumberOfShares;
                }
                else
                {
                    var stockPrice = price.Price;
                    decimal sharesInUsd = 0;
                    if(stock.Currency == Currencies.Usd)
                    {
                        sharesInUsd = stockPrice * stock.NumberOfShares;
                    }
                    else
                    {
                        var rateUsd = currencyData.Quotes[Currencies.Usd + stock.Currency];
                        sharesInUsd = (1 / rateUsd) * stockPrice * stock.NumberOfShares;
                    }

                    if (currency == Currencies.Usd)
                    {
                        totalAmount += sharesInUsd;
                    }else
                    {
                        var toGivenCurrency = currencyData.Quotes[Currencies.Usd + currency];
                        totalAmount += sharesInUsd * toGivenCurrency;
                    }
                }
            }

            return totalAmount;
        }

        public void SoftDeleteOnePortfolio(string portfolio)
        {
            ObjectId objectId;
            try
            {
                objectId = ObjectId.Parse(portfolio);
            }
            catch (FormatException ex)
            {
                throw new FormatException(ExceptionCodes.notAValid24String);
            }

            _portfolioRepository.SoftDeletePortfolio(objectId);
        }
    }
}
