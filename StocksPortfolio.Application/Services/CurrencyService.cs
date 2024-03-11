using AutoMapper;
using MongoDB.Bson;
using StocksPortfolio.Application.Interfaces.Collections;
using StocksPortfolio.Application.Interfaces.Enums;
using StocksPortfolio.Application.Interfaces.Interfaces;
using StocksPortfolio.Application.Interfaces.Models;
using StocksPortfolio.Stocks;
using System.Data;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace StocksPortfolio.Application.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly ICurrencyRepository _currencyRepository;
        private readonly IMapper _mapper;
        private readonly HttpClient _httpClient;

        public CurrencyService(
            ICurrencyRepository currencyRepository,
            IMapper mapper,
            IHttpClientFactory httpClientFactory
            )
        {
            _currencyRepository = currencyRepository ?? throw new NullReferenceException(nameof(currencyRepository));
            _mapper = mapper ?? throw new NullReferenceException(nameof(mapper));
            _httpClient = httpClientFactory.CreateClient("currencyExchangeApi");
        }

        public async Task<bool> UpdateCurrentCurrencyExchangeData()
        {
            bool wasUpdated = false;
            var currenciesData = _currencyRepository.GetAll();
            if (currenciesData == null || DateTimeOffset.FromUnixTimeSeconds(currenciesData.Timestamp).LocalDateTime < DateTime.Now.AddDays(-1))
            {
                var response = await _httpClient.GetAsync("");
                // Docs: https://currencylayer.com/documentation
                //There was problem to use api so in case it do not work below workaround is being used
                //{
                //    "success": false,
                //    "error": {
                //    "code": 104,
                //    "info": "Your monthly usage limit has been reached. Please upgrade your Subscription Plan."
                //    }
                //}

                var data = JsonSerializer.DeserializeAsync<CurrencyExchangeResponseViewModel>(response.Content.ReadAsStream()).Result;

                if(data?.Quotes == null)
                {
                    wasUpdated = false;
                    data = new CurrencyExchangeResponseViewModel()
                    {
                        Success = true,
                        Terms = "Your terms here",
                        Privacy = "Your privacy policy here",
                        Timestamp = 0,
                        Source = "Your source here",
                        Quotes = CurrencyExchangeContext.quotes
                    };
                }else
                {
                    wasUpdated = true;
                }

                if (data == null || data.Quotes == null)
                    throw new NullReferenceException(ExceptionCodes.currencyDoesNotExist);

                data.Timestamp = (int)(((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds());
                var dataToUpdate = _mapper.Map<CurrencyExchangeResponseViewModel, CurrencyCollection>(data);
                dataToUpdate.Id = currenciesData != null ? currenciesData.Id : ObjectId.GenerateNewId(); 
                _currencyRepository.Update(dataToUpdate);
            }

            return wasUpdated;
        }
        public CurrencyViewModel GetCurrencyExchangeData()
        {
            return _currencyRepository.GetAll();
        }
    }
}
