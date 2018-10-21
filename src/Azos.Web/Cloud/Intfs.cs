using Azos.Conf;
using Azos.Instrumentation;
using Azos.Log;

namespace Azos.Web.Cloud
{
  public interface ICloudSystem : Collections.INamed
  {
    IConfigSectionNode DefaultSessionConnectParamsCfg { get; set; }
    CloudSession StartSession(CloudConnectionParameters cParams = null);

    void Deploy(CloudSession session, string id, CloudTemplate template, IConfigSectionNode customData);
  }

  public interface ICloudSystemImplementation : ICloudSystem, IConfigurable, IInstrumentable
  {
    MessageType LogLevel { get; set; }
  }
}
