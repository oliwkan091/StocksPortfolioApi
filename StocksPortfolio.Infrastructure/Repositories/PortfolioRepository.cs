using AutoMapper;
using Microsoft.Extensions.Options;
using Mongo2Go;
using MongoDB.Bson;
using MongoDB.Driver;
using StocksPortfolio.Application.Interfaces.Collections;
using StocksPortfolio.Application.Interfaces.Enums;
using StocksPortfolio.Application.Interfaces.Interfaces;
using StocksPortfolio.Application.Interfaces.Models;

namespace StocksPortfolio.Infrastructure.Repositories
{
    public class PortfolioRepository : IPortfolioRepository
    {
        private readonly MongoDbRunner _runner;
        private IMongoCollection<PortfolioCollection> _portfolioCollection;
        private readonly IOptions<MongoDbSettings> _mongoDbSettings;
        private readonly IMapper _mapper;

        public PortfolioRepository(
            IOptions<MongoDbSettings> mongoDbSettings,
            IMapper mapper
            )
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
            var portfolioCollection = _portfolioCollection.Find(portfolio => portfolio.Id == objectId && portfolio.IsDeleted == false).SingleOrDefault();
            return _mapper.Map<PortfolioCollection, PortfolioViewModel>(portfolioCollection);
        }

        public List<PortfolioViewModel> GetAllPortfoliosThatAreNotSoftDeleted()
        {
            var portfolioCollectionList = _portfolioCollection.Find(portfolio => portfolio.IsDeleted == false).ToList();
            var a = _mapper.Map<List<PortfolioCollection>, List<PortfolioViewModel>>(portfolioCollectionList);
            return a;
        }

        public void SoftDeletePortfolio(ObjectId id)
        {
            if (GetPortfolioByIdThatIsNotSoftDeleted(id) == null)
            {
                throw new NullReferenceException(ExceptionCodes.portfolioDoesNotExist);
            }

            var filter1 = Builders<PortfolioCollection>.Filter.Eq(doc => doc.Id, id);
            var filter2 = Builders<PortfolioCollection>.Filter.Eq(doc => doc.IsDeleted, false);
            var filterCombined = Builders<PortfolioCollection>.Filter.And(filter1, filter2);
            var update = Builders<PortfolioCollection>.Update.Set(portfolio => portfolio.IsDeleted, true);
            var result = _portfolioCollection.UpdateOne(filterCombined, update);

            if (result.ModifiedCount != 1)
                throw new Exception(ExceptionCodes.errorWhileDeletingPortfolio);

            Save();
        }

        private void Save()
        {
            string rootProjectDirectoryPath = Directory.GetParent(Environment.CurrentDirectory).FullName;
            var absolutePathToFile = Path.Combine(rootProjectDirectoryPath, Path.Combine(_mongoDbSettings.Value.PortfolioSaveString));
            _runner.Export(_mongoDbSettings.Value.PortfolioName, _mongoDbSettings.Value.PortfolioCollectionName, absolutePathToFile);
        }
    }
}
