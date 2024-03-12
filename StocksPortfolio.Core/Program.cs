using AutoMapper;
using MongoDB.Bson.Serialization;
using StocksPortfolio.Application.Interfaces.Collections;
using StocksPortfolio.Application.Interfaces.Interfaces;
using StocksPortfolio.Application.Interfaces.Models;
using StocksPortfolio.Application.MappingProfile;
using StocksPortfolio.Application.Services;
using StocksPortfolio.Infrastructure.Repositories;
using StocksPortfolio.Stocks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDB"));

builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddScoped<IStocksService, StocksService>();

builder.Services.AddSingleton<IPortfolioRepository, PortfolioRepository>();
builder.Services.AddSingleton<ICurrencyRepository, CurrencyRepository>();

var configuration = builder.Configuration;

builder.Services.AddHttpClient("currencyExchangeApi", client =>
{
    client.BaseAddress = new Uri($"{configuration["currencyExchangeUrl"]}live?access_key={configuration["apiAccessKey"]}"); 
});

BsonClassMap.RegisterClassMap<CurrencyCollection>(cm =>
{
    cm.AutoMap();
    cm.SetIgnoreExtraElements(true);
});

var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new MappingProfile());
});

IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
