using System;
using System.ServiceModel;

namespace SqlWebServiceClient
{
    // ─── Контракты (дублируем здесь или берём из общей сборки) ────────────

    [System.Runtime.Serialization.DataContract]
    public class ConnectRequest
    {
        [System.Runtime.Serialization.DataMember] public string Server                { get; set; }
        [System.Runtime.Serialization.DataMember] public string Database              { get; set; } = "master";
        [System.Runtime.Serialization.DataMember] public bool   UseIntegratedSecurity { get; set; } = true;
        [System.Runtime.Serialization.DataMember] public string Username              { get; set; }
        [System.Runtime.Serialization.DataMember] public string Password              { get; set; }
        [System.Runtime.Serialization.DataMember] public int    ConnectTimeoutSeconds { get; set; } = 30;
    }

    [System.Runtime.Serialization.DataContract]
    public class ConnectResponse
    {
        [System.Runtime.Serialization.DataMember] public bool   Success   { get; set; }
        [System.Runtime.Serialization.DataMember] public string SessionId { get; set; }
        [System.Runtime.Serialization.DataMember] public string Message   { get; set; }
    }

    [System.Runtime.Serialization.DataContract]
    public class SqlVersionResponse
    {
        [System.Runtime.Serialization.DataMember] public bool   Success { get; set; }
        [System.Runtime.Serialization.DataMember] public string Version { get; set; }
        [System.Runtime.Serialization.DataMember] public string Message { get; set; }
    }

    [System.Runtime.Serialization.DataContract]
    public class DisconnectResponse
    {
        [System.Runtime.Serialization.DataMember] public bool   Success { get; set; }
        [System.Runtime.Serialization.DataMember] public string Message { get; set; }
    }

    // ─── Интерфейс сервиса для клиентского прокси ──────────────────────────

    [ServiceContract(Namespace = "http://sqlwebservice.local/v1")]
    public interface ISqlService
    {
        [OperationContract] ConnectResponse    Connect(ConnectRequest request);
        [OperationContract] SqlVersionResponse GetSqlVersion(string sessionId);
        [OperationContract] DisconnectResponse Disconnect(string sessionId);
    }

    // ─── Обёртка над WCF-каналом ───────────────────────────────────────────

    /// <summary>
    /// Тонкая обёртка над WCF ChannelFactory.
    ///
    /// ПОЧЕМУ НЕ Add Service Reference?
    ///   Add Service Reference генерирует громоздкий код, жёстко привязанный к
    ///   конкретной версии WSDL. При изменении контракта нужно переавтогенерировать.
    ///   ChannelFactory{T} работает с интерфейсом напрямую — проще поддерживать,
    ///   легче тестировать (можно подменить мок).
    ///
    /// ЖИЗНЕННЫЙ ЦИКЛ:
    ///   Create() → Connect() → GetSqlVersion() → Disconnect() → Close()/Dispose()
    ///   Один экземпляр SqlServiceProxy соответствует одному транспортному каналу.
    /// </summary>
    public sealed class SqlServiceProxy : IDisposable
    {
        private readonly ChannelFactory<ISqlService> _factory;
        private readonly ISqlService                 _channel;
        private          bool                        _disposed;

        public SqlServiceProxy(string endpointAddress)
        {
            if (string.IsNullOrWhiteSpace(endpointAddress))
                throw new ArgumentNullException(nameof(endpointAddress));

            var binding = new BasicHttpBinding
            {
                MaxReceivedMessageSize = 1024 * 1024,
                SendTimeout            = TimeSpan.FromMinutes(1),
                ReceiveTimeout         = TimeSpan.FromMinutes(1)
            };

            var endpoint = new EndpointAddress(endpointAddress);
            _factory = new ChannelFactory<ISqlService>(binding, endpoint);
            _channel = _factory.CreateChannel();
        }

        // Делегируем вызовы напрямую в канал — никакой бизнес-логики здесь нет.
        // Обработка ошибок остаётся на стороне UI-слоя (MainForm).

        public ConnectResponse    Connect(ConnectRequest req) => _channel.Connect(req);
        public SqlVersionResponse GetSqlVersion(string id)   => _channel.GetSqlVersion(id);
        public DisconnectResponse Disconnect(string id)       => _channel.Disconnect(id);

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            // WCF-каналы нужно закрывать через Abort при ошибках,
            // иначе Close() может выбросить повторное исключение.
            try
            {
                if (_channel is IClientChannel ch)
                {
                    if (ch.State == CommunicationState.Faulted)
                        ch.Abort();
                    else
                        ch.Close();
                }
                _factory.Close();
            }
            catch
            {
                (_channel as IClientChannel)?.Abort();
                _factory.Abort();
            }
        }
    }
}
