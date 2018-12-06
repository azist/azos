using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Glue;
using Azos.Conf;
using Azos.Collections;
using Azos.Glue.Protocol;
using Azos.Serialization.BSON;

namespace Azos.Sky.Contracts
{
  /// <summary>
  /// Provides access to ServiceClientHub singleton
  /// </summary>
  public static class ServiceClientHubAccessor
  {
    /// <summary>
    /// Provides access to ServiceClientHub singleton instance of app context
    /// </summary>
    public static ServiceClientHub ClientHub(this IApplication app)
      => app.NonNull(nameof(app))
            .Singletons
            .GetOrCreate(() =>
            {
              string tpn = typeof(ServiceClientHub).FullName;
              try
              {
                var mbNode = SkySystem.Metabase.ServiceClientHubConfNode as ConfigSectionNode;
                var appNode = app.ConfigRoot[SysConsts.APPLICATION_CONFIG_ROOT_SECTION]
                                            [ServiceClientHub.CONFIG_SERVICE_CLIENT_HUB_SECTION] as ConfigSectionNode;

                var effectiveConf = new MemoryConfiguration();
                effectiveConf.CreateFromMerge(mbNode, appNode);
                var effective = effectiveConf.Root;

                tpn = effective.AttrByName(FactoryUtils.CONFIG_TYPE_ATTR).ValueAsString(typeof(ServiceClientHub).FullName);

                return FactoryUtils.MakeComponent<ServiceClientHub>(app, effective, typeof(ServiceClientHub), new object[] { effective });
              }
              catch (Exception error)
              {
                throw new Clients.SkyClientException(StringConsts.SKY_SVC_CLIENT_HUB_SINGLETON_CTOR_ERROR
                                                                 .Args(tpn, error.ToMessageWithType()), error);
              }
            }).instance;
  }

}
