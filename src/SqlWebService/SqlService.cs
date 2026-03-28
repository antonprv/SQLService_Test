using System;
using System.Data;
using System.Data.SqlClient;
using System.ServiceModel;
using System.Text;

namespace SqlWebService
{
    /// <summary>
    /// Реализация <see cref="ISqlService"/>.
    ///
    /// РЕЖИМ ИНСТАНЦИРОВАНИЯ: PerCall — каждый входящий вызов получает
    /// новый экземпляр класса. Это безопасно, потому что состояние (подключения)
    /// хранится в статическом <see cref="SessionStore"/>.
    ///
    /// ОШИБКИ: Все исключения перехватываются внутри методов и возвращаются
    /// как структурированные ответы (Success=false, Message=...).
    /// WCF FaultException не используется намеренно — клиентский UI
    /// проще обрабатывает единообразные DTO, чем разные типы fault.
    /// </summary>
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerCall,
        ConcurrencyMode = ConcurrencyMode.Single,
        Namespace = "http://sqlwebservice.local/v1")]
    public class SqlService : ISqlService
    {
        // ─── Connect ────────────────────────────────────────────────────────

        public ConnectResponse Connect(ConnectRequest request)
        {
            if (request == null)
                return Fail<ConnectResponse>("Запрос не может быть пустым.");

            if (string.IsNullOrWhiteSpace(request.Server))
                return Fail<ConnectResponse>("Не указано имя сервера.");

            try
            {
                var connectionString = BuildConnectionString(request);
                var connection = new SqlConnection(connectionString);
                connection.Open();                 // синхронно — .NET Framework 4.x

                var sessionId = SessionStore.Create(connection);

                return new ConnectResponse
                {
                    Success   = true,
                    SessionId = sessionId,
                    Message   = $"Подключение к [{request.Server}] установлено. " +
                                $"База данных: [{connection.Database}]."
                };
            }
            catch (SqlException ex)
            {
                // SqlException содержит номер и класс ошибки SQL Server —
                // выводим их, чтобы администратор мог диагностировать проблему.
                return Fail<ConnectResponse>(
                    $"Ошибка SQL Server (номер {ex.Number}): {ex.Message}");
            }
            catch (Exception ex)
            {
                return Fail<ConnectResponse>($"Неожиданная ошибка: {ex.Message}");
            }
        }

        // ─── GetSqlVersion ──────────────────────────────────────────────────

        public SqlVersionResponse GetSqlVersion(string sessionId)
        {
            var connection = SessionStore.Get(sessionId);
            if (connection == null)
                return Fail<SqlVersionResponse>(SessionNotFound(sessionId));

            try
            {
                // @@VERSION возвращает полную строку вида:
                //   Microsoft SQL Server 2019 (RTM-CU18) - 15.0.4261.1 ...
                // Используем параметризованный запрос (даже для служебного — хорошая привычка).
                using (var cmd = new SqlCommand("SELECT @@VERSION;", connection))
                {
                    var version = cmd.ExecuteScalar() as string;
                    return new SqlVersionResponse
                    {
                        Success = true,
                        Version = version ?? "(версия не определена)",
                        Message = "Версия получена успешно."
                    };
                }
            }
            catch (SqlException ex)
            {
                return Fail<SqlVersionResponse>(
                    $"Ошибка при получении версии (SQL {ex.Number}): {ex.Message}");
            }
            catch (Exception ex)
            {
                return Fail<SqlVersionResponse>($"Неожиданная ошибка: {ex.Message}");
            }
        }

        // ─── Disconnect ─────────────────────────────────────────────────────

        public DisconnectResponse Disconnect(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                return Fail<DisconnectResponse>("SessionId не указан.");

            bool removed = SessionStore.Remove(sessionId);

            return removed
                ? new DisconnectResponse { Success = true,  Message = "Подключение закрыто." }
                : new DisconnectResponse { Success = false, Message = SessionNotFound(sessionId) };
        }

        // ─── Вспомогательные методы ─────────────────────────────────────────

        private static string BuildConnectionString(ConnectRequest req)
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource         = req.Server,
                InitialCatalog     = req.Database ?? "master",
                ConnectTimeout     = req.ConnectTimeoutSeconds > 0 ? req.ConnectTimeoutSeconds : 30,
                // Шифруем канал, но принимаем самоподписанные сертификаты
                // (типично для dev-окружений без CA).
                Encrypt            = false,
                TrustServerCertificate = true
            };

            if (req.UseIntegratedSecurity)
            {
                builder.IntegratedSecurity = true;
            }
            else
            {
                builder.UserID   = req.Username;
                builder.Password = req.Password;
            }

            return builder.ConnectionString;
        }

        /// <summary>Фабрика для единообразного формирования ответов-ошибок.</summary>
        private static T Fail<T>(string message) where T : class, new()
        {
            // Рефлексия не используется — каждый тип обрабатывается явно.
            if (typeof(T) == typeof(ConnectResponse))
                return new ConnectResponse    { Success = false, Message = message } as T;
            if (typeof(T) == typeof(SqlVersionResponse))
                return new SqlVersionResponse { Success = false, Message = message } as T;
            if (typeof(T) == typeof(DisconnectResponse))
                return new DisconnectResponse { Success = false, Message = message } as T;

            return new T();
        }

        private static string SessionNotFound(string sessionId) =>
            $"Сеанс [{sessionId ?? "null"}] не найден или уже закрыт.";
    }
}
