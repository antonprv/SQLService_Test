using System;
using System.Configuration;
using System.Drawing;
using System.Windows.Forms;

namespace SqlWebServiceClient
{
    /// <summary>
    /// Главная форма приложения.
    /// </summary>
    public partial class MainForm : Form
    {
        private SqlServiceProxy _proxy;
        private string          _sessionId;

        public MainForm()
        {
            InitializeComponent();

            var ep = ConfigurationManager.AppSettings["ServiceEndpoint"];
            if (!string.IsNullOrWhiteSpace(ep))
                txtServiceUrl.Text = ep;

            SetUiState(connected: false);
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            var url = txtServiceUrl.Text.Trim();
            if (string.IsNullOrEmpty(url))
            {
                Log("Введите URL сервиса.", Color.OrangeRed);
                return;
            }

            try
            {
                _proxy?.Dispose();
                _proxy = new SqlServiceProxy(url);

                var request = new ConnectRequest
                {
                    Server                = txtServer.Text.Trim(),
                    Database              = string.IsNullOrWhiteSpace(txtDatabase.Text) ? "master" : txtDatabase.Text.Trim(),
                    UseIntegratedSecurity = chkIntegrated.Checked,
                    Username              = txtUsername.Text.Trim(),
                    Password              = txtPassword.Text,
                    ConnectTimeoutSeconds = 30
                };

                Log($"Подключение к [{request.Server}\\{request.Database}]...", Color.Gray);
                var response = _proxy.Connect(request);

                if (response.Success)
                {
                    _sessionId = response.SessionId;
                    Log($"✓ {response.Message}", Color.LimeGreen);
                    Log($"  SessionId: {_sessionId}", Color.DarkCyan);
                    SetUiState(connected: true);
                }
                else
                {
                    Log($"✗ {response.Message}", Color.OrangeRed);
                }
            }
            catch (Exception ex)
            {
                Log($"✗ Ошибка связи: {ex.Message}", Color.OrangeRed);
                _proxy?.Dispose();
                _proxy = null;
            }
        }

        private void btnGetVersion_Click(object sender, EventArgs e)
        {
            if (_proxy == null || string.IsNullOrEmpty(_sessionId)) return;

            try
            {
                Log("Запрос версии SQL Server...", Color.Gray);
                var response = _proxy.GetSqlVersion(_sessionId);

                if (response.Success)
                    Log($"✓ {response.Version}", Color.LimeGreen);
                else
                    Log($"✗ {response.Message}", Color.OrangeRed);
            }
            catch (Exception ex)
            {
                Log($"✗ Ошибка: {ex.Message}", Color.OrangeRed);
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            if (_proxy == null || string.IsNullOrEmpty(_sessionId)) return;

            try
            {
                Log("Закрытие соединения...", Color.Gray);
                var response = _proxy.Disconnect(_sessionId);

                if (response.Success)
                    Log($"✓ {response.Message}", Color.LimeGreen);
                else
                    Log($"✗ {response.Message}", Color.OrangeRed);
            }
            catch (Exception ex)
            {
                Log($"✗ Ошибка при отключении: {ex.Message}", Color.OrangeRed);
            }
            finally
            {
                _proxy?.Dispose();
                _proxy     = null;
                _sessionId = null;
                SetUiState(connected: false);
            }
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            rtbLog.Clear();
        }

        private void SetUiState(bool connected)
        {
            btnConnect.Enabled    = !connected;
            btnGetVersion.Enabled = connected;
            btnDisconnect.Enabled = connected;

            txtServer.Enabled    = !connected;
            txtDatabase.Enabled  = !connected;
            txtUsername.Enabled  = !connected && !chkIntegrated.Checked;
            txtPassword.Enabled  = !connected && !chkIntegrated.Checked;
            chkIntegrated.Enabled= !connected;

            lblStatus.Text       = connected
                ? $"● Подключено  |  Session: {_sessionId?.Substring(0, 8)}..."
                : "○ Не подключено";
            lblStatus.ForeColor  = connected ? Color.LimeGreen : Color.Gray;
        }

        private void Log(string message, Color color)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            rtbLog.SelectionStart  = rtbLog.TextLength;
            rtbLog.SelectionLength = 0;
            rtbLog.SelectionColor  = Color.DimGray;
            rtbLog.AppendText($"[{timestamp}] ");
            rtbLog.SelectionColor  = color;
            rtbLog.AppendText(message + Environment.NewLine);
            rtbLog.ScrollToCaret();
        }

        private void chkIntegrated_CheckedChanged(object sender, EventArgs e)
        {
            bool useIntegrated = chkIntegrated.Checked;
            txtUsername.Enabled = !useIntegrated;
            txtPassword.Enabled = !useIntegrated;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _proxy?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
