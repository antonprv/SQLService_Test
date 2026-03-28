using System;
using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.Timers;

namespace SqlWebService
{
    /// <summary>
    /// Потокобезопасное хранилище активных подключений.
    ///
    /// ДИЗАЙН-РЕШЕНИЕ: Сервис работает в режиме per-call (не singleton),
    /// поэтому подключения нельзя хранить в полях экземпляра.
    /// SessionStore предоставляет единое место хранения, доступное из любого
    /// экземпляра сервиса. Ключ — строковый GUID (SessionId).
    ///
    /// Сборка мусора: таймер раз в минуту закрывает сеансы, открытые
    /// дольше MaxSessionLifetimeMinutes, — защита от утечек при краше клиента.
    /// </summary>
    internal static class SessionStore
    {
        private static readonly ConcurrentDictionary<string, SessionEntry> _sessions
            = new ConcurrentDictionary<string, SessionEntry>(StringComparer.Ordinal);

        private static readonly Timer _gcTimer;

        /// <summary>Максимальное время жизни сеанса без явного Disconnect.</summary>
        public static int MaxSessionLifetimeMinutes { get; set; } = 60;

        static SessionStore()
        {
            _gcTimer = new Timer(60_000); // каждую минуту
            _gcTimer.Elapsed += CollectExpiredSessions;
            _gcTimer.AutoReset = true;
            _gcTimer.Start();
        }

        /// <summary>Создаёт новый сеанс и возвращает его SessionId.</summary>
        public static string Create(SqlConnection connection)
        {
            var sessionId = Guid.NewGuid().ToString("N"); // 32-символьный hex без дефисов
            var entry = new SessionEntry(connection);
            _sessions[sessionId] = entry;
            return sessionId;
        }

        /// <summary>Возвращает подключение по sessionId или null, если сеанс не найден.</summary>
        public static SqlConnection Get(string sessionId)
        {
            if (sessionId != null && _sessions.TryGetValue(sessionId, out var entry))
            {
                entry.Touch();
                return entry.Connection;
            }
            return null;
        }

        /// <summary>Закрывает и удаляет сеанс. Возвращает true, если сеанс существовал.</summary>
        public static bool Remove(string sessionId)
        {
            if (sessionId != null && _sessions.TryRemove(sessionId, out var entry))
            {
                SafeClose(entry.Connection);
                return true;
            }
            return false;
        }

        private static void CollectExpiredSessions(object sender, ElapsedEventArgs e)
        {
            var cutoff = DateTime.UtcNow.AddMinutes(-MaxSessionLifetimeMinutes);
            foreach (var kv in _sessions)
            {
                if (kv.Value.LastUsed < cutoff)
                {
                    Remove(kv.Key);
                }
            }
        }

        private static void SafeClose(SqlConnection conn)
        {
            try { conn?.Close(); conn?.Dispose(); }
            catch { /* игнорируем ошибки при закрытии */ }
        }

        // ─── Вложенный класс записи сеанса ─────────────────────────────────

        private sealed class SessionEntry
        {
            public SqlConnection Connection { get; }
            public DateTime LastUsed { get; private set; }

            public SessionEntry(SqlConnection connection)
            {
                Connection = connection;
                LastUsed = DateTime.UtcNow;
            }

            public void Touch() => LastUsed = DateTime.UtcNow;
        }
    }
}
