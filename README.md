# SqlWebService

WCF-сервис и WinForms-клиент для управления подключениями к Microsoft SQL Server.

## 📋 Описание

Проект представляет собой самохостинговый WCF-сервис (.NET Framework 4.8), который предоставляет SOAP-интерфейс для:
- Подключения к MS SQL Server (с поддержкой Windows и SQL-аутентификации)
- Получения версии SQL Server
- Закрытия подключения

Клиентская часть — WinForms-приложение с удобным интерфейсом для тестирования сервиса.

## 🏗 Архитектура

```
┌─────────────────────────┐         SOAP/HTTP          ┌─────────────────────────┐
│  SqlWebServiceClient    │ ◄───────────────────────►  │    SqlWebService        │
│  (WinForms UI)          │                            │   (WCF ServiceHost)     │
│                         │                            │                         │
│  - MainForm.cs          │                            │  - SqlService.cs        │
│  - SqlServiceProxy.cs   │                            │  - SessionStore.cs      │
│                         │                            │  - ISqlService.cs       │
└─────────────────────────┘                            └────────────┬────────────┘
                                                                    │
                                                                    ▼
                                                         ┌─────────────────────┐
                                                         │   MS SQL Server     │
                                                         └─────────────────────┘
```

### Компоненты

#### Сервис (SqlWebService)

| Файл | Описание |
|------|----------|
| `Program.cs` | Консольный хост WCF-сервиса (ServiceHost) |
| `ISqlService.cs` | Контракт сервиса (ServiceContract) |
| `SqlService.cs` | Реализация сервиса (PerCall instancing) |
| `SqlServiceContracts.cs` | DTO классы для запросов/ответов |
| `SessionStore.cs` | Хранилище активных сессий (статический словарь) |
| `App.config` | Конфигурация WCF (bindings, timeouts) |

#### Клиент (SqlWebServiceClient)

| Файл | Описание |
|------|----------|
| `Program.cs` | Точка входа WinForms-приложения |
| `MainForm.cs` | Главная форма с UI элементами |
| `MainForm.Designer.cs` | Автогенерируемый код формы |
| `SqlServiceProxy.cs` | Обёртка над ChannelFactory для вызова сервиса |
| `App.config` | Конфигурация endpoint URL |

## 🚀 Быстрый старт

### Требования

- Windows 10/11 или Windows Server 2016+
- .NET Framework 4.8 Runtime
- MS SQL Server (локальный или удалённый)
- Visual Studio 2019/2022 или Build Tools (для сборки)

### Запуск через скрипты

```bash
# Запуск сервиса (порт 8080)
scripts\start-service.bat

# В отдельном окне — запуск клиента
scripts\start-client.bat
```

### Ручная сборка и запуск

```powershell
# Сборка сервиса
msbuild src\SqlWebService\SqlWebService.csproj /p:Configuration=Release

# Сборка клиента
msbuild src\SqlWebServiceClient\SqlWebServiceClient.csproj /p:Configuration=Release

# Запуск сервиса
src\SqlWebService\bin\Release\SqlWebService.exe

# Запуск клиента (в отдельном окне)
src\SqlWebServiceClient\bin\Release\SqlWebServiceClient.exe
```

## ⚙️ Конфигурация

### Сервис

Сервис слушает адрес: `http://localhost:8080/SqlService`

WSDL доступен по адресу: `http://localhost:8080/SqlService/mex?wsdl`

Параметры можно изменить в `Program.cs`:
```csharp
const string baseAddress = "http://localhost:8080/SqlService";
```

### Клиент

URL сервиса по умолчанию: `http://localhost:8080/SqlService`

Можно изменить в `App.config`:
```xml
<appSettings>
    <add key="ServiceEndpoint" value="http://localhost:8080/SqlService" />
</appSettings>
```

Или прямо в UI клиента перед подключением.

## 📡 SOAP API

### Контракт сервиса

```csharp
[ServiceContract(Namespace = "http://sqlwebservice.local/v1")]
public interface ISqlService
{
    [OperationContract]
    ConnectResponse Connect(ConnectRequest request);
    
    [OperationContract]
    SqlVersionResponse GetSqlVersion(string sessionId);
    
    [OperationContract]
    DisconnectResponse Disconnect(string sessionId);
}
```

### Пример SOAP-запроса Connect

```xml
POST http://localhost:8080/SqlService
Content-Type: text/xml; charset=utf-8
SOAPAction: ""

<?xml version="1.0" encoding="utf-8"?>
<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
  <s:Body>
    <Connect xmlns="http://sqlwebservice.local/v1">
      <request>
        <Server>localhost</Server>
        <Database>master</Database>
        <UseIntegratedSecurity>true</UseIntegratedSecurity>
        <ConnectTimeoutSeconds>30</ConnectTimeoutSeconds>
      </request>
    </Connect>
  </s:Body>
</s:Envelope>
```

