using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StocksPortfolio.Application.Interfaces.Collections
{
    public class Quote
    {
        [BsonElement("type")]
        public string type { get; set; }

        [BsonElement("value")]
        public double Value { get; set; }
    }
}
