using StocksPortfolio.Application.Interfaces.Collections;
using StocksPortfolio.Application.Interfaces.Models;

namespace StocksPortfolio.Application.Interfaces.Interfaces
{
    public interface ICurrencyRepository
    {
        CurrencyViewModel GetAll();
        void Update(CurrencyCollection collectionToUpdate);
    }
}
