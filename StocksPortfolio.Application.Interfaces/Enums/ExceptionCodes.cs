namespace StocksPortfolio.Application.Interfaces.Enums
{
    public static class ExceptionCodes
    {
        public const string portfolioDoesNotExist = "Portfolio does not exist";
        public const string currencyDoesNotExist = "Currency does not exist";
        public const string currencyNotSupported = "Currency not supported";
        public const string notAValid24String = "Is not a valid 24 digit string";
        public const string errorWhileDeletingPortfolio = "Error while deleting portfolio";
        public const string errorWhileUpdatingCurrenciesExchange = "Error while updating currencies exchange";
    }
}
