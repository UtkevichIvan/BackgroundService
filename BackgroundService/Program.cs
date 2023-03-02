using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Sockets;
using System.Net;
using System.Text;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<GetterService>();
        services.AddHostedService<SenderService>();
    })
    .Build();

host.Run();

public class GetterService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using UdpClient receiver = new UdpClient(22220);

        while (!stoppingToken.IsCancellationRequested)
        {
            var result = await receiver.ReceiveAsync();
            var message = Encoding.UTF8.GetString(result.Buffer);
            Console.WriteLine(message);
        }

        await Task.CompletedTask;
    }
}

public class SenderService : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        while (!stoppingToken.IsCancellationRequested)
        {
            byte[] data = Encoding.UTF8.GetBytes(DateTimeOffset.Now.ToString());
            EndPoint remotePoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 22220);
            int bytes = await udpSocket.SendToAsync(data, remotePoint);
            await Task.Delay(200, stoppingToken);
        }

        await Task.CompletedTask;
    }
}