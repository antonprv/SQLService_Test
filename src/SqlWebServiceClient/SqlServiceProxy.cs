using System;
using System.ServiceModel;

namespace SqlWebServiceClient
{
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

    [System.Runtime.Serialization.DataContract]
    public class DisconnectRequest
    {
        [System.Runtime.Serialization.DataMember] public string SessionId { get; set; }
    }

    [ServiceContract(Namespace = "http://sqlwebservice.local/v1")]
    public interface ISqlService
    {
        [OperationContract] ConnectResponse    Connect(ConnectRequest request);
        [OperationContract] SqlVersionResponse GetSqlVersion(string sessionId);
        [OperationContract] DisconnectResponse Disconnect(DisconnectRequest request);
    }

    /// <summary>
    /// Обёртка над WCF ChannelFactory.
    /// <para>ChannelFactory создаёт прокси напрямую из интерфейса без генерации кода.</para>
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

        public ConnectResponse    Connect(ConnectRequest req) => _channel.Connect(req);
        public SqlVersionResponse GetSqlVersion(string id)   => _channel.GetSqlVersion(id);
        public DisconnectResponse Disconnect(string id)      => _channel.Disconnect(new DisconnectRequest { SessionId = id });

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

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
