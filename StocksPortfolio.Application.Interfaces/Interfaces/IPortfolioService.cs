using StocksPortfolio.Application.Interfaces.Collections;
using StocksPortfolio.Application.Interfaces.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StocksPortfolio.Application.Interfaces.Interfaces
{
    public interface IPortfolioService
    {
        PortfolioViewModel GetPortfolio(string portfolioId);
        decimal GetTotalPortfolioValue(PortfolioCollection portfolio, string currency, CurrencyViewModel currencyData);
    }
}
