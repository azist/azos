/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Conf;
using Azos.Instrumentation;
using Azos.Serialization.Bix;

namespace Azos.Collections
{
  /// <summary>
  /// Define a contract for component which persists capped set data to external medium like a disk
  /// or db store
  /// </summary>
  public interface ICappedSetPersistenceHandler<T>
  {
    /// <summary>
    /// If true then persist bucket data periodically in an async fashion. Data loss is possible
    /// if program crashes before the persistence takes place.
    /// Otherwise, persists the whole bucket on mutation
    /// </summary>
    bool IsAsync { get; }

    /// <summary>
    /// Called upon class init, loads the initial bucket data into a set
    /// </summary>
    IEnumerable<KeyValuePair<T, DateTime>> GetInitBucketData(int idxBucket);

    /// <summary>
    /// Called either on every bucket mutation or asynchronously - persists whole bucket data to disk
    /// </summary>
    void PersistBucketData(int idxBucket, IEnumerable<KeyValuePair<T, DateTime>> data);
  }

  /// <summary>
  /// Persists capped set data in a file
  /// </summary>
  public abstract class CappedSetFilePersistenceHandler<T> : ApplicationComponent, ICappedSetPersistenceHandler<T>
  {
    protected CappedSetFilePersistenceHandler(IApplicationComponent director, IConfigSectionNode cfg) : base(director)
    {
      ConfigAttribute.Apply(this, cfg.NonEmpty(nameof(cfg)));

      if (m_RootPath.IsNullOrWhiteSpace() ||
          m_RootPath == "." ||
          m_RootPath == "./") m_RootPath = Directory.GetCurrentDirectory();

      Directory.Exists(m_RootPath).IsTrue("Existing dir");
      m_FilePrefix.NonBlank("file-prefix");
    }

    [Config] private string m_RootPath;
    [Config] private string m_FilePrefix;

    [Config, ExternalParameter(CoreConsts.EXT_PARAM_GROUP_DATA)]
    public bool IsAsync { get; private set; }


    public override string ComponentLogTopic => CoreConsts.COLLECTIONS_TOPIC;

    public string RootPath => m_RootPath;
    public string FilePrefix => m_FilePrefix;

    public IEnumerable<KeyValuePair<T, DateTime>> GetInitBucketData(int idxBucket)
    {
      var fn = GetFileNameForBucket(idxBucket);

      if (!File.Exists(fn)) return Enumerable.Empty<KeyValuePair<T, DateTime>>();

      using (var fs = new FileStream(fn, FileMode.Open, FileAccess.Read, FileShare.Read))
      {
        var result = DoReadBucketData(fs);
        fs.Close();
        return result;
      }
    }

    public void PersistBucketData(int idxBucket, IEnumerable<KeyValuePair<T, DateTime>> data)
    {
      var fn = GetFileNameForBucket(idxBucket);

      if (data == null || !data.Any())
      {
        IOUtils.EnsureFileEventuallyDeleted(fn);
        return;
      }

      using(var fs = new FileStream(fn, FileMode.Create, FileAccess.Write, FileShare.None))
      {
        DoWriteBucketData(fs, data);
        fs.Close();
      }
    }

    protected virtual string GetFileNameForBucket(int i)
     => Path.Combine(m_RootPath, "{0}{1:x2}.bucket".Args(m_FilePrefix, i));

    protected abstract IEnumerable<KeyValuePair<T, DateTime>> DoReadBucketData(FileStream fs);
    protected abstract void DoWriteBucketData(FileStream fs, IEnumerable<KeyValuePair<T, DateTime>> data);
  }


  /// <summary>
  /// Persists capped set data in a file
  /// </summary>
  public abstract class CappedSetBixFilePersistenceHandler<T> : CappedSetFilePersistenceHandler<T>
  {
    protected CappedSetBixFilePersistenceHandler(IApplicationComponent director, IConfigSectionNode cfg)
               : base(director, cfg) { }

    protected sealed override IEnumerable<KeyValuePair<T, DateTime>> DoReadBucketData(FileStream fs)
    {
      var reader = new BixReader(fs);

      if (fs.Length == 0) return Enumerable.Empty<KeyValuePair<T, DateTime>>();

      var result = new List<KeyValuePair<T, DateTime>>();

      while(fs.Position < fs.Length)
      {
        var hasMore = reader.ReadBool();
        if (!hasMore) break;

        var item = DoReadOneItem(reader);
        var dt = reader.ReadLong();
        result.Add(new KeyValuePair<T, DateTime>(item, dt.FromMillisecondsSinceUnixEpochStart()));
      }

      return result;
    }

    protected abstract T DoReadOneItem(BixReader reader);

    protected sealed override void DoWriteBucketData(FileStream fs, IEnumerable<KeyValuePair<T, DateTime>> data)
    {
      var writer = new BixWriter(fs);

      foreach(var one in data)
      {
        writer.Write(true);
        DoWriteOneItem(writer, one.Key);
        writer.Write(one.Value.ToMillisecondsSinceUnixEpochStart());
      }

      writer.Flush();
    }

    protected abstract void DoWriteOneItem(BixWriter writer, T one);
  }

  /// <summary>
  /// Persists capped set Guid data in a file
  /// </summary>
  public sealed class GuidSetFilePersistenceHandler : CappedSetBixFilePersistenceHandler<Guid>
  {
    public GuidSetFilePersistenceHandler(IApplicationComponent director, IConfigSectionNode cfg) : base(director, cfg) { }
    protected override Guid DoReadOneItem(BixReader reader) => reader.ReadGuid();
    protected override void DoWriteOneItem(BixWriter writer, Guid one) => writer.Write(one);
  }

  /// <summary>
  /// Persists capped set String data in a file
  /// </summary>
  public sealed class StringSetFilePersistenceHandler : CappedSetBixFilePersistenceHandler<string>
  {
    public StringSetFilePersistenceHandler(IApplicationComponent director, IConfigSectionNode cfg) : base(director, cfg) { }
    protected override string DoReadOneItem(BixReader reader) => reader.ReadString();
    protected override void DoWriteOneItem(BixWriter writer, string one) => writer.Write(one);
  }

}
