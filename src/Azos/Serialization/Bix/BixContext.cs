/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Data;
using System;

namespace Azos.Serialization.Bix
{
  /// <summary>
  /// Provides context scope for Bix operations: serialization and de-serialization
  /// </summary>
  public sealed class BixContext : IDisposable
  {
    public const int DEFAULT_MAX_DEPTH = 32;

    [ThreadStatic] public static BixContext ts_Instance;

    /// <summary>
    /// Returns an instance of context initialized for making local blocking call.
    /// The instance needs to be released by calling .Dispose()
    /// </summary>
    public static BixContext Obtain(string targetName  =  null,
                                int maxDepth       =  DEFAULT_MAX_DEPTH,
                                bool hasHeader     =  true,
                                bool polyRoot      =  true,
                                bool polyField     =  true,
                                object state       =  null)
    {
      var self = ts_Instance;
      if (self==null)
        self = new BixContext();
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

    /// <summary>
    /// Recycles the instance so it can be re-used by the next call to .Obtain()
    /// </summary>
    public void Dispose()
    {
      m_Default = false;
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
  }
}
