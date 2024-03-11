using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using StocksPortfolio.Application.Interfaces.Collections;
using StocksPortfolio.Application.Interfaces.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StocksPortfolio.Application.Interfaces.Interfaces
{
    public interface ICurrencyRepository
    {
        CurrencyViewModel GetAll();
        void Update(CurrencyCollection collectionToUpdate);
    }
}
