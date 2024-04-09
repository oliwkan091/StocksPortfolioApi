using AutoMapper;
using Domain;
using Domain.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using StocksPortfolio.Application.Interfaces.Collections;
using StocksPortfolio.Application.Interfaces.Enums;
using StocksPortfolio.Application.Interfaces.Interfaces;
using StocksPortfolio.Application.Interfaces.Models;
using StocksPortfolio.Application.MappingProfile;
using StocksPortfolio.Application.Services;
using StocksPortfolio.Infrastructure.Repositories;
using StocksPortfolio.Stocks;

namespace Tests
{
    public class Tests
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

        public Tests()
        {
            var builder = WebApplication.CreateBuilder();
            var configuration = builder.Configuration;
            builder.Services.AddHttpClient("currencyExchangeApi", client =>
            {
                client.BaseAddress = new Uri(configuration["currencyExchangeUrl"]);
            });

            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });

            _mapper = mapperConfig.CreateMapper();

            builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDB"));

            var app = builder.Build();
            var serviceProvider = app.Services;
            var mongoDbSettings = serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            IOptions<MongoDbSettings> optionsMongoDbSettings = Options.Create(mongoDbSettings);
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

            _portfolioRepositoryObj = new PortfolioRepository(optionsMongoDbSettings, _mapper);
            _currencyRepositoryObj = new CurrencyRepository(optionsMongoDbSettings, _mapper);
            _portfolioServiceObj = new PortfolioService(_portfolioRepository.Object, _mapper, _stocksService.Object);
            _currencyServiceObj = new CurrencyService(_currencyRepository.Object, _mapper, httpClientFactory, configuration);
            _stocksServiceObj = new StocksService();
        }

        [Theory]
        [InlineData("00227b375dff9218248eadc4")]
        public async void CheckIfPortfolioServiceGetPortfolioByIdThatIsNotSoftDeleteReturnsCorrectObject(string portfolioId)
        {
            //Arrange
            var portfolioSample = _portfolioRepositoryObj.GetPortfolioByIdThatIsNotSoftDeleted(ObjectId.Parse(portfolioId));
            var objectId = ObjectId.Parse(portfolioId);
            _portfolioService.Setup(x => x.GetPortfolio(portfolioId)).Returns(portfolioSample);
            _portfolioRepository.Setup(x => x.GetPortfolioByIdThatIsNotSoftDeleted(objectId)).Returns(portfolioSample);

            //Act
            var result = _portfolioServiceObj.GetPortfolio(portfolioId);

            //Assert
            Assert.Equal(result.Id, objectId);
        }

        [Theory]
        [InlineData("00227b375dff9218248eadc4", false, 5, "AAPL")]
        [InlineData("00227b375dff9218248eadc6", false, 5, "ADR")]
        public async void CheckIfPortfolioRepositoryGetPortfolioByIdThatIsNotSoftDeletedReturnsCorrectObject(string portfolioId, bool isDeleted, int numberOfStocks, string firstTicker)
        {
            //Arrange
            var objectId = ObjectId.Parse(portfolioId);
            var portfolioSample = _portfolioRepositoryObj.GetPortfolioByIdThatIsNotSoftDeleted(ObjectId.Parse(portfolioId));
            _portfolioRepository.Setup(x => x.GetPortfolioByIdThatIsNotSoftDeleted(ObjectId.Parse(portfolioId))).Returns(portfolioSample);

            //Act
            var result = _portfolioRepositoryObj.GetPortfolioByIdThatIsNotSoftDeleted(ObjectId.Parse(portfolioId));

            //Assert
            Assert.Equal(result.Id, objectId);
            Assert.Equal(result.IsDeleted, isDeleted);
            Assert.Equal(result.Stocks.Count(), numberOfStocks);
            Assert.Equal(result.Stocks.First().Ticker, firstTicker);
        }

        [Theory]
        [InlineData("00227b375dff9218248eadc4")]
        public async void CheckIfPortfolioServiceGetPortfolioThrowsErrorWhenIdDoNotExist(string portfolioId)
        {
            //Arrange
            var objectId = ObjectId.Parse(portfolioId);
            _portfolioService.Setup(x => x.GetPortfolio(portfolioId)).Throws(new NullReferenceException(ExceptionCodes.portfolioDoesNotExist));
            var portfolioSample = _portfolioRepositoryObj.GetPortfolioByIdThatIsNotSoftDeleted(ObjectId.Parse(portfolioId));
            _portfolioRepository.Setup(x => x.GetPortfolioByIdThatIsNotSoftDeleted(objectId)).Returns(portfolioSample);

            //Act
            var result = _portfolioRepositoryObj.GetPortfolioByIdThatIsNotSoftDeleted(objectId);

            //Assert
            Assert.ThrowsAsync<NullReferenceException>(() => throw new NullReferenceException(ExceptionCodes.portfolioDoesNotExist));
        }

        [Theory]
        [InlineData(2)]
        public async void CheckIfMethodReturnsOnlyDocumnetsWithFieldIsDeletedAsFalse(int numberOfElements)
        {
            //Arrange
            var portfolioSampleList = _portfolioRepositoryObj.GetAllPortfoliosThatAreNotSoftDeleted();
            _portfolioRepository.Setup(x => x.GetAllPortfoliosThatAreNotSoftDeleted()).Returns(portfolioSampleList);
            //Act
            var result = _portfolioRepositoryObj.GetAllPortfoliosThatAreNotSoftDeleted();
            //Assert
            Assert.Equal(result.Count(), numberOfElements);
        }


        [Theory]
        [InlineData("00227b375dff9218248eadc4", Currencies.Usd, 20270, "")]
        public async void CheckIfTotalValueIsCorrectlyAdded(string portfolioId, string currency, int value, string ticker)
        {
            //Arrange
            var objectId = ObjectId.Parse(portfolioId);
            var stockPrice = await _stocksServiceObj.GetStockPrice(ticker);
            _stocksService.Setup(x => x.GetStockPrice(It.IsAny<string>())).ReturnsAsync(stockPrice);
            var currencyViewModel = _currencyRepositoryObj.GetAll();
            _currencyRepository.Setup(x => x.GetAll()).Returns(currencyViewModel);
            var portfolioSample = _portfolioRepositoryObj.GetPortfolioByIdThatIsNotSoftDeleted(ObjectId.Parse(portfolioId));
            _portfolioRepository.Setup(x => x.GetPortfolioByIdThatIsNotSoftDeleted(objectId)).Returns(portfolioSample);
            _portfolioService.Setup(x => x.GetPortfolio(portfolioId)).Returns(portfolioSample);
            _currencyService.Setup(x => x.GetCurrencyExchangeData()).Returns(currencyViewModel);
            var portfolioViewModelMock = _mapper.Map<PortfolioViewModel, PortfolioCollection>(portfolioSample);
            _portfolioService.Setup(x => x.GetTotalPortfolioValue(portfolioViewModelMock, currency, currencyViewModel)).ReturnsAsync(new decimal(1));

            //Act
            var portfolio = _portfolioServiceObj.GetPortfolio(portfolioId);
            var currencyData = _currencyServiceObj.GetCurrencyExchangeData();
            var portfolioViewModel = _mapper.Map<PortfolioViewModel, PortfolioCollection>(portfolio);
            var totalAmount = await _portfolioServiceObj.GetTotalPortfolioValue(portfolioViewModel, currency, currencyData);

            //Assert
            Assert.Equal(totalAmount, value * stockPrice.Price);
        }
    }
}