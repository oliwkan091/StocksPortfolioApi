using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using StocksPortfolio.Api.Controllers;
using StocksPortfolio.Application.Interfaces.Collections;
using StocksPortfolio.Application.Interfaces.Enums;
using StocksPortfolio.Application.Interfaces.Interfaces;
using StocksPortfolio.Application.Interfaces.Models;
using StocksPortfolio.Application.MappingProfile;
using StocksPortfolio.Application.Services;
using StocksPortfolio.Infrastructure.Services;
using StocksPortfolio.Stocks;

namespace Tests
{
    public class UnitTest1
    {
        private readonly IMapper _mapper;

        private readonly PortfolioRepository _portfolioRepositoryObj;
        private readonly Mock<IPortfolioRepository> _portfolioRepository = new Mock<IPortfolioRepository>();

        private readonly PortfolioService _portfolioServiceObj;
        private readonly Mock<IPortfolioService> _portfolioService = new Mock<IPortfolioService>();

        private readonly StocksService _stocksServiceObj;
        private readonly Mock<IStocksService> _stocksService = new Mock<IStocksService>();

        private readonly CurrencyRepository _currencyRepositoryObj;
        private readonly Mock<ICurrencyRepository> _currencyRepository = new Mock<ICurrencyRepository>(); 

        private readonly CurrencyService _currencyServiceObj;
        private readonly Mock<ICurrencyService> _currencyService = new Mock<ICurrencyService>();

        public static readonly PortfolioViewModel portfolioSample = new PortfolioViewModel
        {
            Id = ObjectId.Parse("50227b375dff9218248eadc4"),
            CurrentTotalValue = 0,
            IsDeleted = false,
            Stocks = new List<StockCollection>
            {
                new StockCollection { Ticker = "AAPL", Currency = "USD", NumberOfShares = 20 },
                new StockCollection { Ticker = "TSLA", Currency = "USD", NumberOfShares = 100 },
                new StockCollection { Ticker = "NVDA", Currency = "USD", NumberOfShares = 50 },
                new StockCollection { Ticker = "ALLEGRO", Currency = "USD", NumberOfShares = 19000 },
                new StockCollection { Ticker = "ADR", Currency = "USD", NumberOfShares = 1100 }
            }
        };

        public static readonly CurrencyViewModel currencyViewModel = new CurrencyViewModel
        {     
            Id = ObjectId.Parse("00227b375dff9218248edfd4"),
            Timestamp = 0,
            Quotes = CurrencyExchangeContext.quotes
        };

        public static readonly List<PortfolioViewModel> portfolioSampleList = new List<PortfolioViewModel> { portfolioSample };
        public static readonly StockModel stockModel = new StockModel 
        {
            Price = 1 ,
            Currency = ""
        };

        public UnitTest1()
        {

            var builder = WebApplication.CreateBuilder();
            var configuration = builder.Configuration;
            builder.Services.AddHttpClient("currencyExchangeApi", client =>
            {
                client.BaseAddress = new Uri($"{configuration["currencyExchangeUrl"]}live?access_key={configuration["currencyExchangeUrl"]}");
            });

            var serviceProvider = builder.Services.BuildServiceProvider();
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });

            _mapper = mapperConfig.CreateMapper();

            MongoDbSettings mongoDbSettings = new MongoDbSettings
            {
                PortfolioName = "portfolioDb",
                PortfolioConnectionString = new string[] { "Data","portfoliosMock.json"},
                PortfolioCollectionName = "Portfolios",
                CurrencyName = "currencyDb",
                CurrencyConnectionString = new string[] { "Data", "currenciesMock.json"},
                CurrencyCollectionName = "Currencies",
                PortfolioSaveString = new string[] { "" },
                CurrencySaveString = new string[] { "" }
            };
            IOptions<MongoDbSettings> optionsMongoDbSettings = Options.Create(mongoDbSettings);

