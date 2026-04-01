using System;
using System.Data.SqlClient;
using System.ServiceModel;

namespace SqlWebService
{
  /// <summary>
  /// Реализация <see cref="ISqlService"/>.
  /// <para>Режим инстанцирования: PerCall - каждый вызов получает новый экземпляр.
  /// Состояние хранится в статическом <see cref="SessionStore"/>.</para>
  /// <para>Ошибки возвращаются как структурированные ответы (Success=false, Message=...).</para>
  /// </summary>
  [ServiceBehavior(
      InstanceContextMode = InstanceContextMode.PerCall,
      ConcurrencyMode = ConcurrencyMode.Single,
      Namespace = "http://sqlwebservice.local/v1")]
  public class SqlService : ISqlService
  {
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
        connection.Open();

        var sessionId = SessionStore.Create(connection);

        return new ConnectResponse
        {
          Success = true,
          SessionId = sessionId,
          Message = $"Подключение к [{request.Server}] установлено. " +
                        $"База данных: [{connection.Database}]."
        };
      }
      catch (SqlException ex)
      {
        return Fail<ConnectResponse>(
            $"Ошибка SQL Server (номер {ex.Number}): {ex.Message}");
      }
      catch (Exception ex)
      {
        return Fail<ConnectResponse>($"Неожиданная ошибка: {ex.Message}");
      }
    }

    public SqlVersionResponse GetSqlVersion(string sessionId)
    {
      var connection = SessionStore.Get(sessionId);
      if (connection == null)
        return Fail<SqlVersionResponse>(SessionNotFound(sessionId));

      try
      {
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

    public DisconnectResponse Disconnect(DisconnectRequest request)
    {
      if (request == null)
        return Fail<DisconnectResponse>("Запрос не может быть пустым.");

      if (string.IsNullOrWhiteSpace(request.SessionId))
        return Fail<DisconnectResponse>("SessionId не указан.");

      bool removed = SessionStore.Remove(request.SessionId);

      return removed
          ? new DisconnectResponse { Success = true, Message = "Подключение закрыто." }
          : new DisconnectResponse { Success = false, Message = SessionNotFound(request.SessionId) };
    }

    private static string BuildConnectionString(ConnectRequest req)
    {
      var builder = new SqlConnectionStringBuilder
      {
        DataSource = req.Server,
        InitialCatalog = req.Database ?? "master",
        ConnectTimeout = req.ConnectTimeoutSeconds > 0 ? req.ConnectTimeoutSeconds : 30,
        Encrypt = false,
        TrustServerCertificate = true
      };

      if (req.UseIntegratedSecurity)
      {
        builder.IntegratedSecurity = true;
      }
      else
      {
        builder.UserID = req.Username;
        builder.Password = req.Password;
      }

      return builder.ConnectionString;
    }

    private static T Fail<T>(string message) where T : class, new()
    {
      if (typeof(T) == typeof(ConnectResponse))
        return new ConnectResponse { Success = false, Message = message } as T;
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
