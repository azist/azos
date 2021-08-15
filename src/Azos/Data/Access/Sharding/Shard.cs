/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using Azos.Conf;

namespace Azos.Data.Access.Sharding
{
  /// <summary>
  /// Represents a shard of a <see cref="ShardSet"/>
  /// </summary>
  public class Shard : IShard
  {
    public const string CONFIG_WEIGHT_ATTR = "weight";
    public const string CONFIG_CONNECT_ATTR = "connect";
    public const string CONFIG_CONNECT_STRING_ATTR = "connect-string";
    public const string CONFIG_DB_ATTR = "db";

    protected internal Shard(ShardSet set, IConfigSectionNode conf)
    {
      m_Set = set.NonNull(nameof(set));
      ConfigAttribute.Apply(this, conf.NonEmpty(nameof(conf)));
      m_Name = conf.ValOf(Configuration.CONFIG_NAME_ATTR).NonBlank("$name");
      m_NameHash = ShardKey.ForString(m_Name);

      m_Weight = conf.Of(CONFIG_WEIGHT_ATTR).ValueAsDouble(1.0d);

      m_ConnectString = conf.Of(CONFIG_CONNECT_ATTR, CONFIG_CONNECT_STRING_ATTR).Value.NonBlank($"${CONFIG_CONNECT_ATTR}");
      m_DatabaseName = conf.Of(CONFIG_DB_ATTR).Value.NonBlank($"${CONFIG_DB_ATTR}");
    }

    private readonly ShardSet m_Set;
    private readonly string m_Name;
    protected internal readonly ulong m_NameHash;
    private double m_Weight = 1.0d;
    private string m_ConnectString;
    private string m_DatabaseName;

    public ShardSet Set => m_Set;
    public string Name => m_Name;
    public double ShardWeight => m_Weight;
    public string RouteConnectString => m_ConnectString;
    public string RouteDatabaseName => m_DatabaseName;
    public CrudOperations Operations => new CrudOperations(this);
  }
}
