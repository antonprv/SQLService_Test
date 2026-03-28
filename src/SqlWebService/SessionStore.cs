using System;
using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.Timers;

namespace SqlWebService
{
    /// <summary>
    /// Потокобезопасное хранилище активных подключений.
    /// <para>Сервис работает в режиме per-call, поэтому подключения хранятся в статическом хранилище.</para>
    /// <para>Таймер автоматически закрывает сеансы, открытые дольше MaxSessionLifetimeMinutes.</para>
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
            _gcTimer = new Timer(60_000);
            _gcTimer.Elapsed += CollectExpiredSessions;
            _gcTimer.AutoReset = true;
            _gcTimer.Start();
        }

        public static string Create(SqlConnection connection)
        {
            var sessionId = Guid.NewGuid().ToString("N");
            var entry = new SessionEntry(connection);
            _sessions[sessionId] = entry;
            return sessionId;
        }

        public static SqlConnection Get(string sessionId)
        {
            if (sessionId != null && _sessions.TryGetValue(sessionId, out var entry))
            {
                entry.Touch();
                return entry.Connection;
            }
            return null;
        }

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
            catch { }
        }

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
