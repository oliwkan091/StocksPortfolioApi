using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