### Пример ответа

```xml
<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
  <s:Body>
    <ConnectResponse xmlns="http://sqlwebservice.local/v1">
      <Success>true</Success>
      <SessionId>a1b2c3d4-e5f6-7890-abcd-ef1234567890</SessionId>
      <Message>Подключение к [localhost] установлено. База данных: [master].</Message>
    </ConnectResponse>
  </s:Body>
</s:Envelope>
```

## 🔧 Скрипты

| Скрипт | Описание |
|--------|----------|
| `scripts\start-service.bat` | Сборка и запуск WCF-сервиса |
| `scripts\start-service.sh` | Bash-версия для Linux/macOS (требуется Mono) |
| `scripts\start-client.bat` | Сборка и запуск WinForms-клиента |
| `scripts\start-client.sh` | Bash-версия для Linux/macOS (требуется Mono) |

### Аргументы скриптов

```bash
# Сборка в Release-режиме
start-service.bat release

# Запуск без сборки (если уже собрано)
start-service.bat no-build

# Комбинированный вариант
start-service.bat release no-build
```

## 🧪 CI/CD

Проект включает GitHub Actions workflow (`.github/workflows/build-and-test.yml`):

- **Build** — сборка обоих проектов в режиме Release
- **Smoke Test** — запуск сервиса, проверка WSDL и SOAP-эндпоинтов
- **Release** — создание GitHub Release при пуше тега (v*)

### Публикация релиза

```bash
git tag v1.0.0
git push origin v1.0.0
```

Workflow автоматически создаст релиз с ZIP-архивами сборок.

## 📁 Структура проекта

```
SqlWebService/
├── .github/
│   └── workflows/
│       └── build-and-test.yml    # CI/CD pipeline
├── scripts/
│   ├── start-service.bat         # Скрипт запуска сервиса
│   ├── start-service.sh
│   ├── start-client.bat          # Скрипт запуска клиента
│   └── start-client.sh
├── src/
│   ├── SqlWebService/            # WCF-сервис
│   │   ├── App.config
│   │   ├── ISqlService.cs        # Контракт
│   │   ├── Program.cs            # Хост
│   │   ├── SessionStore.cs       # Хранилище сессий
│   │   ├── SqlService.cs         # Реализация
│   │   ├── SqlServiceContracts.cs # DTO
│   │   ├── SqlWebService.csproj
│   │   └── Properties/
│   │       └── AssemblyInfo.cs
│   └── SqlWebServiceClient/      # WinForms-клиент
│       ├── App.config
│       ├── MainForm.cs
│       ├── MainForm.Designer.cs
│       ├── MainForm.resx
│       ├── Program.cs
│       ├── SqlServiceProxy.cs    # ChannelFactory обёртка
│       ├── SqlWebServiceClient.csproj
│       └── Properties/
│           └── AssemblyInfo.cs
├── .gitignore
├── README.md
├── SqlWebService.sln
└── SqlWebService_Architecture.docx
```

## 🔐 Безопасность

### Аутентификация

Сервис поддерживает два режима:

1. **Windows Authentication** (`UseIntegratedSecurity=true`)
   - Использует текущие учётные данные Windows
   - Требует настройки SQL Server для смешанного режима

2. **SQL Authentication** (`UseIntegratedSecurity=false`)
   - Требует username/password
   - Пароль передаётся в открытом виде (только для доверенной сети!)

### Рекомендации для production

- Включите транспортное шифрование (HTTPS)
- Настройте WCF Security режим
- Не используйте `IncludeExceptionDetailInFaults=true`
- Ограничьте доступ к порту 8080 фаерволом

## 🛠 Troubleshooting

### Сервис не запускается

**Ошибка:** `HTTP URL could not be registered`

**Решение:** Зарегистрируйте URL-префикс от имени администратора:
```cmd
netsh http add urlacl url=http://+:8080/SqlService/ user=Everyone
```

### Клиент не может подключиться

**Проверьте:**
1. Сервис запущен и слушает порт 8080
2. URL в клиенте совпадает с адресом сервиса
3. Фаервол не блокирует порт 8080

### Ошибка SQL Server

Сервис возвращает детализированные ошибки SQL. Проверьте:
- Имя сервера и инстанс (формат: `SERVER\INSTANCE`)
- Доступность SQL Server (ping, telnet порт 1433)
- Права доступа учётной записи

## 📄 Лицензия

Проект предоставлен "как есть" без ограничений на использование.

## 📞 Контакты

Вопросы и предложения направляйте через Issues данного репозитория.
