using MongoDB.Bson;

namespace StocksPortfolio.Application.Interfaces.Models
{
    public class CurrencyViewModel
    {
        public ObjectId Id { get; set; }
        public int Timestamp { get; set; }
        public Dictionary<string, decimal> Quotes { get; set; }
    }
}
