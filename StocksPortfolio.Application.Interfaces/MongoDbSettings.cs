namespace Domain
{
    public class MongoDbSettings
    {
        public string PortfolioName { get; set; }
        public string[] PortfolioConnectionString { get; set; }
        public string PortfolioCollectionName { get; set; }

        public string CurrencyName { get; set; }
        public string[] CurrencyConnectionString { get; set; }
        public string CurrencyCollectionName { get; set; }

        public string[] PortfolioSaveString { get; set; }
        public string[] CurrencySaveString { get; set; }
    }
}
