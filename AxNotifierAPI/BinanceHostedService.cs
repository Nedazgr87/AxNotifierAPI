using Binance.Net.Clients;
using Binance.Net.Objects.Models.Futures.Socket;
using CryptoExchange.Net.Sockets;
using Microsoft.AspNetCore.SignalR;

namespace AxNotifierAPI
{
    public class BinanceHostedService : IHostedService
    {
        public BinanceSocketClient SocketClient;
        private IHubContext<MainHub> _hub;

        public BinanceHostedService(IHubContext<MainHub> hub)
        {
            _hub = hub;
            SocketClient = new BinanceSocketClient();
        }

        public async Task StartAsync(CancellationToken stoppingToken)
        {
            var res = await SocketClient.UsdFuturesStreams.SubscribeToMarkPriceUpdatesAsync("BTCUSDT", 1000, OnMessage, stoppingToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }


        private async void OnMessage(DataEvent<BinanceFuturesUsdtStreamMarkPrice> data)
        {
            await _hub.Clients.All.SendAsync("Method", new object[] { data.Data.Symbol, data.Data.MarkPrice });
        }

        public void Dispose()
        {

        }
    }
}
