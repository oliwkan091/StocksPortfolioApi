using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StocksPortfolio.Application.Interfaces.Collections
{
    public class PortfolioCollection
    {
        [BsonElement("id")]
        public ObjectId Id { get; set; }

        [BsonElement("totalValue")]
        public float CurrentTotalValue { get; set; }
        [BsonElement("isDeleted")]
        public bool IsDeleted { get; set; }

        [BsonElement("stocks")]
        public ICollection<StockCollection> Stocks { get; set; }
    }
}
