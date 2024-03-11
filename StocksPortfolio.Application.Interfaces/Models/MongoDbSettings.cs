using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StocksPortfolio.Application.Interfaces.Models
{
    public class MongoDbSettings
    {
        public string PortfolioName { get; set; }
        public string[] PortfolioConnectionString { get; set; }
        public string PortfolioCollectionName { get; set; }

        public string CurrencyName { get; set; }
        public string[] CurrencyConnectionString { get; set; }
        public string CurrencyCollectionName { get; set; }

        public string[] PortfolioSaveString { get; set; }
        public string[] CurrencySaveString { get; set; }
    }
}
