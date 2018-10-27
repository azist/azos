/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
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
