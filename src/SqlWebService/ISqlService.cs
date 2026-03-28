using System.ServiceModel;

namespace SqlWebService
{
    /// <summary>
    /// Контракт сервиса для работы с MS SQL Server.
    /// Определяет три операции жизненного цикла подключения.
    /// </summary>
    [ServiceContract(Namespace = "http://sqlwebservice.local/v1")]
    public interface ISqlService
    {
        /// <summary>
        /// Устанавливает подключение к указанному MS SQL Server.
        /// </summary>
        /// <param name="request">Параметры подключения (сервер, БД, аутентификация).</param>
        /// <returns>Результат попытки подключения с sessionId для последующих вызовов.</returns>
        [OperationContract(Name = "Connect")]
        [return: System.ServiceModel.MessageParameter(Name = "ConnectResult")]
        ConnectResponse Connect([System.ServiceModel.MessageParameter(Name = "request")] ConnectRequest request);

        /// <summary>
        /// Возвращает строку версии MS SQL Server для активного сеанса.
        /// </summary>
        /// <param name="sessionId">Идентификатор сеанса, полученный при Connect.</param>
        /// <returns>Версия сервера либо описание ошибки.</returns>
        [OperationContract(Name = "GetSqlVersion")]
        [return: System.ServiceModel.MessageParameter(Name = "GetSqlVersionResult")]
        SqlVersionResponse GetSqlVersion([System.ServiceModel.MessageParameter(Name = "sessionId")] string sessionId);

        /// <summary>
        /// Закрывает подключение и освобождает ресурсы сеанса.
        /// </summary>
        /// <param name="request">Запрос на закрытие подключения с sessionId.</param>
        /// <returns>Результат операции закрытия.</returns>
        [OperationContract(Name = "Disconnect")]
        [return: System.ServiceModel.MessageParameter(Name = "DisconnectResult")]
        DisconnectResponse Disconnect([System.ServiceModel.MessageParameter(Name = "request")] DisconnectRequest request);
    }
}
