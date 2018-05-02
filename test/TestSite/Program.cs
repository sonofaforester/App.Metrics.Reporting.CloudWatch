using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using App.Metrics;
using App.Metrics.AspNetCore;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TestSite
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureMetricsWithDefaults((context, builder) =>
                {
                    var awsNamespace = "namespace-test";

                    builder.Configuration.Configure(o =>
                    {
                        o.DefaultContextLabel = "context-test";
                    });

                    if (awsNamespace != null)
                    {
                        builder.Report.ToCloudWatch(awsNamespace,
                            o =>
                            {
                                o.AccessKeyId = "keyId";
                                o.SecretAccessKey = "secretKey";
                                o.Endpoint = RegionEndpoint.USGovCloudWest1;
                                o.FlushInterval = TimeSpan.FromSeconds(5);
                            });
                    }
                })
                .UseMetrics().Build();
    }
}
