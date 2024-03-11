using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
