using Scalar.AspNetCore;
using Stocks.Hub;
using Stocks.Services;
using Stocks.Stocks;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors();
builder.Services.AddMemoryCache();  
builder.Services.AddOpenApi();
builder.Services.AddSignalR();

builder.Services.AddSingleton<ISqlDatasource>(serviceProvider =>
{
    var connectionString = builder.Configuration["STOCKS_CONNECTION_STRING"];

    return new SqlDatasource(connectionString);
});

builder.Services.AddHostedService<DatabaseInitializer>();

builder.Services.AddHttpClient<StocksClient>(httpClient =>
{
    httpClient.BaseAddress = new Uri(builder.Configuration["Stocks:ApiUrl"]!);
});

builder.Services.AddScoped<StockService>();
builder.Services.AddSingleton<ActiveTickerManager>();
builder.Services.AddHostedService<StocksFeedUpdater>();

builder.Services.Configure<StockUpdateOptions>(builder.Configuration.GetSection("StockUpdateOptions"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseCors(policy => policy
    .WithOrigins("http://127.0.0.1:5500")
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());
}

app.MapGet("/api/stocks/{ticker}", async (string ticker, StockService stockService) =>
{
    var result = await stockService.GetLatestStockPrice(ticker);
    return result == null ? Results.NotFound("No Data") : Results.Ok(result);
})
.WithName("GetLatestStockPrice")
.WithOpenApi();

app.MapHub<StocksFeedHub>("/stocks-feed");

app.UseHttpsRedirection();

app.Run();