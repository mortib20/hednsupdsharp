using Microsoft.Extensions.Options;
using System.Buffers.Text;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Unicode;

namespace hednsupd
{
    public class UpdateWorker : BackgroundService
    {
        private readonly ILogger<UpdateWorker> _logger;
        private readonly UpdateOptions _options;

        public UpdateWorker(ILogger<UpdateWorker> logger, IOptions<UpdateOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                UpdateAsync();
                await Task.Delay(TimeSpan.FromSeconds(_options.Delay), stoppingToken);
            }
        }

        private async void UpdateAsync()
        {
            var handler = new SocketsHttpHandler()
            {
                ConnectCallback = async (context, stoppingToken) =>
                {
                    var dnsEntries = await Dns.GetHostEntryAsync(context.DnsEndPoint.Host, AddressFamily.InterNetwork, stoppingToken);

                    var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                    await socket.ConnectAsync(dnsEntries.AddressList, context.DnsEndPoint.Port, stoppingToken);

                    return new NetworkStream(socket, ownsSocket: true);
                }
            };
            using var http = new HttpClient(handler);

            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.Hostname}:{_options.Key}"));
            _logger.LogInformation(auth);
            http.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Basic {auth}");

            var res = await http.GetAsync($"https://dyn.dns.he.net/nic/update?hostname={_options.Hostname}");

            if (res.IsSuccessStatusCode)
            {
                _logger.LogInformation("Updated DNS entry");
                _logger.LogInformation($"Next update at {DateTime.Now.AddSeconds(_options.Delay)}");
            }

            _logger.LogInformation($"{res.StatusCode}");

            var con = await res.Content.ReadAsStringAsync();

            _logger.LogInformation(con);
        }
    }
}