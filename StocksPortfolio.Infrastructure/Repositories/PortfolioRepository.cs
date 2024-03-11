using AutoMapper;
using Microsoft.Extensions.Options;
using Mongo2Go;
using MongoDB.Bson;
using MongoDB.Driver;
using StocksPortfolio.Application.Interfaces.Collections;
using StocksPortfolio.Application.Interfaces.Enums;
using StocksPortfolio.Application.Interfaces.Interfaces;
using StocksPortfolio.Application.Interfaces.Models;
using System.Reflection;

namespace StocksPortfolio.Infrastructure.Services
{
    public class PortfolioRepository: IPortfolioRepository
    {
        private readonly MongoDbRunner _runner;
        private IMongoCollection<PortfolioCollection> _portfolioCollection;
        private readonly IOptions<MongoDbSettings> _mongoDbSettings;
        private readonly IMapper _mapper;

        public PortfolioRepository(IOptions<MongoDbSettings> mongoDbSettings, IMapper mapper)
        {
            _mongoDbSettings = mongoDbSettings ?? throw new NullReferenceException(nameof(mongoDbSettings));
            _mapper = mapper ?? throw new NullReferenceException(nameof(mapper));

            _runner = MongoDbRunner.Start();
            _runner.Import(_mongoDbSettings.Value.PortfolioName, _mongoDbSettings.Value.PortfolioCollectionName, Path.Combine(_mongoDbSettings.Value.PortfolioConnectionString), true);
            var client = new MongoClient(_runner.ConnectionString);
            var db = client.GetDatabase(_mongoDbSettings.Value.PortfolioName); ;
            _portfolioCollection = db.GetCollection<PortfolioCollection>(_mongoDbSettings.Value.PortfolioCollectionName);
        }

        public PortfolioViewModel GetPortfolioByIdThatIsNotSoftDeleted(ObjectId objectId)
        {
            var portfolioCollection = _portfolioCollection.Find(portfolio => (portfolio.Id == objectId && portfolio.IsDeleted == false)).SingleOrDefault();
            return _mapper.Map<PortfolioCollection, PortfolioViewModel>(portfolioCollection);
        }
    }
}
