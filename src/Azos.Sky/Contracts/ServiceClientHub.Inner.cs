/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Linq;

using Azos.Conf;
using Azos.Collections;

namespace Azos.Sky.Contracts
{
  partial class ServiceClientHub
  {
    /// <summary>
    /// Provides mapping information for service contract
    /// </summary>
    [Serializable]
    public sealed class ContractMapping : INamed
    {
      public ContractMapping(IConfigSectionNode config)
      {
        try
        {
          var cname = config.AttrByName(CONFIG_MAP_CLIENT_CONTRACT_ATTR).Value;
          if (cname.IsNullOrWhiteSpace()) throw new Clients.SkyClientException(CONFIG_MAP_CLIENT_CONTRACT_ATTR + " is unspecified");
          m_Contract = Type.GetType(cname, true);

          m_Local = new Data(config, config[CONFIG_LOCAL_SECTION], false);
          m_Global = new Data(config, config[CONFIG_GLOBAL_SECTION], true);
        }
        catch (Exception error)
        {
          throw new Clients.SkyClientException(StringConsts.SKY_SVC_CLIENT_MAPPING_CTOR_ERROR.Args(
                                              config.ToLaconicString(CodeAnalysis.Laconfig.LaconfigWritingOptions.Compact),
                                              error.ToMessageWithType()), error);
        }
      }


      private Type m_Contract;
      //------------------------------
      private Data m_Local;
      private Data m_Global;


      public sealed class Data
      {

        internal Data(IConfigSectionNode baseConfig, IConfigSectionNode config, bool global)
        {
          var cname = config.AttrByName(CONFIG_MAP_IMPLEMENTOR_ATTR).ValueAsString(baseConfig.AttrByName(CONFIG_MAP_IMPLEMENTOR_ATTR).Value);
          if (cname.IsNullOrWhiteSpace()) throw new Clients.SkyClientException(CONFIG_MAP_IMPLEMENTOR_ATTR + " is unspecified");
          m_Implementor = Type.GetType(cname, true);

          if (!m_Implementor.GetInterfaces().Any(i => i == typeof(ISkyServiceClient)))
            throw new Clients.SkyClientException("Implementor {0} is not ISkyServiceClient".Args(m_Implementor.FullName));

          ConfigAttribute.Apply(this, baseConfig);
          ConfigAttribute.Apply(this, config);

          //service may be null

          if (m_Net.IsNullOrWhiteSpace())
            m_Net = global ? SysConsts.NETWORK_INTERNOC : SysConsts.NETWORK_NOCGOV;

          if (m_Binding.IsNullOrWhiteSpace())
            m_Binding = SysConsts.DEFAULT_BINDING;

          if (m_Options == null)
            m_Options = "options{ }".AsLaconicConfig();
        }

        private Type m_Implementor;
        [Config] private string m_Service = string.Empty;
        [Config] private string m_Net;
        [Config] private string m_Binding;
        [Config] private int m_CallTimeoutMs = 0;
        [Config] private bool m_ReserveTransport = false;
        [Config(CONFIG_OPTIONS_SECTION)] private IConfigSectionNode m_Options;

        public Type Implementor { get { return m_Implementor; } }
        public string Service { get { return m_Service; } }
        public string Net { get { return m_Net; } }
        public string Binding { get { return m_Binding; } }
        public int CallTimeoutMs { get { return m_CallTimeoutMs; } }
        public bool ReserveTransport { get { return m_ReserveTransport; } }
        public IConfigSectionNode Options { get { return m_Options; } }
      }

      public string Name { get { return m_Contract.AssemblyQualifiedName; } }
      public Type Contract { get { return m_Contract; } }
      public Data Local { get { return m_Local; } }
      public Data Global { get { return m_Global; } }

      public override string ToString()
      {
        return "Mapping[{0} -> Local: {1}; Global: {2}]".Args(m_Contract.FullName, Local.Implementor.FullName, Global.Implementor.FullName);
      }

      public override bool Equals(object obj)
      {
        var cm = obj as ContractMapping;
        if (cm == null) return false;
        return this.m_Contract == cm.m_Contract;
      }

      public override int GetHashCode()
      {
        return m_Contract.GetHashCode();
      }
    }
  }
}
