using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using static Pocket.Logger<MLS.Agent.Program>;
using Pocket;
using Microsoft.AspNetCore.Hosting;

namespace MLS.Agent
{
    public class Program
    {
        public static X509Certificate2 ParseKey(string base64EncodedKey)
        {
            var bytes = Convert.FromBase64String(base64EncodedKey);
            return new X509Certificate2(bytes);
        }

        internal static IWebHost host = null;

        public static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                var cert = ParseKey(args[0]);
                Log.Info($"CERTIFICATE: {args[0]} - {cert.GetCertHashString()}");
            }
            else
            {
                Log.Warning("No X509Certificate Provided To Main");
            }

            host = new WebHostBuilder()
                .UseKestrel(configure =>
                {
                    configure.AddServerHeader = false;
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .Build();

            host.Run();
        }
    }
}
