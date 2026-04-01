using System.ServiceModel;

namespace SqlWebService
{
  [ServiceContract(Namespace = "http://sqlwebservice.local/v1")]
  public interface ISqlService
  {
    [OperationContract(Name = "Connect")]
    [return: System.ServiceModel.MessageParameter(Name = "ConnectResult")]
    ConnectResponse Connect([System.ServiceModel.MessageParameter(Name = "request")] ConnectRequest request);

    [OperationContract(Name = "GetSqlVersion")]
    [return: System.ServiceModel.MessageParameter(Name = "GetSqlVersionResult")]
    SqlVersionResponse GetSqlVersion([System.ServiceModel.MessageParameter(Name = "sessionId")] string sessionId);

    [OperationContract(Name = "Disconnect")]
    [return: System.ServiceModel.MessageParameter(Name = "DisconnectResult")]
    DisconnectResponse Disconnect([System.ServiceModel.MessageParameter(Name = "request")] DisconnectRequest request);
  }
}
