using AutoMapper;
using Microsoft.Extensions.Options;
using Mongo2Go;
using MongoDB.Bson;
using MongoDB.Driver;
using StocksPortfolio.Application.Interfaces.Collections;
using StocksPortfolio.Application.Interfaces.Enums;
using StocksPortfolio.Application.Interfaces.Interfaces;
using StocksPortfolio.Application.Interfaces.Models;
using System.IO;
using System.Reflection.Metadata;
using static System.Net.WebRequestMethods;

namespace StocksPortfolio.Infrastructure.Services
{
    public class CurrencyRepository: ICurrencyRepository
    {
        private readonly IMongoCollection<CurrencyCollection> _currencyCollection;
        private readonly IOptions<MongoDbSettings> _mongoDbSettings;
        private MongoDbRunner _runner;
        private readonly IMapper _mapper;

        public CurrencyRepository(IOptions<MongoDbSettings> mongoDbSettings, IMapper mapper)
        {
            _mongoDbSettings = mongoDbSettings ?? throw new NullReferenceException(nameof(_mongoDbSettings));
            _mapper = mapper ?? throw new NullReferenceException(nameof(mapper));

            _runner = MongoDbRunner.Start();
            _runner.Import(mongoDbSettings.Value.CurrencyName, mongoDbSettings.Value.CurrencyCollectionName, Path.Combine(mongoDbSettings.Value.CurrencyConnectionString), true);
            var client = new MongoClient(_runner.ConnectionString);

            var db = client.GetDatabase(mongoDbSettings.Value.CurrencyName); ;

            _currencyCollection = db.GetCollection<CurrencyCollection>(
                mongoDbSettings.Value.CurrencyCollectionName);
        }

        public CurrencyViewModel GetAll()
        {
            var currencyCollection =  _currencyCollection.Find(_ => true).Single();
            return _mapper.Map<CurrencyCollection, CurrencyViewModel>(currencyCollection);
        }

    }
}
