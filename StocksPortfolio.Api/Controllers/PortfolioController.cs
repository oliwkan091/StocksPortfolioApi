using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using StocksPortfolio.Application.Interfaces.Collections;
using StocksPortfolio.Application.Interfaces.Enums;
using StocksPortfolio.Application.Interfaces.Interfaces;

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
        public async Task<IActionResult> GetTotalPortfolioValue(string portfolioId, string currency = Currencies.Usd)
        {
            var portfolio = _portfolioService.GetPortfolio(portfolioId);
            await _currencyService.UpdateCurrentCurrencyExchangeData();
            var currencyData = _currencyService.GetCurrencyExchangeData();
            var portfolioViewModel = _mapper.Map<PortfolioViewModel, PortfolioCollection>(portfolio);
            var totalAmount = _portfolioService.GetTotalPortfolioValue(portfolioViewModel, currency, currencyData);
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
