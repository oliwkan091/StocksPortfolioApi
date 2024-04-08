using AutoMapper;
using Domain;
using Domain.Collections;
using Domain.Repositories;
using Microsoft.Extensions.Options;
using Mongo2Go;
using MongoDB.Driver;
using StocksPortfolio.Application.Interfaces.Collections;
using StocksPortfolio.Application.Interfaces.Enums;
using StocksPortfolio.Application.Interfaces.Models;

namespace StocksPortfolio.Infrastructure.Repositories
{
    public class CurrencyRepository : ICurrencyRepository
    {
        private readonly IMongoCollection<CurrencyCollection> _currencyCollection;
        private readonly IOptions<MongoDbSettings> _mongoDbSettings;
        private MongoDbRunner _runner;
        private readonly IMapper _mapper;

        public CurrencyRepository(
            IOptions<MongoDbSettings> mongoDbSettings,
            IMapper mapper
            )
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
            var currencyCollection = _currencyCollection.Find(_ => true).Single();
            return _mapper.Map<CurrencyCollection, CurrencyViewModel>(currencyCollection);
        }

        public void Update(CurrencyCollection collectionToUpdate)
        {
            var filter = Builders<CurrencyCollection>.Filter.Eq(doc => doc.Id, collectionToUpdate.Id);
            var updateTimestamp = Builders<CurrencyCollection>.Update.Set(doc => doc.Timestamp, collectionToUpdate.Timestamp);
            long numberOfChanges;
            numberOfChanges = _currencyCollection.UpdateOne(filter, updateTimestamp).ModifiedCount;

            foreach (var quoteToUpdate in collectionToUpdate.Quotes)
            {
                var updateQuote = Builders<CurrencyCollection>.Update.Set(doc => doc.Quotes[-1].Value, quoteToUpdate.Value);
                var filterQuote = Builders<CurrencyCollection>.Filter.And(
                    Builders<CurrencyCollection>.Filter.Eq(doc => doc.Id, collectionToUpdate.Id),
                    Builders<CurrencyCollection>.Filter.ElemMatch(doc => doc.Quotes,
                    Builders<Quote>.Filter.Eq(q => q.type, quoteToUpdate.type))
                );

                numberOfChanges += _currencyCollection.UpdateOne(filterQuote, updateQuote).ModifiedCount;
            }

            if (numberOfChanges < 1)
                throw new Exception(ExceptionCodes.errorWhileUpdatingCurrenciesExchange);

            Save();
        }

        private void Save()
        {

            string rootProjectDirectoryPath = Directory.GetParent(Environment.CurrentDirectory).FullName;
            var absolutePathToFile = Path.Combine(rootProjectDirectoryPath, Path.Combine(_mongoDbSettings.Value.CurrencySaveString));
            _runner.Export(_mongoDbSettings.Value.CurrencyName, _mongoDbSettings.Value.CurrencyCollectionName, absolutePathToFile);
        }
    }
}
