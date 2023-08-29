using Microsoft.Extensions.Options;

namespace hednsupd
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder();

            builder.Configuration.AddEnvironmentVariables(prefix: "HEDNSUPD_");

            builder.Services.Configure<UpdateOptions>(c =>
            {
                c.Hostname = builder.Configuration["HEDNSUPD_HOSTNAME"];
                c.Key = builder.Configuration["HEDNSUPD_KEY"];
                c.Delay = int.Parse(builder.Configuration["HEDNSUPD_DELAY"]);
            });

            builder.Services.AddHostedService<UpdateWorker>();

            using var host = builder.Build();

            host.Run();
        }
    }
}