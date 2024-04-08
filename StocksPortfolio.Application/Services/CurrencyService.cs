using AutoMapper;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using StocksPortfolio.Application.Interfaces.Collections;
using StocksPortfolio.Application.Interfaces.Enums;
using StocksPortfolio.Application.Interfaces.Interfaces;
using StocksPortfolio.Application.Interfaces.Models;
using System.Text.Json;

namespace StocksPortfolio.Application.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly ICurrencyRepository _currencyRepository;
        private readonly IMapper _mapper;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public CurrencyService(
            ICurrencyRepository currencyRepository,
            IMapper mapper,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration
            )
        {
            _currencyRepository = currencyRepository ?? throw new NullReferenceException(nameof(currencyRepository));
            _mapper = mapper ?? throw new NullReferenceException(nameof(mapper));
            _httpClient = httpClientFactory.CreateClient("currencyExchangeApi");
            _configuration = configuration ?? throw new NullReferenceException(nameof(configuration));
        }

        public async Task<bool> UpdateCurrentCurrencyExchangeData()
        {
            bool wasUpdated = false;
            var currenciesData = _currencyRepository.GetAll();
            if (currenciesData == null || DateTimeOffset.FromUnixTimeSeconds(currenciesData.Timestamp).LocalDateTime < DateTime.Now.AddDays(-1))
            {
                var url = $"live?access_key={_configuration["apiAccessKey"]}";
                var response = await _httpClient.GetAsync(url);

                var data = JsonSerializer.DeserializeAsync<CurrencyExchangeResponseViewModel>(response.Content.ReadAsStream()).Result;

                if (data != null && data.Quotes != null)
                {
                    wasUpdated = true;
                    data.Timestamp = (int)(((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds());
                    var dataToUpdate = _mapper.Map<CurrencyExchangeResponseViewModel, CurrencyCollection>(data);
                    dataToUpdate.Id = currenciesData != null ? currenciesData.Id : ObjectId.GenerateNewId();
                    _currencyRepository.Update(dataToUpdate);
                }
            }

            return wasUpdated;
        }
        public CurrencyViewModel GetCurrencyExchangeData()
        {
            return _currencyRepository.GetAll();
        }
    }
}
