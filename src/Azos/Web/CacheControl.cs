/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Text;

using Azos.Conf;

namespace Azos.Web
{
  /// <summary>
  /// Decorates controller classes or actions that set NoCache headers in response. By default Force = true
  /// </summary>
  public struct CacheControl
  {
    public const int DEFAULT_CACHE_MAX_AGE_SEC = 24 * //hrs
                                                 60 * //min
                                                 60;   //sec

    public enum Type
    {
      Public = 0,
      Private,
      NoCache
    }

    public static CacheControl FromConfig(IConfigSectionNode cfg) => ConfigAttribute.Apply(new CacheControl(), cfg);

    public static CacheControl PublicMaxAgeSec(int maxAgeSec = DEFAULT_CACHE_MAX_AGE_SEC)
     => new CacheControl { Cacheability = Type.Public, MaxAgeSec = maxAgeSec };
    public static CacheControl PrivateMaxAgeSec(int maxAgeSec = DEFAULT_CACHE_MAX_AGE_SEC)
     => new CacheControl { Cacheability = Type.Private, MaxAgeSec = maxAgeSec };

    public static CacheControl NoCache =>new CacheControl { Cacheability = Type.NoCache, NoStore = true };

    [Config] public Type Cacheability { get; set; }
    [Config] public int? MaxAgeSec { get; set; }
    [Config] public int? SharedMaxAgeSec { get; set; }

    [Config] public bool NoStore { get; set; }
    [Config] public bool NoTransform { get; set; }
    [Config] public bool MustRevalidate { get; set; }
    [Config] public bool ProxyRevalidate { get; set; }

    /// <summary>
    /// Build standard cache control header from fields
    /// </summary>
    public string HTTPCacheControl
    {
      get
      {
        var sb = new StringBuilder();

        switch (Cacheability)
        {
          case Type.Private: sb.Append("private, "); break;
          case Type.NoCache: sb.Append("no-cache, "); break;
          default: sb.Append("public, "); break;
        }
        if (Cacheability == Type.NoCache && NoStore) sb.Append("no-store, ");
        if (NoTransform) sb.Append("no-transform, ");
        if (Cacheability != Type.NoCache && MaxAgeSec.HasValue) sb.AppendFormat("max-age={0}, ", MaxAgeSec);
        if (Cacheability != Type.NoCache && SharedMaxAgeSec.HasValue) sb.AppendFormat("s-maxage={0}, ", SharedMaxAgeSec);
        if (MustRevalidate) sb.Append("must-revalidate, ");
        if (ProxyRevalidate) sb.Append("proxy-revalidate, ");
        return sb.ToString(0, sb.Length - 2);
      }
    }
  }
}
