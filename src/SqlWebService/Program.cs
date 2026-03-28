using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace SqlWebService
{
    /// <summary>
    /// Консольный хост WCF-сервиса.
    ///
    /// В production-окружении ServiceHost обычно размещают в IIS или
    /// Windows Service (через Topshelf / SC.exe). Консольный хост удобен
    /// для разработки и демонстрации — запустил, протестировал, остановил.
    /// </summary>
    internal class Program
    {
        private static void Main(string[] args)
        {
            const string baseAddress = "http://localhost:8080/SqlService";

            using (var host = new ServiceHost(typeof(SqlService), new Uri(baseAddress)))
            {
                // BasicHttpBinding — максимальная совместимость:
                //   • SOAP 1.1 / HTTP 1.1
                //   • Без WS-* расширений (Security, ReliableMessaging)
                //   • Работает со стандартным HttpClient и любым WCF-прокси
                //
                // Альтернативы, которые рассматривались:
                //   NetTcpBinding    — быстрее, но требует порт 808 и только .NET клиенты
                //   WSHttpBinding    — поддерживает WS-Security, сложнее конфигурировать
                //   BasicHttpBinding — выбрана как наименее проблемная для демо
                var binding = new BasicHttpBinding
                {
                    MaxReceivedMessageSize = 1024 * 1024, // 1 МБ достаточно для версионных строк
                    ReceiveTimeout         = TimeSpan.FromMinutes(1),
                    SendTimeout            = TimeSpan.FromMinutes(1),
                    OpenTimeout            = TimeSpan.FromSeconds(30),
                    CloseTimeout           = TimeSpan.FromSeconds(30)
                };

                host.AddServiceEndpoint(typeof(ISqlService), binding, "");

                // MEX-эндпоинт — позволяет Visual Studio добавить ссылку на сервис
                // и автоматически сгенерировать клиентский прокси-класс.
                // WSDL доступен по адресу: http://localhost:8080/SqlService/mex?wsdl
                var metadataBehavior = new ServiceMetadataBehavior
                {
                    HttpGetEnabled = true,
                    HttpGetUrl = new Uri(baseAddress + "/mex")
                };
                host.Description.Behaviors.Add(metadataBehavior);

                // Добавляем MEX-эндпоинт для поддержки IMetadataExchange
                host.AddServiceEndpoint(
                    typeof(System.ServiceModel.Description.IMetadataExchange),
                    new BasicHttpBinding(),
                    "mex");

                // Подробные ошибки в ответе — только для dev! В prod → false.
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
