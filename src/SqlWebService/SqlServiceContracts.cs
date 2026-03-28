using System.Runtime.Serialization;

namespace SqlWebService
{
    [DataContract(Name = "ConnectRequestType", Namespace = "http://sqlwebservice.local/v1")]
    public class ConnectRequest
    {
        /// <summary>Сервер / инстанс, например "localhost" или "srv\SQLEXPRESS".</summary>
        [DataMember(IsRequired = true, Order = 1)]
        public string Server { get; set; }

        /// <summary>Имя базы данных. По умолчанию - master.</summary>
        [DataMember(Order = 2)]
        public string Database { get; set; } = "master";

        /// <summary>
        /// true - Windows-аутентификация (IntegratedSecurity=SSPI).
        /// false - SQL-аутентификация; обязательны Username и Password.
        /// </summary>
        [DataMember(Order = 3)]
        public bool UseIntegratedSecurity { get; set; } = true;

        /// <summary>Логин SQL Server (игнорируется при UseIntegratedSecurity = true).</summary>
        [DataMember(Order = 4)]
        public string Username { get; set; }

        /// <summary>Пароль SQL Server (игнорируется при UseIntegratedSecurity = true).</summary>
        [DataMember(Order = 5)]
        public string Password { get; set; }

        /// <summary>Таймаут подключения в секундах. По умолчанию 30.</summary>
        [DataMember(Order = 6)]
        public int ConnectTimeoutSeconds { get; set; } = 30;
    }

    [DataContract(Name = "ConnectResponseType", Namespace = "http://sqlwebservice.local/v1")]
    public class ConnectResponse
    {
        /// <summary>true, если подключение успешно установлено.</summary>
        [DataMember(Order = 1)]
        public bool Success { get; set; }

        /// <summary>
        /// Уникальный идентификатор сеанса (GUID).
        /// Используется как ключ для последующих вызовов GetSqlVersion / Disconnect.
        /// Null при ошибке.
        /// </summary>
        [DataMember(Order = 2)]
        public string SessionId { get; set; }

        /// <summary>Сообщение для отображения пользователю.</summary>
        [DataMember(Order = 3)]
        public string Message { get; set; }
    }

    [DataContract(Name = "SqlVersionResponseType", Namespace = "http://sqlwebservice.local/v1")]
    public class SqlVersionResponse
    {
        /// <summary>true, если запрос выполнен без ошибок.</summary>
        [DataMember(Order = 1)]
        public bool Success { get; set; }

        /// <summary>Строка версии, возвращаемая @@VERSION.</summary>
        [DataMember(Order = 2)]
        public string Version { get; set; }

        /// <summary>Сообщение (ошибка или подтверждение).</summary>
        [DataMember(Order = 3)]
        public string Message { get; set; }
    }

    [DataContract(Name = "DisconnectRequestType", Namespace = "http://sqlwebservice.local/v1")]
    public class DisconnectRequest
    {
        /// <summary>Идентификатор сеанса.</summary>
        [DataMember(Order = 1)]
        public string SessionId { get; set; }
    }

    [DataContract(Name = "DisconnectResponseType", Namespace = "http://sqlwebservice.local/v1")]
    public class DisconnectResponse
    {
        /// <summary>true, если соединение успешно закрыто.</summary>
        [DataMember(Order = 1)]
        public bool Success { get; set; }

        /// <summary>Сообщение.</summary>
        [DataMember(Order = 2)]
        public string Message { get; set; }
    }
}
