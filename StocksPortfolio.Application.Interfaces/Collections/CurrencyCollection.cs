using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using StocksPortfolio.Application.Interfaces.Models;

namespace StocksPortfolio.Application.Interfaces.Collections
{
    public class CurrencyCollection
    {
        [BsonElement("id")]
        public ObjectId Id { get; set; }

        [BsonElement("timestamp")]
        public int Timestamp { get; set; }

        [BsonElement("quotes")]
        public List<Quote> Quotes { get; set; }
    }
}
