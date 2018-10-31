
using Azos.Conf;
using Azos.Glue;

namespace Azos.Sky.Mdb
{
  /// <summary>
  /// Denotes abstract contract for connection builders - entities that turn their instance properties into
  /// a string representation suitable for particular database instance connection
  /// </summary>
  public abstract class ConnectStringBuilderBase : IConfigStringBuilder
  {
    [Config] public string Host;
    [Config] public string Network;
    [Config] public string Service;
    [Config] public string Binding;

    protected Node m_ResolvedNode;
    protected string m_ResolvedService;

    public abstract string BuildString();

    public void Configure(IConfigSectionNode node)
    {
      ConfigAttribute.Apply(this, node);
      m_ResolvedNode = SkySystem.Metabase.ResolveNetworkService(Host, Network, Service, Binding);
    }
  }

  /// <summary>
  /// Defines connection string builder for Mongo Db
  /// </summary>
  public sealed class MongoDbConnectStringBuilder : ConnectStringBuilderBase
  {
    [Config] public string DB;
    //todo specify more detailed parameters in future

    public override string BuildString()
    {
      return "mongo{{server='{0}:{1}' db='{2}'}}".Args(m_ResolvedNode.Host, m_ResolvedNode.Service, DB);
    }
  }

  /// <summary>
  /// Defines connection string builder for My Sql
  /// </summary>
  public sealed class MySqlConnectStringBuilder : ConnectStringBuilderBase
  {
    [Config] public string DB;
    [Config] public string UserID;
    [Config] public string Password;
    [Config] public string ConnectionLifeTimeSec;
    //todo specify more detailed parameters in future

    public override string BuildString()
    {
      return "Server={0};Port={1};Database={2};Uid={3};Pwd={4};ConnectionLifeTime={5};".Args(m_ResolvedNode.Host,
                                                                                             m_ResolvedNode.Service,
                                                                                             DB,
                                                                                             UserID,
                                                                                             Password,
                                                                                             ConnectionLifeTimeSec);
    }
  }


  /// <summary>
  /// Defines connection string builder for Microsoft Sql Server
  /// </summary>
  public sealed class MsSqlConnectStringBuilder : ConnectStringBuilderBase
  {
    public override string BuildString()
    {
      #warning This needs revision for Ms Sql Server
      throw new System.NotImplementedException($"{nameof(MsSqlConnectStringBuilder)} is not implemented yet");
    }
  }

  /// <summary>
  /// Defines connection string builder for Oracle Server
  /// </summary>
  public sealed class OracleConnectStringBuilder : ConnectStringBuilderBase
  {
    public override string BuildString()
    {
       #warning This needs revision for ORACLE
       throw new System.NotImplementedException($"{nameof(OracleConnectStringBuilder)} is not implemented yet");
    }
  }

}
