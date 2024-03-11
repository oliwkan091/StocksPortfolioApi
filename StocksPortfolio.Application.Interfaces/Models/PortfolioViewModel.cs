using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StocksPortfolio.Application.Interfaces.Collections
{
    public class PortfolioViewModel
    {
        public ObjectId Id { get; set; }
        public float CurrentTotalValue { get; set; }
        public bool IsDeleted { get; set; }
        public ICollection<StockCollection> Stocks { get; set; }
    }
}