            _portfolioRepositoryObj = new PortfolioRepository(optionsMongoDbSettings, _mapper);
            _portfolioServiceObj = new PortfolioService(_portfolioRepository.Object, _mapper, _stocksService.Object);
            _currencyServiceObj = new CurrencyService(_currencyRepository.Object, _mapper, httpClientFactory);
        }

        [Theory]
        [InlineData("50227b375dff9218248eadc4")]
        public async void CheckIfPortfolioServiceGetPortfolioByIdReturnsCorrectObject(string id)
        {
            //Arrange
            var objectId = ObjectId.Parse(id);
            _portfolioService.Setup(x => x.GetPortfolio(id)).Returns(portfolioSample);
            _portfolioRepository.Setup(x => x.GetPortfolioByIdThatIsNotSoftDeleted(objectId)).Returns(portfolioSample);

            //Act
            var result = _portfolioServiceObj.GetPortfolio(id);

            //Assert
            Assert.Equal(result.Id, objectId);
        }

        [Theory]
        [InlineData("00227b375dff9218248eadc4", false, 5, "AAPL")]
        [InlineData("00227b375dff9218248eadc6", false, 5, "ADR")]
        public async void CheckIfPortfolioRepositoryGetPortfolioByIdThatIsNotSoftDeletedReturnsCorrectObject(string id, bool isDeleted, int numberOfStocks, string firstTicker)
        {
            var objectId = ObjectId.Parse(id);
            _portfolioRepository.Setup(x => x.GetPortfolioByIdThatIsNotSoftDeleted(ObjectId.Parse(id))).Returns(portfolioSample);

            var result = _portfolioRepositoryObj.GetPortfolioByIdThatIsNotSoftDeleted(ObjectId.Parse(id));

            Assert.Equal(result.Id, objectId);
            Assert.Equal(result.IsDeleted, isDeleted);
            Assert.Equal(result.Stocks.Count(), numberOfStocks);
            Assert.Equal(result.Stocks.First().Ticker, firstTicker);
        }

        [Theory]
        [InlineData("00227b375dff9218248eadc4")]
        public async void CheckIfPortfolioServiceGetPortfolioThrowsErrorWhenIdDoNotExist(string id)
        {
            var objectId = ObjectId.Parse(id);
            _portfolioService.Setup(x => x.GetPortfolio(id)).Throws(new NullReferenceException(ExceptionCodes.portfolioDoesNotExist));
            _portfolioRepository.Setup(x => x.GetPortfolioByIdThatIsNotSoftDeleted(objectId)).Returns(portfolioSample);

            var result = _portfolioRepositoryObj.GetPortfolioByIdThatIsNotSoftDeleted(objectId);

            Assert.ThrowsAsync<NullReferenceException>(() => throw new NullReferenceException(ExceptionCodes.portfolioDoesNotExist));
        }

        [Theory]
        [InlineData(2)]
        public async void CheckIfMethodReturnsOnlyDocumnetsWithFieldIsDeletedAsFalse(int numberOfElements)
        {
            _portfolioRepository.Setup(x => x.GetAllPortfoliosThatAreNotSoftDeleted()).Returns(portfolioSampleList);
            var result = _portfolioRepositoryObj.GetAllPortfoliosThatAreNotSoftDeleted();
            Assert.Equal(result.Count(), numberOfElements);
        }


        [Theory]
        [InlineData("00227b375dff9218248eadc4", Currencies.Usd, 20270)]
        public async void checkIfTotalValueIsCorrectlyAdded(string portfolioId, string currency, int value)
        {
            var objectId = ObjectId.Parse(portfolioId);
            _stocksService.Setup(x => x.GetStockPrice(It.IsAny<string>())).ReturnsAsync(stockModel);
            _currencyRepository.Setup(x => x.GetAll()).Returns(currencyViewModel);
            _portfolioRepository.Setup(x => x.GetPortfolioByIdThatIsNotSoftDeleted(objectId)).Returns(portfolioSample);
            _portfolioService.Setup(x => x.GetPortfolio(portfolioId)).Returns(portfolioSample);
            _currencyService.Setup(x => x.GetCurrencyExchangeData()).Returns(currencyViewModel);
            var portfolioViewModelMock = _mapper.Map<PortfolioViewModel, PortfolioCollection>(portfolioSample);
            _portfolioService.Setup(x => x.GetTotalPortfolioValue(portfolioViewModelMock, currency, currencyViewModel)).ReturnsAsync(new decimal(1));


            var portfolio =  _portfolioServiceObj.GetPortfolio(portfolioId);
            var currencyData = _currencyServiceObj.GetCurrencyExchangeData();
            var portfolioViewModel = _mapper.Map<PortfolioViewModel, PortfolioCollection>(portfolio);
            var totalAmount =  await _portfolioServiceObj.GetTotalPortfolioValue(portfolioViewModel, currency, currencyData);

            Assert.Equal(totalAmount, value);
        }
    }
}