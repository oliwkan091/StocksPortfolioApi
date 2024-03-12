using MongoDB.Bson.Serialization.Attributes;

namespace StocksPortfolio.Application.Interfaces.Collections
{
    public class StockCollection
    {
        [BsonElement("ticker")]
        public string Ticker { get; set; }

        [BsonElement("currency")]
        public string Currency { get; set; }

        [BsonElement("numberOfShares")]
        public int NumberOfShares { get; set; }
    }
}
