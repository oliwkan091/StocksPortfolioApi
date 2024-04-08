using StocksPortfolio.Application.Interfaces.Collections;
using StocksPortfolio.Application.Interfaces.Models;

namespace Domain.Repositories
{
    public interface ICurrencyRepository
    {
        CurrencyViewModel GetAll();
        void Update(CurrencyCollection collectionToUpdate);
    }
}
