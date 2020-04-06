/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.CompilerServices;
using Azos.Data;

#pragma warning disable CA1063

namespace Azos.Serialization.Bix
{
  /// <summary>
  /// Provides context scope for Bix operations: serialization and de-serialization.
  /// You can override and provide custom context which provides custom document read/write handling
  /// </summary>
  public class BixContext : IDisposable
  {
    public const int DEFAULT_MAX_DEPTH = 32;

    [ThreadStatic] public static BixContext ts_Instance;

    /// <summary>
    /// Returns an instance of context initialized for making local blocking call.
    /// The instance needs to be released by calling .Dispose()
    /// </summary>
    public static T Obtain<T>(string targetName  =  null,
                                int maxDepth       =  DEFAULT_MAX_DEPTH,
                                bool hasHeader     =  true,
                                bool polyRoot      =  true,
                                bool polyField     =  true,
                                object state       =  null) where T : BixContext, new()
    {
      var self = ts_Instance as T;
      if (self==null)
        self = new T();
      else
        ts_Instance = null;

      self.TargetName = targetName.IsNullOrWhiteSpace() ? FieldAttribute.ANY_TARGET : targetName;
      self.MaxDepth = maxDepth.KeepBetween(0, 1024);
      self.HasHeader = hasHeader;
      self.PolymorphicRoot = polyRoot;
      self.PolymorphicFields = polyField;
      self.State = state;

      return self;
    }

    internal static BixContext ObtainDefault()
    {
      var self = ts_Instance;
      if (self == null)
        self = new BixContext();
      else
        ts_Instance = null;

      self.m_Default = true;

      return self;
    }

    private bool m_Default;
    private int m_Nesting;

    /// <summary>
    /// Recycles the instance so it can be re-used by the next call to .Obtain()
    /// </summary>
    public virtual void Dispose()
    {
      m_Default = false;
      m_Nesting = 0;
      State = null;
      ts_Instance = this;
    }

    internal void DisposeDefault()
    {
      if (m_Default) Dispose();
    }

    public string  TargetName        { get; private set; }
    public int     MaxDepth          { get; private set; }
    public bool    HasHeader         { get; private set; }
    public bool    PolymorphicRoot   { get; private set; }
    public bool    PolymorphicFields { get; private set; }
    public object  State             { get; private set; }

    /// <summary>
    /// Override to get targeted type of the document to write and perform other pre-processing such
    /// as calling IAmorphousData.BeforeSave(TargetName) if enabled
    /// Return true if your method handled serialization (in which case you need to write appropriate TypeCode),
    /// false to indicate that writing is not handled by your code and the system should continue writing
    /// </summary>
    protected internal virtual (TargetedType type, bool handled) OnDocWrite(BixWriter writer, TypedDoc doc)
    {
      if (doc is IAmorphousData ad && ad.AmorphousDataEnabled)
      {
        ad.BeforeSave(TargetName);
      }
      return (new TargetedType(TargetName, doc.GetType()), false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void IncreaseNesting()
    {
      m_Nesting++;
      if (m_Nesting > MaxDepth) throw new BixException(StringConsts.BIX_MAX_SERIALIZATION_DEPTH_ERROR.Args(MaxDepth));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DecreaseNesting()
    {
      m_Nesting--;
    }

  }
}

#pragma warning restore CA1063
