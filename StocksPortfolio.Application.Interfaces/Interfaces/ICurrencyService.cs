using StocksPortfolio.Application.Interfaces.Models;

namespace StocksPortfolio.Application.Interfaces.Interfaces
{
    public interface ICurrencyService
    {
        CurrencyViewModel GetCurrencyExchangeData();
        Task<bool> UpdateCurrentCurrencyExchangeData();
    }
}