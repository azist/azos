/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;

using Azos.Collections;

namespace Azos.Pile
{
  /// <summary>
  /// Implements a Decorator Pattern over ICache instance.
  /// The class is primarily used to introduce namespace prefix to tableName
  /// </summary>
  public class CacheDecorator : ICache
  {
    public CacheDecorator(ICache cache, string tblNamespace = null)
    {
      m_Cache = cache.NonNull(nameof(cache));
      m_TableNamespace = tblNamespace;
    }

    protected readonly ICache m_Cache;
    protected readonly string m_TableNamespace;

    /// <summary>
    /// References the original ICache object that this objects decorates
    /// </summary>
    public ICache Cache => m_Cache;

    /// <summary>
    /// Namespace prefix appended to every table accessed via this decorator or null if none is used
    /// </summary>
    public virtual string TableNamespace => m_TableNamespace;

    public virtual LocalityKind Locality => m_Cache.Locality;

    public virtual ObjectPersistence Persistence => m_Cache.Persistence;

    public virtual IPileStatus PileStatus => m_Cache.PileStatus;

    public virtual IRegistry<ICacheTable> Tables => m_Cache.Tables;

    public virtual long Count => m_Cache.Count;

    public virtual ICacheTable<TKey> GetOrCreateTable<TKey>(string tableName, IEqualityComparer<TKey> keyComparer = null)
     => m_Cache.GetOrCreateTable(TransformTableName(tableName), keyComparer);

    public virtual ICacheTable<TKey> GetOrCreateTable<TKey>(string tableName, out bool createdNew, IEqualityComparer<TKey> keyComparer = null)
     => m_Cache.GetOrCreateTable(TransformTableName(tableName), out createdNew, keyComparer);

    public virtual ICacheTable<TKey> GetTable<TKey>(string tableName)
     => m_Cache.GetTable<TKey>(TransformTableName(tableName));

    public virtual void PurgeAll() => m_Cache.PurgeAll();


    /// <summary>
    /// Transforms table name per rules set by this accessor.
    /// The default implementation recomposes `tableName` with optional `TableNamespace` if it is set
    /// </summary>
    public virtual string TransformTableName(string tableName)
    {
      tableName.NonBlank(nameof(tableName));
      var ns = TableNamespace;
      if (ns.IsNullOrWhiteSpace()) return tableName;
      return ns + '.' + tableName;
    }

  }
}
