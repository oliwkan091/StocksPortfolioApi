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
        private readonly ICurrencyService _getCurrencyExchangeService;
        private readonly ICurrencyRepository _currencyService;
        private readonly IPortfolioRepository _portfolioRepository;
        private readonly IMapper _mapper;
        private readonly IStocksService _stocksService;
        public PortfolioService(ICurrencyService getCurrencyExchangeService, ICurrencyRepository currencyService, IPortfolioRepository portfolioRepository, IMapper mapper, IStocksService stocksService)
        {
            _getCurrencyExchangeService = getCurrencyExchangeService ?? throw new NullReferenceException(nameof(getCurrencyExchangeService));
            _currencyService = currencyService ?? throw new NullReferenceException(nameof(currencyService));
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
    }
}
