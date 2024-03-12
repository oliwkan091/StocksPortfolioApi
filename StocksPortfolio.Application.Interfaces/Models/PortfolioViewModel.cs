using MongoDB.Bson;
using StocksPortfolio.Application.Interfaces.Collections;

namespace StocksPortfolio.Application.Interfaces.Models
{
    public class PortfolioViewModel
    {
        public ObjectId Id { get; set; }
        public float CurrentTotalValue { get; set; }
        public bool IsDeleted { get; set; }
        public ICollection<StockCollection> Stocks { get; set; }
    }
}
