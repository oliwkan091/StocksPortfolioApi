using MongoDB.Bson;
using StocksPortfolio.Application.Interfaces.Models;

namespace Domain.Repositories
{
    public interface IPortfolioRepository
    {
        PortfolioViewModel GetPortfolioByIdThatIsNotSoftDeleted(ObjectId objectId);
        List<PortfolioViewModel> GetAllPortfoliosThatAreNotSoftDeleted();
        void SoftDeletePortfolio(ObjectId id);
    }
}
