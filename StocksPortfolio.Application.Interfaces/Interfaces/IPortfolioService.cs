using StocksPortfolio.Application.Interfaces.Collections;
using StocksPortfolio.Application.Interfaces.Models;

namespace StocksPortfolio.Application.Interfaces.Interfaces
{
    public interface IPortfolioService
    {
        PortfolioViewModel GetPortfolio(string portfolioId);
        Task<decimal> GetTotalPortfolioValue(PortfolioCollection portfolio, string currency, CurrencyViewModel currencyData);
        void SoftDeleteOnePortfolio(string portfolio);
    }
}
