﻿// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Options;
using TraderShop.Finacials.TdAmeritrade.PriceHistory.DependencyInjection;
using TraderShop.Finacials.TdAmeritrade.PriceHistory.Models;
using TraderShop.Finacials.TdAmeritrade.PriceHistory.Services;
using TraderShop.Financials.TdAmeritrade.Abstractions.DependencyInjection;
using TraderShop.Financials.TdAmeritrade.Abstractions.Options;
using TraderShop.Financials.TdAmeritrade.Abstractions.Services;
using TraderShop.Financials.TdAmeritrade.Accounts.DependencyInjection;
using TraderShop.Financials.TdAmeritrade.Accounts.Services;
using TraderShop.Financials.TdAmeritrade.Instruments.Services;
using TraderShop.Financials.TdAmeritrade.Symbols.DependencyInjection;

internal sealed class Program
{
    private static async Task Main(string[] args)
    {
        await Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<ConsoleHostService>();
                services.AddTdAmeritradeClient();
                services.Configure<TdAmeritradeOptions>(
                    hostContext.Configuration.GetSection("TdAmeritradeOptions")
                        );
                services.AddTdAmeritradeInstrumentProvider();
                services.AddTdAmeritradePriceHistoryProvider();
                services.AddTdAmeritradeAccountProvider();

            })
            .UseEnvironment("development")
            .RunConsoleAsync();
    }

    internal sealed class ConsoleHostService : IHostedService
    {
        private readonly ILogger _logger;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly ITdAmeritradeAuthService _tdClient;
        private readonly ITdAmeritradeInstrumentProvider _tdInstrumentProvider;
        private readonly ITdAmeritradePriceHistoryProvider _tdPriceHistoryProvider;
        private readonly ITdAmeritradeAccountProvider _tdAccountProvider;
        private readonly IOptionsMonitor<TdAmeritradeOptions> _tdOptions;


        public ConsoleHostService(
            ILogger<ConsoleHostService> logger,
            IOptionsMonitor<TdAmeritradeOptions> tdAmeritradeOptions,
            IHostApplicationLifetime appLifetime,
            ITdAmeritradeInstrumentProvider tdClient,
            ITdAmeritradePriceHistoryProvider tdPriceHistory,
            ITdAmeritradeAccountProvider tdAccountProvider,
            ITdAmeritradeAuthService TdAmeritradeAuthService)
        {
            _logger = logger;
            _tdOptions = tdAmeritradeOptions;
            _applicationLifetime = appLifetime;
            _tdClient = TdAmeritradeAuthService;
            _tdInstrumentProvider = tdClient;
            _tdPriceHistoryProvider = tdPriceHistory;
            _tdAccountProvider = tdAccountProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Starting with argumetns : {string.Join(" ", Environment.GetCommandLineArgs())}");

            _applicationLifetime.ApplicationStarted.Register(() =>
            {
                Task.Run(async () =>
                {
                    try
                    {



                        var instruments = await _tdInstrumentProvider.GetInstruments();

                        _logger.LogCritical($"{_tdOptions.CurrentValue.access_token}");

                        _logger.LogInformation($"{instruments.Count}");

                        var futures = await _tdInstrumentProvider.GetAllFuturesInstruments();

                        _logger.LogInformation($"{futures.Count}");

                        var priceHistory = await _tdPriceHistoryProvider.GetPriceHistory(new PriceHistorySpecs());

                        _logger.LogInformation($"{priceHistory[0].Volume.ToString()}");

                        var account = await _tdAccountProvider.GetAccount(***REMOVED***, new string[] { "positions", "orders" });

                        _logger.LogInformation($"{account.CurrentBalances.LiquidationValue}");

                        var accounts = await _tdAccountProvider.GetAccounts(new string[] { "positions", "orders" });

                        _logger.LogInformation($"{accounts.Length}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unhandled Exception!");
                    }
                    finally
                    {
                        _applicationLifetime.StopApplication();
                    }
                });
            });

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }


}
