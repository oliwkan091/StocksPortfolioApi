
using AutoMapper;
using Domain.Collections;
using StocksPortfolio.Application.Interfaces.Collections;
using StocksPortfolio.Application.Interfaces.Models;

namespace StocksPortfolio.Application.MappingProfile
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
             CreateMap<CurrencyCollection, CurrencyExchangeResponseViewModel>()
                .ForMember(dest => dest.Success, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.Terms, opt => opt.Ignore()) 
                .ForMember(dest => dest.Privacy, opt => opt.Ignore()) 
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => Convert.ToInt32(src.Timestamp)))
                .ForMember(dest => dest.Source, opt => opt.Ignore()) 
                .ForMember(dest => dest.Quotes, opt => opt.MapFrom(src => src.Quotes.ToDictionary(q => q.type, q => (decimal)q.Value)));

            CreateMap<CurrencyExchangeResponseViewModel, CurrencyCollection>()
               .ForMember(dest => dest.Id, opt => opt.Ignore())
               .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp))
               .ForMember(dest => dest.Quotes, opt => opt.MapFrom(src =>
                DictionaryToList(src.Quotes)));

            CreateMap<CurrencyExchangeResponseViewModel, CurrencyViewModel>();

            CreateMap<CurrencyCollection, CurrencyViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => Convert.ToInt32(src.Timestamp)))
                .ForMember(dest => dest.Quotes, opt => opt.MapFrom(src => src.Quotes.ToDictionary(q => q.type, q => (decimal)q.Value)));

            CreateMap<PortfolioCollection, PortfolioViewModel>();
            CreateMap<PortfolioViewModel, PortfolioCollection>();
        }

        private List<Quote> DictionaryToList(Dictionary<string, decimal> dictionary)
    {
        var quotes = new List<Quote>();
        foreach (var item in dictionary)
        {
            quotes.Add(new Quote { type = item.Key, Value = (double)item.Value });
        }
        return quotes;
    }

    }
}
