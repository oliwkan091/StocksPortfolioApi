using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Collections
{
    public class Quote
    {
        [BsonElement("type")]
        public string type { get; set; }

        [BsonElement("value")]
        public double Value { get; set; }
    }
}
