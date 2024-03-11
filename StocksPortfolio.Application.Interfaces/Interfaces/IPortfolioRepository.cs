using MongoDB.Bson;
using StocksPortfolio.Application.Interfaces.Collections;

namespace StocksPortfolio.Application.Interfaces.Interfaces
{
    public interface IPortfolioRepository
    {
        PortfolioViewModel GetPortfolioByIdThatIsNotSoftDeleted(ObjectId objectId);
        List<PortfolioViewModel> GetAllPortfoliosThatAreNotSoftDeleted();
        void SoftDeletePortfolio(ObjectId id);
    }
}
