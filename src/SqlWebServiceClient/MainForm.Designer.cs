namespace SqlWebServiceClient
{
  partial class MainForm
  {
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
        components.Dispose();
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
      this.grpConnection = new System.Windows.Forms.GroupBox();
      this.lblServiceUrl = new System.Windows.Forms.Label();
      this.txtServiceUrl = new System.Windows.Forms.TextBox();
      this.lblServer = new System.Windows.Forms.Label();
      this.txtServer = new System.Windows.Forms.TextBox();
      this.lblDatabase = new System.Windows.Forms.Label();
      this.txtDatabase = new System.Windows.Forms.TextBox();
      this.chkIntegrated = new System.Windows.Forms.CheckBox();
      this.lblUsername = new System.Windows.Forms.Label();
      this.txtUsername = new System.Windows.Forms.TextBox();
      this.lblPassword = new System.Windows.Forms.Label();
      this.txtPassword = new System.Windows.Forms.TextBox();
      this.grpActions = new System.Windows.Forms.GroupBox();
      this.btnConnect = new System.Windows.Forms.Button();
      this.btnGetVersion = new System.Windows.Forms.Button();
      this.btnDisconnect = new System.Windows.Forms.Button();
      this.btnClearLog = new System.Windows.Forms.Button();
      this.grpLog = new System.Windows.Forms.GroupBox();
      this.rtbLog = new System.Windows.Forms.RichTextBox();
      this.lblStatus = new System.Windows.Forms.Label();

      this.grpConnection.SuspendLayout();
      this.grpActions.SuspendLayout();
      this.grpLog.SuspendLayout();
      this.SuspendLayout();

      // ─── grpConnection ───────────────────────────────────────────────
      this.grpConnection.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.lblServiceUrl, this.txtServiceUrl,
                this.lblServer, this.txtServer,
                this.lblDatabase, this.txtDatabase,
                this.chkIntegrated,
                this.lblUsername, this.txtUsername,
                this.lblPassword, this.txtPassword
            });
      this.grpConnection.Location = new System.Drawing.Point(12, 12);
      this.grpConnection.Name = "grpConnection";
      this.grpConnection.Size = new System.Drawing.Size(560, 200);
      this.grpConnection.Text = "Параметры подключения";
      this.grpConnection.Font = new System.Drawing.Font("Segoe UI", 9F);

      // Service URL
      this.lblServiceUrl.AutoSize = true;
      this.lblServiceUrl.Location = new System.Drawing.Point(10, 22);
      this.lblServiceUrl.Text = "URL сервиса:";
      this.txtServiceUrl.Location = new System.Drawing.Point(140, 19);
      this.txtServiceUrl.Size = new System.Drawing.Size(400, 23);
      this.txtServiceUrl.Text = "http://localhost:8080/SqlService";

      // Server
      this.lblServer.AutoSize = true;
      this.lblServer.Location = new System.Drawing.Point(10, 52);
      this.lblServer.Text = "SQL Server:";
      this.txtServer.Location = new System.Drawing.Point(140, 49);
      this.txtServer.Size = new System.Drawing.Size(400, 23);
      this.txtServer.Text = "localhost";

      // Database
      this.lblDatabase.AutoSize = true;
      this.lblDatabase.Location = new System.Drawing.Point(10, 82);
      this.lblDatabase.Text = "База данных:";
      this.txtDatabase.Location = new System.Drawing.Point(140, 79);
      this.txtDatabase.Size = new System.Drawing.Size(400, 23);
      this.txtDatabase.Text = "master";

      // Integrated Security
      this.chkIntegrated.AutoSize = true;
      this.chkIntegrated.Location = new System.Drawing.Point(140, 112);
      this.chkIntegrated.Text = "Windows-аутентификация (Integrated Security)";
      this.chkIntegrated.Checked = true;
      this.chkIntegrated.CheckedChanged += new System.EventHandler(this.chkIntegrated_CheckedChanged);

      // Username
      this.lblUsername.AutoSize = true;
      this.lblUsername.Location = new System.Drawing.Point(10, 142);
      this.lblUsername.Text = "Логин:";
      this.txtUsername.Location = new System.Drawing.Point(140, 139);
      this.txtUsername.Size = new System.Drawing.Size(400, 23);
      this.txtUsername.Enabled = false;

      // Password
      this.lblPassword.AutoSize = true;
      this.lblPassword.Location = new System.Drawing.Point(10, 172);
      this.lblPassword.Text = "Пароль:";
      this.txtPassword.Location = new System.Drawing.Point(140, 169);
      this.txtPassword.Size = new System.Drawing.Size(400, 23);
      this.txtPassword.PasswordChar = '*';
      this.txtPassword.Enabled = false;

      // ─── grpActions ──────────────────────────────────────────────────
      this.grpActions.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.btnConnect, this.btnGetVersion, this.btnDisconnect, this.btnClearLog
            });
      this.grpActions.Location = new System.Drawing.Point(12, 220);
      this.grpActions.Name = "grpActions";
      this.grpActions.Size = new System.Drawing.Size(560, 55);
      this.grpActions.Text = "Операции";
      this.grpActions.Font = new System.Drawing.Font("Segoe UI", 9F);

      this.btnConnect.Location = new System.Drawing.Point(10, 20);
      this.btnConnect.Size = new System.Drawing.Size(120, 28);
      this.btnConnect.Text = "⚡ Подключиться";
      this.btnConnect.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
      this.btnConnect.ForeColor = System.Drawing.Color.White;
      this.btnConnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);

      this.btnGetVersion.Location = new System.Drawing.Point(145, 20);
      this.btnGetVersion.Size = new System.Drawing.Size(140, 28);
      this.btnGetVersion.Text = "🔍 Получить версию";
      this.btnGetVersion.BackColor = System.Drawing.Color.FromArgb(16, 124, 16);
      this.btnGetVersion.ForeColor = System.Drawing.Color.White;
      this.btnGetVersion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnGetVersion.Click += new System.EventHandler(this.btnGetVersion_Click);

      this.btnDisconnect.Location = new System.Drawing.Point(300, 20);
      this.btnDisconnect.Size = new System.Drawing.Size(120, 28);
      this.btnDisconnect.Text = "✖ Отключиться";
      this.btnDisconnect.BackColor = System.Drawing.Color.FromArgb(196, 43, 28);
      this.btnDisconnect.ForeColor = System.Drawing.Color.White;
      this.btnDisconnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);

      this.btnClearLog.Location = new System.Drawing.Point(435, 20);
      this.btnClearLog.Size = new System.Drawing.Size(115, 28);
      this.btnClearLog.Text = "🗑 Очистить лог";
      this.btnClearLog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);

      // ─── grpLog ──────────────────────────────────────────────────────
      this.grpLog.Controls.Add(this.rtbLog);
      this.grpLog.Location = new System.Drawing.Point(12, 283);
      this.grpLog.Name = "grpLog";
      this.grpLog.Size = new System.Drawing.Size(560, 240);
      this.grpLog.Text = "Лог событий";
      this.grpLog.Font = new System.Drawing.Font("Segoe UI", 9F);

      this.rtbLog.Dock = System.Windows.Forms.DockStyle.Fill;
      this.rtbLog.BackColor = System.Drawing.Color.FromArgb(20, 20, 20);
      this.rtbLog.ForeColor = System.Drawing.Color.Silver;
      this.rtbLog.Font = new System.Drawing.Font("Consolas", 9F);
      this.rtbLog.ReadOnly = true;
      this.rtbLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.rtbLog.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;

      // ─── lblStatus ───────────────────────────────────────────────────
      this.lblStatus.AutoSize = false;
      this.lblStatus.Location = new System.Drawing.Point(12, 530);
      this.lblStatus.Size = new System.Drawing.Size(560, 20);
      this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
      this.lblStatus.ForeColor = System.Drawing.Color.Gray;
      this.lblStatus.Text = "○ Не подключено";

      // ─── MainForm ────────────────────────────────────────────────────
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(584, 561);
      this.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.grpConnection, this.grpActions, this.grpLog, this.lblStatus
            });
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.MaximizeBox = false;
      this.Name = "MainForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "SqlWebService Client — .NET Framework 4.8";
      this.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);

      this.grpConnection.ResumeLayout(false);
      this.grpConnection.PerformLayout();
      this.grpActions.ResumeLayout(false);
      this.grpLog.ResumeLayout(false);
      this.ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.GroupBox grpConnection;
    private System.Windows.Forms.Label lblServiceUrl;
    private System.Windows.Forms.TextBox txtServiceUrl;
    private System.Windows.Forms.Label lblServer;
    private System.Windows.Forms.TextBox txtServer;
    private System.Windows.Forms.Label lblDatabase;
    private System.Windows.Forms.TextBox txtDatabase;
    private System.Windows.Forms.CheckBox chkIntegrated;
    private System.Windows.Forms.Label lblUsername;
    private System.Windows.Forms.TextBox txtUsername;
    private System.Windows.Forms.Label lblPassword;
    private System.Windows.Forms.TextBox txtPassword;
    private System.Windows.Forms.GroupBox grpActions;
    private System.Windows.Forms.Button btnConnect;
    private System.Windows.Forms.Button btnGetVersion;
    private System.Windows.Forms.Button btnDisconnect;
    private System.Windows.Forms.Button btnClearLog;
    private System.Windows.Forms.GroupBox grpLog;
    private System.Windows.Forms.RichTextBox rtbLog;
    private System.Windows.Forms.Label lblStatus;
  }
}
