using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace SqlWebService
{
    /// <summary>
    /// Консольный хост WCF-сервиса.
    /// </summary>
    internal class Program
    {
        private static void Main(string[] args)
        {
            const string baseAddress = "http://localhost:8080/SqlService";

            using (var host = new ServiceHost(typeof(SqlService), new Uri(baseAddress)))
            {
                var binding = new BasicHttpBinding
                {
                    MaxReceivedMessageSize = 1024 * 1024,
                    ReceiveTimeout         = TimeSpan.FromMinutes(1),
                    SendTimeout            = TimeSpan.FromMinutes(1),
                    OpenTimeout            = TimeSpan.FromSeconds(30),
                    CloseTimeout           = TimeSpan.FromSeconds(30)
                };

                host.AddServiceEndpoint(typeof(ISqlService), binding, "");

                var metadataBehavior = new ServiceMetadataBehavior
                {
                    HttpGetEnabled = true,
                    HttpGetUrl = new Uri(baseAddress + "/mex")
                };
                host.Description.Behaviors.Add(metadataBehavior);

                host.AddServiceEndpoint(
                    typeof(System.ServiceModel.Description.IMetadataExchange),
                    new BasicHttpBinding(),
                    "mex");

                host.Description.Behaviors
                    .Find<ServiceDebugBehavior>()
                    .IncludeExceptionDetailInFaults = true;

                host.Open();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("=== SqlWebService запущен ===");
                Console.ResetColor();
                Console.WriteLine($"Endpoint : {baseAddress}");
                Console.WriteLine($"WSDL/MEX : {baseAddress}/mex");
                Console.WriteLine();
                Console.WriteLine("Нажмите ENTER для остановки...");
                Console.ReadLine();

                host.Close();
                Console.WriteLine("Сервис остановлен.");
            }
        }
    }
}
